using Controller;
using Controller.Actions;
using Controller.Rewards;
using Enums;
using Models.Market;
using Models.Observations;
using Settings;

namespace Factories
{



    public class ActionsFactory
    {
        private readonly JobMarketController _jobMarketController;
        private readonly PopulationController _populationController;
        private readonly ICountryEconomyMarketsModel _market;

        public ActionsFactory(JobMarketController jobMarketController, PopulationController populationController,
            ICountryEconomyMarketsModel market)
        {
            _jobMarketController = jobMarketController;
            _populationController = populationController;
            _market = market;
        }

        public IPersonAction Create(PersonActionType type, PersonController controller, PersonObservations observations,
            PersonRewardController rewardController)
        {
            if (type == PersonActionType.JobDecision)
            {
                return new PersonActionsJobPhase(_jobMarketController, _populationController, rewardController,
                    controller, observations);
            }

            if (type == PersonActionType.BaseProductBuy)
            {
                var baseDemandSettings = new PersonResourceDemandSettings(60, 0.25F, 30);
                return new PersonActionsBuyBaseProductPhase(baseDemandSettings, _market, rewardController,
                    observations);
            }

            var luxuryDemandSettings = new PersonResourceDemandSettings(2, 0, 1);
            return new PersonActionsBuyLuxuryProductPhase(luxuryDemandSettings, _market, rewardController,
                observations);


        }
    }
}