using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui.Controller
{
    public class TimelineContainerController: MonoBehaviour
    {
        public TextMeshProUGUI headerText;
        public Button householdsButtonGo;
        public Button companiesButtonGo;
        public GameObject householdsPanelGo;
        public GameObject companiesPanelGo;

        private void Awake()
        {
            householdsButtonGo.onClick.AddListener(() =>
            {
                householdsPanelGo.SetActive(true);
                companiesPanelGo.SetActive(false);
                headerText.text = "Timeseries Households";
            });
            companiesButtonGo.onClick.AddListener(() =>
            {
                householdsPanelGo.SetActive(false);
                companiesPanelGo.SetActive(true);
                headerText.text = "Timeseries Companies";
            });
        }
    }
}