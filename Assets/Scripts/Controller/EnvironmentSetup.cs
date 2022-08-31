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
    public class EnvironmentSrtup : MonoBehaviour
    {
        public void Start()
        {
            Setup();
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
            var envSettings = new EnvironmentModel("GER", 0);
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
            var governmentController = new GovernmentController(government, populationModel);
            var newRef = new PopulationFactory(policyWrapper, populationPropabilityController, workerController);
            var populationController = new PopulationController(envSettings, populationModel, workerController, newRef,
                populationPropabilityController);
            ICountryEconomyMarketsModel countryEconomyMarket =
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

            List<ICompanyModel> businesses = new()
            {
                fossileEnergyCompany,
                baseProductCompany,
                intermediateProductCompany,
                luxuryProductCompany,
                federalServices
            };


            List<double> xAxis = new();
            List<double> xAxisFull = new();
            var rng = StatisticalDistributionController.Rng;
            

            //populationModel.UpdateData();


            for (int i = 1; i <= iterations; i++)
            {
                xAxisFull.Add(i);
                ConsoleColorPrinter.Write($"\n Round {i} ----------------------------", ConsoleColor.White);
                envSettings.Month = i;

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

                envSettings.Day = 1;
                while (envSettings.Day <= 30)
                {
                    populationController.DailyUpdatePopulation(countryEconomyMarket, envSettings.Day);
                    envSettings.Day++;
                }


                populationController.MonthlyUpdatePopulation(countryEconomyMarket);



                foreach (var business in businesses.OrderBy(_ => rng.Next()))
                {
                    //business.ActionInvestInEfficiency();
                    business.MonthlyBookkeeping();
                    //business.UpdateWorkforce();

                }

                governmentController.PayoutUnemployed();
                governmentController.PayoutRetired();







                if (i % 12 == 0)
                {
                    xAxis.Add(i);
                    populationController.YearlyUpdatePopulation();

                }

                foreach (var business in businesses.OrderBy(_ => rng.Next()))
                {
                    //business.ActionInvestInEfficiency();
                    //business.UpdateWorkforce();
                }

                if (i > 2 && i % 3 == 0)
                {
                    foreach (var business in businesses.OrderBy(_ => rng.Next()))
                    {
                        //business.UpdateWorkforce();
                        business.ActionAdaptProductionCapacity();
                        business.QuarterlyUpdate();
                    }
                }

                if (i > 2)
                {
                    foreach (var business in businesses)
                    {
                        business.ActionAdaptPrices();
                    }
                }



/*
    if (i > 12)
    {
        var newMarketOpportunities = countryEconomyMarket.FindMostDemandedByTrend();
        if (newMarketOpportunities.Count > 0 && businesses.Count < allowed)
        {
            List<ICompanyModel> newCompanies = new();
            newCompanies.AddRange(newMarketOpportunities
                .Select(m => businessFactory.Create(m, new CompanyResourcePolicy(2500, 2500, 10, 1000000),workerController)));
            businesses.AddRange(newCompanies);
            added += newCompanies.Count;
            allowed -= newCompanies.Count;
        }
        
        foreach (var business in businesses.OrderBy(_ => rng.Next()))
        {
            if (business.IsRemoved())
            {
                businesses.Remove(business);
                removed ++;
                allowed++;
            }
        }
    }*/

                foreach (var business in businesses.OrderBy(_ => rng.Next()))
                {
                    business.Reset(i);
                }

                countryEconomyMarket.ResetProductMarkets();
                governmentController.EndMonth();
            }




        }
    }
}


    