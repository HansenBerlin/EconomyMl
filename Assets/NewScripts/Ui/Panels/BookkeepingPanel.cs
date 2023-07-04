using System.Collections.Generic;
using NewScripts.Common;
using NewScripts.DataModelling;
using NewScripts.Ui.Models;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui.Panels
{
    public class BookkeepingPanel : MonoBehaviour
    {
        public GameObject parentGo;
        public GameObject rowPrefab;
        
        public void UpdateUi(List<CompanyLedger> activeCompanyData)
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
                for (var i = activeCompanyData.Count - 1; i >= 0; i--)
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
                    decimal liquidityChange = booksInfo.LiquidityEndCheck - booksInfo.LiquidityStart;
                    row.cashflowText.text = $"{liquidityChange:0.##}";
                    row.cashflowText.color = liquidityChange < 0 ? Colors.DarkRed : Colors.DarkGreen;
                    float hue = i % 2 == 0 ? 0.04F : 0.08F;
                    var rawImage = row.GetComponent<RawImage>();
                    rawImage.color = new Color(1, 1, 1, hue);
                }
            }
        }
    }
}