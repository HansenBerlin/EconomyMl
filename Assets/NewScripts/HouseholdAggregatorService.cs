using System;
using System.Collections.Generic;
using NewScripts.Enums;
using NewScripts.Ui;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewScripts
{
    public class HouseholdAggregatorService : MonoBehaviour
    {
        [FormerlySerializedAs("periodAggregateAddedEvent")] public PeriodAggregateAddedEvent periodHouseholdAggregateAddedEvent = new();
        public PeriodAggregateAddedEvent periodCompanyAggregateAddedEvent = new();
        public List<HouseholdsAggregate> HouseholdsAggregates { get; } = new();
        public List<CompaniesAggregate> CompaniesAggregates { get; } = new();

        private void Awake()
        {
            if (HouseholdsAggregates.Count == 0)
            {
                HouseholdsAggregates.Add(new HouseholdsAggregate(1, 1));
            }
            if (CompaniesAggregates.Count == 0)
            {
                CompaniesAggregates.Add(new CompaniesAggregate(1, 1));
            }
        }

        public void Add(HouseholdData data)
        {
            HouseholdsAggregates[^1].UpdateHouseholdData(data);
        }
        
        public void Add(CompanyData data)
        {
            CompaniesAggregates[^1].UpdateCompanyData(data);
        }

        public void StartNewPeriod(int month, int year)
        {
            if (HouseholdsAggregates.Count > 120)
            {
                HouseholdsAggregates.RemoveRange(0, HouseholdsAggregates.Count - 120);
                CompaniesAggregates.RemoveRange(0, CompaniesAggregates.Count - 120);
            }
            periodHouseholdAggregateAddedEvent.Invoke(HouseholdsAggregates[^1]);
            periodCompanyAggregateAddedEvent.Invoke(CompaniesAggregates[^1]);
            HouseholdsAggregates.Add(new HouseholdsAggregate(month, year));
            CompaniesAggregates.Add(new CompaniesAggregate(month, year));
        }
    }
}