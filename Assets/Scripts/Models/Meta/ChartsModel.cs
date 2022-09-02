using System.Collections.Generic;
using Enums;
using Factories;
using Models.Population;
using Repositories;
using ScottPlot;

namespace Models.Meta
{
    public class ChartsModel
    {
        private readonly StatisticalDataRepository statsRepository;
        private readonly PopulationModel populationModel;

        public ChartsModel(StatisticalDataRepository statsRepository, PopulationModel populationModel)
        {
            this.statsRepository = statsRepository;
            this.populationModel = populationModel;
        }

        public void CreateAll()
        {
            ChartFactory.CreateScatter(
                new[]
                {
                    populationModel.TotalPopulationTrend, populationModel.TotalUnderAgeChildrenTrend,
                    populationModel.TotalWorkerAgeTrend, populationModel.TotalRetiredAgeTrend
                },
                ChartType.Population, "population-trend-per-age-cohort",
                new[] {"total", "child age", "worker age", "retired age"}
            );


            ChartFactory.CreateScatter(
                new[] {populationModel.AverageAge},
                ChartType.Population, "PopAge",
                new[] {"avg age"},
                0, 0);

            ChartFactory.CreateScatter(
                new[] {populationModel.BornPercentageStat, populationModel.DiedPercentageStat},
                ChartType.Population, "PopPercentTrend",
                new[] {"born", "died"},
                0, 0);

            ChartFactory.CreateScatter(
                new[] {populationModel.ChildrenStatAdded},
                ChartType.Population, "ChildrenAdded");


            ChartFactory.CreateScatter(
                new[]
                {
                    populationModel.AllTimePopulationTrendStat, populationModel.AllTimeChildrenStat,
                    populationModel.AllTimeDeathStat, populationModel.TotalPopulationTrend
                },
                ChartType.Population, "PopulationTrend",
                new[] {"trned", "born", "died", "total"});


            ChartFactory.CreateScatter(
                new[]
                {
                    populationModel.EmploymentRate
                },
                ChartType.Economy, "EmploymentRate", yLimitMin: 0, yLimitMax: 100
            );


            ChartFactory.CreateScatter(
                new[]
                {
                    populationModel.AverageIncomeEmployed, populationModel.AverageIncomeUnemployed,
                    populationModel.AverageIncomeWorkerAge, populationModel.AverageIncomeAdultAge,
                    populationModel.AverageIncomeRetiredAge
                },
                ChartType.Economy, "average-income",
                new[]
                {
                    "employed", "unemployed", "workerage", "all adults", "retired"
                });

            List<Plot> pricePlots = new();
            List<Plot> productionPlots = new();
            foreach ((string? key, var values) in statsRepository.ProductData)
            {
                var prodPlot = ChartFactory.CreateScatterPlot(
                    new[]
                    {
                        values.ProductionTrend, values.SalesTrend, values.SupplyTrend, values.ProfitTrend,
                        values.PriceTrend
                    },
                    ChartType.Businesses, "production-trends-" + key,
                    new[]
                    {
                        "production", "sales", "supply", "profit", "price"
                    }, isLogScale: true);
                productionPlots.Add(prodPlot);

                var plot = ChartFactory.CreateScatterPlot(
                    new[]
                    {
                        values.PriceTotal, values.CppTotal
                    },
                    ChartType.Businesses, key,
                    new[]
                    {
                        "price", "cpp"
                    });
                pricePlots.Add(plot);
            }

            ChartFactory.Multiplot(pricePlots, "product-price-total", ChartType.Businesses);
            ChartFactory.Multiplot(productionPlots, "product-trends", ChartType.Businesses);

            List<Plot> governmentFinancials = new();
            foreach ((string? key, var values) in statsRepository.GovernmentData)
            {
                var plot = ChartFactory.CreateScatterPlot(
                    new[]
                    {
                        values.ConsumerTaxes, values.IncomeTaxes, values.ProfitTaxes, values.TotalIncome
                    },
                    ChartType.Government, "income-" + key,
                    new[]
                    {
                        "consumer tax", "income tax", "profit tax", "total"
                    });
                var plot2 = ChartFactory.CreateScatterPlot(
                    new[]
                    {
                        values.UnemployedCosts, values.RetiredCosts, values.PublicServiceCosts, values.TotalExpenses
                    },
                    ChartType.Government, "expenses-" + key,
                    new[]
                    {
                        "unemployed", "retired", "public services", "total"
                    });
                governmentFinancials.Add(plot);
                governmentFinancials.Add(plot2);
            }

            ChartFactory.Multiplot(governmentFinancials, "gov-financials", ChartType.Government);


            var balanceData = statsRepository.GetBusinessBalanceComparison(false);
            ChartFactory.CreateScatter(balanceData.Item1, ChartType.Businesses, "balance-comparison",
                balanceData.Item2);


            List<Plot> businessPlotsOne = new();
            List<Plot> businessPlotsTwo = new();
            foreach ((string? key, var values) in statsRepository.CompanyMarketData)
            {
                var plot1 = ChartFactory.CreateScatterPlot(
                    new[]
                    {
                        values.MoneyInStat, values.MoneyOutStat
                    },
                    ChartType.Businesses, key,
                    new[]
                    {
                        "cash in", "cash out"
                    });

                var plot2 = ChartFactory.CreateScatterPlot(
                    new[]
                    {
                        values.WorkersStat
                    },
                    ChartType.Businesses, key,
                    new[]
                    {
                        "total"
                    });
                businessPlotsOne.Add(plot1);
                businessPlotsTwo.Add(plot2);
            }

            ChartFactory.Multiplot(businessPlotsOne, "cashflow-data", ChartType.Businesses);
            ChartFactory.Multiplot(businessPlotsTwo, "worker-data", ChartType.Businesses);

            List<Plot> productMarketPlots = new();

            foreach ((string? key, var values) in statsRepository.ProductMarketData)
            {
                if (!key.Contains(ProductType.BaseProduct.ToString()) &&
                    !key.Contains(ProductType.FossileEnergy.ToString()) &&
                    !key.Contains(ProductType.IntermediateProduct.ToString()) &&
                    !key.Contains(ProductType.LuxuryProduct.ToString()) &&
                    !key.Contains(ProductType.FederalService.ToString()))
                {
                    continue;
                }

                var plot = ChartFactory.CreateScatterPlot(
                    new[]
                    {
                        values.Production, values.Demand, values.Sales
                    },
                    ChartType.Businesses, key,
                    new[]
                    {
                        "production", "demand", "sales"
                    });
                productMarketPlots.Add(plot);
            }

            ChartFactory.Multiplot(productMarketPlots, "product-market-data", ChartType.Businesses);
        }
    }
}