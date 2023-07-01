using System.Collections;
using System.Linq;
using NewScripts.Enums;
using NewScripts.Ui;
using NewScripts.Ui.Company;
using TMPro;
using Unity.MLAgents.Policies;
using UnityEngine;

namespace NewScripts
{
    public class SetupEnvironment : MonoBehaviour
    {
        public GameObject foodCompanyPrefab;
        public GameObject foodCompanyPrefabPlayer;
        public GameObject companyPanelGo;
        //public TextMeshProUGUI roundText;
        //public TextMeshProUGUI buttonText;
        public int aiCompaniesPerType = 100;
        public int playerCompaniesPerType = 1;
        
        private const int GridGap = 30;
        public bool isThrottled;
        public bool isTraining;
        public bool writeToDatabase;
        private int _currentActionStep = 0;
        private bool _isInitDone;

        private void Awake()
        {
            if (ServiceLocator.Instance is not null && _isInitDone == false)
            {
                SetupGameObjects();
            }
        }
        

        private ICompany GetFromGameObject(float xPos, float zPos, GameObject instance, bool isAi)
        {
            //var go = Instantiate(FoodCompanyPrefab);
            instance.transform.position = new Vector3(xPos, 0, zPos);
            Transform[] transforms = instance.GetComponentsInChildren<Transform>();
            ICompany company = null;
 
            foreach (var transform in transforms)
            {
                company = transform.GetComponent<ICompany>();
                if (company is not null)
                {
                    if (isAi)
                    {
                        var agent = transform.GetComponent<BehaviorParameters>();
                        agent.BehaviorType = isTraining ? BehaviorType.Default : BehaviorType.InferenceOnly;
                    }
                    break;
                }
            }

            return company;
        }
        
        private void SetupGameObjects()
        {
            ServiceLocator.Instance.Settings.IsTraining = isTraining;
            ServiceLocator.Instance.Settings.IsThrottled = isThrottled;
            ServiceLocator.Instance.Settings.WriteToDatabase = writeToDatabase;
            ServiceLocator.Instance.CompanyPanelController = companyPanelGo.GetComponent<CompanyPanelController>();
            
            //Academy.Instance.OnEnvironmentReset += EnvironmentReset;
            ServiceLocator.Instance.LaborMarket.InitWorkers(1000);
            //ProductTemplateFactory.CompanysPerType = companysPerType;
            int zPos = 0;
            int xPos = 0;
            decimal liquidity = 10000 / (decimal) (aiCompaniesPerType + playerCompaniesPerType);
            for (var i = 0; i < aiCompaniesPerType + playerCompaniesPerType; i++)
            {
                if (i != 0 && i % 10 == 0)
                {
                    zPos++;
                    xPos = 0;
                }
                if(i < aiCompaniesPerType)
                {
                    var go = Instantiate(foodCompanyPrefab);
                    ICompany company = GetFromGameObject(GridGap * xPos, GridGap * zPos * -1, go, true);
                    company.Liquidity = liquidity;
                    ServiceLocator.Instance.Companys.Add(company);
                }
                else
                {
                    var go = Instantiate(foodCompanyPrefabPlayer);
                    ICompany company = GetFromGameObject(GridGap * xPos, GridGap * zPos * -1, go, false);
                    company.Liquidity = liquidity;
                    ServiceLocator.Instance.Companys.Add(company);
                }
                xPos++;
            }
            
            ServiceLocator.Instance.InitFlowController();
            _isInitDone = true;
        }

        //private bool DecisionRequested;
        
        public void Update()
        {
            if (_isInitDone == false)
            {
                SetupGameObjects();
            }

            if (ServiceLocator.Instance.FlowController.Proceed())
            {
                StartCoroutine(Run());
            }
            else
            {
                foreach (var company in ServiceLocator.Instance.Companys.Where(x => x.DecisionStatus == CompanyDecisionStatus.Requested))
                {
                    company.RequestMonthlyDecision();
                }
            }
        }

        private IEnumerator Run()
        {
            decimal averageIncome = ServiceLocator.Instance.LaborMarket.AveragePayment();
            decimal averageFoodPrice = ServiceLocator.Instance.ProductMarket.AveragePrice();
            foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
            {
                worker.SearchForJob(averageIncome, averageFoodPrice);
            }
                
            ServiceLocator.Instance.LaborMarket.ResolveMarket();
                
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                company.Produce();
            }
                
            foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
            {
                worker.AddProductBids(averageFoodPrice);
            }
                
            ServiceLocator.Instance.ProductMarket.ResolveMarket(isTraining);
            
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                company.EndMonth();
            }
            
            foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
            {
                worker.EndMonth();
            }

            ServiceLocator.Instance.FlowController.IncrementMonth();
            //ServiceLocator.Instance.Stats.UpdateStats();
            //roundText.GetComponent<TextMeshProUGUI>().text = ServiceLocator.Instance.FlowController.Current();
            yield return new WaitForFixedUpdate();
        }

        //public void RequestStep()
        //{
        //    if (_currentActionStep == 1)
        //    {
        //        foreach (var company in ServiceLocator.Instance.Companys)
        //        {
        //            company.RequestMonthlyDecision();
        //        }
        //        ServiceLocator.Instance.Stats.UpdateStats();
        //    }
        //    else if (_currentActionStep == 2)
        //    {
        //        decimal averageIncome = ServiceLocator.Instance.LaborMarket.AveragePayment();
        //        decimal averageFoodPrice = ServiceLocator.Instance.ProductMarket.AveragePrice();
        //        foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
        //        {
        //            worker.SearchForJob(averageIncome, averageFoodPrice);
        //        }
        //        
        //        ServiceLocator.Instance.LaborMarket.ResolveMarket();
        //        ServiceLocator.Instance.Stats.UpdateStats();
        //    }
        //    else if (_currentActionStep == 3)
        //    {
        //        decimal averageFoodPrice = ServiceLocator.Instance.ProductMarket.AveragePrice();
//
        //        foreach (var company in ServiceLocator.Instance.Companys)
        //        {
        //            company.Produce();
        //        }
        //        
        //        foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
        //        {
        //            worker.AddProductBids(averageFoodPrice);
        //        }
        //        
        //        ServiceLocator.Instance.ProductMarket.ResolveMarket(isTraining);
        //        ServiceLocator.Instance.Stats.UpdateStats();
        //    }
        //    else if (_currentActionStep == 4)
        //    {
        //        foreach (var company in ServiceLocator.Instance.Companys)
        //        {
        //            company.EndMonth();
        //        }
        //        
        //        foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
        //        {
        //            worker.EndMonth();
        //        }
//
        //        ServiceLocator.Instance.FlowController.IncrementMonth();
        //        ServiceLocator.Instance.Stats.UpdateStats();
        //        //roundText.GetComponent<TextMeshProUGUI>().text = ServiceLocator.Instance.FlowController.Current();
        //    }
        //    
//
        //    _currentActionStep = _currentActionStep == 4 ? 1 : _currentActionStep + 1;
        //    string buttonCaption = _currentActionStep == 1
        //        ? "AI Decision"
        //        : _currentActionStep == 2
        //            ? "Arbeitsmarkt"
        //            : _currentActionStep == 3
        //                ? "Gütermarkt"
        //                : "Monatsende";
        //    //buttonText.GetComponent<TextMeshProUGUI>().text = buttonCaption;
        //}
    }
}