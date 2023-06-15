using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
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
        public int companysPerType = 100;
        public bool isThrottled;
        public TextMeshProUGUI roundText;
        private readonly Random _rand = new();
        private int _step = 0;
        private const int GridGap = 30;
        private bool _isInitDone;

        

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
            ServiceLocator.Instance.LaborMarketService.InitWorkers();
            //ProductTemplateFactory.CompanysPerType = companysPerType;
            int zPos = 0;
            int xPos = 0;
            for (var i = 0; i < companysPerType; i++)
            {
                if (i != 0 && i % 10 == 0)
                {
                    zPos++;
                    xPos = 0;
                }
                var go = Instantiate(foodCompanyPrefab);
                Company company = GetFromGameObject(GridGap * xPos, GridGap * zPos * -1, go);
                company.Init();
                ServiceLocator.Instance.Companys.Add(company);
                xPos++;
            }

            foreach (var worker in ServiceLocator.Instance.LaborMarketService.Workers)
            {
                var randomIndices = Utilitis.GenerateRandomArray(0, ServiceLocator.Instance.Companys.Count, 7);
                List<Company> suppliers = new();
                foreach (int i in randomIndices)
                {
                    suppliers.Add(ServiceLocator.Instance.Companys[i]);
                }
                worker.InitialSuppliersSetup(suppliers);
            }

            _isInitDone = true;
        }
        
        public void FixedUpdate()
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

        private IEnumerator RunAiSequence()
        {
            roundText.GetComponent<TextMeshProUGUI>().text = ServiceLocator.Instance.FlowController.Current();

            var companies = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.Companys);
            foreach (var company in companies)
            {
                company.StartMonth();
            }

            var workers = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.LaborMarketService.Workers);
            foreach (var worker in workers)
            {
                worker.SearchNewSupplier();
                worker.SearchJob();
                worker.SetDailySpending();
            }

            while (ServiceLocator.Instance.FlowController.Day != 21)
            {
                companies = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.Companys);
                foreach (var company in companies)
                {
                    company.StartDay();
                }

                workers = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.LaborMarketService.Workers);
                foreach (var worker in workers)
                {
                    worker.Buy();
                }
                ServiceLocator.Instance.FlowController.Increment();
            }
            
            companies = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.Companys);
            foreach (var company in companies)
            {
                company.EndMonth();
            }

            workers = Utilitis.GenerateRandomLoop(ServiceLocator.Instance.LaborMarketService.Workers);
            foreach (var worker in workers)
            {
                worker.SetReservationWage();
            }

            ServiceLocator.Instance.FlowController.Increment();
            //ServiceLocator.Instance.LaborMarketService.NewRound();
            ServiceLocator.Instance.Stats.UpdateStats();
            yield return new WaitForFixedUpdate();

        }


        public void RequestStep()
        {
            StartCoroutine(RunAiSequence());
        }
    }
}