using Agents;
using Controller.RepositoryController;
using Factories;

namespace Controller.Agents
{
    public class BusinessRespawnController
    {
        private readonly BusinessFactory _factory;
        private readonly JobMarketController _jobMarket;

        public BusinessRespawnController(BusinessFactory factory, JobMarketController jobMarket)
        {
            _factory = factory;
            _jobMarket = jobMarket;
        }

        public CompanyBaseAgent Respawn(CompanyBaseAgent oldBusiness)
        {
            var business = _factory.Create(oldBusiness.TypeProduced, _jobMarket);
            return business;
        }
    }
}