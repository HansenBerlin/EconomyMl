using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Enums;
using Factories;
using Models.Finance;
using Models.Market;
using Policies;
using Repositories;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Models.Business
{
    public class PublicServiceAgent : CompanyBaseAgent
    {
        private decimal ObservationAveragePriceResource => AverageResourcePrice("r");
        private decimal ObservationAveragePriceEnergy => AverageResourcePrice("e");
        private decimal ObservationSupply => CountryEconomyMarkets.TotalSupply(TypeProduced);

        private decimal ObservationCostPerPiece =>
            ObservationFixSpendingsPerProduct + ObservationVariableSpendingsPerProduct;

        private decimal ObservationCompetitorAveragePrice => CountryEconomyMarkets.AveragePrice(TypeProduced);

        //private decimal ObserveMarketShare { get; set; }
        private decimal ObsAveragePerWorkerSalary => GetWorkforcePayments() / ObserveTotalWorkers;

        private decimal ObservationFixSpendingsPerProduct => ObsAveragePerWorkerSalary / Production.UnitsPerWorker +
                                                             Production.BaseCostPerPieceProduced;

        private decimal ObservationVariableSpendingsPerProduct =>
            ObsVariableSpendingsEnergy + ObsVariableSpendingsResource;

        private decimal ObsVariableSpendingsEnergy => ObservationAveragePriceEnergy * Production.EnergyNeededPerPiece;

        private decimal ObsVariableSpendingsResource =>
            ObservationAveragePriceResource * Production.ResourceNeededPerPiece;

        private CompanyActionPhase currentActionPhase;
        public override void MakeDecision(CompanyActionPhase phase)
        {
            currentActionPhase = phase;
            RequestDecision();
            Academy.Instance.EnvironmentStep();

        }

        public override void EndYear()
        {
            EndEpisode();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            //if (Death != DeathReason.HasNotDied) return;

            // durchschn Marktpreis Resourcen
            // durchschn Makrtpreis Produkt
            // verkäufe, Demand, produktion
            // verfügbare resourcen
            sensor.AddObservation(Workers.Count);
            sensor.AddObservation((long)Balance);
            sensor.AddObservation(ProductController.TotalSupply);
            sensor.AddObservation((float)ProductController.Price);
            sensor.AddObservation(Production.AvailableProductionEnergy);
            sensor.AddObservation(Production.AvailableProductionResources);
            sensor.AddObservation((float) ProductController.Profit);
            sensor.AddObservation((float) ProductController.ProfitLastMonth);
            sensor.AddObservation(ProductController.SalesThisMonth);
            sensor.AddObservation(ProductController.SalesLastMonth);
            sensor.AddObservation(ProductController.ProductionThisMonth);
            sensor.AddObservation(ProductController.ProductionLastMonth);

        }


        private List<LoanModel> _loans = new();
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var requestCredit = actionBuffers.DiscreteActions[0] + 1;
            var maxProduction = actionBuffers.DiscreteActions[1] + 1;
            var setSalary = actionBuffers.DiscreteActions[2] + 1; // 1-100
            var adaptPrice = actionBuffers.ContinuousActions[0];
            var buyResourcesFromBalance = actionBuffers.ContinuousActions[1];
            var adaptWorkForce = actionBuffers.ContinuousActions[2];

            
            
            if (currentActionPhase == CompanyActionPhase.Produce)
            {
                ActionProduce(maxProduction);
            }
            
            if (currentActionPhase == CompanyActionPhase.BuyResources)
            {
                if (buyResourcesFromBalance < 0)
                {
                    AddReward(-0.1f);
                }
                else if (EnergyTypeNeeded != ProductType.None && ResourceTypeNeeded != ProductType.None)
                {
                    ActionBuyNeededProductionResources((decimal)buyResourcesFromBalance);
                }
            }

            if (currentActionPhase == CompanyActionPhase.AdaptWorkerCapacity)
            {
                if (adaptWorkForce < 0)
                {
                    AddReward(-0.1f);
                }
                else
                {
                    ActionAdaptProductionCapacity(adaptWorkForce, setSalary);
                }
            }
        }


        public override void ActionProduce(int productionPercentage)
        {
            var maxUnitsProduced = PossibleProduction;
            //int finalProduction = (int) (maxUnitsProduced * productionPercentage) / 10;
            int finalProduction = maxUnitsProduced;
            ProductController.AddNew(finalProduction);
            _unitsProducedInMonth += finalProduction;

            CountryEconomyMarkets.ReportProduction(finalProduction, TypeProduced);

            Production.AvailableProductionEnergy -= (int)(maxUnitsProduced * Production.EnergyNeededPerPiece);
            Production.AvailableProductionResources -= (int)(maxUnitsProduced * Production.ResourceNeededPerPiece);
        }
        

        private decimal AverageResourcePrice(string type)
        {
            if (type == "e")
            {
                decimal price = 0;
                if (Production.EnergyNeededPerPiece != 0)
                    price = CountryEconomyMarkets.AveragePrice(Production.EnergyTypeNeeded);
                return price;
            }
            else
            {
                decimal price = 0;
                if (Production.ResourceNeededPerPiece != 0)
                    price = CountryEconomyMarkets.AveragePrice(Production.ResourceTypeNeeded);
                return price;
            }
        }

        private decimal last;

        private decimal CalculateMaxWorkerSalary()
        {
            decimal costsPpWithoutWorkerCosts =
                Production.BaseCostPerPieceProduced + ObservationVariableSpendingsPerProduct;
            decimal leftToSpend = (ProductController.Price * 0.9M - costsPpWithoutWorkerCosts) *
                                  Production.UnitsPerWorker;
            if (TypeProduced == ProductType.FossileEnergy)
            {
                Console.WriteLine($"Last payment: {last}");
                Console.WriteLine($"New payment: {leftToSpend}");
                Console.WriteLine($"Cost per worker: {ObsAveragePerWorkerSalary}");
            }

            last = leftToSpend;
            return leftToSpend;
        }

        public void ActionBuyNeededProductionResources(decimal maxSpendings)
        {
            decimal maxSpendinsTotal = Balance * maxSpendings;
            
            if (Production.ResourceTypeNeeded != ProductType.None)
            {
                try
                {
                    decimal resourceSplit = Production.ResourceNeededPerPiece / (Production.ResourceNeededPerPiece + Production.EnergyNeededPerPiece);
                    decimal maxResourceSpendings = resourceSplit * maxSpendinsTotal;
                    //int resourcesDemanded = (int)(maxResourceSpendings / CountryEconomyMarkets.AveragePrice(ResourceTypeNeeded));                    int resourcesDemanded = (int)CalculateDemandForMonthlyProduction("r");
                    int resourcesDemanded = (int)CalculateDemandForMonthlyProduction("r");

                    ActionBuyResources(maxResourceSpendings, resourcesDemanded);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            if (Production.EnergyTypeNeeded != ProductType.None)
            {
                try
                {
                    decimal energySplit = Production.EnergyNeededPerPiece / (Production.ResourceNeededPerPiece + Production.EnergyNeededPerPiece);
                    decimal maxEnergySpendings = energySplit * maxSpendinsTotal;
                    //int energyDemanded = (int)(maxEnergySpendings / CountryEconomyMarkets.AveragePrice(EnergyTypeNeeded));
                    int energyDemanded = (int)CalculateDemandForMonthlyProduction("e");

                    ActionBuyEnergy(maxEnergySpendings, energyDemanded);

                }
                catch (OverflowException e)
                {
                    Console.WriteLine(e);
                }
            }
            
        }

        public override void ActionBuyResources(decimal maxSpendings, long resourcesDemanded)
        {
            var requestBuyResource = new ProductRequestModel(ResourceTypeNeeded,
                ProductRequestSearchType.MaxSpendable, totalSpendable: maxSpendings);

            var productReceipt = CountryEconomyMarkets.Buy(requestBuyResource);
            Production.AvailableProductionResources += productReceipt.AmountBought;
            Balance -= productReceipt.TotalPricePaid;
            LastProdCostsInMonthForRessourcesAndEnergy += productReceipt.TotalPricePaid;
            resourcesDemanded -= productReceipt.AmountBought;
            if (resourcesDemanded > 0)
            {
                CountryEconomyMarkets.ReportDemand(resourcesDemanded, ResourceTypeNeeded);
            }
        }
        
        public override void ActionBuyEnergy(decimal maxSpendings, long energyDemanded)
        {
            var requestBuyResource = new ProductRequestModel(EnergyTypeNeeded,
                ProductRequestSearchType.MaxSpendable, totalSpendable: maxSpendings);

            var productReceipt = CountryEconomyMarkets.Buy(requestBuyResource);
            Production.AvailableProductionEnergy += productReceipt.AmountBought;
            Balance -= productReceipt.TotalPricePaid;
            LastProdCostsInMonthForRessourcesAndEnergy += productReceipt.TotalPricePaid;
            energyDemanded -= productReceipt.AmountBought;
            if (energyDemanded > 0)
            {
                CountryEconomyMarkets.ReportDemand(energyDemanded, EnergyTypeNeeded);
            }
        }
        

        public override void ActionAdaptPrices(float newPriceMultiplier)
        {
            // -0.2 = auf 80% senken 1 + -0.2 = 0,8
            // -0.7 = auf 30% senken 1 + -0,7 = 0.3
            // 0.4 = auf 140% heben 1 + 0,4
            // 1 = verdoppeln
            decimal change = (decimal)(1 + newPriceMultiplier);
            var oldP = ProductController.Price;
            var newP = ProductController.Price * change;

            ProductController.UpdatePrice(newP);
        }

        public override void ActionAdaptProductionCapacity(float changeProductionCapabilities, int maxSalary)
        {
            /*decimal supply = ProductController.QuarterlySupplyAverage;
            decimal production = ProductController.QuarterlyProductionAverage;
            decimal sales = ProductController.QuarterlySalesAverage;

            decimal trend = WorkerAdaptionController.CalculateWorkerModifier(supply, production, sales);

            decimal reductionRateCapacityUsed =
                WorkerAdaptionController.CalculateCapacityModifier(CapacityUsed);*/


            if (changeProductionCapabilities > 0)
            {
                AddReward(0.1F);
                maxSalary *= 100;
                var additionalWorkers = Workers.Count * changeProductionCapabilities;
                if (additionalWorkers <= 0) return;
                var openPositions = JobPositionFactory.Create((int) additionalWorkers, maxSalary, Id, Workers, TypeProduced);
                JobMarket.AdaptSalaryForLeftopenPositions(maxSalary, Id);
                JobMarket.AddOpenJobPositions(openPositions, (int) additionalWorkers, Id);
            }
            else if (changeProductionCapabilities < 0)
            {
                int fireWorkers = (int)(Workers.Count * changeProductionCapabilities * -1);
                FireWorkers(fireWorkers);
                JobMarket.RemoveOpenJobPositions(fireWorkers, Id);
            }
        }
        
        public override void AddRewards()
        {
            AddReward((float)ProductController.ObsProductionTrend);
            AddReward((float)ProductController.ObsSalesTrend);
            AddReward((float)ProductController.ObsProfitTrend);
            float capitalReward = Balance < 0 ? -1 : 1;
            AddReward(capitalReward);
            _unitsProducedInMonth = 0;
            LastProdCostsInMonthForRessourcesAndEnergy = 0;
            _lastWorkerPayments = 0;
            ProfitTaxPaidInMonth = 0;
            ProfitAfterTaxesInMonth = 0;
            UpgradeEffiencyCosts = 0;
            MissingResourceDemand = 0;
            CashflowIn = 0;
            LoanPayments = 0;
        }

        public override void MonthlyBookkeeping()
        {
            PayWorkers();
            decimal profit = ProductController.Profit - TotalCostBeforeTaxes - UpgradeEffiencyCosts;
            ProfitTaxPaidInMonth = Government.PayProfitTax(profit);
            ProfitAfterTaxesInMonth = profit - ProfitTaxPaidInMonth;
            Balance -= FixedPerProductCosts + ProfitTaxPaidInMonth;
            Balance += ProductController.Profit;
            CashflowIn = ProductController.Profit;
        }

        /*protected override void UpdateStats(decimal income)
        {
            if (TypeProduced == ProductType.FossileEnergy)
            {
                Console.WriteLine();
            }
            //ProductController.Update(EpisodeCut.Month, cpp, capacityUsed);
            Data.BalanceStats.Add((double)Balance);
            Data.TotalProduced.Add(UnitsProducedInMonth);
            Data.WorkersStat.Add(Workers.Count);
            Data.MoneyOutStat.Add((double)(TotalCostBeforeTaxes + ProfitTaxPaidInMonth + UpgradeEffiencyCosts));
            Data.MoneyInStat.Add((double)(income));
        }*/
        
    }
}