using System;
using TMPro;
using UnityEngine;

namespace NewScripts.Ui
{
    public class FooterMenuController : MonoBehaviour
    {
        public TextMeshProUGUI selectedCompanyText;
        public TextMeshProUGUI currentRoundText;

        public void RegisterEvents()
        {
            ServiceLocator.Instance.UiUpdateManager.companySelectedEvent.AddListener(company =>
            {
                selectedCompanyText.text = $"Company: {company.Name} ({company.PlayerType})";
            });
            ServiceLocator.Instance.UiUpdateManager.newPeriodStartedEvent.AddListener((month, year) =>
            {
                currentRoundText.text = $"Round: {month}/{year}";
            });
        }
    }
}