using System;
using Controller.Rewards;
using Enums;
using Models.Market;
using Models.Observations;
using Settings;

namespace Controller.Actions
{



    public class PersonActionsBuyBaseProductPhase : IPersonAction
    {
        private readonly PersonResourceDemandSettings _settings;
        private readonly ICountryEconomy _market;

        public PersonActionsBuyBaseProductPhase(PersonResourceDemandSettings settings, ICountryEconomy market)
        {
            _settings = settings;
            _market = market;
        }

        public void BuyExactAmountOfDemandedBaseResources(PersonObservations observations, int underageChildCount, PersonRewardController rewardController)
        {
            int demand = GetDemand(observations, underageChildCount);
            var request = new ProductRequestModel(ProductType.BaseProduct, ProductRequestSearchType.MaxAmount,
                maxAmount: demand);
            var receipt = _market.Buy(request);
            UpdateProperties(receipt, demand, observations, rewardController);
        }

        public void BuyDemandedBaseResourcesWithIncomeSpendingLimit(PersonObservations observations, int underageChildCount, PersonRewardController rewardController)
        {
            int demand = GetDemand(observations, underageChildCount);
            decimal maxSpendable = observations.Salary;
            ReceiptModel receipt = new ReceiptModel();
            if (maxSpendable > 0)
            {
                var request = new ProductRequestModel(ProductType.BaseProduct,
                    ProductRequestSearchType.MaxAmountWithSpendingLimit, maxAmount: demand,
                    totalSpendable: maxSpendable);
                receipt = _market.Buy(request);

            }

            UpdateProperties(receipt, demand, observations, rewardController);
        }

        public void BuyDemandedBaseResourcesWithCapitalSpendingLimit(PersonObservations observations, int underAgeChildCount, PersonRewardController rewardController)
        {
            int demand = GetDemand(observations, underAgeChildCount);
            decimal maxSpendable = observations.Capital;
            ReceiptModel receipt = new ReceiptModel();
            if (maxSpendable > 0)
            {
                var request = new ProductRequestModel(ProductType.BaseProduct,
                    ProductRequestSearchType.MaxAmountWithSpendingLimit, maxAmount: demand,
                    totalSpendable: maxSpendable);
                receipt = _market.Buy(request);

            }

            UpdateProperties(receipt, demand, observations, rewardController);
        }

        private void UpdateProperties(ReceiptModel receipt, long demandLeft, PersonObservations observations, PersonRewardController rewardController)
        {
            observations.MonthlyExpensesAccumulatedForYear += receipt.TotalPricePaid;
            observations.Capital -= receipt.AmountBought;
            rewardController.RewardForBaseProductSatisfaction(receipt.AmountBought, demandLeft, observations);
            demandLeft -= receipt.AmountBought;

            if (demandLeft > 0)
            {
                _market.ReportDemand(demandLeft, ProductType.BaseProduct);
                observations.UnsatisfiedBaseDemand += demandLeft;
            }
        }

        private int GetDemand(PersonObservations observations, int childCount)
        {
            float personDemand = observations.AgeStatus == AgeStatus.RetiredAge
                ? _settings.DemandRetired
                : _settings.DemandWorkerAge;
            float childDemand = childCount * _settings.DemandChild;
            int demand = (int) Math.Ceiling(personDemand + childDemand);
            return demand;
        }


    }
}