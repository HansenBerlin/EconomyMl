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
    public class TimelineStatPanelController : MonoBehaviour
    {
        public GameObject buttonPrefab;
        [FormerlySerializedAs("timelinePrefab")] public GameObject timelineGo;
        public GameObject buttonParent;
        public TextMeshProUGUI breadcrumb;
        private TimelineGraphDrawer _timelineGraphDrawer;
        private TimelineSelection _currentSelection = TimelineSelection.AveragePurchasingPower;

        private void Awake()
        {
            _timelineGraphDrawer = timelineGo.GetComponent<TimelineGraphDrawer>();
            var options = (TimelineSelection[])Enum.GetValues(typeof(TimelineSelection));

            foreach (var option in options)
            {
                CreateInstance(option.ToString(), option);
            }

            var values = GetCorrspondingValues(_currentSelection);
            if (values.Count > 0)
            {
                UpdatePanels(_currentSelection, values, true);
            }
            
            ServiceLocator.Instance.HouseholdAggregator.periodAggregateAddedEvent.AddListener((x) =>
            {
                var value = GetProperty(x, _currentSelection).GetValue(x);
                _timelineGraphDrawer.AddDatapoint((float)value);
            });
        }

        private void CreateInstance(string label, TimelineSelection type)
        {
            var button = Instantiate(buttonPrefab, buttonParent.transform);
            button.GetComponentInChildren<TextMeshProUGUI>().text = type.ToString();
            button.GetComponent<Button>().onClick.AddListener(() =>
            {
                var values = GetCorrspondingValues(type);
                UpdatePanels(type, values);
            });
        }

        private List<object> GetCorrspondingValues(TimelineSelection type)
        {
            var values = ServiceLocator.Instance.HouseholdAggregator.HouseholdsAggregates
                .Select(x => GetProperty(x, type).GetValue(x))
                .ToList();
            return values;
        }

        private PropertyInfo GetProperty(object obj, TimelineSelection propertyName)
        {
            Type type = obj.GetType();
            var property = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == propertyName.ToString());
            return property;
        }

        private void UpdatePanels<T>(TimelineSelection selection, List<T> data, bool isInit = false)
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