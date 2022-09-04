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



    public class PrivateCompanyAgent : CompanyBaseAgent
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

        public override void EndYear(CompanyActionPhase phase)
        {
            Academy.Instance.StatsRecorder.Add("YEAR/PROD-TREND" + TypeProduced, (float)ProductController.ObsProductionTrend);
            Academy.Instance.StatsRecorder.Add("YEAR/PROFIT-TREND" + TypeProduced, (float)ProductController.ObsProfitTrend);
            Academy.Instance.StatsRecorder.Add("YEAR/SALESTREND" + TypeProduced, (float)ProductController.ObsSalesTrend);
            
            currentActionPhase = phase;
            float capitalReward = Balance < BalanceLastYear ? -0.5f : 0.5f;
            float rewardTrends = Normalize((float)ProductController.ObsProductionTrend + ((float)ProductController.ObsSalesTrend +(float)ProductController.ObsProfitTrend));
            AddReward(capitalReward + rewardTrends / 2);
            BalanceLastYear = Balance;
            EndEpisode();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
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
            var totalDemand = CountryEconomyMarkets.GetTotalUnfulfilledDemand(TypeProduced);
            var marketShare = CountryEconomyMarkets.MarketShare(TypeProduced, ProductController.Id);
            sensor.AddObservation(totalDemand);
            sensor.AddObservation((float)marketShare);
        }


        private List<LoanModel> _loans = new();
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (_month > 240)
            {
                Debug.Log("");
            }
            var requestCredit = actionBuffers.DiscreteActions[0];
            var setSalary = actionBuffers.DiscreteActions[1] + 1; // 1-100
            var maxProduction = actionBuffers.DiscreteActions[2] + 1; // 1 - 2 (50%, 100%)
            var adaptPrice = actionBuffers.ContinuousActions[0];
            var buyResourcesFromBalance = (actionBuffers.ContinuousActions[1] + 2) / 3;  // yw 33 und 100%
            var adaptWorkForce = actionBuffers.ContinuousActions[2];

            try
            {
                if (currentActionPhase != CompanyActionPhase.Produce && currentActionPhase != CompanyActionPhase.AdaptPrice)
                {
                    if (_loans.Sum(x => x.TotalSumLeft) < 1000000 && requestCredit > 0)
                    {
                        decimal creditSum = Balance < 0 ? Balance * -1 * requestCredit : Balance * requestCredit;
                        creditSum = creditSum < 1000 ? 1000 : creditSum;
                        var loan = CountryEconomyMarkets.GetLoan(creditSum, CreditRating.A);
                        if (loan.IsDeclined == false)
                        {
                            _loans.Add(loan);
                            Balance += loan.TotalSumLeft;
                        }
                    }
                    else
                    {
                        AddReward(0.05f);
                    }
                }
                if (currentActionPhase == CompanyActionPhase.AdaptPrice)
                {
                    ActionAdaptPrices(adaptPrice);
                }
                if (currentActionPhase == CompanyActionPhase.Produce)
                {
                    ActionProduce(maxProduction);
                    //AddReward((float)CapacityUsed);
                }
            
                if (currentActionPhase == CompanyActionPhase.BuyResources)
                {
                    ActionBuyNeededProductionResources((decimal)buyResourcesFromBalance);
                }

                if (currentActionPhase == CompanyActionPhase.AdaptWorkerCapacity)
                {
                    ActionAdaptProductionCapacity(adaptWorkForce, setSalary);
                }

            }
            catch (OverflowException e)
            {
                Console.WriteLine(e);
            }
        }


        public override void ActionProduce(int productionPercentage)
        {
            var maxUnitsProduced = PossibleProduction;
            int finalProduction = productionPercentage == 1 ? maxUnitsProduced / 2 : maxUnitsProduced;
            //int finalProduction = maxUnitsProduced * productionPercentage / 10;
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
                    int energyDemanded = (int)CalculateDemandForMonthlyProduction("e");
                    ActionBuyEnergy(maxEnergySpendings, energyDemanded);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            
        }

        public override void ActionBuyResources(decimal maxSpendings, long resourcesDemanded)
        {
            var requestBuyResource = new ProductRequestModel(ResourceTypeNeeded,
                ProductRequestSearchType.MaxAmountWithSpendingLimit, totalSpendable: maxSpendings, maxAmount:resourcesDemanded);

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
                ProductRequestSearchType.MaxAmountWithSpendingLimit, totalSpendable: maxSpendings, maxAmount:energyDemanded);

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
            change = change < 0.8M ? 0.8M : change > 1.2M ? 1.2M : change;
            var oldP = ProductController.Price;
            if (change > 0)
            {
                var newP = ProductController.Price * change;
                ProductController.UpdatePrice(newP);
            }

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
                maxSalary *= 1000;
                var additionalWorkers = Math.Ceiling(Workers.Count * changeProductionCapabilities);
                additionalWorkers = Workers.Count < 10 ? 10 : additionalWorkers;
                if (additionalWorkers <= 0) return;
                var openPositions = JobPositionFactory.Create((int) additionalWorkers, maxSalary, Id, Workers, TypeProduced);
                JobMarket.AdaptSalaryForLeftopenPositions(maxSalary, Id);
                JobMarket.AddOpenJobPositions(openPositions, (int) additionalWorkers, Id);
            }
            else if (changeProductionCapabilities < 0)
            {
                int fireWorkers = (int)Math.Ceiling(Workers.Count * changeProductionCapabilities * -1);
                if (Workers.Count > 3)
                {
                    FireWorkers(fireWorkers);
                }
            }
        }

        public override void MonthlyBookkeeping()
        {
            PayWorkers();

            for (int i = _loans.Count - 1; i >= 0; i--)
            {
                var loan = _loans[i];
                var payment = loan.MakeMonthlyPayment();
                if (payment == 0)
                {
                    _loans.Remove(loan);
                }

                LoanPayments += payment;
                
            }

            if (Workers.Count == 0 && LoanPayments > 0)
            {
                Debug.Log("");
            }
            
            decimal profit = ProductController.Profit - TotalCostBeforeTaxes - UpgradeEffiencyCosts;
            ProfitTaxPaidInMonth = Government.PayProfitTax(profit);
            ProfitAfterTaxesInMonth = profit - ProfitTaxPaidInMonth;
            Balance -= FixedPerProductBaseCosts + ProfitTaxPaidInMonth + LoanPayments;
            Balance += ProductController.Profit;
            CashflowIn = ProductController.Profit;
            CountryEconomyMarkets.ReportStats(TypeProduced, Workers.Count, (float)Balance, (float)CashflowIn, (float)CashflowOut, 
                ProductController.ProductionThisMonth, ProductController.SalesThisMonth, (float)ProductController.Price, (float)Cpp);

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