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
        public int companysPerType = 3;
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
            ProductTemplateFactory.CompanysPerType = companysPerType;
            for (var i = 0; i < companysPerType; i++)
            {
                var go = Instantiate(foodCompanyPrefab);
                float zPos = i == 0 ? 0 : i == 2 ? GridGap : GridGap * -1;
                Company company = GetFromGameObject(GridGap * -1, zPos, go);
                var productTemplate = ProductTemplateFactory.Create(ProductType.Food); 
                company.Init(productTemplate);
                ServiceLocator.Instance.Companys.Add(company);
            }
            
            for (var i = 0; i < companysPerType; i++)
            {
                var go = Instantiate(interCompanyPrefab);
                float zPos = i == 0 ? 0 : i == 2 ? GridGap : GridGap * -1;
                Company company = GetFromGameObject(0, zPos, go);
                var productTemplate = ProductTemplateFactory.Create(ProductType.Intermediate);
                company.Init(productTemplate);
                ServiceLocator.Instance.Companys.Add(company);
            }
            
            for (var i = 0; i < companysPerType; i++)
            {
                var go = Instantiate(luxuryCompanyPrefab);
                float zPos = i == 0 ? 0 : i == 2 ? GridGap : GridGap * -1;
                Company company = GetFromGameObject(GridGap, zPos, go);
                var productTemplate = ProductTemplateFactory.Create(ProductType.Luxury);
                company.Init(productTemplate);
                ServiceLocator.Instance.Companys.Add(company);
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

        private IEnumerator RunAiSequence()
        {
            roundText.GetComponent<TextMeshProUGUI>().text = ServiceLocator.Instance.FlowController.Current();

            if (ServiceLocator.Instance.FlowController.Year == 10)
            {
                foreach (var company in ServiceLocator.Instance.Companys)
                {
                    company.EndCompanyEpisode();
                }
                
                ServiceLocator.Instance.FlowController.Reset();
                foreach (var worker in ServiceLocator.Instance.LaborMarketService.Workers)
                {
                    float money = worker.IsCeo ? 2000 : 800;
                    worker.Money = money;
                    worker.Health = 1000;
                }
                            
                yield return new WaitForFixedUpdate();
            }

            var companies = GenerateRandomLoop(ServiceLocator.Instance.Companys);
            foreach (var company in companies)
            {
                company.RequestNextStep();
            }
            
            if (ServiceLocator.Instance.FlowController.Day == 29)
            {
                foreach (var company in ServiceLocator.Instance.Companys)
                {
                    company.PayWorkers();
                }
            }
            
            ServiceLocator.Instance.LaborMarketService.PaySocialWelfare(
                ServiceLocator.Instance.ProductMarketService.AveragePrice(ProductType.Food) * 10, 100);
            ServiceLocator.Instance.ProductMarketService.SimulateDemand();
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                company.EndRound();
            }

            ServiceLocator.Instance.LaborMarketService.NewRound();
            ServiceLocator.Instance.FlowController.Increment();
            yield return new WaitForFixedUpdate();

        }


        public void RequestStep()
        {
            StartCoroutine(RunAiSequence());
        }

        private IEnumerator RunCompanies()
        {
            var companies = GenerateRandomLoop(ServiceLocator.Instance.Companys);
            foreach (var company in companies)
            {
                company.RequestNextStep();
            }
            yield return new WaitForFixedUpdate();
        }
        
        private IEnumerator EndEpisode()
        {
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                company.EndCompanyEpisode();
            }
            yield return new WaitForFixedUpdate();
        }

        private void SimulateDemand()
        {
            ServiceLocator.Instance.ProductMarketService.SimulateDemand();
            ServiceLocator.Instance.LaborMarketService.NewRound();
        }

        private void BookKeeping()
        {
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                company.EndRound();
            }
            //ServiceLocator.Instance.LaborMarketService.NewRound();
        }
        
        private void PayWorkers()
        {
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                company.PayWorkers();
            }
            ServiceLocator.Instance.LaborMarketService.NewRound();
        }

        public List<Company> GenerateRandomLoop(List<Company> listToShuffle)
        {
            for (int i = listToShuffle.Count - 1; i > 0; i--)
            {
                var k = _rand.Next(i + 1);
                (listToShuffle[k], listToShuffle[i]) = (listToShuffle[i], listToShuffle[k]);
            }
            return listToShuffle;
        }
    }
}