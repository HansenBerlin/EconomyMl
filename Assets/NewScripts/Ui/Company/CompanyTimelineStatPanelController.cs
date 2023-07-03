using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NewScripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NewScripts.Ui.Company
{
    public class CompanyTimelineStatPanelController : MonoBehaviour
    {
        public GameObject buttonPrefab;
        [FormerlySerializedAs("timelinePrefab")] public GameObject timelineGo;
        public GameObject buttonParent;
        public TextMeshProUGUI breadcrumb;
        private TimelineGraphDrawer _timelineGraphDrawer;
        private CompanyTimelineSelection _currentSelection = CompanyTimelineSelection.AverageLiquidity;
        private readonly PropertyConverter<CompanyTimelineSelection, CompaniesAggregate> _propertyConverter = new(); 

        private void Awake()
        {
            _timelineGraphDrawer = timelineGo.GetComponent<TimelineGraphDrawer>();
            var options = (CompanyTimelineSelection[])Enum.GetValues(typeof(CompanyTimelineSelection));

            foreach (var option in options)
            {
                CreateInstance(option.ToString(), option);
            }

            if (ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates.Count > 1)
            {
                var values = _propertyConverter.GetCorrspondingValues(_currentSelection, 
                    ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates);
                UpdatePanels(_currentSelection, values, true);
            }
            
            ServiceLocator.Instance.HouseholdAggregator.periodCompanyAggregateAddedEvent.AddListener(x =>
            {
                var value = _propertyConverter.GetProperty(x, _currentSelection.ToString()).GetValue(x);
                _timelineGraphDrawer.AddDatapoint(Convert.ToSingle(value));
            });
        }

        private void CreateInstance(string label, CompanyTimelineSelection type)
        {
            var button = Instantiate(buttonPrefab, buttonParent.transform);
            button.GetComponentInChildren<TextMeshProUGUI>().text = type.ToString().Substring(0, 1);
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates.Count > 1)
                {
                    var values = _propertyConverter.GetCorrspondingValues(type,
                        ServiceLocator.Instance.HouseholdAggregator.CompaniesAggregates);
                    UpdatePanels(type, values);
                }
            });
        }

        

        private void UpdatePanels<T>(CompanyTimelineSelection selection, List<T> data, bool isInit = false)
        {
            if (selection == _currentSelection && isInit == false)
            {
                return;
            }
            _currentSelection = selection;
            breadcrumb.text = _currentSelection.ToString();
            _timelineGraphDrawer.RemoveGraph();
            _timelineGraphDrawer.DrawGraph(data);
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