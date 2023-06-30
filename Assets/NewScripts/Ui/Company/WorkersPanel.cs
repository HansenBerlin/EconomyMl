using System.Collections.Generic;
using NewScripts.Ui.Company.Rows;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui.Company
{
    public class WorkersPanel : MonoBehaviour
    {
        public GameObject parentGo;
        public GameObject rowPrefab;
        
        public void UpdateUi(List<CompanyData> activeCompanyData)
        {
            foreach (Transform child in parentGo.transform.GetComponentsInChildren<Transform>())
            {
                if (child.gameObject.name == "WorkersListItem(Clone)")
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
                    WorkerRow row = instance.GetComponent<WorkerRow>();
                    
                    WorkersLedger workerInfo = dataset.Workers;
                    row.periodText.text = $"{dataset.Month}/{dataset.Year}";
                    row.startText.text = $"{workerInfo.StartCount}";
                    row.hiredText.text = $"{workerInfo.Hired}";
                    row.firedText.text = $"{workerInfo.Fired}";
                    row.quitText.text = $"{workerInfo.Quit}";
                    row.endText.text = $"{workerInfo.EndCount}";
                    row.offeredWageText.text = $"{workerInfo.OfferedWage:0}";
                    row.avgWageText.text = $"{workerInfo.AverageWage:0}";
                    row.paidText.text = $"{workerInfo.PaidCount}/{workerInfo.UnpaidCount}";
                    row.openPositionsText.text = $"{workerInfo.OpenPositions}";
                    float hue = i % 2 == 0 ? 0.04F : 0.08F;
                    var rawImage = row.GetComponent<RawImage>();
                    rawImage.color = new Color(1, 1, 1, hue);
                }
            }
        }
    }
}