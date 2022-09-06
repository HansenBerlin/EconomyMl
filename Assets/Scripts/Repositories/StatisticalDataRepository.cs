using System;
using System.Collections.Generic;
using Enums;

namespace Repositories
{
    public class StatisticalDataRepository
    {
        public readonly Dictionary<string, CompanyDataRepository> CompanyMarketData = new();
        public readonly Dictionary<string, GovernmentDataRepository> GovernmentData = new();
        public readonly Dictionary<string, ProductDataRepository> ProductData = new();
        public readonly Dictionary<string, ProductMarketDataRepository> ProductMarketData = new();

        public void AddProductDataset(ProductDataRepository data)
        {
            ProductData.Add(data.StatName, data);
        }

        public void AddGovernmentDataset(GovernmentDataRepository data)
        {
            GovernmentData.Add(data.StatName, data);
        }

        public void AddProductMarketDataset(ProductMarketDataRepository data)
        {
            ProductMarketData.Add(data.StatName, data);
        }

        public void AddCompanyDataset(CompanyDataRepository data)
        {
            CompanyMarketData.Add(data.StatName, data);
        }

        public void Remove(CompanyDataRepository data)
        {
            CompanyMarketData.Add(data.StatName, data);
        }

        public Tuple<List<double>[], string[]> GetBusinessBalanceComparison(bool isFederalServiceIncluded)
        {
            List<List<double>> data = new();
            List<string> labels = new();
            foreach ((string key, var values) in CompanyMarketData)
            {
                if (key.Contains(ProductType.FederalService.ToString()) && isFederalServiceIncluded == false) continue;

                data.Add(values.BalanceStats);
                labels.Add(key);
            }

            return new Tuple<List<double>[], string[]>(data.ToArray(), labels.ToArray());
        }
    }
}