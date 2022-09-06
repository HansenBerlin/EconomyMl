using System;
using System.Collections.Generic;

namespace Repositories
{
    public class GovernmentDataRepository
    {
        public readonly List<double> Balance = new();
        public readonly List<double> ConsumerTaxes = new();
        public readonly List<double> IncomeTaxes = new();
        public readonly List<double> ProfitTaxes = new();
        public readonly List<double> PublicServiceCosts = new();
        public readonly List<double> RetiredCosts = new();
        public readonly string StatName;
        public readonly List<double> TotalExpenses = new();
        public readonly List<double> TotalIncome = new();
        public readonly List<double> UnemployedCosts = new();
        public string Id = Guid.NewGuid().ToString();

        public GovernmentDataRepository(string statName)
        {
            StatName = statName;
        }
    }
}