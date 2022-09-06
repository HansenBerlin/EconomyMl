using System;
using System.Collections.Generic;

namespace Repositories
{
    public class CompanyDataRepository
    {
        public readonly List<double> BalanceStats = new();
        public readonly List<double> MoneyInStat = new();
        public readonly List<double> MoneyOutStat = new();
        public readonly string StatName;
        public readonly List<double> TotalProduced = new();
        public readonly List<double> WorkersStat = new();
        public string Id = Guid.NewGuid().ToString();

        public CompanyDataRepository(string statName)
        {
            StatName = statName;
        }
    }
}