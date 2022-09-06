using System;
using Enums;
using Models.Market;
using Settings;

namespace Controller.Agents
{
    public class PersonBuyActionBaseProduct : PersonBuyAction
    {
        public PersonBuyActionBaseProduct(PersonResourceDemandSettings settings, ICountryEconomy market) : base(settings, market) { }
        
        public override void BuyDemandedProduct(int underageChildCount, decimal maxSpendable = -1)
        {
            maxSpendable = maxSpendable == -1 ? Observations.Capital : maxSpendable; 
            int demand = GetDemand(underageChildCount);
            ReceiptModel receipt = new ReceiptModel();
            if (maxSpendable > 0)
            {
                var request = new ProductRequestModel(ProductType.BaseProduct, 
                    ProductRequestSearchType.MaxAmountWithSpendingLimit, maxAmount: demand,
                    totalSpendable: maxSpendable);
                receipt = Market.Buy(request);

            }

            UpdateProperties(receipt, demand);
        }

        public override void UpdateProperties(ReceiptModel receipt, long demandLeft)
        {
            Observations.MonthlyExpensesAccumulatedForYear += receipt.TotalPricePaid;
            Observations.ThisMonthExpenses += receipt.TotalPricePaid;
            Controller.PayBill(receipt.TotalPricePaid);
            RewardController.RewardForBaseProductSatisfaction(receipt.AmountBought, demandLeft);
            demandLeft -= receipt.AmountBought;

            if (demandLeft > 0)
            {
                Market.ReportDemand(demandLeft, ProductType.BaseProduct);
                Observations.UnsatisfiedBaseDemand += demandLeft;
            }
        }

        public override int GetDemand(int childCount)
        {
            float personDemand = Observations.AgeStatus == AgeStatus.RetiredAge
                ? Settings.DemandRetired
                : Settings.DemandWorkerAge;
            float childDemand = childCount * Settings.DemandChild;
            int demand = (int) Math.Ceiling(personDemand + childDemand);
            return demand;
        }
    }
}