using System;
using Controller;
using Enums;
using Factories;
using Models.Market;
using Policies;
using Repositories;

namespace Models.Business
{



    public class PublicService : CompanyBase
    {


        public override void ActionBuyResources(int daysLeft)
        {
            if (ObservationTotalResourceDemandPerMonth <= 0) return;
            int maxBuyAmount = (int) ObservationTotalResourceDemandPerMonth / daysLeft;
            switch (maxBuyAmount)
            {
                case 0:
                    return;
                case < 0:
                    throw new Exception();
            }


            var requestBuyResource = new ProductRequestModel(ResourceTypeNeeded,
                ProductRequestSearchType.MaxAmount, maxAmount: maxBuyAmount);

            var productReceipt = CountryEconomyMarkets.Buy(requestBuyResource);
            Production.AvailableProductionResources += productReceipt.AmountBought;
            Balance -= productReceipt.TotalPricePaid;
            LastProdCostsInMonthForRessourcesAndEnergy += productReceipt.TotalPricePaid;

            maxBuyAmount -= productReceipt.AmountBought;
            if (maxBuyAmount > 0)
            {
                CountryEconomyMarkets.ReportDemand(maxBuyAmount, ResourceTypeNeeded);
            }
        }

        public override void ActionBuyEnergy(int daysLeft)
        {
            if (ObservationTotalEnergyDemandPerMonth <= 0) return;
            int maxBuyAmount = (int) ObservationTotalEnergyDemandPerMonth / daysLeft;

            switch (maxBuyAmount)
            {
                case 0:
                    return;
                case < 0:
                    throw new Exception();
            }


            var requestBuyEnergy = new ProductRequestModel(EnergyTypeNeeded,
                ProductRequestSearchType.MaxAmount, maxAmount: maxBuyAmount);
            var energyReceipt = CountryEconomyMarkets.Buy(requestBuyEnergy);
            Production.AvailableProductionEnergy += energyReceipt.AmountBought;
            Balance -= energyReceipt.TotalPricePaid;
            LastProdCostsInMonthForRessourcesAndEnergy += energyReceipt.TotalPricePaid;
            maxBuyAmount -= energyReceipt.AmountBought;

            if (maxBuyAmount > 0)
            {
                CountryEconomyMarkets.ReportDemand(maxBuyAmount, EnergyTypeNeeded);
            }
        }

        public override void ActionAdaptProductionCapacity()
        {
            var desiredProduction = Government.RecalculateFederalWorkerDemand();
            var workersNeeded = desiredProduction / Production.UnitsPerWorker - Workers.Count;

            if (workersNeeded > 0)
            {
                var payment = Government.GetMaxFederalWorkerPayment();
                var openPositions = JobPositionFactory.Create((int) workersNeeded, payment, Id, Workers, TypeProduced);
                JobMarket.AddOpenJobPositions(openPositions, (int) workersNeeded, Id);
            }

            if (workersNeeded < 0)
            {
                var fireWorkers = workersNeeded * -1;
                FireWorkers((int) fireWorkers);

            }
        }

        public override void MonthlyBookkeeping()
        {
            PayWorkers();
            Balance -= FixedPerProductCosts;
            CashflowIn = Government.GetFederalMoneyForService(TotalCostBeforeTaxes + UpgradeEffiencyCosts);
            Balance += ProfitAfterTaxesInMonth;
        }



        public override void ActionAdaptPrices()
        {
        }

        public override void ActionInvestInEfficiency()
        {
            if (Government.InvestInEfficientFederalServices())
            {
                if (MissingResourceDemand > 0)
                {
                    Production.MachineEfficiencyMultiplier += 0.05M;
                }
                else
                {
                    Production.WorkerEfficiencyMultiplier += 0.05M;
                }

                Balance -= 1000000;
                //Balance -= Government.PayConsumerTax(1000000);
                UpgradeEffiencyCosts += 1000000;
            }
        }

        public PublicService(ICountryEconomyMarketsModel countryEconomyMarkets, ProductController productController,
            CompanyResourcePolicy policy, GovernmentController government, CompanyDataRepository data,
            JobMarketController jobMarket) : base(countryEconomyMarkets, productController, policy, government, data,
            jobMarket)
        {
        }
    }
}