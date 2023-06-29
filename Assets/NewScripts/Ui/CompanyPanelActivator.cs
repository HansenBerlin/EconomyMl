using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui
{
    public class CompanyPanelActivator : MonoBehaviour
    {
        public GameObject activatorButton;
        public GameObject parentGo;
        public GameObject rowPrefab;
        public List<CompanyData> ActiveCompanyData { get; set; } = new();
        public int ActiveCompanyId { get; set; }

        private void Awake()
        {
            activatorButton.GetComponent<Button>().onClick.AddListener(UpdateUi);
            UpdateUi();
        }

        public void UpdateUi()
        {
            foreach (Transform child in parentGo.transform.GetComponentsInChildren<Transform>())
            {
                if (child.gameObject.name == "ProductListItem(Clone)")
                {
                    Destroy(child.gameObject);
                }
            }

            if (ActiveCompanyData.Count > 0)
            {
                foreach (var dataset in ActiveCompanyData)
                {
                    GameObject instance = Instantiate(rowPrefab, parentGo.transform);
                    ProductRow row = instance.GetComponent<ProductRow>();
                    ProductLedger productInfo = dataset.Product;
                    row.periodText.text = $"{dataset.Month}/{dataset.Year}";
                    row.priceText.text = $"{productInfo.PriceSet:0.##}";
                    row.stockText.text = $"{productInfo.StockStart:0}";
                    row.productionText.text = $"{productInfo.Production:0}";
                    row.salesText.text = $"{productInfo.Sales:0}";
                    row.stockCheckText.text = $"{productInfo.StockEndCheck:0}";
                }
            }
        }
    }
}