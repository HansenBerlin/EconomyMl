using Interfaces;
using Models.Market;
using Models.Observations;
using Settings;

namespace Controller.Agents
{
    public abstract class PersonBuyAction : IPersonAction
    {
        protected readonly PersonResourceDemandSettings Settings;
        protected readonly ICountryEconomy Market;
        protected PersonObservations Observations;
        protected PersonRewardController RewardController;
        protected PersonController Controller;

        protected PersonBuyAction(PersonResourceDemandSettings settings, ICountryEconomy market)
        {
            Settings = settings;
            Market = market;
        }

        public void Init(PersonObservations observations, PersonRewardController rewardController, PersonController personController)
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