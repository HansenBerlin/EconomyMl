using System.Collections.Generic;
using Enums;
using Factories;
using Models.Agents;
using Models.Business;
using Policies;

namespace Controller
{
    public class BusinessRespawnController
    {
        
        
        private BusinessFactory _factory;
        private JobMarketController _jobMarket;
        private List<CompanyBaseAgent> _companys;

        public BusinessRespawnController(BusinessFactory factory, JobMarketController jobMarket, List<CompanyBaseAgent> companys)
        {
            _factory = factory;
            _jobMarket = jobMarket;
            _companys = companys;
        }
        public void Respawn(CompanyBaseAgent oldBusiness)
        {
            _companys.Remove(oldBusiness);
            var business = _factory.Create(oldBusiness.TypeProduced, _jobMarket);
            _companys.Add(business);
        }

        
    }
}