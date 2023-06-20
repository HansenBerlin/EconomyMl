using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NewScripts
{
    public class CompanyInfoPopup : MonoBehaviour
    {
        public TextMeshProUGUI liquidityText;
        public TextMeshProUGUI salesText;
        public TextMeshProUGUI workersText;
        public TextMeshProUGUI stockText;
        public TextMeshProUGUI wageText;
        public TextMeshProUGUI realWageText;
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI lifetimeText;
        private List<TextMeshProUGUI> _textBlocks = new();
        public int CurrentlyActive { get; private set; }

        public void SetTexts(List<string> values, int companyId)
        {
            CurrentlyActive = companyId;
            _textBlocks = new();
            _textBlocks.AddRange(new []{liquidityText, salesText, workersText, stockText, wageText, realWageText, priceText, lifetimeText});
            if (_textBlocks.Count != values.Count)
            {
                throw new Exception("Length not matching");
            }
            
            for (int i = 0; i < values.Count; i++)
            {
                _textBlocks[i].GetComponent<TextMeshProUGUI>().text = values[i];
            }
        }

        public void Close()
        {
            
        }
    }
}