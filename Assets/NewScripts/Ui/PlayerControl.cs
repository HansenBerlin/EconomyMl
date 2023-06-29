using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui
{
    public class PlayerControl : MonoBehaviour
    {
        public TextMeshProUGUI statusText;
        public Slider  workerCountSlider;
        public TextMeshProUGUI workerCountText;
        public Slider  workerWageSlider;
        public TextMeshProUGUI workerWageText;
        public Slider  priceSlider;
        public TextMeshProUGUI priceText;
        
        public GameObject companyGo;
        private ICompany _company;
        private int _workerCount;
        private decimal _workerWageAverage;
        private decimal _price;

        public void Awake()
        {
            _company = companyGo.GetComponent<ICompany>();
            workerCountSlider.onValueChanged.AddListener(OnWorkerCountChanged);
            workerWageSlider.onValueChanged.AddListener(OnWorkerWageChanged);
            priceSlider.onValueChanged.AddListener(OnPriceChanged);
            _company.DecisionRequestEventProp.AddListener(SetValues);
            statusText.text = "WAITING";
        }

        private void SetValues(int workerCount, decimal workerWage, decimal price)
        {
            _workerCount = workerCount;
            _workerWageAverage = workerWage;
            _price = price;
            workerCountSlider.value = workerCount;
            workerWageSlider.value = (int)workerWage;
            priceSlider.value = (float)price;
            statusText.text = "ACTIVE";
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
            workerCountText.text = $"Workers: {_workerCount}({text})";
        }
        
        private void OnWorkerWageChanged(float val)
        {
            decimal newWage = (decimal) val;
            string text = newWage - _workerWageAverage > 0 
                ? $"+{newWage - _workerWageAverage:0.##}" 
                : newWage - _workerWageAverage < 0 
                    ? $"-{newWage - _workerWageAverage:0.##}" 
                    : "0";
            workerWageText.text = $"Wage: {_workerWageAverage:0}({text})";
        }
        
        private void OnPriceChanged(float val)
        {
            decimal newPrice = (decimal) val;
            string text = newPrice - _price > 0 
                ? $"+{newPrice - _price:0.##}" 
                : newPrice - _price < 0 
                    ? $"-{newPrice - _price:0.##}" 
                    : "0";
            priceText.text = $"Price: {_price:0.##}({text})";
        }

        public void Confirm()
        {
            statusText.text = "WAITING";
            _company.StartNextPeriod((decimal)priceSlider.value, (int) (workerCountSlider.value - _workerCount), 
                (decimal) workerWageSlider.value);
        }
    }
}