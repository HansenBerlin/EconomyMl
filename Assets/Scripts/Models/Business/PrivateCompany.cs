using System;
using System.Linq;
using Controller;
using Enums;
using Factories;
using Models.Market;
using Policies;
using Repositories;

namespace Models.Business
{



    public class PrivateCompany : CompanyBase
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

        public override void ActionBuyResources(int daysLeft)
        {
            if (ObservationTotalResourceDemandPerMonth <= 0) return;
            var profitLeft = ProductController.Price - ObservationFixSpendingsPerProduct; // von 5 noch 4
            var maxPrice = profitLeft * 0.9M / Production.ResourceNeededPerPiece; // von 4: 3,50
            int maxBuyAmount = (int) ObservationTotalResourceDemandPerMonth / daysLeft;
            switch (maxBuyAmount)
            {
                case 0:
                    return;
                case < 0:
                    throw new Exception();
            }


            var requestBuyResource = new ProductRequestModel(ResourceTypeNeeded,
                ProductRequestSearchType.MaxAmountForMaxPrice, maxAmount: maxBuyAmount, maxPrice: maxPrice);

            var productReceipt = CountryEconomyMarkets.Buy(requestBuyResource);
            Production.AvailableProductionResources += productReceipt.AmountBought;
            Balance -= productReceipt.TotalPricePaid;
            LastProdCostsInMonthForRessourcesAndEnergy += productReceipt.TotalPricePaid;
            maxBuyAmount -= productReceipt.AmountBought;

            if (maxBuyAmount > 0)
            {
                decimal maxSpendable = maxBuyAmount * ObservationFixSpendingsPerProduct * 1.2M;
                requestBuyResource = new ProductRequestModel(ResourceTypeNeeded,
                    ProductRequestSearchType.MaxAmountWithSpendingLimit, maxAmount: maxBuyAmount,
                    totalSpendable: maxSpendable);
                var productReceiptTwo = CountryEconomyMarkets.Buy(requestBuyResource);
                Production.AvailableProductionResources += productReceiptTwo.AmountBought;
                Balance -= productReceiptTwo.TotalPricePaid;
                LastProdCostsInMonthForRessourcesAndEnergy += productReceiptTwo.TotalPricePaid;
                maxBuyAmount -= productReceiptTwo.AmountBought;

            }

            CountryEconomyMarkets.ReportDemand(maxBuyAmount, ResourceTypeNeeded);
            MissingResourceDemand += maxBuyAmount;
        }

        public override void ActionBuyEnergy(int daysLeft)
        {
            if (ObservationTotalEnergyDemandPerMonth <= 0) return;
            var profitLeft = ProductController.Price - ObservationFixSpendingsPerProduct;
            var maxPrice = profitLeft * 0.9M / Production.EnergyNeededPerPiece;
            int maxBuyAmount = (int) ObservationTotalEnergyDemandPerMonth / daysLeft;

            switch (maxBuyAmount)
            {
                case 0:
                    return;
                case < 0:
                    throw new Exception();
            }


            var requestBuyEnergy = new ProductRequestModel(EnergyTypeNeeded,
                ProductRequestSearchType.MaxAmountForMaxPrice, maxAmount: maxBuyAmount, maxPrice: maxPrice);
            var energyReceipt = CountryEconomyMarkets.Buy(requestBuyEnergy);
            Production.AvailableProductionEnergy += energyReceipt.AmountBought;
            Balance -= energyReceipt.TotalPricePaid;
            LastProdCostsInMonthForRessourcesAndEnergy += energyReceipt.TotalPricePaid;
            maxBuyAmount -= energyReceipt.AmountBought;

            if (maxBuyAmount > 0)
            {

                decimal maxSpendable = maxBuyAmount * ObservationFixSpendingsPerProduct * 1.2M;
                if (maxSpendable > 32000000)
                    Console.WriteLine();
                requestBuyEnergy = new ProductRequestModel(EnergyTypeNeeded,
                    ProductRequestSearchType.MaxAmountWithSpendingLimit, maxAmount: maxBuyAmount,
                    totalSpendable: maxSpendable);
                var energyReceiptTwo = CountryEconomyMarkets.Buy(requestBuyEnergy);

                Production.AvailableProductionEnergy += energyReceiptTwo.AmountBought;
                Balance -= energyReceiptTwo.TotalPricePaid;
                LastProdCostsInMonthForRessourcesAndEnergy += energyReceiptTwo.TotalPricePaid;
                maxBuyAmount -= energyReceiptTwo.AmountBought;
            }

            CountryEconomyMarkets.ReportDemand(maxBuyAmount, EnergyTypeNeeded);
            MissingResourceDemand += maxBuyAmount;
        }

