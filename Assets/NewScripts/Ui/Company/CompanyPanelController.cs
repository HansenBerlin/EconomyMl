using System.Collections.Generic;
using NewScripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui.Company
{
    public class CompanyPanelController : MonoBehaviour
    {
        public GameObject dashboardButtonGo;
        public GameObject productButtonGo;
        public GameObject workersButtonGo;
        public GameObject booksButtonGo;
        public GameObject decisionButtonGo;
        
        public GameObject dashboardPanelGo;
        public GameObject productPanelGo;
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
        private List<CompanyData> _activeCompanyData = new();
        private CompanyPanelSelection _currentSelection;

        private void Awake()
        {
            dashboardButtonGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                _currentSelection = CompanyPanelSelection.Dashboard; 
                UpdatePanelData(); 
            });
            productButtonGo.GetComponent<Button>().onClick.AddListener(() =>
            {
                _currentSelection = CompanyPanelSelection.Product;
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
            ServiceLocator.Instance.UiUpdateManager.companySelectedEvent.AddListener((x) =>
            {
                _activeCompanyId = x.Id;
                _activeCompanyData = x.Ledger;
                UpdatePanelData();
            });
        }

        private void UpdatePanelData()
        {
            GameObject[] panels = { dashboardPanelGo, productPanelGo, workersPanelGo, booksPanelGo, decisionPanelGo };
            for (var i = 0; i < panels.Length; i++)
            {
                panels[i].SetActive(i == (int) _currentSelection);
            }

            switch (_currentSelection)
            {
                case CompanyPanelSelection.Dashboard:
                    dashboardPanelGo.GetComponent<DashboardPanel>().UpdateUi(_activeCompanyData);
                    break;
                case CompanyPanelSelection.Product:
                    productPanelGo.GetComponent<ProductPanel>().UpdateUi(_activeCompanyData);
                    break;
                case CompanyPanelSelection.Workers:
                    workersPanelGo.GetComponent<WorkersPanel>().UpdateUi(_activeCompanyData);
                    break;
                case CompanyPanelSelection.Books:
                    booksPanelGo.GetComponent<BooksPanel>().UpdateUi(_activeCompanyData);
                    break;
                case CompanyPanelSelection.Decision:
                    decisionPanelGo.GetComponent<DecisionPanel>().UpdateUi(_activeCompanyData);
                    break;
            }
            
            SetBreadcrumbText();
        }

        private void SetBreadcrumbText()
        {
            string text = _currentSelection switch
            {
                CompanyPanelSelection.Dashboard => "Dashboard",
                CompanyPanelSelection.Product => "Production",
                CompanyPanelSelection.Workers => "Workforce",
                CompanyPanelSelection.Decision => "Decisions",
                _ => "Bookkeeping"
            };
            breadcrumb.text = text;
        }
    }
}