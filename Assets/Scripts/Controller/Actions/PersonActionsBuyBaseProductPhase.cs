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
        private PersonObservations _observations;
        private PersonRewardController _rewardController;
        private PersonController _controller;

        public PersonActionsBuyBaseProductPhase(PersonResourceDemandSettings settings, ICountryEconomy market)
        {
            _settings = settings;
            _market = market;
        }
        
        public void Init(PersonObservations observations, PersonRewardController rewardController, PersonController controller)
        {
            _observations = observations;
            _rewardController = rewardController;
            _controller = controller;
        }

        public void BuyExactAmountOfDemandedBaseResources(int underageChildCount)
        {
            int demand = GetDemand(underageChildCount);
            var request = new ProductRequestModel(ProductType.BaseProduct, ProductRequestSearchType.MaxAmountWithSpendingLimit,
                maxAmount: demand, totalSpendable:_observations.Capital);
            var receipt = _market.Buy(request);
            UpdateProperties(receipt, demand);
        }

        public void BuyDemandedBaseResourcesWithIncomeSpendingLimit(int underageChildCount)
        {
            int demand = GetDemand(underageChildCount);
            decimal maxSpendable = _observations.Salary;
            ReceiptModel receipt = new ReceiptModel();
            if (maxSpendable > 0)
            {
                var request = new ProductRequestModel(ProductType.BaseProduct,
                    ProductRequestSearchType.MaxAmountWithSpendingLimit, maxAmount: demand,
                    totalSpendable: maxSpendable);
                receipt = _market.Buy(request);

            }

            UpdateProperties(receipt, demand);
        }

        public void BuyDemandedBaseResourcesWithCapitalSpendingLimit(int underAgeChildCount)
        {
            int demand = GetDemand(underAgeChildCount);
            decimal maxSpendable = _observations.Capital;
            ReceiptModel receipt = new ReceiptModel();
            if (maxSpendable > 0)
            {
                var request = new ProductRequestModel(ProductType.BaseProduct,
                    ProductRequestSearchType.MaxAmountWithSpendingLimit, maxAmount: demand,
                    totalSpendable: maxSpendable);
                receipt = _market.Buy(request);

            }

            UpdateProperties(receipt, demand);
        }

        private void UpdateProperties(ReceiptModel receipt, long demandLeft)
        {
            _observations.MonthlyExpensesAccumulatedForYear += receipt.TotalPricePaid;
            _observations.ThisMonthExpenses += receipt.TotalPricePaid;
            _controller.PayBill(receipt.TotalPricePaid);
            _rewardController.RewardForBaseProductSatisfaction(receipt.AmountBought, demandLeft, _observations);
            demandLeft -= receipt.AmountBought;

            if (demandLeft > 0)
            {
                _market.ReportDemand(demandLeft, ProductType.BaseProduct);
                _observations.UnsatisfiedBaseDemand += demandLeft;
            }
        }

        public int GetDemand(int childCount)
        {
            float personDemand = _observations.AgeStatus == AgeStatus.RetiredAge
                ? _settings.DemandRetired
                : _settings.DemandWorkerAge;
            float childDemand = childCount * _settings.DemandChild;
            int demand = (int) Math.Ceiling(personDemand + childDemand);
            return demand;
        }
    }
}