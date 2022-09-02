using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Enums;
using Factories;
using Models.Agents;
using Models.Business;
using Models.Market;
using Models.Meta;
using Models.Population;
using Policies;
using Repositories;
using ScottPlot;
using Settings;
using UnityEngine;
using UnityEngine.Serialization;

namespace Controller
{
    public class EnvironmentSetup : MonoBehaviour
    {
        //public int Month { get; set; }
        public bool isTraining;
        public int simulateYears = 1;
        public int initPopulation = 1000;
        public GameObject jobMarketController;
        public GameObject environmentSettings;
        public GameObject popFactory;
        public GameObject policyWrapperGo;
        public GameObject popDataTemplate;
        public GameObject propabilityController;
        private EnvironmentModel envSettings;
        private GovernmentController governmentController;
        private PopulationController populationController;
        private ICountryEconomyMarketsModel countryEconomyMarket;
        private List<ICompanyModel> businesses;
        private PoliciesWrapper _policies;
        private readonly List<PersonAgent> _population = new();
        private readonly System.Random _rng = StatisticalDistributionController.Rng;
        private PopulationModel populationModel;
        private StatisticalDataRepository statsRepository;

        public void Awake()
        {
            statsRepository = new StatisticalDataRepository();
            envSettings = environmentSettings.GetComponent<EnvironmentModel>();
            _policies = policyWrapperGo.GetComponent<PoliciesWrapper>();
            var popDataModel = popDataTemplate.GetComponent<PopulationDataTemplateModel>();
            var populationFactory = popFactory.GetComponent<PopulationFactory>();
            
            var govData = new GovernmentDataRepository("GER");
            statsRepository.AddGovernmentDataset(govData);
            var populationPropabilityController = propabilityController.GetComponent<PopulationPropabilityController>();
            var populationData = new PopulationDataRepository();
            var workerController = jobMarketController.GetComponent<JobMarketController>();
            var productMarkets = ProductionFactory.CreateMarkets(statsRepository, envSettings);
            populationModel = new PopulationModel(_population, populationData, envSettings);
            var government = new GovernmentModel(_policies.FederalPolicies, govData);
            governmentController = new GovernmentController(government, populationModel);
            populationController = new PopulationController(envSettings, populationModel, workerController, populationFactory, populationPropabilityController);
            countryEconomyMarket = new CountryEconomyMarketsModel(productMarkets, workerController, populationModel, governmentController);
            var actionsFactory = new ActionsFactory(workerController, countryEconomyMarket);
            var initialPopulation = populationFactory.CreateInitialPopulation(actionsFactory);
            _population.AddRange(initialPopulation);

            var businessFactory =
                new BusinessFactory(countryEconomyMarket, envSettings, statsRepository, governmentController);

            var fossilePolicy = new CompanyResourcePolicy(2200, 2200, 50, 10000000, 200000);
            var basePolicy = new CompanyResourcePolicy(2300, 2300, 100, 10000000, 50000);
            var interPolicy = new CompanyResourcePolicy(2400, 2400, 180, 10000000, 1000);
            var luxPolicy = new CompanyResourcePolicy(2600, 2600, 40, 10000000, 100);
            var fedPolicy = new CompanyResourcePolicy(2300, 2300, 60, 10000000, 0);

            var fossileEnergyCompany =
                businessFactory.Create(ProductType.FossileEnergy, fossilePolicy, workerController);
            var baseProductCompany = businessFactory.Create(ProductType.BaseProduct, basePolicy, workerController);
            var intermediateProductCompany =
                businessFactory.Create(ProductType.IntermediateProduct, interPolicy, workerController);
            var luxuryProductCompany = businessFactory.Create(ProductType.LuxuryProduct, luxPolicy, workerController);
            var federalServices = businessFactory.Create(ProductType.FederalService, fedPolicy, workerController);
            
            businesses = new()
            {
                fossileEnergyCompany,
                baseProductCompany,
                intermediateProductCompany,
                luxuryProductCompany,
                federalServices
            };
        }

        public void Update()
        {
            if (envSettings.Month > 120)
            {
                //var charts = new ChartsModel(statsRepository, populationModel);
                //charts.CreateAll();
            }

            UpdateBusinesses();
        }

        private void UpdateBusinesses()
        {
            envSettings.Month++;
            //xAxisFull.Add(i);

            //int day = 1;
            foreach (var business in businesses.Where(b => b.TypeProduced == ProductType.FossileEnergy))
            {
                business.ActionProduce();
            }

            envSettings.Day = 1;
            while (envSettings.Day <= 30)
            {
                foreach (var business in businesses.OrderBy(_ => _rng.Next()))
                {
                    if (business.TypeProduced == ProductType.FossileEnergy)
                    {
                        continue;
                    }

                    business.ActionBuyEnergy(31 - envSettings.Day);
                    //business.DailyBookkeeping();
                }

                envSettings.Day++;
            }

            foreach (var business in businesses
                         .Where(business => business.TypeProduced
                             is not (ProductType.FossileEnergy or ProductType.LuxuryProduct
                             or ProductType.FederalService)))
            {
                business.ActionProduce();
            }

            envSettings.Day = 1;
            while (envSettings.Day <= 30)
            {
                foreach (var business in businesses.OrderBy(_ => _rng.Next()))
                {
                    if (business.TypeProduced != ProductType.LuxuryProduct &&
                        business.TypeProduced != ProductType.FederalService)
                    {
                        continue;
                    }

                    business.ActionBuyResources(31 - envSettings.Day);
                    //business.DailyBookkeeping();
                }

                envSettings.Day++;
            }

            foreach (var business in businesses.Where(business =>
                         business.TypeProduced is ProductType.LuxuryProduct or ProductType.FederalService))
            {
                business.ActionProduce();
            }

            populationController.MonthlyUpdatePopulation(countryEconomyMarket, envSettings.Month);


            foreach (var business in businesses.OrderBy(_ => _rng.Next()))
            {
                //business.ActionInvestInEfficiency();
                business.MonthlyBookkeeping();
                //business.UpdateWorkforce();
            }

            governmentController.PayoutUnemployed();
            governmentController.PayoutRetired();

            if (envSettings.Month % 12 == 0)
            {
                populationController.YearlyUpdatePopulation();
            }

            foreach (var business in businesses.OrderBy(_ => _rng.Next()))
            {
                business.Reset(envSettings.Month);
            }

            countryEconomyMarket.ResetProductMarkets();
            governmentController.EndMonth();
        }

        

        /*private void CreateCharts()
        {
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
        }*/
    }
}