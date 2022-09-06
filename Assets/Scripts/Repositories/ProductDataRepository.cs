using System;
using System.Collections.Generic;

namespace Repositories
{



    public class ProductDataRepository
    {
        public string Id = Guid.NewGuid().ToString();
        public readonly string StatName;
        public readonly List<double> SupplyTrend = new();
        public readonly List<double> ProfitTrend = new();
        public readonly List<double> PriceTrend = new();
        public readonly List<double> CapacityTrend = new();
        public readonly List<double> CppTrend = new();
        public readonly List<double> SalesTrend = new();
        public readonly List<double> ProductionTrend = new();

        public readonly List<double> SalesTotal = new();
        public readonly List<double> ProducedTotal = new();
        public readonly List<double> ProfitTotal = new();
        public readonly List<double> PriceTotal = new();
        public readonly List<double> CppTotal = new();
        public readonly List<double> SupplyTotal = new();

        public ProductDataRepository(string statName)
        {
            StatName = statName;
        }
    }
}