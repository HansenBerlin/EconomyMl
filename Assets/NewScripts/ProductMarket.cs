using System;
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
        //public CompanyPayEvent payCompanyEvent;

        private readonly Random _rand = new();

        public void Awake()
        {
            //if (payCompanyEvent == null)
            //{
            //    payCompanyEvent = new CompanyPayEvent();
            //}
        }

        public float AveragePrice(ProductType type)
        {
            if (type == ProductType.None)
            {
                return 0;
            }
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
                ProductType.Food => 0.33F,
                ProductType.Intermediate => 10,
                _ => 25
            };
        }
        
        public float LowestPrice(ProductType type)
        {
            if (type == ProductType.None)
            {
                return 0;
            }
            float lowest = float.MaxValue;
            foreach (var company in ServiceLocator.Instance.Companys)
            {
                if (company.ProducedProduct.ProductTypeOutput == type)
                {
                    lowest = company.ProducedProduct.Price < lowest 
                        ? company.ProducedProduct.Price 
                        : lowest;
                }
            }

            return lowest;
        }

        public Receipt Buy(ProductType type, int demand, float maxSpending, float maxPricPerPiece = float.MaxValue)
        {
            int countFullfilled = 0;
            float amountSpent = 0;

            var matches = ServiceLocator.Instance.Companys
                .Where(x => x.ProducedProduct.ProductTypeOutput == type && x.ProducedProduct.Amount > 0)
                .OrderBy(x => x.ProducedProduct.Price);

            foreach (var company in matches)
            {
                var product = company.ProducedProduct;
                if (product.Price <= maxSpending - amountSpent)
                {
                    if (type != ProductType.Food)
                    {
                        Debug.Log("");
                    }
                    while (countFullfilled < demand && amountSpent + product.Price < maxSpending && company.ProducedProduct.Amount > 0)
                    {
                        countFullfilled++;
                        amountSpent += company.BuyFromCompany();
                        //payCompanyEvent.Invoke(offer.OfferedBy.Name);
                    }
                }
                else
                {
                    //Debug.Log($"Offer for {type} with demand {demand} and max pending {maxSpending} rejected.");
                }
            }

            if (amountSpent > maxSpending)
            {
                throw new Exception("NOT POSSIBLE");
            }

            if (countFullfilled > 0 && type != ProductType.Food)
            {
                //Debug.Log($"Offer for {type} completed. Bought: {countFullfilled}, Spent: {amountSpent}. Demand: {demand}, Max: {maxSpending}");
                
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
                if (receipt.AmountPaid > worker.Money)
                {
                    throw new Exception("OVERPAY");
                }
                if (receipt.CountBought < 10)
                {
                    worker.Health -= 2;
                    worker.Health = worker.Health <= 0 ? 0 : worker.Health;
                }
                else
                {
                    worker.Health++;
                }
                worker.Money -= receipt.AmountPaid;

                //float foodAvg = ServiceLocator.Instance.ProductMarketService.AveragePrice(ProductType.Food);
                //float maxSpending = worker.Money - foodAvg * worker.FoodDemand;
                if (true)
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