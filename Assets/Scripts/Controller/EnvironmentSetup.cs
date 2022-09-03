using System.Collections;
using System.Collections.Generic;
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
using Settings;
using Unity.MLAgents;
using UnityEngine;

namespace Controller
{
    public class EnvironmentSetup : MonoBehaviour
    {
        //public int Month { get; set; }
        public bool isTraining;
        public int simulateYears;

        public int year = 1;

        //public int initPopulation = 1000;
        public GameObject jobMarketControllerGo;
        public GameObject environmentSettings;
        public GameObject popFactory;
        public GameObject policyWrapperGo;
        public GameObject popDataTemplate;
        public GameObject buisnessFactoryGo;

        //public GameObject propabilityController;
        private EnvironmentModel envSettings;
        private GovernmentController governmentController;
        private PopulationController populationController;
        private ICountryEconomy countryEconomyMarket;
        private List<CompanyBaseAgent> businesses;
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
            var jobMarketController = jobMarketControllerGo.GetComponent<JobMarketController>();
            var businessFactory = buisnessFactoryGo.GetComponent<BusinessFactory>();

            var govData = new GovernmentDataRepository("GER");
            statsRepository.AddGovernmentDataset(govData);
            var populationPropabilityController = new PopulationPropabilityController(popDataModel);
            var populationData = new PopulationDataRepository();
            var productMarkets = ProductionFactory.CreateMarkets(statsRepository, envSettings);
            populationModel = new PopulationModel(_population, populationData, envSettings);
            var government = new GovernmentModel(_policies.FederalPolicies, govData);
            governmentController = new GovernmentController(government, populationModel);
            populationController = new PopulationController(envSettings, populationModel, jobMarketController,
                populationFactory, populationPropabilityController);
            var bankingMarkets = new BankingMarkets();
            var bank = new BankAgent();
            bankingMarkets.AddBank(bank);
            countryEconomyMarket = new CountryEconomy(productMarkets, jobMarketController, populationModel,
                governmentController, bankingMarkets);
            var actionsFactory = new ActionsFactory(jobMarketController, countryEconomyMarket);
            populationFactory.Init(actionsFactory, jobMarketController, _policies, populationPropabilityController);
            var initialPopulation = populationFactory.CreateInitialPopulation();
            _population.AddRange(initialPopulation);

            businessFactory.Init(countryEconomyMarket, envSettings, statsRepository, governmentController);

            var fossilePolicy = new CompanyResourcePolicy(2200, 2200, 5, 100000, 20000);
            var basePolicy = new CompanyResourcePolicy(2300, 2300, 10, 100000, 5000);
            var interPolicy = new CompanyResourcePolicy(2400, 2400, 10, 100000, 100);
            var luxPolicy = new CompanyResourcePolicy(2600, 2600, 5, 100000, 100);

            var fedPolicy = new CompanyResourcePolicy(2300, 2300, 60, 10000000, 0);

            businesses = new List<CompanyBaseAgent>();
            for (int i = 0; i < 1; i++)
            {
                var fossileEnergyCompany = businessFactory.Create(ProductType.FossileEnergy, fossilePolicy, jobMarketController);
                var baseProductCompany = businessFactory.Create(ProductType.BaseProduct, basePolicy, jobMarketController);
                var intermediateProductCompany = businessFactory.Create(ProductType.IntermediateProduct, interPolicy, jobMarketController);
                var luxuryProductCompany = businessFactory.Create(ProductType.LuxuryProduct, luxPolicy, jobMarketController);
                businesses.Add(fossileEnergyCompany);
                businesses.Add(fossileEnergyCompany);
                businesses.Add(baseProductCompany);
                businesses.Add(intermediateProductCompany);
                businesses.Add(luxuryProductCompany);
            }

            var federalServices = businessFactory.Create(ProductType.FederalService, fedPolicy, jobMarketController);
            businesses.Add(federalServices);
        }


        public void Start()
        {
            academy = Academy.Instance;
            populationController.Setup();
        }


        public void Update()
        {
            if (envSettings.Year >= simulateYears)
            {
                Application.Quit();
            }
            else
            {
                StartCoroutine(UpdateBusinesses());
                Debug.Log("YEAR " + envSettings.Year + " MONTH " + envSettings.Month);
            }
        }

        private Academy academy;
        public int stepCountEpisode;
        public int stepCountTotal;
        public int episodeCount;

        IEnumerator UpdateBusinesses()
        {
            year = envSettings.Year;
            //Thread.Sleep(1000);
            //Academy.Instance.EnvironmentStep();
            stepCountEpisode = academy.StepCount;
            stepCountTotal = academy.TotalStepCount;
            episodeCount = academy.EpisodeCount;
            envSettings.Month++;
            //xAxisFull.Add(i);
            populationController.SetupMonth();


            //int day = 1;
            
            foreach (var business in businesses.OrderBy(_ => _rng.Next()))
            {
                business.MakeDecision(CompanyActionPhase.AdaptCapital);
                business.MakeDecision(CompanyActionPhase.BuyResources);
                business.MakeDecision(CompanyActionPhase.Produce);
            }

            populationController.MonthlyUpdatePopulation(countryEconomyMarket, envSettings.Month);


            foreach (var business in businesses.OrderBy(_ => _rng.Next()))
            {
                business.MakeDecision(CompanyActionPhase.AdaptWorkerCapacity);
                business.MonthlyBookkeeping();
            }

            governmentController.PayoutUnemployed();
            governmentController.PayoutRetired();

            if (envSettings.Month % 12 == 0)
            {
                populationController.YearlyUpdatePopulation();
                foreach (var business in businesses.OrderBy(_ => _rng.Next()))
                {
                    business.MakeDecision(CompanyActionPhase.AdaptWorkerCapacity);
                    business.EndYear(CompanyActionPhase.AdaptCapital);
                }
            }

            foreach (var business in businesses.OrderBy(_ => _rng.Next()))
            {
                business.UpdateStats(envSettings.Month);
                business.AddRewards();
                //business.MakeDecision(CompanyActionPhase.AdaptPrice);
            }

            countryEconomyMarket.ResetProductMarkets();
            governmentController.EndMonth();
            yield return new WaitForSeconds(0);
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