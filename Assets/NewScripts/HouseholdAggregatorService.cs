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
        [FormerlySerializedAs("timeline")] public GameObject buyerTimeline;
        public GameObject demandTimeline;
        public GameObject employmentTimeline;
        public float initialMaxValueBuyerPower = 100;
        public float initialMaxValueDemand = 100;
        public float initialMaxValueEmployment = 50;
        private readonly List<HouseholdsAggregate> _householdsAggregates = new();
        private TimelineGraphDrawer _buyerPowerTimeline;
        private TimelineGraphDrawer _demandTimeline;
        private TimelineGraphDrawer _employmentTimeline;

        private void Awake()
        {
            _buyerPowerTimeline = buyerTimeline.GetComponent<TimelineGraphDrawer>();
            _buyerPowerTimeline.InitializeValues(initialMaxValueBuyerPower);
            _employmentTimeline = employmentTimeline.GetComponent<TimelineGraphDrawer>();
            _employmentTimeline.InitializeValues(initialMaxValueEmployment);
            _demandTimeline = demandTimeline.GetComponent<TimelineGraphDrawer>();
            _demandTimeline.InitializeValues(initialMaxValueDemand);
            if (_householdsAggregates.Count == 0)
            {
                _householdsAggregates.Add(new HouseholdsAggregate(1, 1));
            }
        }

        public void Add(HouseholdLedger data)
        {
            _householdsAggregates[^1].Update(data);
        }

        public void StartNewPeriod(int month, int year)
        {
            _buyerPowerTimeline.AddDatapoint((float)_householdsAggregates[^1].AveragePurchasingPower);
            _demandTimeline.AddDatapoint((float)_householdsAggregates[^1].AverageDemand);
            _employmentTimeline.AddDatapoint((float)_householdsAggregates[^1].EmploymentRate * 100);
            _householdsAggregates.Add(new HouseholdsAggregate(month, year));
        }
    }
}