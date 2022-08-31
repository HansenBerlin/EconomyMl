using EconomyBase.Controller;
using EconomyBase.Controller.Actions;
using EconomyBase.Controller.Rewards;
using EconomyBase.Enums;
using EconomyBase.Models.Market;
using EconomyBase.Models.Observations;
using EconomyBase.Settings;

namespace EconomyBase.Factories
{



    public class ActionsFactory
    {
        private readonly JobMarketController _jobMarketController;
        private readonly PopulationController _populationController;
        private readonly CountryEconomyMarketsModel _market;

        public ActionsFactory(JobMarketController jobMarketController, PopulationController populationController,
            CountryEconomyMarketsModel market)
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