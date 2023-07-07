using System.Collections;
using System.Linq;
using NewScripts.Enums;
using NewScripts.Game.Entities;
using NewScripts.Game.Services;
using NewScripts.Interfaces;
using NewScripts.Ui.Controller;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewScripts.Game.World
{
    public class EnvironmentSetup : MonoBehaviour
    {
        [FormerlySerializedAs("companyPrefabAi")] [FormerlySerializedAs("foodCompanyPrefab")] public GameObject companyPrefabAiPpo;
        public GameObject companyPrefabAiSac;
        [FormerlySerializedAs("foodCompanyPrefabPlayer")] public GameObject companyPrefabPlayer;
        public GameObject dummyTilePrefab;
        public GameObject companyPanelGo;
        public GameObject governmentGo;
        //public TextMeshProUGUI roundText;
        //public TextMeshProUGUI buttonText;
        [FormerlySerializedAs("aiCompaniesPerType")] public int aiPpoCompaniesPerType = 10;
        public int aiSacCompaniesPerType = 10;
        public int playerCompaniesPerType = 1;
        public int startingCapitalCompanies;
        public int startingCapitalPlayers;
        private const int GridGap = 40;
        public bool isTraining;
        public bool writeToDatabase;
        private bool _isInitDone;
        private Government _government;

        private void Awake()
        {
            if (ServiceLocator.Instance is not null && _isInitDone == false)
            {
                SetupGameObjects();
            }
        }
        
        private ICompany GetFromGameObject(float xPos, float zPos, GameObject instance, bool isAi, bool ignoreTraining = false)
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
                    if (isAi && ignoreTraining == false)
                    {
                        var agent = transform.GetComponent<BehaviorParameters>();
                        agent.BehaviorType = isTraining ? BehaviorType.Default : BehaviorType.InferenceOnly;
                    }

                    if (isAi && ignoreTraining)
                    {
                        var agent = transform.GetComponent<BehaviorParameters>();
                        agent.BehaviorType = BehaviorType.Default;
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
            //ServiceLocator.Instance.CompanyContainerPanelController = companyPanelGo.GetComponent<CompanyContainerPanelController>();
            var priceBidCalculator = new BidCalculatorService();
            ServiceLocator.Instance.LaborMarket.InitWorkers(1000, priceBidCalculator);
            int zPos = 0;
            int xPos = 0;
            decimal liquidity = 300000 / (decimal) (aiPpoCompaniesPerType + playerCompaniesPerType);
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
                else if (i < aiPpoCompaniesPerType + playerCompaniesPerType)
                {
                    var go = Instantiate(companyPrefabAiPpo);
                    ICompany company = GetFromGameObject(GridGap * xPos, GridGap * zPos * -1, go, true, true);
                    company.Liquidity = liquidity;
                    ServiceLocator.Instance.Companys.Add(company);
                }
                else if (i < aiSacCompaniesPerType + aiPpoCompaniesPerType + playerCompaniesPerType)
                {
                    var go = Instantiate(companyPrefabAiSac);
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
            
            var governmentInstance = Instantiate(governmentGo);
            _government = governmentInstance.GetComponent<Government>();
            ServiceLocator.Instance.AddInstances(_government, companyPanelGo.GetComponent<CompanyContainerPanelController>());
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

            if (ServiceLocator.Instance.FlowController.ProceedWithAutonomous())
            {
                StartCoroutine(Run());
            }
            else if (ServiceLocator.Instance.FlowController.IsGovernmentDecisionCommitted)
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
                
            foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
            {
                worker.AddProductBids(averageFoodPrice, ProductType.Food, ServiceLocator.Instance.FoodProductMarket);
            }
            
            ServiceLocator.Instance.FoodProductMarket.ResolveMarket(isTraining);
            
            _government.AddFoodBids();
            ServiceLocator.Instance.FoodProductMarket.ResolveMarket(isTraining, true);
            _government.DistributeFood();
            
            decimal averageLuxuryPrice = ServiceLocator.Instance.LuxuryProductMarket.AveragePriceInLastYear();
            foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
            {
                worker.AddProductBids(averageLuxuryPrice, ProductType.Luxury, ServiceLocator.Instance.LuxuryProductMarket);
            }
            
            ServiceLocator.Instance.LuxuryProductMarket.ResolveMarket(isTraining);
            
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                company.EndMonth();
            }
            
            _government.PayOutSocialFare();
            _government.PayOutSubsidy();
            
            
            foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
            {
                worker.EndMonth();
            }
            
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                company.AddRewards(ServiceLocator.Instance.FlowController.Year);
            }
            
            _government.EndMonth();
            if (ServiceLocator.Instance.FlowController.Month == 12)
            {
                _government.EndYear();
            }
            
            ServiceLocator.Instance.FlowController.IncrementMonth();
            yield return new WaitForFixedUpdate();
        }
    }
}