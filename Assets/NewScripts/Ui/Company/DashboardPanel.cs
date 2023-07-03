using System.Collections.Generic;
using NewScripts.Ui.Company.Rows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui.Company
{
    public class DashboardPanel : MonoBehaviour
    {
        public TextMeshProUGUI periodText;
        public TextMeshProUGUI companyIdText;
        public TextMeshProUGUI lifetimeText;
        public TextMeshProUGUI reputationText;
        public TextMeshProUGUI liquidityText;
        public TextMeshProUGUI cashflowText;
        
        public void UpdateUi(List<CompanyData> activeCompanyData)
        {
            if (activeCompanyData.Count == 0)
            {
                return;
            }
            var dataset = activeCompanyData[^1];
            periodText.text = $"{dataset.Month}/{dataset.Year}";
            companyIdText.text = $"{dataset.CompanyId}";
            lifetimeText.text = $"{dataset.Lifetime:0}";
            reputationText.text = $"{dataset.Reputation:0}";
            liquidityText.text = $"{dataset.Books.LiquidityStart:0}";
            cashflowText.text = $"{dataset.Books.LiquidityEndCheck - dataset.Books.LiquidityStart:0}";
        }
    }
}