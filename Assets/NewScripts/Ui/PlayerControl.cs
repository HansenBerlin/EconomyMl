using System.Linq;
using NewScripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NewScripts.Ui
{
    public class PlayerControl : MonoBehaviour
    {
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI workerCountText;
        public TextMeshProUGUI workerWageText;
        public TextMeshProUGUI priceText;
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
            luxuryPriceSlider.onValueChanged.AddListener(OnFoodPriceChanged);
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
            workerCountText.text = $"Distribution current: {last}% food, {100 - last}% luxury, goal: {current}% food, {1 - current}% luxury";
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
            priceText.text = $"Change food price from: {_activeCompany.LastDecision.PriceFood:0.##} to {val:0.##} ({text})";
        }
        
        private void OnLuxPriceChanged(float val)
        {
            decimal newPrice = (decimal) val;
            string text = newPrice - _activeCompany.LastDecision.PriceLuxury > 0 
                ? $"+{newPrice - _activeCompany.LastDecision.PriceLuxury:0.##}" 
                : newPrice - _activeCompany.LastDecision.PriceLuxury < 0 
                    ? $"-{newPrice - _activeCompany.LastDecision.PriceLuxury:0.##}" 
                    : "0";
            priceText.text = $"Change luxury price from: {_activeCompany.LastDecision.PriceLuxury:0.##} to {val:0.##} ({text})";
        }

        private void Confirm()
        {
            //statusText.text = $"{CompanyDecisionStatus.Commited}";
            var decision = new Decision((decimal)foodPriceSlider.value, (decimal)luxuryPriceSlider.value, 
                resourceDistributionSlider.value, (int) (workerCountSlider.value - _activeCompany.WorkerCount), 
                (decimal) workerWageSlider.value, adjustWageToggle.isOn);
            _activeCompany.StartNextPeriod(decision);
            _commitButton.interactable = false;
        }
    }
}