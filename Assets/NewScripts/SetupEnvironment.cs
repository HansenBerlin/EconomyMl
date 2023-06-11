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
        public TextMeshProUGUI RoundText;


        //private ProductMarket _productMarket;
        private readonly Random _rand = new();

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
            for (var i = 0; i < 3; i++)
            {
                float zPos = i == 0 ? -15 : i == 2 ? 15 : 0;
                Company company = GetFromGameObject(-15, zPos);
                
                var product = new Product
                {
                    ProductTypeInput = ProductType.None,
                    ProductTypeOutput = ProductType.Food,
                    Price = 1
                };
                company.Init(product);
                company.Capital = 30000;
                ServiceLocator.Instance.Companys.Add(company);
            }
            
            for (var i = 0; i < 3; i++)
            {
                float zPos = i == 0 ? -15 : i == 2 ? 15 : 0;
                Company company = GetFromGameObject(0, zPos);
                var product = new Product
                {
                    ProductTypeInput = ProductType.None,
                    ProductTypeOutput = ProductType.Intermediate,
                    Price = 2.5F
                };
                company.Init(product);
                company.Capital = 40000;
                ServiceLocator.Instance.Companys.Add(company);
            }
            
            for (var i = 0; i < 3; i++)
            {
                float zPos = i == 0 ? -15 : i == 2 ? 15 : 0;
                Company company = GetFromGameObject(15, zPos);
                var product = new Product
                {
                    ProductTypeInput = ProductType.Intermediate,
                    ProductTypeOutput = ProductType.Luxury,
                    Price = 25
                };
                company.Init(product);
                company.Capital = 80000;
                ServiceLocator.Instance.Companys.Add(company);
            }
        }
        
        public void Update()
        {
            if (IsManual == false)
            {
                //StartCoroutine(UpdateBusinesses());
                RunRound();
            }
        }

        public void RequestStep()
        {
            if (IsManual)
            {
                //StartCoroutine(UpdateBusinesses());
                RunRound();
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
                else
                {
                    if (company.ExtinctPenaltyRounds > 0)
                    {
                        company.ExtinctPenaltyRounds--;
                    }
                    else
                    {
                        company.RequestNextStep();
                    }
                }
            }
            ServiceLocator.Instance.ProductMarketService.SimulateDemand();
            foreach (var company in companies)
            {
                if (company.ExtinctPenaltyRounds == 0)
                {
                    company.EndRound();
                }
            }
            ServiceLocator.Instance.LaborMarketService.RemoveSick();
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