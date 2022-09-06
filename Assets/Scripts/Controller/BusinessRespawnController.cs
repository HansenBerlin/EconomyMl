using Assets.Scripts.Factories;
using Assets.Scripts.Models.Business;

namespace Assets.Scripts.Controller
{
    public class BusinessRespawnController
    {
        
        
        private BusinessFactory _factory;
        private JobMarketController _jobMarket;

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