﻿using System;
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
        public GameObject interCompanyPrefab;
        public GameObject luxuryCompanyPrefab;
        [FormerlySerializedAs("companysPerType")] public int companiesPerType = 100;
        
        public bool isThrottled;
        public bool isTraining;
        public bool writeToDatabase;
        public TextMeshProUGUI roundText;
        private readonly Random _rand = new();
        private int _step = 0;
        private const int GridGap = 30;
        private bool _isInitDone;
        public TextMeshProUGUI buttonText;

        

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

        
        public void Awake()
        {
            if (ServiceLocator.Instance is not null && _isInitDone == false)
            {
                SetupGameObjects();
            }
        }

        private void SetupGameObjects()
        {
            //Academy.Instance.OnEnvironmentReset += EnvironmentReset;
            ServiceLocator.Instance.LaborMarketService.InitWorkers(1000);
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

            foreach (var worker in ServiceLocator.Instance.LaborMarketService.Workers)
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
        
        public void Update()
        {
            if (_isInitDone == false)
            {
                SetupGameObjects();
            }
            if (isThrottled == false)
            {
                StartCoroutine(RunAiSequence());
            }
        }

        private IEnumerator StartMonthStep()
        {
            roundText.GetComponent<TextMeshProUGUI>().text = ServiceLocator.Instance.FlowController.Current();

            var companies = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.Companys);
            foreach (var company in companies)
            {
                company.StartMonth();
            }
            
            StartCoroutine (ServiceLocator.Instance.FlowController.WaitUntilStartMonthHouseholdPhase(() =>
            {
                var workers = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.LaborMarketService.Workers);
                foreach (var worker in workers)
                {
                    ServiceLocator.Instance.Companys = ServiceLocator.Instance.Companys.OrderBy(x => x.ProductPrice).ToList();
                    worker.SearchNewSupplier();
                    
                    ServiceLocator.Instance.Companys = ServiceLocator.Instance.Companys.OrderByDescending(x => x.WorkersCount).ToList();
                    worker.SearchJob();
                    worker.SetDailySpending();
                }
                ServiceLocator.Instance.Stats.UpdateStats();
            }));
            yield return new WaitForFixedUpdate();
        }

        private void StartDaysStep(List<Company> companies, List<Worker> workers)
        {
            ServiceLocator.Instance.FlowController.IncrementDay();
            while (ServiceLocator.Instance.FlowController.Day != 20)
            {
                foreach (var company in companies)
                {
                    company.StartDay();
                }

                    foreach (var worker in workers)
                    {
                        worker.Buy();
                    }
                //StartCoroutine(ServiceLocator.Instance.FlowController.WaitUntilStartDaysHouseholdPhase(() =>
                //{
                //}));
                ServiceLocator.Instance.FlowController.IncrementDay();
            }
            ServiceLocator.Instance.Stats.UpdateStats();
            //yield return new WaitForFixedUpdate();
        }

        private void EndMonthStep()
        {
            var companies = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.Companys);
            foreach (var company in companies)
            {
               company.EndMonth();
            }

            var workers = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.LaborMarketService.Workers);
            foreach (var worker in workers)
            {
                worker.SetReservationWage();
            }

            ServiceLocator.Instance.Stats.UpdateStats();
            ServiceLocator.Instance.FlowController.IncrementMonth();
            //ServiceLocator.Instance.LaborMarketService.NewRound();
        }

        private IEnumerator RunAiSequence()
        {
            StartCoroutine(StartMonthStep());
            var companies = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.Companys);
            var workers = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.LaborMarketService.Workers);
            ServiceLocator.Instance.Companys = ServiceLocator.Instance.Companys.OrderBy(x => x.ProductPrice).ToList();
            StartDaysStep(companies, workers);
            EndMonthStep();
            yield return new WaitForFixedUpdate();
        }


        public void RequestStep()
        {
            if (_step == 0)
            {
                StartCoroutine(StartMonthStep());
            }
            if (_step == 1)
            {
                var companies = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.Companys);
                var workers = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.LaborMarketService.Workers);
                ServiceLocator.Instance.Companys = ServiceLocator.Instance.Companys.OrderBy(x => x.ProductPrice).ToList();
                StartDaysStep(companies, workers);
            }
            if (_step == 2)
            {
                EndMonthStep();
            }

            _step = _step == 2 ? 0 : _step + 1;
            string buttonCaption = _step == 1 ? "Tagesphase starten" : _step == 2 ? "Monat beenden" : "Neuer Monat";
            buttonText.GetComponent<TextMeshProUGUI>().text = buttonCaption;
        }
    }
}