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
        
        public List<CompanyData> ActiveCompanyData
        {
            get => _activeCompanyData;
            set
            {
                _activeCompanyData = value;
                ActiveCompanyId = _activeCompanyData[0].CompanyId;
                UpdatePanelData();
            } 
        }
        
        public int ActiveCompanyId { get; private set; }
        
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
                    dashboardPanelGo.GetComponent<DashboardPanel>().UpdateUi(ActiveCompanyData);
                    break;
                case CompanyPanelSelection.Product:
                    productPanelGo.GetComponent<ProductPanel>().UpdateUi(ActiveCompanyData);
                    break;
                case CompanyPanelSelection.Workers:
                    workersPanelGo.GetComponent<WorkersPanel>().UpdateUi(ActiveCompanyData);
                    break;
                case CompanyPanelSelection.Books:
                    booksPanelGo.GetComponent<BooksPanel>().UpdateUi(ActiveCompanyData);
                    break;
                case CompanyPanelSelection.Decision:
                    decisionPanelGo.GetComponent<DecisionPanel>().UpdateUi(ActiveCompanyData);
                    break;
            }
            
            SetBreadcrumbText();
        }

        private void SetBreadcrumbText()
        {
            string text = _currentSelection switch
            {
                CompanyPanelSelection.Dashboard => "Dashboard",
                CompanyPanelSelection.Product => "Produktion",
                CompanyPanelSelection.Workers => "Arbeiter",
                CompanyPanelSelection.Decision => "Entscheidungen",
                _ => "Buchhaltung"
            };
            breadcrumb.text = text;
        }
    }
}