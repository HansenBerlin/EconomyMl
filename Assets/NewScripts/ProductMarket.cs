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
        public List<Company> Companys;
        public List<ProductOffer> Offers;
        public GameObject LaborMarketGameObject;
        private LaborMarket _laborMarket;
        public CompanyPayEvent payCompanyEvent;

        public void Awake()
        {
            _laborMarket = LaborMarketGameObject.GetComponent<LaborMarket>();
            Offers = new List<ProductOffer>();
            Companys = new();
            if (payCompanyEvent == null)
            {
                payCompanyEvent = new CompanyPayEvent();
            }
        }

        public float AveragePrice(ProductType type)
        {
            
            var offers = Offers
                .Where(x => x.Product.ProductTypeOutput == type);
            if (offers != null && offers.ToList().Count > 0)
            {
                return offers
                    .Select(x => x.Product.Price)
                    .Average();
            }

            return type == ProductType.Food ? 1 : type == ProductType.Intermediate ? 5 : 25;


        }

        public void RemoveOffer(int guid)
        {
            for (var index = Offers.Count - 1; index >= 0; index--)
            {
                var offer = Offers[index];
                if (offer.OfferedBy.Id == guid)
                {
                    Offers.Remove(offer);
                    //offer.Product.Amount = newOffer.Product.Amount;
                }
            }
        }

        public void AddOffer(ProductOffer newOffer)
        {
            if (Offers.ToList().Count == 0)
            {
                Offers.Add(newOffer);
                return;
            }

            foreach (var offer in Offers)
            {
                if (offer.OfferedBy.Id == newOffer.OfferedBy.Id)
                {
                    offer.Product.Price = newOffer.Product.Price;
                    offer.Product.Amount = newOffer.Product.Amount;
                }
            }
            
        }

        public Receipt Buy(ProductType type, int demand, float maxSpending)
        {
            var matchingOffers = Offers
                .Where(x => x.Product.ProductTypeOutput == type)
                .OrderBy(x => x.Product.Price);

            int countFullfilled = 0;
            float amountSpent = 0;

            foreach (var offer in matchingOffers)
            {
                if (offer.Product.Price <= maxSpending - amountSpent)
                {
                    var company = Companys.FirstOrDefault(x => x.Id == offer.OfferedBy.Id);
                    while (countFullfilled < demand && amountSpent < maxSpending && offer.Product.Amount > 0)
                    {
                        countFullfilled++;
                        amountSpent += offer.Buy();
                        company.MakeSale();
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
            int workers = _laborMarket.Workers;
            var receipt = Buy(ProductType.Food, workers * 10, _laborMarket.WorkerAccumulatedIncome);
            _laborMarket.Decrease(receipt.AmountPaid);
            receipt = Buy(ProductType.Luxury, workers, _laborMarket.WorkerAccumulatedIncome);
            _laborMarket.Decrease(receipt.AmountPaid);
            //Debug.Log("Worker Money after: " + _laborMarket.WorkerAccumulatedIncome);
        }
    }
}