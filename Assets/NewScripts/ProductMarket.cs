﻿using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.VisualScripting;
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
        private readonly List<decimal> _salesHistory = new();
        //private LaborMarket _laborMarket;
        //public CompanyPayEvent payCompanyEvent;

        public ProductMarketUpdateEvent updateEvent;

        private List<ProductOffer> ProductOffers { get; } = new();
        private List<ProductBid> ProductBids { get; } = new();
        public int DemandForProduct { get; private set; }

        private readonly Random _rand = new();
        private int CountAdded { get; set; }

        private void Awake()
        {
            updateEvent ??= new ProductMarketUpdateEvent();
        }

        public decimal AveragePrice()
        {
            decimal average = _salesHistory.Count > 0 ? _salesHistory.Average() : 1;
            return average;
        }

        public void AddBid(ProductBid bid)
        {
            ProductBids.Add(bid);
        }
        
        public void AddOffer(ProductOffer offer)
        {
            ProductOffers.Add(offer);
        }
        
        public void ResolveMarket(bool isTraining)
        {
            //updateEvent.Invoke(ProductOffers, ProductBids);
            CountAdded = 0;
            var offers = ProductOffers.OrderBy(x => x.Price).ToList();
            var bids = ProductBids.OrderByDescending(x => x.Price).ToList();
            //List<ProductOffer> fullfilledOffers = new();
            List<Deal> successfulDeals = new();
            var (offersCopy, bidsCopy) = DeepCopy(ProductOffers, ProductBids);
            

            while (offers.Count > 0 && bids.Count > 0)
            {
                var offer = offers[0];
                var bid = bids[0];
                if (offer.Price < bid.Price)
                {
                    if (offer.Amount < bid.Amount)
                    {
                        Academy.Instance.StatsRecorder.Add("Market/P-Add", CountAdded+=offer.Amount);

                        bid.Amount -= offer.Amount;
                        successfulDeals.Add(new Deal(offer.Price, offer.Amount));
                        bid.Buyer.FullfillBid(offer.Product, offer.Amount, offer.Price);
                        offer.Seller.FullfillBid(offer.Product, offer.Amount, offer.Price);
                        offers.Remove(offer);
                    }
                    else if (offer.Amount > bid.Amount)
                    {
                        Academy.Instance.StatsRecorder.Add("Market/P-Add", CountAdded+=bid.Amount);

                        offer.Amount -= bid.Amount;
                        successfulDeals.Add(new Deal(offer.Price, bid.Amount));
                        bid.Buyer.FullfillBid(offer.Product, bid.Amount, offer.Price);
                        offer.Seller.FullfillBid(offer.Product, bid.Amount, offer.Price);
                        bids.Remove(bid);
                    }
                    else
                    {
                        Academy.Instance.StatsRecorder.Add("Market/P-Add", CountAdded+=bid.Amount);
                        successfulDeals.Add(new Deal(offer.Price, bid.Amount));
                        bid.Buyer.FullfillBid(offer.Product, bid.Amount, offer.Price);
                        offer.Seller.FullfillBid(offer.Product, bid.Amount, offer.Price);
                        offers.Remove(offer);
                        bids.Remove(bid);
                    }
                    _salesHistory.Add(offer.Price);
                }
                else
                {
                    break;
                }
            }

            DemandForProduct = bids.Count - offers.Count;
            Academy.Instance.StatsRecorder.Add("Market/Demand", DemandForProduct);

            if (isTraining == false)
            {
                updateEvent.Invoke(offersCopy, bidsCopy, successfulDeals);
            }


            ProductOffers.Clear();
            ProductBids.Clear();
        }

        private (List<ProductOffer> offers, List<ProductBid> bids) DeepCopy(
            List<ProductOffer> offersSource,
            List<ProductBid> bidsSource)
        {
            List<ProductOffer> offers = offersSource
                .Select(offer => new ProductOffer(offer.Product, offer.Seller, offer.Price, offer.Amount))
                .ToList();

            List<ProductBid> bids = bidsSource
                .Select(bid => new ProductBid(bid.Product, bid.Buyer, bid.Price, bid.Amount))
                .ToList();

            return (offers, bids);
        }
    }
}