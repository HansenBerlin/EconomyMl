using System.Linq;
using NewScripts.Enums;
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
        public GameObject commitButtonGo;
        
        private ICompany _activeCompany;
        private Button _commitButton;
        

        public void Awake()
        {
            workerCountSlider.onValueChanged.AddListener(OnWorkerCountChanged);
            workerWageSlider.onValueChanged.AddListener(OnWorkerWageChanged);
            priceSlider.onValueChanged.AddListener(OnPriceChanged);
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
            workerWageSlider.value = (int)_activeCompany.OfferedWageRate;
            priceSlider.value = (float)_activeCompany.ProductPrice;
            statusText.text = $"{_activeCompany.DecisionStatus}";
            OnWorkerCountChanged(_activeCompany.WorkerCount);
            OnWorkerWageChanged((float)_activeCompany.OfferedWageRate);
            OnPriceChanged((float)_activeCompany.ProductPrice);
            
        }

        private void OnWorkerCountChanged(float val)
        {
            string text = val - _activeCompany.WorkerCount > 0 
                ? $"+{val - _activeCompany.WorkerCount:0}" 
                : val - _activeCompany.WorkerCount < 0 
                    ? $"-{val - _activeCompany.WorkerCount:0}" 
                    : "0";
            workerCountText.text = $"Workers goal: {_activeCompany.WorkerCount} (change: {text})";
        }
        
        private void OnWorkerWageChanged(float val)
        {
            var average = _activeCompany.AverageWageRate;
            decimal newWage = (decimal) val;
            string text = newWage - _activeCompany.AverageWageRate > 0 
                ? $"+{newWage - _activeCompany.AverageWageRate:0.##}" 
                : newWage - _activeCompany.AverageWageRate < 0 
                    ? $"-{newWage - _activeCompany.AverageWageRate:0.##}" 
                    : "0";
            workerWageText.text = $"Wage: {_activeCompany.AverageWageRate:0} (change: {text})";
        }
        
        private void OnPriceChanged(float val)
        {
            decimal newPrice = (decimal) val;
            string text = newPrice - _activeCompany.ProductPrice > 0 
                ? $"+{newPrice - _activeCompany.ProductPrice:0.##}" 
                : newPrice - _activeCompany.ProductPrice < 0 
                    ? $"-{newPrice - _activeCompany.ProductPrice:0.##}" 
                    : "0";
            priceText.text = $"Price: {_activeCompany.ProductPrice:0.##} (change: {text})";
        }

        private void Confirm()
        {
            //statusText.text = $"{CompanyDecisionStatus.Commited}";
            _activeCompany.StartNextPeriod((decimal)priceSlider.value, (int) (workerCountSlider.value - _activeCompany.WorkerCount), 
                (decimal) workerWageSlider.value);
            _commitButton.interactable = false;
        }
    }
}