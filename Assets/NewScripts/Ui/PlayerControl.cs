using TMPro;
using UnityEngine;
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
        public Slider  priceSlider;
        
        private ICompany _activeCompany;
        private int _workerCount;
        private decimal _workerWageAverage;
        private decimal _price;
        private int _companyId;

        public void Awake()
        {
            workerCountSlider.onValueChanged.AddListener(OnWorkerCountChanged);
            workerWageSlider.onValueChanged.AddListener(OnWorkerWageChanged);
            priceSlider.onValueChanged.AddListener(OnPriceChanged);
            ServiceLocator.Instance.CompanySelectionManager.playerDecisionEvent.AddListener(SetValues);
        }

        private void SetValues(int workerCount, decimal workerWage, decimal price, int companyId)
        {
            _companyId = companyId;
            _workerCount = workerCount;
            _workerWageAverage = workerWage;
            _price = price;
            workerCountSlider.value = workerCount;
            workerWageSlider.value = (int)workerWage;
            priceSlider.value = (float)price;
            statusText.text = $"{_activeCompany.DecisionStatus})";
            OnWorkerCountChanged(workerCount);
            OnWorkerWageChanged((float)workerWage);
            OnPriceChanged((float)price);
        }

        private void OnWorkerCountChanged(float val)
        {
            string text = val - _workerCount > 0 
                ? $"+{val - _workerCount:0}" 
                : val - _workerCount < 0 
                    ? $"-{val - _workerCount:0}" 
                    : "0";
            workerCountText.text = $"Workers goal: {_workerCount} (change: {text})";
        }
        
        private void OnWorkerWageChanged(float val)
        {
            decimal newWage = (decimal) val;
            string text = newWage - _workerWageAverage > 0 
                ? $"+{newWage - _workerWageAverage:0.##}" 
                : newWage - _workerWageAverage < 0 
                    ? $"-{newWage - _workerWageAverage:0.##}" 
                    : "0";
            workerWageText.text = $"Wage: {_workerWageAverage:0} (change: {text})";
        }
        
        private void OnPriceChanged(float val)
        {
            decimal newPrice = (decimal) val;
            string text = newPrice - _price > 0 
                ? $"+{newPrice - _price:0.##}" 
                : newPrice - _price < 0 
                    ? $"-{newPrice - _price:0.##}" 
                    : "0";
            priceText.text = $"Price: {_price:0.##} (change: {text})";
        }

        public void Confirm()
        {
            statusText.text = $"{_activeCompany.DecisionStatus})";
            _activeCompany.StartNextPeriod((decimal)priceSlider.value, (int) (workerCountSlider.value - _workerCount), 
                (decimal) workerWageSlider.value);
        }
    }
}