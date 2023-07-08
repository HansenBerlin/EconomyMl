using System.Collections.Generic;
using System.Linq;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Models;
using NewScripts.Game.Services;
using NewScripts.Interfaces;
using Unity.MLAgents;
using UnityEngine;

namespace NewScripts.Game.Entities
{
    public class ProductMarket
    {
        public int DemandForProduct { get; private set; }
        public PriceAnalysisStatsModel PriceAnalysisStats { get; private set; }
        private readonly ProductType _productType;
        private List<ProductOffer> ProductOffers { get; } = new();
        private readonly List<decimal> _salesHistory = new();
        private List<ProductBid> PrivateProductBids { get; } = new();
        private List<ProductBid> GovernmentProductBids { get; } = new();
        private readonly decimal _averageStartingPrice;

        public ProductMarket(ProductType productType, decimal startingAverage)
        {
            _productType = productType;
            _averageStartingPrice = startingAverage;
        }

        public decimal AveragePriceInLastYear()
        {
            if (_salesHistory.Count > 12)
            {
                _salesHistory.RemoveRange(0, _salesHistory.Count - 12);
            }
            decimal average = _salesHistory.Count > 0 ? _salesHistory.Average() : _averageStartingPrice;
            return average;
        }

        public void AddBid(ProductBid bid, bool isGovernmentBid = false)
        {
            if (isGovernmentBid)
            {
                GovernmentProductBids.Add(bid);
                return;
            }
            PrivateProductBids.Add(bid);
        }
        
        public void AddOffer(ProductOffer offer)
        {
            ProductOffers.Add(offer);
        }
        
        public void ResolveMarket(bool isTraining, bool isGovernmentRequest = false)
        {
            var offers = ProductOffers.OrderBy(x => x.Price).ToList();
            List<ProductBid> bids;
            List<Deal> successfulDeals = new();
            
            if (isGovernmentRequest)
            {
                bids = GovernmentProductBids.OrderByDescending(x => x.Price).ToList();    
            }
            else
            {
                bids = PrivateProductBids.OrderByDescending(x => x.Price).ToList();
                PriceAnalysisStats = new PriceAnalysisStatsModel(ProductOffers, PrivateProductBids, _productType);
            }

            while (offers.Count > 0 && bids.Count > 0)
            {
                var offer = offers[0];
                var bid = bids[0];
                if (offer.Price < bid.Price)
                {
                    if (offer.Amount < bid.Amount)
                    {
                        bid.Amount -= offer.Amount;
                        successfulDeals.Add(new Deal(offer.Price, offer.Amount));
                        bid.Buyer.FullfillBid(offer.Product, offer.Amount, offer.Price);
                        offer.Seller.FullfillBid(offer.Product, offer.Amount, offer.Price);
                        if (offer.Amount == 0 || bid.Amount == 0)
                        {
                            Debug.LogWarning("Offer amount is 0");
                        }
                        offers.Remove(offer);
                    }
                    else if (offer.Amount > bid.Amount)
                    {
                        offer.Amount -= bid.Amount;
                        successfulDeals.Add(new Deal(offer.Price, bid.Amount));
                        bid.Buyer.FullfillBid(offer.Product, bid.Amount, offer.Price);
                        offer.Seller.FullfillBid(offer.Product, bid.Amount, offer.Price);
                        if (offer.Amount == 0 || bid.Amount == 0)
                        {
                            Debug.LogWarning("Offer amount is 0");
                        }
                        bids.Remove(bid);

                    }
                    else
                    { 
                        successfulDeals.Add(new Deal(offer.Price, bid.Amount));
                        bid.Buyer.FullfillBid(offer.Product, bid.Amount, offer.Price);
                        offer.Seller.FullfillBid(offer.Product, bid.Amount, offer.Price);
                        if (offer.Amount == 0 || bid.Amount == 0)
                        {
                            Debug.LogWarning("Offer amount is 0");
                        }
                        offers.Remove(offer);
                        bids.Remove(bid);
                    }

                    if (bid.Amount == 0)
                    {
                        //bids.Remove(bid);
                    }
                    if (offer.Amount == 0)
                    {
                        //offers.Remove(offer);
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
                PriceAnalysisStats.Deals = successfulDeals;
                if (_productType == ProductType.Food)
                {
                    ServiceLocator.Instance.UiUpdateManager.foodPricesupdateEvent.Invoke(PriceAnalysisStats);
                }
                else
                {
                    ServiceLocator.Instance.UiUpdateManager.luxuryPricesupdateEvent.Invoke(PriceAnalysisStats);
                }
            }

            if(isGovernmentRequest)
            {
                GovernmentProductBids.Clear();
            }
            else
            {
                PrivateProductBids.Clear();
            }

            if (isGovernmentRequest || _productType == ProductType.Luxury)
            {
                ProductOffers.Clear();
            }
        }
    }
}