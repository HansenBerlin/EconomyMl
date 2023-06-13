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
        public GameObject CompanyPrefab;
        public bool IsThrottled;
        public TextMeshProUGUI RoundText;
        private readonly Random _rand = new();
        private int companysPerType = 3;
        private int step = 0;
        private int month = 0;

        

        private Company GetFromGameObject(float xPos, float zPos)
        {
            var go = Instantiate(CompanyPrefab);
            go.transform.position = new Vector3(xPos, 0, zPos);
            Transform[] transforms = go.GetComponentsInChildren<Transform>();
            Company company = null;
 
            foreach (var transform in transforms)
            {
                company = transform.GetComponent<Company>();
                if (company != null)
                {
                    break;
                }
            }

            return company;
        }
        
        void EnvironmentReset()
        {
            // Reset the scene here
        }

        
        public void Awake()
        {
            Academy.Instance.OnEnvironmentReset += EnvironmentReset;
            ProductTemplateFactory.CompanysPerType = companysPerType;
            ServiceLocator.Instance.LaborMarketService.InitWorkers();
            for (var i = 0; i < companysPerType; i++)
            {
                float zPos = i == 0 ? 0 : i == 2 ? 15 : -15;
                Company company = GetFromGameObject(-15, zPos);
                var productTemplate = ProductTemplateFactory.Create(ProductType.Food); 
                company.Init(productTemplate);
                ServiceLocator.Instance.Companys.Add(company);
            }
            
            for (var i = 0; i < companysPerType; i++)
            {
                float zPos = i == 0 ? 0 : i == 2 ? 15 : -15;
                Company company = GetFromGameObject(0, zPos);
                var productTemplate = ProductTemplateFactory.Create(ProductType.Intermediate);
                company.Init(productTemplate);
                ServiceLocator.Instance.Companys.Add(company);
            }
            
            for (var i = 0; i < companysPerType; i++)
            {
                float zPos = i == 0 ? 0 : i == 2 ? 15 : -15;
                Company company = GetFromGameObject(15, zPos);
                var productTemplate = ProductTemplateFactory.Create(ProductType.Luxury);
                company.Init(productTemplate);
                ServiceLocator.Instance.Companys.Add(company);
            }
        }
        
        public void FixedUpdate()
        {
            if (IsThrottled == false)
            {
                month = month == 120 ? 0 : month + 1;
                string stepText = ((SimulationStep)step).ToString();
                RoundText.GetComponent<TextMeshProUGUI>().text = month + " | " + stepText;

                if (step == 0)
                {
                    StartCoroutine(RunCompanies());
                }
                else if (step == 1)
                {
                    SimulateDemand();
                }
                else if (step == 2)
                {
                    BookKeeping();
                }

                step = step == 2 ? 0 : step + 1;
            }
        }


        public void RequestStep()
        {
            month = month == 120 ? 0 : month + 1;
            string stepText = ((SimulationStep)step).ToString();
            RoundText.GetComponent<TextMeshProUGUI>().text = month + " | " + stepText;

            if (step == 0)
            {
                StartCoroutine(RunCompanies());
            }
            else if (step == 1)
            {
                SimulateDemand();
            }
            else if (step == 2)
            {
                BookKeeping();
            }

            step = step == 2 ? 0 : step + 1;
        }

        private IEnumerator RunCompanies()
        {
            var companies = GenerateRandomLoop(ServiceLocator.Instance.Companys);
            foreach (var company in companies)
            {
                company.RequestNextStep(month);
            }
            yield return new WaitForFixedUpdate();
        }
        
        private IEnumerator EndEpisode()
        {
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                company.EndRound();
            }
            yield return new WaitForFixedUpdate();
        }

        private void SimulateDemand()
        {
            ServiceLocator.Instance.ProductMarketService.SimulateDemand();
        }

        private void BookKeeping()
        {
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                company.EndRound();
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