using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NewScripts.Http;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace NewScripts
{
    public class SetupEnvironment : MonoBehaviour
    {
        public GameObject foodCompanyPrefab;
        public int companiesPerType = 100;
        
        public bool isThrottled;
        public bool isTraining;
        public bool writeToDatabase;
        public TextMeshProUGUI roundText;
        private int _currentActionStep = 0;
        private const int GridGap = 30;
        private bool _isInitDone;
        public TextMeshProUGUI buttonText;

        private void Awake()
        {
            if (ServiceLocator.Instance is not null && _isInitDone == false)
            {

                ServiceLocator.Instance.Settings.IsTraining = isTraining;
                ServiceLocator.Instance.Settings.IsThrottled = isThrottled;
                ServiceLocator.Instance.Settings.WriteToDatabase = writeToDatabase;
                SetupGameObjects();
            }
        }
        

        private Company GetFromGameObject(float xPos, float zPos, GameObject instance)
        {
            //var go = Instantiate(FoodCompanyPrefab);
            instance.transform.position = new Vector3(xPos, 0, zPos);
            Transform[] transforms = instance.GetComponentsInChildren<Transform>();
            Company company = null;
 
            foreach (var transform in transforms)
            {
                company = transform.GetComponent<Company>();
                if (company is not null)
                {
                    var agent = transform.GetComponent<BehaviorParameters>();
                    agent.BehaviorType = isTraining ? BehaviorType.Default : BehaviorType.InferenceOnly;
                    break;
                }
            }

            return company;
        }
        
        //void EnvironmentReset()
        //{
        //    foreach (var worker in ServiceLocator.Instance.LaborMarketService.Workers)
        //    {
        //        float money = worker.IsCeo ? 300 : 30;
        //        worker.Money = money;
        //        worker.Health = 1000;
        //    }
        //}

        private void SetupGameObjects()
        {
            //Academy.Instance.OnEnvironmentReset += EnvironmentReset;
            ServiceLocator.Instance.LaborMarket.InitWorkers(1000);
            //ProductTemplateFactory.CompanysPerType = companysPerType;
            int zPos = 0;
            int xPos = 0;
            for (var i = 0; i < companiesPerType; i++)
            {
                if (i != 0 && i % 10 == 0)
                {
                    zPos++;
                    xPos = 0;
                }
                var go = Instantiate(foodCompanyPrefab);
                Company company = GetFromGameObject(GridGap * xPos, GridGap * zPos * -1, go);
                company.Liquidity = 70000 / (decimal)companiesPerType;
                ServiceLocator.Instance.Companys.Add(company);
                xPos++;
            }

            _isInitDone = true;
        }

        private bool DecisionRequested;
        
        public void Update()
        {
            if (_isInitDone == false)
            {
                SetupGameObjects();
            }

            if (isThrottled)
            {
                return;
            }

            if (ServiceLocator.Instance.FlowController.Proceed())
            {
                StartCoroutine(Run());
            }
            else if (DecisionRequested == false)
            {
                DecisionRequested = true;
                foreach (var company in ServiceLocator.Instance.Companys)
                {
                    company.RequestDecision();
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
                worker.AddProductBids();
            }
                
            ServiceLocator.Instance.ProductMarket.ResolveMarket();
            
            foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
            {
                worker.EndMonth();
            }
                
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                company.EndMonth();
            }
                
            ServiceLocator.Instance.FlowController.IncrementMonth();
            ServiceLocator.Instance.Stats.UpdateStats();
            roundText.GetComponent<TextMeshProUGUI>().text = ServiceLocator.Instance.FlowController.Current();
            DecisionRequested = false;
            yield return new WaitForFixedUpdate();
        }

        public void RequestStep()
        {
            if (_currentActionStep == 1)
            {
                foreach (var company in ServiceLocator.Instance.Companys)
                {
                    company.RequestDecision();
                }
                ServiceLocator.Instance.Stats.UpdateStats();
            }
            else if (_currentActionStep == 2)
            {
                decimal averageIncome = ServiceLocator.Instance.LaborMarket.AveragePayment();
                decimal averageFoodPrice = ServiceLocator.Instance.ProductMarket.AveragePrice();
                foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
                {
                    worker.SearchForJob(averageIncome, averageFoodPrice);
                }
                
                ServiceLocator.Instance.LaborMarket.ResolveMarket();
                ServiceLocator.Instance.Stats.UpdateStats();
            }
            else if (_currentActionStep == 3)
            {
                foreach (var company in ServiceLocator.Instance.Companys)
                {
                    company.Produce();
                }
                
                foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
                {
                    worker.AddProductBids();
                }
                
                ServiceLocator.Instance.ProductMarket.ResolveMarket();
                ServiceLocator.Instance.Stats.UpdateStats();
            }
            else if (_currentActionStep == 4)
            {
                foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
                {
                    worker.EndMonth();
                }
                
                foreach (var company in ServiceLocator.Instance.Companys)
                {
                    company.EndMonth();
                }
                
                ServiceLocator.Instance.FlowController.IncrementMonth();
                ServiceLocator.Instance.Stats.UpdateStats();
                roundText.GetComponent<TextMeshProUGUI>().text = ServiceLocator.Instance.FlowController.Current();
            }
            

            _currentActionStep = _currentActionStep == 4 ? 1 : _currentActionStep + 1;
            string buttonCaption = _currentActionStep == 1
                ? "AI Decision"
                : _currentActionStep == 2
                    ? "Arbeitsmarkt"
                    : _currentActionStep == 3
                        ? "Gütermarkt"
                        : "Monatsende";
            buttonText.GetComponent<TextMeshProUGUI>().text = buttonCaption;
        }
    }
}