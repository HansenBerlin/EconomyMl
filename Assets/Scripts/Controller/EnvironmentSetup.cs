using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Enums;
using Factories;
using Models.Business;
using Models.Market;
using Models.Meta;
using Models.Population;
using Policies;
using Repositories;
using ScottPlot;
using Settings;
using UnityEngine;

namespace Controller
{
    public class EnvironmentSetup : MonoBehaviour
    {
        public List<GameObject> population = new();
        public int Month { get; set; }
        private EnvironmentModel envSettings;
        public GameObject environment;
        public GovernmentController governmentController;
        public PopulationController populationController;
        public ICountryEconomyMarketsModel countryEconomyMarket;
        public List<ICompanyModel> businesses;
        private System.Random rng = StatisticalDistributionController.Rng;
        private bool isInitDone = false;

        public void Start()
        {
            Setup();
        }

        public void Update()
        {
            if (Month > 120)
            {
                Setup();
                Month = 0;
                return;
            }
            UpdateBusinesses();
        }

        public void UpdateBusinesses()
        {
            envSettings.Month++;
            Month = envSettings.Month;
            //xAxisFull.Add(i);

                //int day = 1;
                foreach (var business in businesses.Where(b => b.TypeProduced == ProductType.FossileEnergy))
                {
                    business.ActionProduce();
                }

                envSettings.Day = 1;
                while (envSettings.Day <= 30)
                {
                    foreach (var business in businesses.OrderBy(_ => rng.Next()))
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
                    foreach (var business in businesses.OrderBy(_ => rng.Next()))
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
                
                populationController.MonthlyUpdatePopulation(countryEconomyMarket, Month);



                foreach (var business in businesses.OrderBy(_ => rng.Next()))
                {
                    //business.ActionInvestInEfficiency();
                    business.MonthlyBookkeeping();
                    //business.UpdateWorkforce();

                }

                governmentController.PayoutUnemployed();
                governmentController.PayoutRetired();

                foreach (var business in businesses.OrderBy(_ => rng.Next()))
                {
                    business.Reset(envSettings.Month);
                }

                countryEconomyMarket.ResetProductMarkets();
                governmentController.EndMonth();
        }

        public void Setup()
        {
            int simulateYears = 1;
            int initPopulation = 1000;
            int iterations = simulateYears * 12;


            var ageBoundaryPolicies = new AgeBoundaryPolicy(18, 67);
            var educationPolicies = new EducationBoundaryPolicy(6, 10, 12);
            var workerPolicies = new WorkerPolicy(0, 0.5M, 2000, 1000);
            var federalPolicies = new FederalServicesPolicy(10, 2300, 0.35M, 0.4M, 0.2M);
            var policyWrapper =
                new PoliciesWrapper(ageBoundaryPolicies, educationPolicies, workerPolicies, federalPolicies);
            var statsRepository = new StatisticalDataRepository();
            //GameObject go = new GameObject();
            envSettings = environment.GetComponent<EnvironmentModel>();
            //envSettings.Init("GER", 0);
            var govData = new GovernmentDataRepository("GER");
            statsRepository.AddGovernmentDataset(govData);

            var populationDistributionController =
                new PopulationDataTemplateModel(DemographyType.IndustrialCountrySociety, initPopulation);
            var populationPropabilityController = new PopulationPropabilityController(populationDistributionController);
            var populationData = new PopulationDataRepository();
            var dataController = new DataController(populationData, simulateYears);
            var workerController = new JobMarketController();
            var population = new List<IPersonBase>();
            var productMarkets = ProductionFactory.CreateMarkets(statsRepository, envSettings);
            var populationModel = new PopulationModel(population, populationData, envSettings);
            var government = new GovernmentModel(federalPolicies, govData);
            governmentController = new GovernmentController(government, populationModel);
            var newRef = new PopulationFactory(policyWrapper, populationPropabilityController, workerController);
            populationController = new PopulationController(envSettings, populationModel, workerController, newRef,
                populationPropabilityController);
            countryEconomyMarket =
                new CountryEconomyMarketsModel(productMarkets, workerController, populationModel, governmentController);

            var actionsFactory = new ActionsFactory(workerController, populationController, countryEconomyMarket);
            newRef.SetupActions(actionsFactory);
            var initialPopulation = newRef.CreateInitialPopulation();
            population.AddRange(initialPopulation);
            
            var businessFactory =
                new BusinessFactory(countryEconomyMarket, envSettings, statsRepository, governmentController);

            var fossilePolicy = new CompanyResourcePolicy(2200, 2200, 20, 10000000);
            var basePolicy = new CompanyResourcePolicy(2300, 2300, 40, 10000000);
            var interPolicy = new CompanyResourcePolicy(2400, 2400, 75, 10000000);
            var luxPolicy = new CompanyResourcePolicy(2600, 2600, 15, 10000000);
            var fedPolicy = new CompanyResourcePolicy(2300, 2300, 25, 10000000);

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

            isInitDone = true;






        }
    }
}


    