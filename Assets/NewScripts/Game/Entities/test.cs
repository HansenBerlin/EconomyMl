using System;
using NewScripts.Game.Models;
using UnityEngine;

namespace NewScripts.Game.Entities
{
    public static class ProductDemandPriceCalculator
    {
        public static float Money = 1000000f;
        
        public static (float bidPrice, int amount) CalculateDemandAndPrice(int inventorycount, float averageMarketPrice, 
            int lastFullfilled, float lastdemand, float minDemand, float maxDemand, float avgDemand, float avgHistorical)
        {
            (int low, int high) = DemandModifier(lastFullfilled, lastdemand, minDemand, maxDemand);
            var newDemand = DemandByMarginalUtility(low, high);
            float bidPrice = DetermineBiddingPrice(averageMarketPrice, newDemand, inventorycount, minDemand, avgDemand, avgHistorical);
            
            if (newDemand * bidPrice > Money)
            {
                newDemand = (int)Math.Floor(Money / bidPrice);
            }
            return (bidPrice, newDemand);
        }
        
        private static (int low, int high) DemandModifier(float lastfullfilled, float lastdemand, float minDemand, float maxDemand)
        {
            float ratio = lastfullfilled > 0 && lastdemand > 0 ? 
                lastfullfilled / lastdemand : 0;
            float modifier = (ratio + 1) / 2;
            int low = (int)(minDemand * modifier);
            int high = (int)(maxDemand * modifier);
            return (low, high);
        }

        private static float DetermineBiddingPrice(float averageMarketPrice, float currentDemand, int inventoryCount, 
            float mindemand, float avgdemand, float avgHistorical)
        {
            float priceWillingness = currentDemand - inventoryCount;

            if (inventoryCount < mindemand)
            {
                priceWillingness += avgHistorical - averageMarketPrice; 
            }

            float minPrice = averageMarketPrice / 2;
            float maxPrice = averageMarketPrice * 2;
            float biddingPrice = Math.Max(minPrice, Math.Min(maxPrice, avgHistorical + priceWillingness / (avgdemand * 2 - mindemand)));

            return biddingPrice;
        }

        private static int DemandByMarginalUtility(int minDemand, int maxDemand)
        {
            double random = new System.Random().NextDouble();
            int demand = minDemand + (int)Math.Round(Math.Pow(random, 2) * (maxDemand - minDemand));
            return demand;
        }
    }
}