        public override void ActionAdaptPrices()
        {
            var oldP = ProductController.Price;
            var newP = ProductController.Price;
            if (Cpp * 1.1M > ProductController.Price)
            {
                newP = Cpp * 1.1M;
            }
            else if (ProductController.Price > Cpp * 1.2M)
            {
                newP = Cpp * 1.2M;
            }

            if (newP > oldP * 1.5M)
            {
                newP = oldP * 1.5M;
            }

            ProductController.UpdatePrice(newP);
        }

        public override void ActionInvestInEfficiency()
        {
            decimal profit = ProductController.Profit - TotalCostBeforeTaxes;

            if (profit > 100000 && Balance > 10000000)
            {
                if (MissingResourceDemand > 0)
                {
                    Production.MachineEfficiencyMultiplier += 0.01M;
                }
                else
                {
                    Production.WorkerEfficiencyMultiplier += 0.01M;
                }

                Balance -= 1000000;
                Balance -= Government.PayConsumerTax(1000000);
                UpgradeEffiencyCosts += 1000000;
            }
        }

        private decimal AverageSalary()
        {
            var sum = Workers.Sum(x => x.MonthlyIncome);
            var count = Workers.Count;
            return sum / count;
        }

        public override void ActionAdaptProductionCapacity()
        {
            decimal supply = ProductController.QuarterlySupplyAverage;
            decimal production = ProductController.QuarterlyProductionAverage;
            decimal sales = ProductController.QuarterlySalesAverage;

            decimal trend = WorkerAdaptionController.CalculateWorkerModifier(supply, production, sales);

            decimal reductionRateCapacityUsed =
                WorkerAdaptionController.CalculateCapacityModifier(CapacityUsed);


            if (trend > 1 && Balance > 0 && reductionRateCapacityUsed > 0.9M)
            {
                var additionalWorkers = Workers.Count * trend - Workers.Count;
                if (additionalWorkers <= 0) return;
                decimal salary = CalculateMaxWorkerSalary();
                var openPositions =
                    JobPositionFactory.Create((int) additionalWorkers, salary, Id, Workers, TypeProduced);
                JobMarket.AdaptSalaryForLeftopenPositions(salary, Id);
                JobMarket.AddOpenJobPositions(openPositions, (int) additionalWorkers, Id);
            }

            if ((trend < 1 && supply > sales * 3) || reductionRateCapacityUsed < 0.9M)
            {
                var overallTrend = trend > reductionRateCapacityUsed ? reductionRateCapacityUsed : trend;
                var fireWorkers = Workers.Count - Workers.Count * overallTrend;
                fireWorkers = fireWorkers > Workers.Count * 0.5M ? Workers.Count * 0.5M : fireWorkers;
                FireWorkers((int) fireWorkers);
            }
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


        public PrivateCompany(ICountryEconomyMarketsModel countryEconomyMarkets, ProductController productController,
            CompanyResourcePolicy policy, GovernmentController government, CompanyDataRepository data,
            JobMarketController jobMarket) : base(countryEconomyMarkets, productController, policy, government, data,
            jobMarket)
        {
        }
    }
}