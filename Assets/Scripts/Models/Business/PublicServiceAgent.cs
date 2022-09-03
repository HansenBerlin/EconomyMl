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
            sensor.AddObservation(Workers.Count);
            sensor.AddObservation((long)Balance);
            sensor.AddObservation(ProductController.TotalSupply);
            sensor.AddObservation(Production.AvailableProductionEnergy);
            sensor.AddObservation(Production.AvailableProductionResources);
            sensor.AddObservation(ProductController.ProductionThisMonth);
            sensor.AddObservation(ProductController.ProductionLastMonth);
        }


        private List<LoanModel> _loans = new();
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var adaptWorkForce = actionBuffers.ContinuousActions[0];

            ActionAdaptProductionCapacity(adaptWorkForce, 0);
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
                ProductRequestSearchType.MaxAmount, maxAmount: resourcesDemanded);

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
                ProductRequestSearchType.MaxAmount, maxAmount: energyDemanded);

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
            
        }

        public override void ActionAdaptProductionCapacity(float changeProductionCapabilities, int maxSalary)
        {
            /*decimal supply = ProductController.QuarterlySupplyAverage;
            decimal production = ProductController.QuarterlyProductionAverage;
            decimal sales = ProductController.QuarterlySalesAverage;

            decimal trend = WorkerAdaptionController.CalculateWorkerModifier(supply, production, sales);

            decimal reductionRateCapacityUsed =
                WorkerAdaptionController.CalculateCapacityModifier(CapacityUsed);*/


            var workersNeeded = Government.RecalculateFederalWorkerDemand();
            if (workersNeeded > Workers.Count)
            {
                maxSalary = (int)Government.GetMaxFederalWorkerPayment();
                var additionalWorkers = workersNeeded - Workers.Count;
                if (additionalWorkers <= 0) return;
                var openPositions = JobPositionFactory.Create((int) additionalWorkers, maxSalary, Id, Workers, TypeProduced);
                JobMarket.AdaptSalaryForLeftopenPositions(maxSalary, Id);
                JobMarket.AddOpenJobPositions(openPositions, (int) additionalWorkers, Id);
            }
            else if (workersNeeded < Workers.Count)
            {
                int fireWorkers = Workers.Count - workersNeeded;
                FireWorkers(fireWorkers);
                JobMarket.RemoveOpenJobPositions(fireWorkers, Id);
            }
        }
        
        public override void AddRewards()
        {
            float productionReward = ProductController.ProductionThisMonth > ProductController.ProductionLastMonth ? 0.5f : -0.5f;
            float capitalReward = Balance < 0 ? -0.5f : 0.5f;
            AddReward(capitalReward + productionReward);
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