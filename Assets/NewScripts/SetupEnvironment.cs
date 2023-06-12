using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace NewScripts
{
    public class SetupEnvironment : MonoBehaviour
    {
        public GameObject CompanyPrefab;
        public bool IsManual;
        public bool IsThrottled;
        public TextMeshProUGUI RoundText;
        private readonly Random _rand = new();
        private int companysPerType = 1;

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

        
        public void Awake()
        {
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
        
        public void Update()
        {
            if (IsManual == false && IsThrottled == false)
            {
                StartCoroutine(UpdateBusinesses());
                //RunRound();
            }
        }

        public void RequestStep()
        {
            if (IsManual && IsThrottled == false)
            {
                //StartCoroutine(UpdateBusinesses());
                RunRound();
            }
            else if (IsManual == false && IsThrottled)
            {
                StartCoroutine(UpdateBusinesses());
            }
        }

        private int round = 0;

        private IEnumerator UpdateBusinesses()
        {
            RunRound();
            yield return new WaitForFixedUpdate();
        }

        private void RunRound()
        {
            round++;
            RoundText.GetComponent<TextMeshProUGUI>().text = round.ToString();
            var companies = GenerateRandomLoop(ServiceLocator.Instance.Companys);
            foreach (var company in companies)
            {
                if (IsManual)
                {
                    company.RunManual();
                }
                else if (company.HasPenalty == false)
                {
                    //company.ExtinctPenaltyRounds--;
                    company.RequestNextStep();
                }
            }
            
            ServiceLocator.Instance.ProductMarketService.SimulateDemand();
            
            foreach (var company in companies)
            {
                if (company.HasPenalty == false)
                {
                    company.EndRound();
                }
                else
                {
                    company.DecreasePenalty();
                }
            }
            //ServiceLocator.Instance.LaborMarketService.RemoveSick();
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