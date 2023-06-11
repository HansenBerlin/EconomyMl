using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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
            //Debug.Log("Worker Money before: " + _laborMarket.WorkerAccumulatedIncome);
            int workers = ServiceLocator.Instance.LaborMarketService.Workers;
            var receipt = Buy(ProductType.Food, workers * 10, ServiceLocator.Instance.LaborMarketService.WorkerAccumulatedIncome);
            ServiceLocator.Instance.LaborMarketService.Decrease(receipt.AmountPaid);
            receipt = Buy(ProductType.Luxury, workers, ServiceLocator.Instance.LaborMarketService.WorkerAccumulatedIncome);
            ServiceLocator.Instance.LaborMarketService.Decrease(receipt.AmountPaid);
            //Debug.Log("Worker Money after: " + _laborMarket.WorkerAccumulatedIncome);
        }
    }
}