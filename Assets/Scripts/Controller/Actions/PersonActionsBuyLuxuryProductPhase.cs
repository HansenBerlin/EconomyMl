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
        private readonly PersonObservations _observations;
        private readonly PersonRewardController _rewardController;

        public PersonActionsBuyLuxuryProductPhase(PersonResourceDemandSettings settings,
            ICountryEconomyMarketsModel market, PersonRewardController rewardController, PersonObservations observations)
        {
            _settings = settings;
            _market = market;
            _rewardController = rewardController;
            _observations = observations;
        }

        public void BuyExactAmountOfDemandedLuxuryProduct()
        {
            int demand = GetDemand();
            var request = new ProductRequestModel(ProductType.LuxuryProduct, ProductRequestSearchType.MaxAmount,
                maxAmount: demand);
            var receipt = _market.Buy(request);
            UpdateProperties(receipt, demand);
        }

        public void BuyDemandedLuxuryProductWithIncomeSpendingLimit()
        {
            int demand = GetDemand();
            decimal maxSpendable = _observations.MonthlyIncome;
            ReceiptModel receipt = new ReceiptModel();
            if (maxSpendable > 0)
            {
                var request = new ProductRequestModel(ProductType.LuxuryProduct,
                    ProductRequestSearchType.MaxAmountWithSpendingLimit, maxAmount: demand,
                    totalSpendable: maxSpendable);
                receipt = _market.Buy(request);

            }

            UpdateProperties(receipt, demand);
        }

        public void BuyDemandedBaseProductWithCapitalSpendingLimit()
        {
            int demand = GetDemand();
            decimal maxSpendable = _observations.Capital;
            ReceiptModel receipt = new ReceiptModel();
            if (maxSpendable > 0)
            {
                var request = new ProductRequestModel(ProductType.LuxuryProduct,
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
            demandLeft -= receipt.AmountBought;

            if (demandLeft > 0)
            {
                _market.ReportDemand(demandLeft, ProductType.BaseProduct);
            }

            _observations.LuxuryProducts += receipt.AmountBought;
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