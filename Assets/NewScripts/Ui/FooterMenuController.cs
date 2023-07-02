using System;
using NewScripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui
{
    public class FooterMenuController : MonoBehaviour
    {
        public TextMeshProUGUI selectedCompanyText;
        public TextMeshProUGUI currentRoundText;
        public Button decisionButton;

        public void RegisterEvents()
        {
            decisionButton.interactable = false;
            ServiceLocator.Instance.UiUpdateManager.companySelectedEvent.AddListener(company =>
            {
                selectedCompanyText.text = $"Company: {company.Name} ({company.PlayerType})";
                decisionButton.interactable = company.PlayerType == PlayerType.Human;
            });
            ServiceLocator.Instance.UiUpdateManager.newPeriodStartedEvent.AddListener((month, year) =>
            {
                currentRoundText.text = $"Round: {month}/{year}";
            });
        }
    }
}