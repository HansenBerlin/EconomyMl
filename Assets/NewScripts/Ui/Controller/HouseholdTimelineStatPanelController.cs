using System;
using System.Collections.Generic;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewScripts.Ui.Controller
{
    public class HouseholdTimelineStatPanelController : MonoBehaviour
    {
        public TMP_Dropdown dropdownGo;
        [FormerlySerializedAs("timelinePrefab")] public GameObject timelineGo;
        public TextMeshProUGUI breadcrumb;
        private TimelineDataPanelController _timelineDataPanelController;
        private WorkersTimelineSelection _currentSelection = WorkersTimelineSelection.AveragePurchasingPower;
        private readonly PropertyConverter<WorkersTimelineSelection, HouseholdsAggregate> _propertyConverter = new(); 


        private void Awake()
        {
            _timelineDataPanelController = timelineGo.GetComponent<TimelineDataPanelController>();
            var options = (WorkersTimelineSelection[])Enum.GetValues(typeof(WorkersTimelineSelection));

            foreach (var option in options)
            {
                var optionData = new TMP_Dropdown.OptionData(option.ToString());
                dropdownGo.options.Add(optionData);
            }
            
            dropdownGo.onValueChanged.AddListener(x =>
            {
                if (ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates.Count > 1)
                {
                    WorkersTimelineSelection type = (WorkersTimelineSelection) x;
                    var values = _propertyConverter.GetCorrspondingValues(type,
                        ServiceLocator.Instance.HouseholdAggregator.HouseholdsAggregates);
                    UpdatePanels(type, values);
                } 
            });

            if (ServiceLocator.Instance.HouseholdAggregator.HouseholdsAggregates.Count > 1)
            {
                var values = _propertyConverter.GetCorrspondingValues(_currentSelection, 
                    ServiceLocator.Instance.HouseholdAggregator.HouseholdsAggregates);
                UpdatePanels(_currentSelection, values, true);
            }
            
            ServiceLocator.Instance.HouseholdAggregator.periodHouseholdAggregateAddedEvent.AddListener(x =>
            {
                var value = _propertyConverter.GetProperty(x, _currentSelection.ToString()).GetValue(x);
                _timelineDataPanelController.AddDatapoint(Convert.ToSingle(value));
            });
        }
        
        private void UpdatePanels<T>(WorkersTimelineSelection selection, List<T> data, bool isInit = false)
        {
            if (selection == _currentSelection && isInit == false)
            {
                return;
            }
            _currentSelection = selection;
            breadcrumb.text = _currentSelection.ToString();
            _timelineDataPanelController.RemoveGraph();
            _timelineDataPanelController.DrawGraph(data);
        }

       //private string MapText(TimelineSelection selection)
       //{
       //    return selection switch
       //    {
       //        TimelineSelection.BuyPower => "Kaufkraft",
       //        TimelineSelection.Demand => "Nachfrage",
       //        _ => "Beschäftigungsquote"
       //    };
       //}
    }
}