using EconomyBase.Controller;
using EconomyBase.Enums;
using EconomyBase.Models.Business;
using EconomyBase.Models.Market;
using EconomyBase.Models.Meta;
using EconomyBase.Policies;
using EconomyBase.Repositories;
using EconomyBase.Settings;

namespace EconomyBase.Factories
{



    public class BusinessFactory
    {
        private readonly ICountryEconomyMarketsModel _countryEconomyMarkets;
        private readonly StatisticalDataRepository _stats;
        private readonly GovernmentController _government;
        private readonly EnvironmentModel _environment;

        public BusinessFactory(ICountryEconomyMarketsModel countryEconomyMarkets, EnvironmentModel environment,
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