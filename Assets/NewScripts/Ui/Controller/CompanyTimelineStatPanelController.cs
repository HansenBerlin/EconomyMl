using System;
using System.Collections.Generic;
using NewScripts.Common;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewScripts.Ui.Controller
{
    public class CompanyTimelineStatPanelController : MonoBehaviour
    {
        public TMP_Dropdown dropdownGo;
        [FormerlySerializedAs("timelinePrefab")] public GameObject timelineGo;
        public TextMeshProUGUI breadcrumb;
        private TimelineDataPanelController _timelineDataPanelController;
        private CompanyTimelineSelection _currentSelection = CompanyTimelineSelection.AverageLiquidity;
        private readonly PropertyConverter<CompanyTimelineSelection, CompaniesAggregate> _propertyConverter = new(); 

        public void Activate()
        {
            breadcrumb.text = _currentSelection.ToString();
        }
        
        private void Awake()
        {
            _timelineDataPanelController = timelineGo.GetComponent<TimelineDataPanelController>();
            _timelineDataPanelController.color = Colors.Cyan;
            var options = (CompanyTimelineSelection[])Enum.GetValues(typeof(CompanyTimelineSelection));

            foreach (var option in options)
            {
                var optionData = new TMP_Dropdown.OptionData(option.ToString());
                dropdownGo.options.Add(optionData);
            }
            
            dropdownGo.onValueChanged.AddListener(x =>
            {
                if (ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates.Count > 1)
                {
                    CompanyTimelineSelection type = (CompanyTimelineSelection) x;
                    var values = _propertyConverter.GetCorrspondingValues(type,
                        ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates);
                    UpdatePanels(type, values);
                } 
            });

            if (ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates.Count > 1)
            {
                var values = _propertyConverter.GetCorrspondingValues(_currentSelection, 
                    ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates);
                UpdatePanels(_currentSelection, values, true);
            }
            
            ServiceLocator.Instance.HouseholdAggregator.periodCompanyAggregateAddedEvent.AddListener(x =>
            {
                var value = _propertyConverter.GetProperty(x, _currentSelection.ToString()).GetValue(x);
                _timelineDataPanelController.AddDatapoint(Convert.ToSingle(value));
            });
        }

        private void CreateInstance(string label, CompanyTimelineSelection type, bool setactive = false)
        {
            //var button = Instantiate(buttonPrefab, buttonParent.transform);
            var optionData = new TMP_Dropdown.OptionData(type.ToString());
            dropdownGo.options.Add(optionData);
        }

        

        private void UpdatePanels<T>(CompanyTimelineSelection selection, List<T> data, bool isInit = false)
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