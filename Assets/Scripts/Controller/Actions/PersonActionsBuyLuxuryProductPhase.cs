using System;
using Controller.Rewards;
using Enums;
using Models.Market;
using Models.Observations;
using Settings;

namespace Controller.Actions
{
    public class PersonActionsBuyLuxuryProductPhase : IPersonAction
    {
        private readonly PersonResourceDemandSettings _settings;
        private readonly ICountryEconomyMarketsModel _market;

        public PersonActionsBuyLuxuryProductPhase(PersonResourceDemandSettings settings, ICountryEconomyMarketsModel market)
        {
            _settings = settings;
            _market = market;
        }

        public void BuyExactAmountOfDemandedLuxuryProduct(PersonObservations observations, int underageChildCount, PersonRewardController rewardController)
        {
            int demand = GetDemand(observations, underageChildCount);
            var request = new ProductRequestModel(ProductType.LuxuryProduct, ProductRequestSearchType.MaxAmount,
                maxAmount: demand);
            var receipt = _market.Buy(request);
            
            UpdateProperties(receipt, demand, observations, rewardController);
        }

        public void BuyDemandedLuxuryProductWithIncomeSpendingLimit(PersonObservations observations, int underageChildCount, PersonRewardController rewardController)
        {
            int demand = GetDemand(observations, underageChildCount);
            decimal maxSpendable = observations.MonthlyIncome;
            ReceiptModel receipt = new ReceiptModel();
            if (maxSpendable > 0)
            {
                var request = new ProductRequestModel(ProductType.LuxuryProduct,
                    ProductRequestSearchType.MaxAmountWithSpendingLimit, maxAmount: demand,
                    totalSpendable: maxSpendable);
                receipt = _market.Buy(request);

            }

            UpdateProperties(receipt, demand, observations, rewardController);
        }

        public void BuyDemandedBaseProductWithCapitalSpendingLimit(PersonObservations observations, int underageChildCount, PersonRewardController rewardController)
        {
            int demand = GetDemand(observations, underageChildCount);
            decimal maxSpendable = observations.Capital;
            ReceiptModel receipt = new ReceiptModel();
            if (maxSpendable > 0)
            {
                var request = new ProductRequestModel(ProductType.LuxuryProduct,
                    ProductRequestSearchType.MaxAmountWithSpendingLimit, maxAmount: demand,
                    totalSpendable: maxSpendable);
                receipt = _market.Buy(request);

            }

            UpdateProperties(receipt, demand, observations, rewardController);
        }

        private void UpdateProperties(ReceiptModel receipt, int demandLeft, PersonObservations observations, PersonRewardController rewardController)
        {
            observations.MonthlyExpenses += receipt.TotalPricePaid;
            observations.Capital -= receipt.AmountBought;
            rewardController.RewardForLuxuryProductSatisfaction(receipt.AmountBought, demandLeft, observations);
            demandLeft -= receipt.AmountBought;

            if (demandLeft > 0)
            {
                _market.ReportDemand(demandLeft, ProductType.BaseProduct);
            }

            observations.LuxuryProducts += receipt.AmountBought;
        }

        private int GetDemand(PersonObservations observations, int underageChildCount)
        {
            float personDemand = observations.AgeStatus == AgeStatus.RetiredAge
                ? _settings.DemandRetired
                : _settings.DemandWorkerAge;
            float childDemand = underageChildCount * _settings.DemandChild;
            int demand = (int) Math.Ceiling(personDemand + childDemand);
            return demand;
        }


    }
}