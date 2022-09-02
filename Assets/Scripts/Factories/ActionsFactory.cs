using Controller;
using Controller.Actions;
using Enums;
using Models.Market;
using Settings;

namespace Factories
{



    public class ActionsFactory
    {
        private readonly JobMarketController _jobMarketController;
        private readonly ICountryEconomyMarketsModel _market;

        public ActionsFactory (JobMarketController jobMarketController, ICountryEconomyMarketsModel market)
        {
            _jobMarketController = jobMarketController;
            _market = market;
        }

        public IPersonAction Create(PersonActionType type)
        {
            if (type == PersonActionType.JobDecision)
            {
                return new PersonActionsJobPhase(_jobMarketController);
            }

            if (type == PersonActionType.BaseProductBuy)
            {
                var baseDemandSettings = new PersonResourceDemandSettings(60, 0.25F, 30);
                return new PersonActionsBuyBaseProductPhase(baseDemandSettings, _market);
            }

            var luxuryDemandSettings = new PersonResourceDemandSettings(2, 0, 1);
            return new PersonActionsBuyLuxuryProductPhase(luxuryDemandSettings, _market);


        }
    }
}