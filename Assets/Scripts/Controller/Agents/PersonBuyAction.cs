using Agents;
using Interfaces;
using Models;
using Settings;

namespace Controller.Agents
{
    public abstract class PersonBuyAction : IPersonAction
    {
        protected readonly ICountryEconomy Market;
        protected readonly PersonResourceDemandSettings Settings;
        protected PersonController Controller;
        protected PersonObservations Observations;
        protected PersonRewardController RewardController;

        protected PersonBuyAction(PersonResourceDemandSettings settings, ICountryEconomy market)
        {
            Settings = settings;
            Market = market;
        }

        public void Init(PersonObservations observations, PersonRewardController rewardController,
            PersonController personController)
        {
            Observations = observations;
            RewardController = rewardController;
            Controller = personController;
        }

        public abstract void BuyDemandedProduct(int underageChildCount, decimal maxSpendable = -1);
        public abstract void UpdateProperties(ReceiptModel receipt, long demandLeft);
        public abstract int GetDemand(int childCount);
    }
}