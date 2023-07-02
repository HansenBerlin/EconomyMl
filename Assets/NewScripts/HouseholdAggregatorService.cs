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
        public PeriodAggregateAddedEvent periodAggregateAddedEvent = new();
        public List<HouseholdsAggregate> HouseholdsAggregates { get; } = new();

        private void Awake()
        {
            if (HouseholdsAggregates.Count == 0)
            {
                HouseholdsAggregates.Add(new HouseholdsAggregate(1, 1));
            }
        }

        public void Add(HouseholdData data)
        {
            HouseholdsAggregates[^1].UpdateHouseholdData(data);
        }

        public void StartNewPeriod(int month, int year)
        {
            periodAggregateAddedEvent.Invoke(HouseholdsAggregates[^1]);
            HouseholdsAggregates.Add(new HouseholdsAggregate(month, year));
        }
    }
}