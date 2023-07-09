using System;
using NewScripts.Game.Models;
using UnityEngine;

namespace NewScripts.Game.Services
{
    public class BidCalculatorService
    {
        private readonly System.Random _rand = new();
        
        public (int low, int high) DemandModifier(InventoryItem inventoryItem)
        {
            float ratio = inventoryItem.FullfilledInMonth > 0 && inventoryItem.ConsumeInMonth > 0 ? 
                inventoryItem.FullfilledInMonth / (float)inventoryItem.ConsumeInMonth : 0;
            float modifier = (ratio + 1) / 2;
            int low = (int)(inventoryItem.MonthlyMinimumDemand * modifier);
            int high = (int)(inventoryItem.MonthlyMaximumDemand * modifier);
            return (low, high);
        }

        public decimal DetermineBiddingPrice(decimal averageMarketPrice, InventoryItem inventoryItem)
        {
            decimal priceWillingness = inventoryItem.ConsumeInMonth - inventoryItem.Count;

            if (inventoryItem.Count < inventoryItem.MonthlyMinimumDemand)
            {
                priceWillingness += inventoryItem.AvgPaid - averageMarketPrice; 
            }

            decimal minPrice = averageMarketPrice / 2;
            decimal maxPrice = averageMarketPrice * 2;
            decimal biddingPrice = Math.Max(minPrice, Math.Min(maxPrice, 
                inventoryItem.AvgPaid + priceWillingness / (inventoryItem.MonthlyAverageDemand * 2 - inventoryItem.MonthlyMinimumDemand))
            );

            return biddingPrice;
        }
        
        public int DemandByMarginalUtility(int minDemand, int maxDemand)
        {
            double random = _rand.NextDouble();
            int demand = minDemand + (int)Math.Round(Math.Pow(random, 2) * (maxDemand - minDemand));
            return demand;
        }
    }
}