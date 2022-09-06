using Agents;
using Controller.Data;
using Controller.RepositoryController;
using Enums;
using Interfaces;
using Models;
using Policies;
using Repositories;
using Settings;
using UnityEngine;

namespace Factories
{
    public class BusinessFactory : MonoBehaviour
    {
        public GameObject privateBusinessPrefab;
        public GameObject publicServicePrefab;
        private readonly CompanyResourcePolicy _basePolicy = new(2300, 2300, 20, 100000, 5000);
        private readonly CompanyResourcePolicy _fedPolicy = new(2300, 2300, 100, 10000000, 0);

        private readonly CompanyResourcePolicy _fossilePolicy = new(2200, 2200, 15, 100000, 20000);
        private readonly CompanyResourcePolicy _interPolicy = new(2400, 2400, 10, 100000, 1000);
        private readonly CompanyResourcePolicy _luxPolicy = new(2600, 2600, 5, 100000, 100);
        private ICountryEconomy _countryEconomyMarkets;
        private EnvironmentModel _environment;
        private GovernmentAgent _government;
        private StatisticalDataRepository _stats;

        public void Init(ICountryEconomy countryEconomyMarkets, EnvironmentModel environment,
            StatisticalDataRepository stats, GovernmentAgent government)
        {
            _countryEconomyMarkets = countryEconomyMarkets;
            _environment = environment;
            _stats = stats;
            _government = government;
        }

        public CompanyBaseAgent Create(ProductType typeProduced, JobMarketController jobMarket)
        {
            CompanyBaseAgent business;
            var policy = GetPolicy(typeProduced);
            var model = ProductionFactory.CreateProductModel(typeProduced, _stats, _environment);
            var controller = ProductionFactory.CreateProductController(model);
            string id = IdGenerator.Create(_environment.Month, _environment.CountryName, typeProduced);
            var dataRepo = new CompanyDataRepository(id);
            _stats.AddCompanyDataset(dataRepo);
            if (typeProduced == ProductType.FederalService)
            {
                var go = Instantiate(publicServicePrefab);
                business = go.GetComponent<PublicServiceAgent>();
                business.Init(_countryEconomyMarkets, controller, policy, _government, dataRepo, jobMarket,
                    new NormalizationController());
            }
            else
            {
                var go = Instantiate(privateBusinessPrefab);
                business = go.GetComponent<PrivateCompanyAgent>();
                business.Init(_countryEconomyMarkets, controller, policy, _government, dataRepo, jobMarket,
                    new NormalizationController());
            }

            _countryEconomyMarkets.AddProduct(controller);
            return business;
        }

        private CompanyResourcePolicy GetPolicy(ProductType type)
        {
            return type switch
            {
                ProductType.BaseProduct => _basePolicy,
                ProductType.FossileEnergy => _fossilePolicy,
                ProductType.LuxuryProduct => _luxPolicy,
                ProductType.IntermediateProduct => _interPolicy,
                _ => _fedPolicy
            };
        }
    }
}