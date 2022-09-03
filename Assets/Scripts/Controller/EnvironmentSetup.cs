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

            var fossilePolicy = new CompanyResourcePolicy(2200, 2200, 150, 100000, 20000);
            var basePolicy = new CompanyResourcePolicy(2300, 2300, 200, 100000, 5000);
            var interPolicy = new CompanyResourcePolicy(2400, 2400, 100, 100000, 1000);
            var luxPolicy = new CompanyResourcePolicy(2600, 2600, 50, 100000, 100);

            var fedPolicy = new CompanyResourcePolicy(2300, 2300, 100, 10000000, 0);

            businesses = new List<CompanyBaseAgent>();
            for (int i = 0; i < 10; i++)
            {
                var fossileEnergyCompany = businessFactory.Create(ProductType.FossileEnergy, fossilePolicy, jobMarketController);
                var baseProductCompany = businessFactory.Create(ProductType.BaseProduct, basePolicy, jobMarketController);
                var intermediateProductCompany = businessFactory.Create(ProductType.IntermediateProduct, interPolicy, jobMarketController);
                var luxuryProductCompany = businessFactory.Create(ProductType.LuxuryProduct, luxPolicy, jobMarketController);
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
            
            foreach (var business in businesses)
            {
                //business.MakeDecision(CompanyActionPhase.AdaptCapital);
                business.MakeDecision(CompanyActionPhase.BuyResources);
            }

            populationController.MonthlyUpdatePopulation(countryEconomyMarket, envSettings.Month);


            foreach (var business in businesses)
            {
                business.MakeDecision(CompanyActionPhase.Produce);
                business.MonthlyBookkeeping();
                business.MakeDecision(CompanyActionPhase.AdaptPrice);
                business.MakeDecision(CompanyActionPhase.AdaptWorkerCapacity);
            }

            //governmentController.PayoutUnemployed();
            governmentController.PayoutRetired();

            if (envSettings.Month % 12 == 0)
            {
                populationController.YearlyUpdatePopulation();
                foreach (var business in businesses)
                {
                    business.MakeDecision(CompanyActionPhase.AdaptWorkerCapacity);
                    business.EndYear(CompanyActionPhase.AdaptCapital);
                }
            }

            foreach (var business in businesses)
            {
                business.UpdateStats(envSettings.Month);
            }

            countryEconomyMarket.ResetProductMarkets();
            governmentController.EndMonth();
            yield return new WaitForFixedUpdate();
        }
    }
}