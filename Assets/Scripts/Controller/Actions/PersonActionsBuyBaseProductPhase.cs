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
        private readonly PersonObservations _observations;
        private readonly CountryEconomyMarketsModel _market;
        private readonly PersonRewardController _rewardController;

        public PersonActionsBuyBaseProductPhase(PersonResourceDemandSettings settings,
            CountryEconomyMarketsModel market, PersonRewardController rewardController, PersonObservations observations)
        {
            _settings = settings;
            _market = market;
            _rewardController = rewardController;
            _observations = observations;
        }

        public void BuyExactAmountOfDemandedBaseResources()
        {
            int demand = GetDemand();
            var request = new ProductRequestModel(ProductType.BaseProduct, ProductRequestSearchType.MaxAmount,
                maxAmount: demand);
            var receipt = _market.Buy(request);
            UpdateProperties(receipt, demand);
        }

        public void BuyDemandedBaseResourcesWithIncomeSpendingLimit()
        {
            int demand = GetDemand();
            decimal maxSpendable = _observations.MonthlyIncome;
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

        public void BuyDemandedBaseResourcesWithCapitalSpendingLimit()
        {
            int demand = GetDemand();
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

        private void UpdateProperties(ReceiptModel receipt, int demandLeft)
        {
            _observations.MonthlyExpenses += receipt.TotalPricePaid;
            _observations.Capital -= receipt.AmountBought;
            _rewardController.RewardForBaseProductSatisfaction(receipt.AmountBought, demandLeft);
            demandLeft -= receipt.AmountBought;

            if (demandLeft > 0)
            {
                _market.ReportDemand(demandLeft, ProductType.BaseProduct);
            }
        }

        private int GetDemand()
        {
            float personDemand = _observations.AgeStatus == AgeStatus.RetiredAge
                ? _settings.DemandRetired
                : _settings.DemandWorkerAge;
            float childDemand = _observations.UnderageChildrenCount * _settings.DemandChild;
            int demand = (int) Math.Ceiling(personDemand + childDemand);
            return demand;
        }


    }
}