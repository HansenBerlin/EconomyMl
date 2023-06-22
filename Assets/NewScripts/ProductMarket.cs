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
        private List<double> _salesHistory = new();
        //private LaborMarket _laborMarket;
        //public CompanyPayEvent payCompanyEvent;
        
        public List<ProductOffer> ProductOffers { get; } = new();
        public List<ProductBid> ProductBids { get; } = new();
        public int DemandForProduct { get; private set; }

        private readonly Random _rand = new();

        public double AveragePrice()
        {
            double average = _salesHistory.Average();
            return average == 0 ? 1 : average;
        }

        public void AddBids(List<ProductBid> bids)
        {
            ProductBids.AddRange(bids);
        }
        
        public void AddOffers(List<ProductOffer> offers)
        {
            ProductOffers.AddRange(offers);
        }
        
        public void ResolveMarket()
        {
            var offers = ProductOffers.OrderBy(x => x.Price).ToList();
            var bids = ProductBids.OrderByDescending(x => x.Price).ToList();

            while (offers.Count > 0 && bids.Count > 0)
            {
                var offer = offers[0];
                var bid = bids[0];
                if (offer.Price < bid.Price)
                {
                    if (offer.Amount < bid.Amount)
                    {
                        bid.Amount -= offer.Amount;
                        bid.Buyer.FullfillBid(offer.Product.ProductTypeOutput, offer.Amount, offer.Price);
                        offers.Remove(offer);
                    }
                    else if (offer.Amount > bid.Amount)
                    {
                        offer.Amount -= bid.Amount;
                        bid.Buyer.FullfillBid(offer.Product.ProductTypeOutput, bid.Amount, offer.Price);
                        bids.Remove(bid);
                    }
                    else
                    {
                        bid.Buyer.FullfillBid(offer.Product.ProductTypeOutput, bid.Amount, offer.Price);
                        offers.Remove(offer);
                        bids.Remove(bid);
                    }
                }
                else
                {
                    break;
                }
            }

            DemandForProduct = bids.Count - offers.Count;
            ProductOffers.Clear();
            ProductBids.Clear();
        }

        public void Awake()
        {
            //if (payCompanyEvent == null)
            //{
            //    payCompanyEvent = new CompanyPayEvent();
            //}
        }

        
    }
}