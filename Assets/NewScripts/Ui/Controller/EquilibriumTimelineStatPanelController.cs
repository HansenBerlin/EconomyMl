using System;
using System.Collections.Generic;
using System.Linq;
using NewScripts.Common;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewScripts.Ui.Controller
{
    public class EquilibriumTimelineStatPanelController : MonoBehaviour
    {
        public TMP_Dropdown dropdownGo;
        public GameObject timelineOneGo;
        public GameObject timelineTwoGo;
        public TextMeshProUGUI bottomTick;
        public TextMeshProUGUI breadcrumb;
        private TimelineDataPanelController _timelineDataPanelControllerOne;
        private TimelineDataPanelController _timelineDataPanelControllerTwo;
        
        private void Awake()
        {
            _timelineDataPanelControllerOne = timelineOneGo.GetComponent<TimelineDataPanelController>();
            _timelineDataPanelControllerOne.color = Colors.Amber;
            _timelineDataPanelControllerTwo = timelineTwoGo.GetComponent<TimelineDataPanelController>();
            _timelineDataPanelControllerTwo.color = Colors.Blue;
            _timelineDataPanelControllerOne.drawRightTicks = false;
            _timelineDataPanelControllerTwo.drawRightTicks = false;
        }
        
        public void UpdatePanels(List<decimal> dataOne, List<decimal> dataTwo)
        {
            float stepWidth = 860 / (dataOne.Count - 1);
            decimal maxOne = dataOne.Max();
            decimal maxTwo = dataTwo.Max();
            decimal max = maxOne > maxTwo ? maxOne : maxTwo;
            _timelineDataPanelControllerOne.stepWidth = stepWidth;
            _timelineDataPanelControllerTwo.stepWidth = stepWidth;
            _timelineDataPanelControllerOne.drawLeftTicks = maxOne > maxTwo;
            _timelineDataPanelControllerTwo.drawLeftTicks = maxTwo > maxOne;
            _timelineDataPanelControllerOne.RemoveGraph();
            _timelineDataPanelControllerOne.DrawGraph(dataOne, (float)max);
            _timelineDataPanelControllerTwo.RemoveGraph();
            _timelineDataPanelControllerTwo.DrawGraph(dataTwo, (float)max);
        }
        
        private readonly List<GameObject> _ticks = new ();

        private void DrawBottomTicks(float stepX, float stepRange)
        {
            foreach (var tick in _ticks)
            {
                Destroy(tick.gameObject);
            }
            
            for (var i = 0; i < 6; i++)
            {
                var tick = Instantiate(bottomTick, transform);
                float value = i * stepRange;
                tick.text = value > 10 ? $"{value:#}" : $"{value:0.##}";
                tick.transform.localPosition = new Vector3(i * stepX + 42, 10, 0);
                _ticks.Add(tick.gameObject);
            }
        }

        public void UpdateSimulation()
        {
            int offersCount = 200;
            decimal minPriceOffer = 10;
            decimal maxPriceOffer = 20;
            int bidsCount = 150;
            decimal minPriceBid = 7.5M;
            decimal prohibitivePrice = 15;
            
            decimal gradientOffers = (maxPriceOffer - minPriceOffer) / offersCount;
            decimal gradientBids = (minPriceBid - prohibitivePrice) / bidsCount;
            int stepValueRange = offersCount > bidsCount ? offersCount / 6 : bidsCount / 6;
            var offersData = new List<decimal>();
            var bidsData = new List<decimal>();
            decimal margin = 0.01m;
            for (var i = 0; i < 7; i++)
            {
                decimal newValueOffers = minPriceOffer + stepValueRange * gradientOffers * i;
                if (newValueOffers <= maxPriceOffer + margin)
                {
                    offersData.Add(newValueOffers);
                }
                else
                {
                    offersData.Add(-1);
                }
                decimal newValueBids = prohibitivePrice + stepValueRange * gradientBids * i;
                if (newValueBids >= minPriceBid - margin)
                {
                    bidsData.Add(newValueBids);
                }
                else
                {
                    bidsData.Add(-1);
                }
            }
            float stepWidth = 172;
            UpdatePanels(offersData, bidsData);
            DrawBottomTicks(stepWidth, stepValueRange);

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