using System;
using System.Collections.Generic;

namespace Repositories
{



    public class ProductMarketDataRepository
    {
        public string Id = Guid.NewGuid().ToString();
        public string StatName;

        public readonly List<double> Sales = new();
        public readonly List<double> Production = new();
        public readonly List<double> Demand = new();

        public ProductMarketDataRepository(string statName)
        {
            StatName = statName;
        }
    }
}