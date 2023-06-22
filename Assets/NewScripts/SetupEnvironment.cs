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
                company.Init(companiesPerType, isTraining, writeToDatabase);
                ServiceLocator.Instance.Companys.Add(company);
                xPos++;
            }

            foreach (var worker in ServiceLocator.Instance.LaborMarket.Workers)
            {
                var randomIndices = Utilitis.GenerateRandomArray(0, 
                    ServiceLocator.Instance.Companys.Count, 
                    (int)Math.Ceiling((decimal)companiesPerType / 10));
                List<Company> suppliers = new();
                foreach (int i in randomIndices)
                {
                    suppliers.Add(ServiceLocator.Instance.Companys[i]);
                }
                worker.InitialSuppliersSetup(suppliers);
            }

            _isInitDone = true;
        }
        
        private int fixedFrames { get; set; }= 0;

        private int lastday;
        public void FixedUpdate()
        {
            if (_isInitDone == false)
            {
                SetupGameObjects();
            }
            fixedFrames++;
            if (fixedFrames >= 4)
            {
                fixedFrames = 0;
                if (isThrottled == false)
                {
                    ServiceLocator.Instance.FlowController.IncrementDay();
                }
            }
            
            if (lastday == ServiceLocator.Instance.FlowController.Day)
            {
                return;
            }
            roundText.GetComponent<TextMeshProUGUI>().text = ServiceLocator.Instance.FlowController.Current();

            var workers = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.LaborMarket.Workers);
            var companies = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.Companys);
            if (ServiceLocator.Instance.FlowController.Day == 20)
            {
                foreach (var company in companies)
                {
                    company.EndMonth();
                }
                foreach (var worker in workers)
                {
                    worker.SetReservationWage();
                }
            }
            else if (ServiceLocator.Instance.FlowController.Day == 2)
            {
                foreach (var worker in workers)
                {
                    ServiceLocator.Instance.Companys = ServiceLocator.Instance.Companys.OrderBy(x => x.ProductPrice).ToList();
                    worker.SearchNewSupplier();
                    
                    ServiceLocator.Instance.Companys = ServiceLocator.Instance.Companys.OrderByDescending(x => x.WorkersCount).ToList();
                    worker.SearchJob();
                    worker.SetDailySpending();
                }
            }
            foreach (var company in companies.Where(x => x.IsBlocked == false))
            {
                company.StartDay();
            }
            foreach (var worker in workers)
            {
                worker.Buy();
            }
            ServiceLocator.Instance.Stats.UpdateStats();
            lastday = ServiceLocator.Instance.FlowController.Day;
        }

        public void RequestStep()
        {
            if (_currentActionStep == 0)
            {
                //StartCoroutine(StartMonthStep());
                ServiceLocator.Instance.FlowController.IncrementDay();
                _currentActionStep++;
            }
            else if (_currentActionStep == 1)
            {
                ServiceLocator.Instance.FlowController.IncrementDay();
                if (ServiceLocator.Instance.FlowController.Day == 19)
                {
                    _currentActionStep++;
                }
            }
            else if (_currentActionStep == 2)
            {
                ServiceLocator.Instance.FlowController.IncrementDay();
                _currentActionStep = 0;
            }

            //_currentActionStep = _currentActionStep == 2 ? 0 : _currentActionStep + 1;
            string buttonCaption = _currentActionStep == 1 ? "Tagesphase starten" : _currentActionStep == 2 ? "Monat beenden" : "Neuer Monat";
            buttonText.GetComponent<TextMeshProUGUI>().text = buttonCaption;
        }
    }
}