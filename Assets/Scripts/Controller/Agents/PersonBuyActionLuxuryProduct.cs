using System;
using Enums;
using Models.Market;
using Settings;
using UnityEngine;

namespace Controller.Agents
{
    public class PersonBuyActionLuxuryProduct : PersonBuyAction
    {
        public PersonBuyActionLuxuryProduct(PersonResourceDemandSettings settings, ICountryEconomy market) : base(settings, market) { }

        public override void BuyDemandedProduct(int underageChildCount, decimal maxSpendable = -1)
        {
            int demand = GetDemand(underageChildCount);
            maxSpendable = maxSpendable == -1 ? Observations.Capital : maxSpendable; 
            ReceiptModel receipt = new ReceiptModel();
            if (maxSpendable > 0)
            {
                var request = new ProductRequestModel(ProductType.LuxuryProduct,
                    ProductRequestSearchType.MaxAmountWithSpendingLimit, maxAmount: demand,
                    totalSpendable: maxSpendable);
                receipt = Market.Buy(request);

            }

            UpdateProperties(receipt, demand);
        }

        public override void UpdateProperties(ReceiptModel receipt, long demandLeft)
        {
            Observations.MonthlyExpensesAccumulatedForYear += receipt.TotalPricePaid;
            Controller.PayBill(receipt.TotalPricePaid);
            RewardController.RewardForLuxuryProductSatisfaction(receipt.AmountBought, demandLeft);
            demandLeft -= receipt.AmountBought;

            if (demandLeft > 0)
            {
                Market.ReportDemand(demandLeft, ProductType.BaseProduct);
            }

            Observations.LuxuryProducts += receipt.AmountBought;
        }

        public override int GetDemand(int underageChildCount)
        {
            if (underageChildCount > 1000 || underageChildCount < 0)
            {
                Debug.Log("");
            }
            float personDemand = Observations.AgeStatus == AgeStatus.RetiredAge
                ? Settings.DemandRetired
                : Settings.DemandWorkerAge;
            float childDemand = underageChildCount * Settings.DemandChild;
            int demand = (int) Math.Ceiling(personDemand + childDemand);
            if (demand > 1000 || underageChildCount < 0)
            {
                Debug.Log("");
            }
            return demand;
        }


    }
}