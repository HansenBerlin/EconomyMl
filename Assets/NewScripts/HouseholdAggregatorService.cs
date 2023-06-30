using System.Collections.Generic;

namespace NewScripts
{
    public class HouseholdAggregatorService
    {
        public List<HouseholdsAggregate> HouseholdsAggregates { get; } = new();
        
        public void Add(HouseholdLedger data)
        {
            HouseholdsAggregates[^1].Update(data);
        }
        
        public void StartNewPeriod(int month, int year)
        {
            HouseholdsAggregates.Add(new HouseholdsAggregate(month, year));
        }
    }
}