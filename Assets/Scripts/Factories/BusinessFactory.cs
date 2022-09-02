using Controller;
using Enums;
using Models.Business;
using Models.Market;
using Models.Meta;
using Policies;
using Repositories;
using Settings;

namespace Factories
{



    public class BusinessFactory
    {
        private readonly ICountryEconomy _countryEconomyMarkets;
        private readonly StatisticalDataRepository _stats;
        private readonly GovernmentController _government;
        private readonly EnvironmentModel _environment;

        public BusinessFactory(ICountryEconomy countryEconomyMarkets, EnvironmentModel environment,
            StatisticalDataRepository stats, GovernmentController government)
        {
            _countryEconomyMarkets = countryEconomyMarkets;
            _environment = environment;
            _stats = stats;
            _government = government;
        }

        public ICompanyModel Create(ProductType typeProduced, CompanyResourcePolicy policy,
            JobMarketController jobMarket)
        {
            ICompanyModel business;
            var model = ProductionFactory.CreateProductModel(typeProduced, _stats, _environment);
            var controller = ProductionFactory.CreateProductController(model);
            string id = IdGenerator.Create(_environment.Month, _environment.CountryName, typeProduced);
            var dataRepo = new CompanyDataRepository(id);
            _stats.AddCompanyDataset(dataRepo);
            if (typeProduced == ProductType.FederalService)
            {
                business = new PublicService(_countryEconomyMarkets, controller, policy, _government, dataRepo,
                    jobMarket);
            }
            else
            {
                business = new PrivateCompany(_countryEconomyMarkets, controller, policy, _government, dataRepo,
                    jobMarket);
            }

            _countryEconomyMarkets.AddBusiness(business);
            _countryEconomyMarkets.AddProduct(controller);
            return business;
        }
    }
}