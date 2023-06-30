﻿using System.Collections.Generic;
using NewScripts.Ui.Company.Rows;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui.Company
{
    public class DecisionPanel : MonoBehaviour
    {
        public GameObject parentGo;
        public GameObject rowPrefab;
        
        public void UpdateUi(List<CompanyData> activeCompanyData)
        {
            foreach (Transform child in parentGo.transform.GetComponentsInChildren<Transform>())
            {
                if (child.gameObject.name == "DecisionListItem(Clone)")
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
                    DecisionRow row = instance.GetComponent<DecisionRow>();

                    DecisionLedger decisionInfo = dataset.Decision;
                    row.periodText.text = $"{dataset.Month}/{dataset.Year}";
                    row.fireWorkersText.text = $"{decisionInfo.FireWorkers:0}";
                    row.openPositionsText.text = $"{decisionInfo.OpenPositions:0}";
                    row.setPriceText.text = $"{decisionInfo.SetPrice:0.##}";
                    row.setWageText.text = $"{decisionInfo.SetWorkerWage:0.##}";
                    float hue = i % 2 == 0 ? 0.04F : 0.08F;
                    var rawImage = row.GetComponent<RawImage>();
                    rawImage.color = new Color(1, 1, 1, hue);
                }
            }
        }
    }
}