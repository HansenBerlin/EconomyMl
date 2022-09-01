using Controller;
using Controller.Actions;
using Controller.Rewards;
using Enums;
using Models.Market;
using Models.Observations;
using Settings;
using UnityEngine;

namespace Factories
{



    public class ActionsFactory : MonoBehaviour
    {
        private JobMarketController _jobMarketController;
        private PopulationController _populationController;
        private ICountryEconomyMarketsModel _market;

        public void Init(JobMarketController jobMarketController, PopulationController populationController,
            ICountryEconomyMarketsModel market)
        {
            _jobMarketController = jobMarketController;
            _populationController = populationController;
            _market = market;
        }

        public IPersonAction Create(PersonActionType type)
        {
            if (type == PersonActionType.JobDecision)
            {
                return new PersonActionsJobPhase(_jobMarketController, _populationController);
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