using Enums;
using Factories;
using Models;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

namespace Agents
{
    public class PublicServiceAgent : CompanyBaseAgent
    {
        private decimal _last;

        public override void MakeDecision(CompanyActionPhase phase)
        {
            if (phase == CompanyActionPhase.Produce)
            {
                ActionProduce();
            }
            else if (phase == CompanyActionPhase.BuyResources)
            {
                ActionBuyNeededProductionResources(0);
            }
            else if (phase == CompanyActionPhase.AdaptWorkerCapacity)
            {
                RequestDecision();
                Academy.Instance.EnvironmentStep();
            }
        }

        public override void EndYear(CompanyActionPhase phase)
        {
            float capitalReward = Balance < BalanceLastYear ? -0.5f : 0.5f;
            float rewardTrends = Normalize((float) ProductController.ObsProductionTrend);
            AddReward(capitalReward + rewardTrends / 2);
            BalanceLastYear = Balance;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(NormCtr.Normalize(nameof(Workers.Count), Workers.Count));
            sensor.AddObservation(NormCtr.Normalize(nameof(ProductController.TotalSupply),
                ProductController.TotalSupply));
            sensor.AddObservation(NormCtr.Normalize(nameof(ProductController.ProductionThisMonth),
                ProductController.ProductionThisMonth));
            sensor.AddObservation(NormCtr.Normalize(nameof(ProductController.ProductionLastMonth),
                ProductController.ProductionLastMonth));
            sensor.AddObservation(NormCtr.Normalize(nameof(Production.AvailableProductionEnergy),
                Production.AvailableProductionEnergy));
            sensor.AddObservation(NormCtr.Normalize(nameof(Production.AvailableProductionResources),
                Production.AvailableProductionResources));
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            float adaptWorkForce = actionBuffers.ContinuousActions[0];
            ActionAdaptProductionCapacity();
        }


        private void ActionProduce()
        {
            int maxUnitsProduced = PossibleProduction;
            int finalProduction = maxUnitsProduced;
            ProductController.AddNew(finalProduction);
            UnitsProducedInMonth += finalProduction;
            Production.AvailableProductionEnergy -= (int) (maxUnitsProduced * Production.EnergyNeededPerPiece);
            Production.AvailableProductionResources -= (int) (maxUnitsProduced * Production.ResourceNeededPerPiece);
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

        private void ActionBuyNeededProductionResources(decimal maxSpendings)
        {
            decimal maxSpendinsTotal = Balance * maxSpendings;

            if (Production.ResourceTypeNeeded != ProductType.None)
            {
                decimal resourceSplit = Production.ResourceNeededPerPiece /
                                        (Production.ResourceNeededPerPiece + Production.EnergyNeededPerPiece);
                decimal maxResourceSpendings = resourceSplit * maxSpendinsTotal;
                var resourcesDemanded = (int) CalculateDemandForMonthlyProduction("r");
                ActionBuyResources(resourcesDemanded);
            }

            if (Production.EnergyTypeNeeded != ProductType.None)
            {
                decimal energySplit = Production.EnergyNeededPerPiece /
                                      (Production.ResourceNeededPerPiece + Production.EnergyNeededPerPiece);
                decimal maxEnergySpendings = energySplit * maxSpendinsTotal;
                var energyDemanded = (int) CalculateDemandForMonthlyProduction("e");
                ActionBuyEnergy(energyDemanded);
            }
        }

        private void ActionBuyResources(long resourcesDemanded)
        {
            var requestBuyResource = new ProductRequestModel(ResourceTypeNeeded,
                ProductRequestSearchType.MaxAmount, maxAmount: resourcesDemanded);

            var productReceipt = CountryEconomyMarkets.Buy(requestBuyResource);
            Production.AvailableProductionResources += productReceipt.AmountBought;
            BankAccount.Withdraw(productReceipt.TotalPricePaid);
            LastProdCostsInMonthForRessourcesAndEnergy += productReceipt.TotalPricePaid;
            resourcesDemanded -= productReceipt.AmountBought;
            if (resourcesDemanded > 0) CountryEconomyMarkets.ReportDemand(resourcesDemanded, ResourceTypeNeeded);
        }

        private void ActionBuyEnergy(long energyDemanded)
        {
            var requestBuyResource = new ProductRequestModel(EnergyTypeNeeded,
                ProductRequestSearchType.MaxAmount, maxAmount: energyDemanded);

            var productReceipt = CountryEconomyMarkets.Buy(requestBuyResource);
            Production.AvailableProductionEnergy += productReceipt.AmountBought;
            BankAccount.Withdraw(productReceipt.TotalPricePaid);
            LastProdCostsInMonthForRessourcesAndEnergy += productReceipt.TotalPricePaid;
            energyDemanded -= productReceipt.AmountBought;
            if (energyDemanded > 0) CountryEconomyMarkets.ReportDemand(energyDemanded, EnergyTypeNeeded);
        }

        private void ActionAdaptProductionCapacity()
        {
            decimal workersNeeded = Government.RecalculateFederalWorkerDemand() / Production.UnitsPerWorker;
            if (workersNeeded > Workers.Count)
            {
                var maxSalary = (int) Government.GetMaxFederalWorkerPayment();
                decimal additionalWorkers = workersNeeded - Workers.Count;
                if (additionalWorkers <= 0) return;
                var openPositions =
                    JobPositionFactory.Create((int) additionalWorkers, maxSalary, Id, Workers, TypeProduced);
                JobMarket.AdaptSalaryForLeftopenPositions(maxSalary, Id);
                JobMarket.AddOpenJobPositions(openPositions, (int) additionalWorkers, Id);
            }
            else if (workersNeeded < Workers.Count)
            {
                int fireWorkers = Workers.Count - (int) workersNeeded;
                FireWorkers(fireWorkers);
                JobMarket.RemoveOpenJobPositions(fireWorkers, Id, false);
            }
        }

        public override void MonthlyBookkeeping()
        {
            PayWorkers();
            decimal profit = Government.GetFederalMoneyForService(FixedPerProductBaseCosts + LoanPayments);
            BankAccount.Withdraw(FixedPerProductBaseCosts);
            BankAccount.Deposit(profit);
            CashflowIn = profit;
        }
    }
}