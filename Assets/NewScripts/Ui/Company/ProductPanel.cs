using System.Collections.Generic;
using NewScripts.Ui.Company.Rows;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui.Company
{
    public class ProductPanel : MonoBehaviour
    {
        public GameObject parentGo;
        public GameObject rowPrefab;
        
        public void UpdateUi(List<CompanyData> activeCompanyData)
        {
            foreach (Transform child in parentGo.transform.GetComponentsInChildren<Transform>())
            {
                if (child.gameObject.name == "ProductListItem(Clone)")
                {
                    Destroy(child.gameObject);
                }
            }

            if (activeCompanyData.Count > 0)
            {
                for (var i = 0; i < activeCompanyData.Count; i++)
                {
                    var dataset = activeCompanyData[i];
                    GameObject instance = Instantiate(rowPrefab, parentGo.transform);
                    ProductRow row = instance.GetComponent<ProductRow>();
                    
                    ProductLedger productInfo = dataset.Product;
                    row.periodText.text = $"{dataset.Month}/{dataset.Year}";
                    row.priceText.text = $"{productInfo.PriceSet:0.##}";
                    row.stockText.text = $"{productInfo.StockStart:0}";
                    row.productionText.text = $"{productInfo.Production:0}";
                    row.salesText.text = $"{productInfo.Sales:0}";
                    row.destroyedText.text = $"{productInfo.Destroyed:0}";
                    row.stockCheckText.text = $"{productInfo.StockEndCheck:0}";
                    float hue = i % 2 == 0 ? 0.04F : 0.08F;
                    var rawImage = row.GetComponent<RawImage>();
                    rawImage.color = new Color(1, 1, 1, hue);
                }
            }
        }
    }
}