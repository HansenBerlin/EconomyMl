using System.Collections.Generic;
using System.Linq;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Services;
using NewScripts.Ui.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NewScripts.Ui.Controller
{
    public class CompanyContainerPanelController : MonoBehaviour
    {
        public GameObject dashboardButtonGo;
        [FormerlySerializedAs("productButtonGo")] public GameObject productFoodButtonGo;
        public GameObject workersButtonGo;
        public GameObject booksButtonGo;
        public GameObject decisionButtonGo;
        public GameObject productLuxuryButtonGo;
        
        public GameObject dashboardPanelGo;
        [FormerlySerializedAs("productPanelGo")] public GameObject productPanelFoodGo;
        public GameObject productPanelLuxuryGo;
        public GameObject workersPanelGo;
        public GameObject booksPanelGo;
        public GameObject decisionPanelGo;

        public TextMeshProUGUI breadcrumb;
        
        //public List<CompanyData> ActiveCompanyData
        //{
        //    get => _activeCompanyData;
        //    set
        //    {
        //        if(value.Count == 0) return;
        //        _activeCompanyData = value;
        //        ActiveCompanyId = _activeCompanyData[0].CompanyId;
        //        UpdatePanelData();
        //    } 
        //}

        private int _activeCompanyId;
        private List<CompanyLedger> _activeCompanyData = new();
        private CompanyPanelSelection _currentSelection;

        private void Awake()
        {
            dashboardButtonGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                _currentSelection = CompanyPanelSelection.Dashboard; 
                UpdatePanelData(); 
            });
            productFoodButtonGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                _currentSelection = CompanyPanelSelection.ProductFood;
                UpdatePanelData();
            });
            productLuxuryButtonGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                _currentSelection = CompanyPanelSelection.ProductLux;
                UpdatePanelData();
            });
            workersButtonGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                _currentSelection = CompanyPanelSelection.Workers;
                UpdatePanelData();
            });
            booksButtonGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                _currentSelection = CompanyPanelSelection.Books;
                UpdatePanelData();
            });
            decisionButtonGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                _currentSelection = CompanyPanelSelection.Decision;
                UpdatePanelData();
            });
            ServiceLocator.Instance.UiUpdateManager.playerDecisionValuesUpdateEvent.AddListener(x =>
            {
                _activeCompanyId = x.Id;
                _activeCompanyData = x.Ledger;
                UpdatePanelData();
            });
            if (_activeCompanyData.Count == 0)
            {
                var activeCompany = ServiceLocator.Instance.Companys
                    .FirstOrDefault(x => x.Id == ServiceLocator.Instance.UiUpdateManager.SelectedCompanyId);
                if (activeCompany?.Ledger.Count > 0)
                {
                    _activeCompanyId = activeCompany.Id;
                    _activeCompanyData = activeCompany.Ledger;
                    UpdatePanelData();
                }
            }
        }

        private void UpdatePanelData()
        {
            GameObject[] panels = { dashboardPanelGo, productPanelFoodGo, productPanelLuxuryGo, workersPanelGo, booksPanelGo, decisionPanelGo };
            for (var i = 0; i < panels.Length; i++)
            {
                panels[i].SetActive(i == (int) _currentSelection);
            }

            List<CompanyLedger> activeCompanyData = _activeCompanyData.Count >= 120 
                ? _activeCompanyData.GetRange(_activeCompanyData.Count - 120, 120) 
                : _activeCompanyData;

            switch (_currentSelection)
            {
                case CompanyPanelSelection.Dashboard:
                    dashboardPanelGo.GetComponent<DashboardPanel>().UpdateUi(_activeCompanyData);
                    break;
                case CompanyPanelSelection.ProductFood:
                    productPanelFoodGo.GetComponent<ProductPanel>().UpdateUi(activeCompanyData, ProductType.Food);
                    break;
                case CompanyPanelSelection.ProductLux:
                    productPanelLuxuryGo.GetComponent<ProductPanel>().UpdateUi(activeCompanyData, ProductType.Luxury);
                    break;
                case CompanyPanelSelection.Workers:
                    workersPanelGo.GetComponent<WorkersPanel>().UpdateUi(activeCompanyData);
                    break;
                case CompanyPanelSelection.Books:
                    booksPanelGo.GetComponent<BookkeepingPanel>().UpdateUi(activeCompanyData);
                    break;
                case CompanyPanelSelection.Decision:
                    decisionPanelGo.GetComponent<DecisionPanel>().UpdateUi(activeCompanyData);
                    break;
            }
            
            SetBreadcrumbText();
        }

        private void SetBreadcrumbText()
        {
            string text = _currentSelection switch
            {
                CompanyPanelSelection.Dashboard => "Dashboard",
                CompanyPanelSelection.ProductFood => "Production Food",
                CompanyPanelSelection.ProductLux => "Production Luxury",
                CompanyPanelSelection.Workers => "Workforce",
                CompanyPanelSelection.Decision => "Decisions",
                _ => "Bookkeeping"
            };
            breadcrumb.text = text;
        }
    }
}