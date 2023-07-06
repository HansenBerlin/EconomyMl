using NewScripts.Enums;
using NewScripts.Game.Services;
using Unity.MLAgents;
using UnityEngine;

namespace NewScripts.Game.Models
{
    public class InventoryItem
    {
        public ProductType Product { get; set; }
        public int Count { get; private set; } = 0;
        public decimal AvgPaid { get; private set; }
        public int ConsumeInMonth { get; set; }
        public int FullfilledInMonth { get; set; }
        public int MonthlyMinimumDemand => _monthlyMinimumDemand * ServiceLocator.Instance.Settings.DemandModifier(Product);
        public int MonthlyAverageDemand => _monthlyAverageDemand * ServiceLocator.Instance.Settings.DemandModifier(Product);
        public int MonthlyMaximumDemand => _monthlyMaximumDemand * ServiceLocator.Instance.Settings.DemandModifier(Product);
        private long _totalBought;
        private readonly int _monthlyMinimumDemand;
        private readonly int _monthlyAverageDemand;
        private readonly int _monthlyMaximumDemand;
        
        public InventoryItem(ProductType product, int monthlyAverageDemand, 
            int monthlyMinimumDemand, int monthlyMaximumDemand, decimal startPrice)
        {
            Product = product;
            AvgPaid = startPrice;
            _monthlyAverageDemand = monthlyAverageDemand;
            _monthlyMinimumDemand = monthlyMinimumDemand;
            _monthlyMaximumDemand = monthlyMaximumDemand;
        }

        public void Add(int count, decimal price)
        {
            //Debug.Log("Add " + count + " " + Product + " for " + price + " each");
            if (count == 0 || _totalBought + count == 0)
            {
                return;
            }
            Academy.Instance.StatsRecorder.Add("New/Inventory-Add-" + Product, count);
            AvgPaid = (AvgPaid * _totalBought + price * count) / (_totalBought + count);
            Count += count;
            FullfilledInMonth += count;
            _totalBought += count;
        }

        public void Consume()
        {
            //Debug.Log("Consume " + ConsumeInMonth + " " + Product);
            Academy.Instance.StatsRecorder.Add("New/Inventory-Consume-" + Product, ConsumeInMonth);
            Count = Count - ConsumeInMonth < 0 ? 0 : Count - ConsumeInMonth;
            if (Count < 0)
            {
                Debug.LogError("Count is negative");
            }
        }
    }
}