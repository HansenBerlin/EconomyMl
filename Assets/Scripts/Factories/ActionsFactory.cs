using Assets.Scripts.Controller;
using Assets.Scripts.Controller.Actions;
using Assets.Scripts.Enums;
using Assets.Scripts.Models.Market;
using Assets.Scripts.Settings;

namespace Assets.Scripts.Factories
{



    public class ActionsFactory
    {
        private readonly JobMarketController _jobMarketController;
        private readonly ICountryEconomy _market;

        public ActionsFactory (JobMarketController jobMarketController, ICountryEconomy market)
        {
            _jobMarketController = jobMarketController;
            _market = market;
        }

        public IPersonAction Create(PersonActionType type)
        {
            if (type == PersonActionType.JobDecision)
            {
                return new PersonActionsJobPhaseFree(_jobMarketController);
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