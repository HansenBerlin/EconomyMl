using System.Collections.Generic;
using NewScripts.Ui.Company.Rows;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui.Company
{
    public class BooksPanel : MonoBehaviour
    {
        public GameObject parentGo;
        public GameObject rowPrefab;
        
        public void UpdateUi(List<CompanyData> activeCompanyData)
        {
            foreach (Transform child in parentGo.transform.GetComponentsInChildren<Transform>())
            {
                if (child.gameObject.name == "BooksListItem(Clone)")
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
                    BooksRow row = instance.GetComponent<BooksRow>();
                    
                    BookKeepingLedger booksInfo = dataset.Books;
                    row.periodText.text = $"{dataset.Month}/{dataset.Year}";
                    row.incomeText.text = $"{booksInfo.Income:0.##}";
                    row.liquidityStartText.text = $"{booksInfo.LiquidityStart:0.##}";
                    row.liquidityEndText.text = $"{booksInfo.LiquidityEndCheck:0.##}";
                    row.workerPaymentsText.text = $"{booksInfo.WagePayments:0.##}";
                    row.taxPaymentsText.text = $"{booksInfo.TaxPayments:0.##}";
                    row.cashflowText.text = $"{booksInfo.LiquidityStart - booksInfo.LiquidityEndCheck:0.##}";
                    float hue = i % 2 == 0 ? 0.04F : 0.08F;
                    var rawImage = row.GetComponent<RawImage>();
                    rawImage.color = new Color(1, 1, 1, hue);
                }
            }
        }
    }
}