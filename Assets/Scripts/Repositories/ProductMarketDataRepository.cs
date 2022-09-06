using System;
using System.Collections.Generic;

namespace Repositories
{
    public class ProductMarketDataRepository
    {
        public readonly List<double> Demand = new();
        public readonly List<double> Production = new();

        public readonly List<double> Sales = new();
        public string Id = Guid.NewGuid().ToString();
        public string StatName;

        public ProductMarketDataRepository(string statName)
        {
            StatName = statName;
        }
    }
}