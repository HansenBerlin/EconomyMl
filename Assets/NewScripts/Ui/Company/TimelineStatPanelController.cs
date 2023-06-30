using System.Collections.Generic;
using NewScripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui.Company
{
    public class TimelineStatPanelController : MonoBehaviour
    {
        public GameObject buyPowerButtonGo;
        public GameObject employmentButtonGo;
        public GameObject demandButtonGo;
        
        public GameObject buyerTimeline;
        public GameObject demandTimeline;
        public GameObject employmentTimeline;

        public TextMeshProUGUI breadcrumb;
        
        private TimelineSelection _currentSelection;

        private void Awake()
        {
            buyPowerButtonGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                _currentSelection = TimelineSelection.BuyPower; 
                UpdatePanelData(); 
            });
            employmentButtonGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                _currentSelection = TimelineSelection.Employment;
                UpdatePanelData();
            });
            demandButtonGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                _currentSelection = TimelineSelection.Demand;
                UpdatePanelData();
            });
        }

        private void UpdatePanelData()
        {
            GameObject[] panels = { buyerTimeline, employmentTimeline, demandTimeline };
            for (var i = 0; i < panels.Length; i++)
            {
                panels[i].SetActive(i == (int) _currentSelection);
            }
            SetBreadcrumbText();
        }

        private void SetBreadcrumbText()
        {
            string text = _currentSelection switch
            {
                TimelineSelection.BuyPower => "Kaufkraft",
                TimelineSelection.Demand => "Nachfrage",
                _ => "Beschäftigungsquote"
            };
            breadcrumb.text = text;
        }
    }
}