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
        private CompanyPlayer _company;
        private int _workerCount;
        private decimal _workerWageAverage;
        private decimal _price;

        public void Awake()
        {
            _company = companyGo.GetComponent<CompanyPlayer>();
            workerCountSlider.onValueChanged.AddListener(OnWorkerCountChanged);
            workerWageSlider.onValueChanged.AddListener(OnWorkerWageChanged);
            priceSlider.onValueChanged.AddListener(OnPriceChanged);
            _company.DecisionRequestEvent.AddListener(SetValues);
            statusText.text = "WAITING";
        }

        private void SetValues(int workerCount, decimal workerWage, decimal price)
        {
            _workerCount = workerCount;
            _workerWageAverage = workerWage;
            _price = price;
            statusText.text = "ACTIVE";
        }

        private void OnWorkerCountChanged(float val)
        {
            string text = _workerCount - val > 0 
                ? $"+{_workerCount - val:0}" 
                : _workerCount - val < 0 
                    ? $"-{_workerCount - val:0}" 
                    : "0";
            workerCountText.text = text;
        }
        
        private void OnWorkerWageChanged(float val)
        {
            decimal newWage = (decimal) val;
            string text = _workerWageAverage - newWage > 0 
                ? $"+{_workerWageAverage - newWage:0.##}" 
                : _workerWageAverage - newWage < 0 
                    ? $"-{_workerWageAverage - newWage:0.##}" 
                    : "0";
            workerWageText.text = text;
        }
        
        private void OnPriceChanged(float val)
        {
            decimal newPrice = (decimal) val;
            string text = _price - newPrice > 0 
                ? $"+{_price - newPrice:0.##}" 
                : _price - newPrice < 0 
                    ? $"-{_price - newPrice:0.##}" 
                    : "0";
            priceText.text = text;
        }

        public void Confirm()
        {
            statusText.text = "WAITING";
            _company.SendDecision((decimal)priceSlider.value, (int) workerCountSlider.value, (decimal) workerWageSlider.value);
        }
    }
}