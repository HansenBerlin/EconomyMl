using System;
using System.Collections.Generic;
using NewScripts.Ui;
using UnityEngine;

namespace NewScripts
{
    public class HouseholdAggregatorService : MonoBehaviour
    {
        public GameObject timeline;
        public List<HouseholdsAggregate> HouseholdsAggregates { get; } = new();
        private TimelineGraphDrawer _timelineGraphDrawer;

        private void Awake()
        {
            _timelineGraphDrawer = timeline.GetComponent<TimelineGraphDrawer>();
            _timelineGraphDrawer.InitializeValues(1000);
            HouseholdsAggregates.Add(new HouseholdsAggregate(1, 1));
        }

        public void Add(HouseholdLedger data)
        {
            HouseholdsAggregates[^1].Update(data);
        }
        
        public void StartNewPeriod(int month, int year)
        {
            _timelineGraphDrawer.AddDatapoint((float)HouseholdsAggregates[^1].AveragePurchasingPower);
        }
    }
}