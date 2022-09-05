using Controller;
using Enums;
using Models.Business;
using Models.Market;
using Models.Meta;
using Policies;
using Repositories;
using Settings;
using UnityEngine;

namespace Factories
{



    public class BusinessFactory : MonoBehaviour
    {
        private ICountryEconomy _countryEconomyMarkets;
        private StatisticalDataRepository _stats;
        private GovernmentAgent _government;
        private EnvironmentModel _environment;
        public GameObject privateBusinessPrefab;
        public GameObject publicServicePrefab;
        
        CompanyResourcePolicy fossilePolicy = new CompanyResourcePolicy(2200, 2200, 15, 100000, 20000);
        CompanyResourcePolicy basePolicy = new CompanyResourcePolicy(2300, 2300, 20, 100000, 5000);
        CompanyResourcePolicy interPolicy = new CompanyResourcePolicy(2400, 2400, 10, 100000, 1000);
        CompanyResourcePolicy luxPolicy = new CompanyResourcePolicy(2600, 2600, 5, 100000, 100);
        CompanyResourcePolicy fedPolicy = new CompanyResourcePolicy(2300, 2300, 100, 10000000, 0);

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
                var go =Instantiate(publicServicePrefab);
                business = go.GetComponent<PublicServiceAgent>();
                business.Init(_countryEconomyMarkets, controller, policy, _government, dataRepo, jobMarket, new NormalizationController());
            }
            else
            {
                var go =Instantiate(privateBusinessPrefab);
                business = go.GetComponent<PrivateCompanyAgent>();
                business.Init(_countryEconomyMarkets, controller, policy, _government, dataRepo, jobMarket, new NormalizationController());
            }

            _countryEconomyMarkets.AddBusiness(business);
            _countryEconomyMarkets.AddProduct(controller);
            return business;
        }
        
        private CompanyResourcePolicy GetPolicy(ProductType type)
        {
            return type switch
            {
                ProductType.BaseProduct => basePolicy,
                ProductType.FossileEnergy => fossilePolicy,
                ProductType.LuxuryProduct => luxPolicy,
                ProductType.IntermediateProduct => interPolicy,
                _ => fedPolicy
            };
        }
    }
}