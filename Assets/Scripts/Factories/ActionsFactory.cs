using Controller.Agents;
using Controller.RepositoryController;
using Enums;
using Interfaces;
using Settings;

namespace Factories
{
    public class ActionsFactory
    {
        private readonly JobMarketController _jobMarketController;
        private readonly ICountryEconomy _market;

        public ActionsFactory(JobMarketController jobMarketController, ICountryEconomy market)
        {
            _jobMarketController = jobMarketController;
            _market = market;
        }

        public IPersonAction Create(PersonActionType type)
        {
            if (type == PersonActionType.JobDecision) return new PersonJobAction(_jobMarketController);

            if (type == PersonActionType.BaseProductBuy)
            {
                var baseDemandSettings = new PersonResourceDemandSettings(60, 0.25F, 30);
                return new PersonBuyActionBaseProduct(baseDemandSettings, _market);
            }

            var luxuryDemandSettings = new PersonResourceDemandSettings(2, 0, 1);
            return new PersonBuyActionLuxuryProduct(luxuryDemandSettings, _market);
        }
    }
}