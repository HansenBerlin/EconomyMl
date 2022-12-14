using System.Collections;
using System.Collections.Generic;
using Agents;
using Controller.Agents;
using Controller.Data;
using Controller.RepositoryController;
using Enums;
using Factories;
using Interfaces;
using Models;
using Policies;
using Repositories;
using Unity.MLAgents;
using UnityEngine;
using Random = System.Random;

namespace Controller.SetupAndFlowControl
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
        public GameObject gvnAgentPrefab;
        private readonly List<PersonAgent> _population = new();
        private readonly Random _rng = StatisticalDistributionController.Rng;

        private Academy _academy;
        private BankingMarkets _bankingMarkets;
        private List<CompanyBaseAgent> _businesses;
        private BusinessRespawnController _businessRespawner;
        private ICountryEconomy _countryEconomyMarket;

        private EnvironmentModel _envSettings;
        private GovernmentAgent _governmentAgent;
        private PoliciesWrapper _policies;
        private PopulationController _populationController;
        private PopulationModel _populationModel;
        private StatisticalDataRepository _statsRepository;

        public void Awake()
        {
            _statsRepository = new StatisticalDataRepository();
            _envSettings = environmentSettings.GetComponent<EnvironmentModel>();
            _policies = policyWrapperGo.GetComponent<PoliciesWrapper>();
            var popDataModel = popDataTemplate.GetComponent<PopulationDataTemplateModel>();
            var populationFactory = popFactory.GetComponent<PopulationFactory>();
            var jobMarketController = jobMarketControllerGo.GetComponent<JobMarketController>();
            var businessFactory = buisnessFactoryGo.GetComponent<BusinessFactory>();
            var govData = new GovernmentDataRepository(_envSettings.CountryName);
            _statsRepository.AddGovernmentDataset(govData);
            var populationPropabilityController = new PopulationPropabilityController(popDataModel);
            var populationData = new PopulationDataRepository();
            var productMarkets = ProductionFactory.CreateMarkets(_statsRepository, _envSettings);
            _populationModel = new PopulationModel(_population, populationData, _envSettings);
            var government = new GovernmentModel(_policies, govData);
            var gvCtr = Instantiate(gvnAgentPrefab);
            _governmentAgent = gvCtr.GetComponent<GovernmentAgent>();
            _governmentAgent.Init(government, _populationModel, new NormalizationController());
            _populationController = new PopulationController(_envSettings, _populationModel, jobMarketController,
                populationFactory, populationPropabilityController);
            _bankingMarkets = new BankingMarkets();
            var bankFactory = bankingFactoryGo.GetComponent<BankFactory>();
            var cbAent = new CentralBankAgent();
            bankFactory.Init(_policies.CentralBankPolicies, cbAent);
            _bankingMarkets.AddBank(bankFactory.Create());
            _bankingMarkets.AddBank(bankFactory.Create());
            _bankingMarkets.AddBank(bankFactory.Create());
            _countryEconomyMarket = new CountryEconomy(productMarkets, _governmentAgent, _bankingMarkets);
            var actionsFactory = new ActionsFactory(jobMarketController, _countryEconomyMarket);
            populationFactory.Init(actionsFactory, jobMarketController, _policies, populationPropabilityController,
                _countryEconomyMarket);
            var initialPopulation = populationFactory.CreateInitialPopulation();
            _population.AddRange(initialPopulation);
            businessFactory.Init(_countryEconomyMarket, _envSettings, _statsRepository, _governmentAgent);
            _businessRespawner = new BusinessRespawnController(businessFactory, jobMarketController);

            _businesses = new List<CompanyBaseAgent>();
            for (var i = 0; i < 10; i++)
            {
                var fossileEnergyCompany = businessFactory.Create(ProductType.FossileEnergy, jobMarketController);
                var baseProductCompany = businessFactory.Create(ProductType.BaseProduct, jobMarketController);
                var intermediateProductCompany =
                    businessFactory.Create(ProductType.IntermediateProduct, jobMarketController);
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

        private IEnumerator UpdateBusinesses()
        {
            year = _envSettings.Year;
            _envSettings.Month++;
            _populationController.SetupMonth();

            foreach (var business in _businesses)
                business.MakeDecision(CompanyActionPhase.BuyResources);

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

            _governmentAgent.PayoutUnemployed();
            _governmentAgent.PayoutRetired();

            if (_envSettings.Month % 12 == 0)
            {
                _bankingMarkets.AddRewards();
                _populationController.YearlyUpdatePopulation();
                foreach (var business in _businesses)
                {
                    business.MakeDecision(CompanyActionPhase.AdaptWorkerCapacity);
                    business.EndYear(CompanyActionPhase.AdaptCapital);
                    _governmentAgent.EndYear();
                }
            }

            _governmentAgent.MakeDecision();
            _bankingMarkets.PayOutInterestForSavings();
            _bankingMarkets.Decide();

            foreach (var business in _businesses) business.UpdateStats(_envSettings.Month);

            _countryEconomyMarket.ResetProductMarkets();
            yield return new WaitForFixedUpdate();
        }
    }
}