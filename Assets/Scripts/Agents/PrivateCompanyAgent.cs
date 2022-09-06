using System;
using Controller.Agents;
using Enums;
using Factories;
using Models;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agents
{
    public class PrivateCompanyAgent : CompanyBaseAgent
    {
        private CompanyActionPhase _currentActionPhase;

        private decimal _last;

        public override void MakeDecision(CompanyActionPhase phase)
        {
            _currentActionPhase = phase;
            RequestDecision();
            Academy.Instance.EnvironmentStep();
        }

        public override void EndYear(CompanyActionPhase phase)
        {
            Academy.Instance.StatsRecorder.Add("YEAR/PROD-TREND" + TypeProduced,
                (float) ProductController.ObsProductionTrend);
            Academy.Instance.StatsRecorder.Add("YEAR/PROFIT-TREND" + TypeProduced,
                (float) ProductController.ObsProfitTrend);
            Academy.Instance.StatsRecorder.Add("YEAR/SALESTREND" + TypeProduced,
                (float) ProductController.ObsSalesTrend);

            _currentActionPhase = phase;
            float capitalReward = Balance < BalanceLastYear ? -0.5f : 0.5f;
            float rewardTrends = Normalize((float) ProductController.ObsProductionTrend +
                                           ((float) ProductController.ObsSalesTrend +
                                            (float) ProductController.ObsProfitTrend));
            AddReward(capitalReward + rewardTrends / 2);
            BalanceLastYear = Balance;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(NormCtr.Normalize(nameof(Workers.Count), Workers.Count));
            sensor.AddObservation(NormCtr.Normalize(nameof(ProductController.TotalSupply),
                ProductController.TotalSupply));
            sensor.AddObservation(NormCtr.Normalize(nameof(ProductController.Price), (float) ProductController.Price));
            sensor.AddObservation(NormCtr.Normalize(nameof(ProductController.Profit),
                (float) ProductController.Profit));
            sensor.AddObservation(NormCtr.Normalize(nameof(ProductController.ProfitLastMonth),
                (float) ProductController.ProfitLastMonth));
            sensor.AddObservation(NormCtr.Normalize(nameof(ProductController.SalesLastMonth),
                ProductController.SalesLastMonth));
            sensor.AddObservation(NormCtr.Normalize(nameof(ProductController.SalesThisMonth),
                ProductController.SalesThisMonth));
            sensor.AddObservation(NormCtr.Normalize(nameof(ProductController.ProductionThisMonth),
                ProductController.ProductionThisMonth));
            sensor.AddObservation(NormCtr.Normalize(nameof(ProductController.ProductionLastMonth),
                ProductController.ProductionLastMonth));
            sensor.AddObservation(NormCtr.Normalize(nameof(Production.AvailableProductionEnergy),
                Production.AvailableProductionEnergy));
            sensor.AddObservation(NormCtr.Normalize(nameof(Production.AvailableProductionResources),
                Production.AvailableProductionResources));
            sensor.AddObservation(NormCtr.Normalize(nameof(TotalDemand), TotalDemand));
            sensor.AddObservation(NormCtr.Normalize(nameof(MarketShare), (float) MarketShare));
            sensor.AddObservation(NormCtr.Normalize(nameof(BankAccount.LoansSum), (float) BankAccount.LoansSum));
            sensor.AddObservation(NormCtr.Normalize(nameof(BankAccount.Savings), (float) BankAccount.Savings));
        }


        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            int requestCredit = actionBuffers.DiscreteActions[0];
            int setSalary = actionBuffers.DiscreteActions[1] + 1;
            int maxProduction = actionBuffers.DiscreteActions[2] + 1;
            float adaptPrice = actionBuffers.ContinuousActions[0];
            float buyResourcesFromBalance = (actionBuffers.ContinuousActions[1] + 2) / 3;
            float adaptWorkForce = actionBuffers.ContinuousActions[2];
            if (_currentActionPhase != CompanyActionPhase.Produce &&
                _currentActionPhase != CompanyActionPhase.AdaptPrice)
            {
                if (BankAccount.LoansSum < 10000000 && requestCredit > 0)
                {
                    decimal creditSum = Balance < 0 ? Balance * -1 * requestCredit : Balance * requestCredit;
                    creditSum = creditSum < 1000 ? 1000 : creditSum;
                    if (BankAccount.IsLoanAdded(creditSum, CurrentRating) == false) AddReward(-0.02f);
                }
                else
                {
                    AddReward(0.05f);
                }
            }

            if (_currentActionPhase == CompanyActionPhase.AdaptPrice) ActionAdaptPrices(adaptPrice);
            if (_currentActionPhase == CompanyActionPhase.Produce)
                ActionProduce(maxProduction);

            if (_currentActionPhase == CompanyActionPhase.BuyResources)
                ActionBuyNeededProductionResources((decimal) buyResourcesFromBalance);

            if (_currentActionPhase == CompanyActionPhase.AdaptWorkerCapacity)
                ActionAdaptProductionCapacity(adaptWorkForce, setSalary);
        }


        private void ActionProduce(int productionPercentage)
        {
            int maxUnitsProduced = PossibleProduction;
            int finalProduction = productionPercentage == 1 ? maxUnitsProduced / 2 : maxUnitsProduced;
            ProductController.AddNew(finalProduction);
            UnitsProducedInMonth += finalProduction;
            Production.AvailableProductionEnergy -= (int) (maxUnitsProduced * Production.EnergyNeededPerPiece);
            Production.AvailableProductionResources -= (int) (maxUnitsProduced * Production.ResourceNeededPerPiece);
        }

        private void ActionBuyNeededProductionResources(decimal maxSpendings)
        {
            decimal maxSpendinsTotal = Balance * maxSpendings;

            if (Production.ResourceTypeNeeded != ProductType.None)
            {
                decimal resourceSplit = Production.ResourceNeededPerPiece /
                                        (Production.ResourceNeededPerPiece + Production.EnergyNeededPerPiece);
                decimal maxResourceSpendings = resourceSplit * maxSpendinsTotal;
                var resourcesDemanded = (int) CalculateDemandForMonthlyProduction("r");
                ActionBuyResources(maxResourceSpendings, resourcesDemanded);
            }

            if (Production.EnergyTypeNeeded == ProductType.None) return;
            decimal energySplit = Production.EnergyNeededPerPiece /
                                  (Production.ResourceNeededPerPiece + Production.EnergyNeededPerPiece);
            decimal maxEnergySpendings = energySplit * maxSpendinsTotal;
            var energyDemanded = (int) CalculateDemandForMonthlyProduction("e");
            ActionBuyEnergy(maxEnergySpendings, energyDemanded);
        }

        private void ActionBuyResources(decimal maxSpendings, long resourcesDemanded)
        {
            var requestBuyResource = new ProductRequestModel(ResourceTypeNeeded,
                ProductRequestSearchType.MaxAmountWithSpendingLimit, totalSpendable: maxSpendings,
                maxAmount: resourcesDemanded);

            var productReceipt = CountryEconomyMarkets.Buy(requestBuyResource);
            Production.AvailableProductionResources += productReceipt.AmountBought;
            BankAccount.Withdraw(productReceipt.TotalPricePaid);
            LastProdCostsInMonthForRessourcesAndEnergy += productReceipt.TotalPricePaid;
            resourcesDemanded -= productReceipt.AmountBought;
            if (resourcesDemanded > 0) CountryEconomyMarkets.ReportDemand(resourcesDemanded, ResourceTypeNeeded);
        }

        private void ActionBuyEnergy(decimal maxSpendings, long energyDemanded)
        {
            var requestBuyResource = new ProductRequestModel(EnergyTypeNeeded,
                ProductRequestSearchType.MaxAmountWithSpendingLimit, totalSpendable: maxSpendings,
                maxAmount: energyDemanded);

            var productReceipt = CountryEconomyMarkets.Buy(requestBuyResource);
            Production.AvailableProductionEnergy += productReceipt.AmountBought;
            BankAccount.Withdraw(productReceipt.TotalPricePaid);
            LastProdCostsInMonthForRessourcesAndEnergy += productReceipt.TotalPricePaid;
            energyDemanded -= productReceipt.AmountBought;
            if (energyDemanded > 0) CountryEconomyMarkets.ReportDemand(energyDemanded, EnergyTypeNeeded);
        }


        private void ActionAdaptPrices(float newPriceMultiplier)
        {
            var change = (decimal) (1 + newPriceMultiplier);
            change = change < 0.5M ? 0.5M : change > 1.5M ? 1.5M : change;
            if (change <= 0) return;
            decimal newP = ProductController.Price * change;
            ProductController.UpdatePrice(newP);
        }

        private void ActionAdaptProductionCapacity(float changeProductionCapabilities, int maxSalary)
        {
            if (changeProductionCapabilities > 0)
            {
                maxSalary *= 1000;
                double additionalWorkers = Math.Ceiling(Workers.Count * changeProductionCapabilities);
                additionalWorkers = Workers.Count < 10 ? 10 : additionalWorkers;
                if (additionalWorkers <= 0) return;
                var openPositions =
                    JobPositionFactory.Create((int) additionalWorkers, maxSalary, Id, Workers, TypeProduced);
                JobMarket.AdaptSalaryForLeftopenPositions(maxSalary, Id);
                JobMarket.AddOpenJobPositions(openPositions, (int) additionalWorkers, Id);
            }
            else if (changeProductionCapabilities < 0)
            {
                var fireWorkers = (int) Math.Ceiling(Workers.Count * changeProductionCapabilities * -1);
                if (Workers.Count > 3) FireWorkers(fireWorkers);
            }
        }

        public override void MonthlyBookkeeping()
        {
            PayWorkers();
            CurrentRating = RatingController.Calculate(Balance, ProductController.ObsProfitTrend, BankAccount.LoansSum,
                ProductController.ProfitLastMonth, CurrentRating);
            LoanPayments += BankAccount.MonthlyPaymentForLoans();

            //if (Cpp > ProductController.Price) Debug.Log("");

            decimal profit = ProductController.Profit - TotalCostBeforeTaxes - UpgradeEffiencyCosts;
            ProfitTaxPaidInMonth = Government.PayProfitTax(profit);
            BankAccount.Withdraw(FixedPerProductBaseCosts + ProfitTaxPaidInMonth + LoanPayments);
            BankAccount.Deposit(ProductController.Profit);
            if (Balance > BalanceLastYear) AddReward(0.05f);
            CashflowIn = ProductController.Profit;
            CountryEconomyMarkets.ReportStats(TypeProduced, Workers.Count, (float) Balance, (float) CashflowIn,
                (float) CashflowOut,
                ProductController.ProductionThisMonth, ProductController.SalesThisMonth,
                (float) ProductController.Price, (float) Cpp);
        }
    }
}