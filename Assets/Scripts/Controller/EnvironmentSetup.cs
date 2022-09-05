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
        public GameObject bankingFactoryGo;

        private EnvironmentModel _envSettings;
        private GovernmentController _governmentController;
        private PopulationController _populationController;
        private ICountryEconomy _countryEconomyMarket;
        private List<CompanyBaseAgent> _businesses;
        private PoliciesWrapper _policies;
        private readonly List<PersonAgent> _population = new();
        private readonly System.Random _rng = StatisticalDistributionController.Rng;
        private PopulationModel _populationModel;
        private StatisticalDataRepository _statsRepository;
        private BusinessRespawnController _businessRespawner;

        public void Awake()
        {
            _statsRepository = new StatisticalDataRepository();
            _envSettings = environmentSettings.GetComponent<EnvironmentModel>();
            _policies = policyWrapperGo.GetComponent<PoliciesWrapper>();
            var popDataModel = popDataTemplate.GetComponent<PopulationDataTemplateModel>();
            var populationFactory = popFactory.GetComponent<PopulationFactory>();
            var jobMarketController = jobMarketControllerGo.GetComponent<JobMarketController>();
            var businessFactory = buisnessFactoryGo.GetComponent<BusinessFactory>();

            var govData = new GovernmentDataRepository("GER");
            _statsRepository.AddGovernmentDataset(govData);
            var populationPropabilityController = new PopulationPropabilityController(popDataModel);
            var populationData = new PopulationDataRepository();
            var productMarkets = ProductionFactory.CreateMarkets(_statsRepository, _envSettings);
            _populationModel = new PopulationModel(_population, populationData, _envSettings);
            var government = new GovernmentModel(_policies.FederalPolicies, govData);
            _governmentController = new GovernmentController(government, _populationModel);
            _populationController = new PopulationController(_envSettings, _populationModel, jobMarketController,
                populationFactory, populationPropabilityController);
            var bankingMarkets = new BankingMarkets();
            var bankFactory = bankingFactoryGo.GetComponent<BankFactory>();
            var cbAent = new CentralBankAgent();
            bankFactory.Init(_policies.CentralBankPolicies, cbAent);
            bankingMarkets.AddBank(bankFactory.Create());
            bankingMarkets.AddBank(bankFactory.Create());
            bankingMarkets.AddBank(bankFactory.Create());
            
            _countryEconomyMarket = new CountryEconomy(productMarkets, jobMarketController, _populationModel,
                _governmentController, bankingMarkets);
            var actionsFactory = new ActionsFactory(jobMarketController, _countryEconomyMarket);
            populationFactory.Init(actionsFactory, jobMarketController, _policies, populationPropabilityController, _countryEconomyMarket);
            var initialPopulation = populationFactory.CreateInitialPopulation();
            _population.AddRange(initialPopulation);

            businessFactory.Init(_countryEconomyMarket, _envSettings, _statsRepository, _governmentController);
            _businessRespawner = new BusinessRespawnController(businessFactory, jobMarketController);

            

            _businesses = new List<CompanyBaseAgent>();
            for (int i = 0; i < 10; i++)
            {
                var fossileEnergyCompany = businessFactory.Create(ProductType.FossileEnergy, jobMarketController);
                var baseProductCompany = businessFactory.Create(ProductType.BaseProduct, jobMarketController);
                var intermediateProductCompany = businessFactory.Create(ProductType.IntermediateProduct, jobMarketController);
                var luxuryProductCompany = businessFactory.Create(ProductType.LuxuryProduct, jobMarketController);
                _businesses.Add(fossileEnergyCompany);
                _businesses.Add(baseProductCompany);
                _businesses.Add(intermediateProductCompany);
                _businesses.Add(luxuryProductCompany);
            }

            var federalServices = businessFactory.Create(ProductType.FederalService, jobMarketController);
            _businesses.Add(federalServices);
        }


        public void Start()
        {
            _academy = Academy.Instance;
            _populationController.Setup();
        }


        public void Update()
        {
            if (_envSettings.Year >= simulateYears)
            {
                Application.Quit();
            }
            else
            {
                StartCoroutine(UpdateBusinesses());
                Debug.Log("YEAR " + _envSettings.Year + " MONTH " + _envSettings.Month);
            }
        }

        private Academy _academy;
        public int stepCountEpisode;
        public int stepCountTotal;
        public int episodeCount;

        IEnumerator UpdateBusinesses()
        {
            year = _envSettings.Year;
            //Thread.Sleep(1000);
            //Academy.Instance.EnvironmentStep();
            stepCountEpisode = _academy.StepCount;
            stepCountTotal = _academy.TotalStepCount;
            episodeCount = _academy.EpisodeCount;
            _envSettings.Month++;
            //xAxisFull.Add(i);
            _populationController.SetupMonth();


            //int day = 1;
            
            foreach (var business in _businesses)
            {
                //business.MakeDecision(CompanyActionPhase.AdaptCapital);
                business.MakeDecision(CompanyActionPhase.BuyResources);
            }

            _populationController.MonthlyUpdatePopulation(_countryEconomyMarket, _envSettings.Month);


            foreach (var business in _businesses)
            {
                business.MakeDecision(CompanyActionPhase.Produce);
                business.MonthlyBookkeeping();
            }

            for (int i = _businesses.Count - 1; i >= 0; i--)
            {
                var business = _businesses[i];
                if (business.IsRemoved())
                {
                    var newBusiness = _businessRespawner.Respawn(business);
                    _businesses.Add(newBusiness);
                    _businesses.Remove(business);
                    i--;
                }
                else
                {
                    business.MakeDecision(CompanyActionPhase.AdaptPrice);
                    business.MakeDecision(CompanyActionPhase.AdaptWorkerCapacity);
                }

            }
            

            _governmentController.PayoutUnemployed();
            _governmentController.PayoutRetired();

            if (_envSettings.Month % 12 == 0)
            {
                _populationController.YearlyUpdatePopulation();
                foreach (var business in _businesses)
                {
                    business.MakeDecision(CompanyActionPhase.AdaptWorkerCapacity);
                    business.EndYear(CompanyActionPhase.AdaptCapital);
                }
            }

            foreach (var business in _businesses)
            {
                business.UpdateStats(_envSettings.Month);
            }

            _countryEconomyMarket.ResetProductMarkets();
            _governmentController.EndMonth();
            yield return new WaitForFixedUpdate();
        }
    }
}