using System.Collections;
using System.Linq;
using NewScripts.Enums;
using NewScripts.Ui;
using NewScripts.Ui.Company;
using TMPro;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NewScripts
{
    public class SetupEnvironment : MonoBehaviour
    {
        [FormerlySerializedAs("foodCompanyPrefab")] public GameObject companyPrefabAi;
        [FormerlySerializedAs("foodCompanyPrefabPlayer")] public GameObject companyPrefabPlayer;
        public GameObject dummyTilePrefab;
        public GameObject companyPanelGo;
        //public TextMeshProUGUI roundText;
        //public TextMeshProUGUI buttonText;
        public int aiCompaniesPerType = 100;
        public int playerCompaniesPerType = 1;
        
        private const int GridGap = 40;
        public bool isTraining;
        public bool writeToDatabase;
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
            ServiceLocator.Instance.Settings.WriteToDatabase = writeToDatabase;
            ServiceLocator.Instance.CompanyPanelController = companyPanelGo.GetComponent<CompanyPanelController>();
            ServiceLocator.Instance.LaborMarket.InitWorkers(1000);
            int zPos = 0;
            int xPos = 0;
            decimal liquidity = 500000 / (decimal) (aiCompaniesPerType + playerCompaniesPerType);
            for (var i = 0; i < 100; i++)
            {
                if (i != 0 && i % 10 == 0)
                {
                    zPos++;
                    xPos = 0;
                }
                if(i < playerCompaniesPerType)
                {
                    var go = Instantiate(companyPrefabPlayer);
                    ICompany company = GetFromGameObject(GridGap * xPos, GridGap * zPos * -1, go, false);
                    company.Liquidity = liquidity;
                    ServiceLocator.Instance.Companys.Add(company);
                }
                else if (i < aiCompaniesPerType)
                {
                    var go = Instantiate(companyPrefabAi);
                    ICompany company = GetFromGameObject(GridGap * xPos, GridGap * zPos * -1, go, true);
                    company.Liquidity = liquidity;
                    ServiceLocator.Instance.Companys.Add(company);
                }
                else
                {
                    var instance = Instantiate(dummyTilePrefab);
                    instance.transform.position = new Vector3(GridGap * xPos, 0, GridGap * zPos * -1);
                }
                xPos++;
            }
            
            ServiceLocator.Instance.InitFlowController();
            _isInitDone = true;
        }
        
        public void Update()
        {
            if (_isInitDone == false)
            {
                SetupGameObjects();
            }

            if (ServiceLocator.Instance.Settings.IsPaused)
            {
                return;
            }

            if (ServiceLocator.Instance.FlowController.Proceed())
            {
                StartCoroutine(Run());
            }
            else
            {
                foreach (var company in ServiceLocator.Instance.Companys
                             .Where(x => x.DecisionStatus == CompanyDecisionStatus.Pending))
                {
                    company.RequestMonthlyDecision();
                }
            }
        }

        private IEnumerator Run()
        {
            decimal averageIncome = ServiceLocator.Instance.LaborMarket.AveragePayment();
            decimal averageFoodPrice = ServiceLocator.Instance.FoodProductMarket.AveragePriceInLastYear();
            foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
            {
                worker.SearchForJob(averageIncome, averageFoodPrice);
            }
                
            ServiceLocator.Instance.LaborMarket.ResolveMarket();
                
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                company.Produce();
            }
                
            decimal averageLuxuryPrice = ServiceLocator.Instance.LuxuryProductMarket.AveragePriceInLastYear();
            foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
            {
                worker.AddProductBids(averageFoodPrice, ProductType.Food, ServiceLocator.Instance.FoodProductMarket);
                worker.AddProductBids(averageLuxuryPrice, ProductType.Luxury, ServiceLocator.Instance.LuxuryProductMarket);
            }
            
            ServiceLocator.Instance.FoodProductMarket.ResolveMarket(isTraining);
            ServiceLocator.Instance.LuxuryProductMarket.ResolveMarket(isTraining);
            
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                company.EndMonth();
            }
            
            foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
            {
                worker.EndMonth();
            }
            
            ServiceLocator.Instance.FlowController.IncrementMonth();
            yield return new WaitForFixedUpdate();
        }
    }
}