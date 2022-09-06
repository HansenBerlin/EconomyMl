using System;
using Assets.Scripts.Controller.Rewards;
using Assets.Scripts.Enums;
using Assets.Scripts.Models.Market;
using Assets.Scripts.Models.Observations;
using Assets.Scripts.Settings;
using UnityEngine;

namespace Assets.Scripts.Controller.Actions
{
    public class PersonActionsBuyLuxuryProductPhase : IPersonAction
    {
        private readonly PersonResourceDemandSettings _settings;
        private readonly ICountryEconomy _market;
        private PersonObservations _observations;
        private PersonRewardController _rewardController;
        private PersonController _controller;

        public PersonActionsBuyLuxuryProductPhase(PersonResourceDemandSettings settings, ICountryEconomy market)
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

        public void BuyExactAmountOfDemandedLuxuryProduct(int underageChildCount)
        {
            int demand = GetDemand(_observations, underageChildCount);
            var request = new ProductRequestModel(ProductType.LuxuryProduct, ProductRequestSearchType.MaxAmountWithSpendingLimit,
                maxAmount: demand, totalSpendable:_observations.Capital);
            var receipt = _market.Buy(request);
            
            UpdateProperties(receipt, demand);
        }

        public void BuyDemandedLuxuryProductWithIncomeSpendingLimit(int underageChildCount)
        {
            int demand = GetDemand(_observations, underageChildCount);
            decimal maxSpendable = _observations.Salary;
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

        public void BuyDemandedLuxuryProductWithCapitalSpendingLimit(int underageChildCount)
        {
            int demand = GetDemand(_observations, underageChildCount);
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

        private void UpdateProperties(ReceiptModel receipt, long demandLeft)
        {
            _observations.MonthlyExpensesAccumulatedForYear += receipt.TotalPricePaid;
            _controller.PayBill(receipt.TotalPricePaid);
            _rewardController.RewardForLuxuryProductSatisfaction(receipt.AmountBought, demandLeft, _observations);
            demandLeft -= receipt.AmountBought;

            if (demandLeft > 0)
            {
                _market.ReportDemand(demandLeft, ProductType.BaseProduct);
            }

            _observations.LuxuryProducts += receipt.AmountBought;
        }

        private int GetDemand(PersonObservations observations, int underageChildCount)
        {
            if (underageChildCount > 1000 || underageChildCount < 0)
            {
                Debug.Log("");
            }
            float personDemand = observations.AgeStatus == AgeStatus.RetiredAge
                ? _settings.DemandRetired
                : _settings.DemandWorkerAge;
            float childDemand = underageChildCount * _settings.DemandChild;
            int demand = (int) Math.Ceiling(personDemand + childDemand);
            if (demand > 1000 || underageChildCount < 0)
            {
                Debug.Log("");
            }
            return demand;
        }


    }
}