using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = System.Random;

namespace NewScripts
{
    [System.Serializable]
    public class CompanyPayEvent : UnityEvent<string>
    {
    }
    
    public class ProductMarket : MonoBehaviour
    {
        //private LaborMarket _laborMarket;
        public CompanyPayEvent payCompanyEvent;

        private readonly Random _rand = new();

        public void Awake()
        {
            if (payCompanyEvent == null)
            {
                payCompanyEvent = new CompanyPayEvent();
            }
        }

        public float AveragePrice(ProductType type)
        {
            float accumulatedPrice = 0;
            float countOffers = 0;
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                if (company.ProducedProduct.ProductTypeOutput == type)
                {
                    accumulatedPrice += company.ProducedProduct.Price;
                    countOffers++;
                }
            }

            if (countOffers > 0)
            {
                return accumulatedPrice / countOffers;
            }

            return type switch
            {
                ProductType.Food => 1,
                ProductType.Intermediate => 5,
                _ => 25
            };
        }

        public Receipt Buy(ProductType type, int demand, float maxSpending)
        {
            int countFullfilled = 0;
            float amountSpent = 0;

            foreach (var company in ServiceLocator.Instance.Companys)
            {
                if (company.ProducedProduct.ProductTypeOutput != type)
                {
                    continue;
                }
                if (company.ProducedProduct.Price <= maxSpending - amountSpent)
                {
                    while (countFullfilled < demand && amountSpent < maxSpending && company.ProducedProduct.Amount > 0)
                    {
                        countFullfilled++;
                        amountSpent += company.BuyFromCompany();
                        //payCompanyEvent.Invoke(offer.OfferedBy.Name);
                    }
                }
                else
                {
                    Debug.Log($"Offer for {type} with demand {demand} and max pending {maxSpending} rejected.");
                }
            }

            return new Receipt
            {
                AmountPaid = amountSpent,
                CountBought = countFullfilled
            };
        }

        public void SimulateDemand()
        {
            var workers = GenerateRandomLoop(ServiceLocator.Instance.LaborMarketService.Workers);
            for (var index = workers.Count - 1; index >= 0; index--)
            {
                var worker = ServiceLocator.Instance.LaborMarketService.Workers[index];
                var receipt = Buy(ProductType.Food, worker.FoodDemand, worker.Money);
                if (receipt.CountBought < 10)
                {
                    worker.Health -= 10 - receipt.CountBought;
                }
                worker.Money -= receipt.AmountPaid;
                if (worker.Health < 0)
                {
                    ServiceLocator.Instance.LaborMarketService.Workers.Remove(worker);
                    workers.Remove(worker);
                }
                else
                {
                    receipt = Buy(ProductType.Luxury, worker.LuxuryDemand, worker.Money);
                    worker.Money -= receipt.AmountPaid;
                }
            }
        }
        
        public List<Worker> GenerateRandomLoop(List<Worker> listToShuffle)
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