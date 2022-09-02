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
        private GovernmentController _government;
        private EnvironmentModel _environment;
        public GameObject businessPrefab;

        public void Init(ICountryEconomy countryEconomyMarkets, EnvironmentModel environment,
            StatisticalDataRepository stats, GovernmentController government)
        {
            _countryEconomyMarkets = countryEconomyMarkets;
            _environment = environment;
            _stats = stats;
            _government = government;
        }

        public CompanyBaseAgent Create(ProductType typeProduced, CompanyResourcePolicy policy,
            JobMarketController jobMarket)
        {
            CompanyBaseAgent business;
            var model = ProductionFactory.CreateProductModel(typeProduced, _stats, _environment);
            var controller = ProductionFactory.CreateProductController(model);
            string id = IdGenerator.Create(_environment.Month, _environment.CountryName, typeProduced);
            var dataRepo = new CompanyDataRepository(id);
            _stats.AddCompanyDataset(dataRepo);
            if (typeProduced == ProductType.FederalService)
            {
                var go =Instantiate(businessPrefab);
                business = go.GetComponent<PublicServiceAgent>();
                business.Init(_countryEconomyMarkets, controller, policy, _government, dataRepo, jobMarket);
            }
            else
            {
                var go =Instantiate(businessPrefab);
                business = go.GetComponent<PrivateCompanyAgent>();
                business.Init(_countryEconomyMarkets, controller, policy, _government, dataRepo, jobMarket);
            }

            _countryEconomyMarkets.AddBusiness(business);
            _countryEconomyMarkets.AddProduct(controller);
            return business;
        }
    }
}