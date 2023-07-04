using System.Linq;
using NewScripts.Enums;
using NewScripts.Game.Models;
using NewScripts.Game.Services;
using NewScripts.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NewScripts.Ui.Controller
{
    public class DecisionPanelController : MonoBehaviour
    {
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI workerCountText;
        public TextMeshProUGUI workerWageText;
        [FormerlySerializedAs("priceText")] public TextMeshProUGUI foodPriceText;
        public TextMeshProUGUI luxuryPriceText;
        public TextMeshProUGUI resourceDistributionText;
        public Slider  workerCountSlider;
        public Slider  workerWageSlider;
        public Slider  resourceDistributionSlider;
        [FormerlySerializedAs("priceSlider")] public Slider  foodPriceSlider;
        public Slider  luxuryPriceSlider;
        public Toggle adjustWageToggle;
        public GameObject commitButtonGo;
        
        private ICompany _activeCompany;
        private Button _commitButton;
        

        public void Awake()
        {
            workerCountSlider.onValueChanged.AddListener(OnWorkerCountChanged);
            workerWageSlider.onValueChanged.AddListener(OnWorkerWageChanged);
            foodPriceSlider.onValueChanged.AddListener(OnFoodPriceChanged);
            luxuryPriceSlider.onValueChanged.AddListener(OnLuxPriceChanged);
            resourceDistributionSlider.onValueChanged.AddListener(OnWorkerDistributionChanged);
            _commitButton = commitButtonGo.GetComponent<Button>();
            _commitButton.onClick.AddListener(Confirm);
            ServiceLocator.Instance.UiUpdateManager.playerDecisionValuesUpdateEvent.AddListener(SetValues);

            if (_activeCompany == null)
            {
                var activeCompany = ServiceLocator.Instance.Companys
                    .FirstOrDefault(x => x.Id == ServiceLocator.Instance.UiUpdateManager.SelectedCompanyId);
                if (activeCompany?.PlayerType == PlayerType.Human)
                {
                    _activeCompany = activeCompany;
                    SetValues(_activeCompany);
                }
            }
        }

        private void SetValues(ICompany company)
        {
            _commitButton.interactable = company.DecisionStatus == CompanyDecisionStatus.Requested;
            _activeCompany = company;
            workerCountSlider.value = _activeCompany.WorkerCount;
            workerWageSlider.value = (int) _activeCompany.LastDecision.Wage;
            foodPriceSlider.value = (float)_activeCompany.LastDecision.PriceFood;
            luxuryPriceSlider.value = (float)_activeCompany.LastDecision.PriceLuxury;
            resourceDistributionSlider.value = (float)_activeCompany.LastDecision.RessourceDistribution;
            statusText.text = $"{_activeCompany.DecisionStatus}";
            OnWorkerCountChanged(_activeCompany.WorkerCount);
            OnWorkerWageChanged((float)_activeCompany.LastDecision.Wage);
            OnFoodPriceChanged((float)_activeCompany.LastDecision.PriceFood);
            OnLuxPriceChanged((float)_activeCompany.LastDecision.PriceLuxury);
            OnWorkerDistributionChanged(_activeCompany.LastDecision.RessourceDistribution);
        }

        private void OnWorkerCountChanged(float val)
        {
            string text = val - _activeCompany.WorkerCount > 0 
                ? $"+{val - _activeCompany.WorkerCount:0}" 
                : val - _activeCompany.WorkerCount < 0 
                    ? $"-{val - _activeCompany.WorkerCount:0}" 
                    : "0";
            workerCountText.text = $"Workers current: {_activeCompany.WorkerCount}, goal: {val} ({text})";
        }
        
        private void OnWorkerDistributionChanged(float val)
        {
            float last = _activeCompany.LastDecision.RessourceDistribution * 100;
            float current = val * 100;
            resourceDistributionText.text = $"Distribution current: {last:0.##}% food, {100 - last:0.##}% luxury, goal: {current:0.##}% food, {100 - current:0.##}% luxury";
        }
        
        private void OnWorkerWageChanged(float val)
        {
            decimal newWage = (decimal) val;
            string text = newWage - _activeCompany.AverageWageRate > 0 
                ? $"+{newWage - _activeCompany.AverageWageRate:0.##}" 
                : newWage - _activeCompany.AverageWageRate < 0 
                    ? $"-{newWage - _activeCompany.AverageWageRate:0.##}" 
                    : "0";
            workerWageText.text = $"Change wage from {_activeCompany.AverageWageRate:0} to {val} ({text})";
        }
        
        private void OnFoodPriceChanged(float val)
        {
            decimal newPrice = (decimal) val;
            string text = newPrice - _activeCompany.LastDecision.PriceFood > 0 
                ? $"+{newPrice - _activeCompany.LastDecision.PriceFood:0.##}" 
                : newPrice - _activeCompany.LastDecision.PriceFood < 0 
                    ? $"-{newPrice - _activeCompany.LastDecision.PriceFood:0.##}" 
                    : "0";
            foodPriceText.text = $"Change food price from: {_activeCompany.LastDecision.PriceFood:0.##} to {val:0.##} ({text})";
        }
        
        private void OnLuxPriceChanged(float val)
        {
            decimal newPrice = (decimal) val;
            string text = newPrice - _activeCompany.LastDecision.PriceLuxury > 0 
                ? $"+{newPrice - _activeCompany.LastDecision.PriceLuxury:0.##}" 
                : newPrice - _activeCompany.LastDecision.PriceLuxury < 0 
                    ? $"-{newPrice - _activeCompany.LastDecision.PriceLuxury:0.##}" 
                    : "0";
            luxuryPriceText.text = $"Change luxury price from: {_activeCompany.LastDecision.PriceLuxury:0.##} to {val:0.##} ({text})";
        }

        private void Confirm()
        {
            //statusText.text = $"{CompanyDecisionStatus.Commited}";
            var decision = new Decision
            {
                PriceFood = (decimal)foodPriceSlider.value,
                PriceLuxury = (decimal)luxuryPriceSlider.value,
                RessourceDistribution = resourceDistributionSlider.value,
                WorkerChange = (int) (workerCountSlider.value - _activeCompany.WorkerCount), 
                Wage = (decimal) workerWageSlider.value,
                AdjustWages = adjustWageToggle.isOn
            };
            _activeCompany.StartNextPeriod(decision);
            _commitButton.interactable = false;
        }
    }
}