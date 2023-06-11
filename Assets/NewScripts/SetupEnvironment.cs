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
        public GameObject ProductMarketGameObject;

        private List<Company> _companies = new();
        private ProductMarket _productMarket;
        private readonly Random _rand = new();

        
        public void Awake()
        {
            _productMarket = ProductMarketGameObject.GetComponent<ProductMarket>();
            
            for (var i = 0; i < 1; i++)
            {
                var go = Instantiate(CompanyPrefab);
                var company = go.GetComponent<Company>();
                var product = new Product
                {
                    ProductTypeInput = ProductType.None,
                    ProductTypeOutput = ProductType.Food,
                    Price = 1
                };
                company.Init(product);
                company.Capital = 1000000;
                _companies.Add(company);
            }
            
            for (var i = 0; i < 1; i++)
            {
                var go = Instantiate(CompanyPrefab);
                var company = go.GetComponent<Company>();
                var product = new Product
                {
                    ProductTypeInput = ProductType.None,
                    ProductTypeOutput = ProductType.Intermediate,
                    Price = 5
                };
                company.Init(product);
                company.Capital = 1000000;
                _companies.Add(company);
            }
            
            for (var i = 0; i < 1; i++)
            {
                var go = Instantiate(CompanyPrefab);
                var company = go.GetComponent<Company>();
                var product = new Product
                {
                    ProductTypeInput = ProductType.Intermediate,
                    ProductTypeOutput = ProductType.Luxury,
                    Price = 25
                };
                company.Init(product);
                company.Capital = 5000000;
                _companies.Add(company);
            }
        }
        
        public void Update()
        {
            StartCoroutine(UpdateBusinesses());
        }

        private IEnumerator UpdateBusinesses()
        {
            _companies = GenerateRandomLoop(_companies);
            bool isfirst = true;
            foreach (var company in _companies)
            {
                if (isfirst)
                {
                    //Debug.Log(company.Id);
                    isfirst = false;
                }
                company.RequestNextStep();
            }
            _productMarket.SimulateDemand();
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