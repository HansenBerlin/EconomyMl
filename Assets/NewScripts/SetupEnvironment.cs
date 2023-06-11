using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace NewScripts
{
    public class SetupEnvironment : MonoBehaviour
    {
        public GameObject CompanyPrefab;
        public bool IsManual;

        //private ProductMarket _productMarket;
        private readonly Random _rand = new();

        private Company GetFromGameObject(float xPos)
        {
            var go = Instantiate(CompanyPrefab);
            go.transform.position = new Vector3(xPos, 0, 0);
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
            for (var i = 0; i < 1; i++)
            {
                Company company = GetFromGameObject(-15);
                
                var product = new Product
                {
                    ProductTypeInput = ProductType.None,
                    ProductTypeOutput = ProductType.Food,
                    Price = 1
                };
                company.Init(product);
                company.Capital = 100000;
                ServiceLocator.Instance.Companys.Add(company);
            }
            
            for (var i = 0; i < 1; i++)
            {
                Company company = GetFromGameObject(0);
                var product = new Product
                {
                    ProductTypeInput = ProductType.None,
                    ProductTypeOutput = ProductType.Intermediate,
                    Price = 5
                };
                company.Init(product);
                company.Capital = 100000;
                ServiceLocator.Instance.Companys.Add(company);
            }
            
            for (var i = 0; i < 1; i++)
            {
                Company company = GetFromGameObject(15);
                var product = new Product
                {
                    ProductTypeInput = ProductType.Intermediate,
                    ProductTypeOutput = ProductType.Luxury,
                    Price = 25
                };
                company.Init(product);
                company.Capital = 500000;
                ServiceLocator.Instance.Companys.Add(company);
            }
        }
        
        public void Update()
        {
            if (IsManual == false)
            {
                StartCoroutine(UpdateBusinesses());
            }
        }

        public void RequestStep()
        {
            if (IsManual)
            {
                StartCoroutine(UpdateBusinesses());
            }
        }

        private IEnumerator UpdateBusinesses()
        {
            var companies = GenerateRandomLoop(ServiceLocator.Instance.Companys);
            bool isfirst = true;
            foreach (var company in companies)
            {
                if (isfirst)
                {
                    //Debug.Log(company.Id);
                    isfirst = false;
                }
                company.RequestNextStep();
            }
            ServiceLocator.Instance.ProductMarketService.SimulateDemand();
            yield return new WaitForFixedUpdate();
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