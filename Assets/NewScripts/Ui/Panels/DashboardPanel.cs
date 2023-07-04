using System.Collections.Generic;
using NewScripts.DataModelling;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewScripts.Ui.Panels
{
    public class DashboardPanel : MonoBehaviour
    {
        public TextMeshProUGUI periodText;
        [FormerlySerializedAs("companyIdText")] public TextMeshProUGUI companyNameText;
        public TextMeshProUGUI liquidityText;
        public TextMeshProUGUI cashflowText;
        public TextMeshProUGUI reputationText;
        public TextMeshProUGUI workerCount;
        public TextMeshProUGUI salesFood;
        public TextMeshProUGUI salesLux;
        public TextMeshProUGUI productionFood;
        public TextMeshProUGUI productionLux;

        public void UpdateUi(List<CompanyLedger> activeCompanyData)
        {
            if (activeCompanyData.Count == 0)
            {
                return;
            }
            var dataset = activeCompanyData[^1];
            periodText.text = $"{dataset.Month}/{dataset.Year}({dataset.Lifetime})";
            companyNameText.text = $"{dataset.CompanyName}";
            reputationText.text = $"{dataset.Reputation:0}";
            liquidityText.text = $"{dataset.Books.LiquidityStart:0}";
            cashflowText.text = $"{dataset.Books.LiquidityEndCheck - dataset.Books.LiquidityStart:0}";
            workerCount.text = $"{dataset.Workers.EndCount}";
            salesFood.text = $"{dataset.Food.Sales:0}";
            salesLux.text = $"{dataset.Luxury.Sales:0}";
            productionFood.text = $"{dataset.Food.Production:0}";
            productionLux.text = $"{dataset.Luxury.Production:0}";
        }
    }
}