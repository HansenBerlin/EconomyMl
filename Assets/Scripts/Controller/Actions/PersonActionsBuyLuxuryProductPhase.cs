using System;
using Controller.Rewards;
using Enums;
using Models.Market;
using Models.Observations;
using Settings;
using UnityEngine;

namespace Controller.Actions
{
    public class PersonActionsBuyLuxuryProductPhase : IPersonAction
    {
        private readonly PersonResourceDemandSettings _settings;
        private readonly ICountryEconomy _market;

        public PersonActionsBuyLuxuryProductPhase(PersonResourceDemandSettings settings, ICountryEconomy market)
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
            decimal maxSpendable = observations.Salary;
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

        public void BuyDemandedLuxuryProductWithCapitalSpendingLimit(PersonObservations observations, int underageChildCount, PersonRewardController rewardController)
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

        private void UpdateProperties(ReceiptModel receipt, long demandLeft, PersonObservations observations, PersonRewardController rewardController)
        {
            observations.MonthlyExpensesAccumulatedForYear += receipt.TotalPricePaid;
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