using System;
using System.Linq;
using NewScripts.Game.Services;

namespace NewScripts.DataModelling
{
    public class EconomyMetricsCalculator
    {
        public decimal CalculateInflationRate()
        {
            var companyAggregates = ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates;
         
            if (companyAggregates.Count < 2)
            {
                return 0;
            }
            
            decimal averagePriceFoodLastPeriod = companyAggregates[^2].AveragePriceOfferFood;
            decimal averagePriceFoodThisPeriod = companyAggregates[^1].AveragePriceOfferFood;
            decimal averagePriceLuxuryLastPeriod = companyAggregates[^2].AveragePriceOfferLuxury;
            decimal averagePriceLuxuryThisPeriod = companyAggregates[^1].AveragePriceOfferLuxury;

            decimal averageFoodDemand = ServiceLocator.Instance.Settings.FoodDemandModifier * 150;
            decimal averageLuxuryDemand = ServiceLocator.Instance.Settings.LuxuryDemandModifier * 10;
            decimal vpiTwo = averageFoodDemand * averagePriceFoodLastPeriod + averageLuxuryDemand * averagePriceLuxuryLastPeriod;
            decimal vpiOne = averageFoodDemand * averagePriceFoodThisPeriod + averageLuxuryDemand * averagePriceLuxuryThisPeriod;
            decimal inflationRate = (vpiOne / vpiTwo - 1) * 100;
            
            return inflationRate;
        }

        public int CalculateBip(int countCompanies)
        {
            var companyAggregate = ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates[^1];
            double averagePriceFoodThisPeriod = (double)companyAggregate.AveragePriceOfferFood;
            double averagePriceLuxuryThisPeriod = (double)companyAggregate.AveragePriceOfferLuxury;

            double foodProduction = companyAggregate.AverageSupplyFood * countCompanies;
            double luxuryProduction = companyAggregate.AverageSupplyFood * countCompanies;

            int bip = (int)Math.Floor(averagePriceFoodThisPeriod * foodProduction + averagePriceLuxuryThisPeriod * luxuryProduction);
            return bip;
        }
    }
}