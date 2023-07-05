using System.Collections.Generic;
using System.Linq;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Models;
using NewScripts.Game.Services;
using NewScripts.Interfaces;
using Unity.MLAgents;

namespace NewScripts.Game.Entities
{
    public class ProductMarket
    {
        public int DemandForProduct { get; private set; }
        public PriceAnalysisStatsModel PriceAnalysisStats { get; private set; }
        private readonly ProductType _productType;
        private List<ProductOffer> ProductOffers { get; } = new();
        private readonly List<decimal> _salesHistory = new();
        private List<ProductBid> ProductBids { get; } = new();
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
            var offers = ProductOffers.OrderBy(x => x.Price).ToList();
            var bids = ProductBids.OrderByDescending(x => x.Price).ToList();
            List<Deal> successfulDeals = new();
            PriceAnalysisStats = new PriceAnalysisStatsModel(ProductOffers, ProductBids, _productType);

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
                        offers.Remove(offer);
                    }
                    else if (offer.Amount > bid.Amount)
                    {
                        offer.Amount -= bid.Amount;
                        successfulDeals.Add(new Deal(offer.Price, bid.Amount));
                        bid.Buyer.FullfillBid(offer.Product, bid.Amount, offer.Price);
                        offer.Seller.FullfillBid(offer.Product, bid.Amount, offer.Price);
                        bids.Remove(bid);
                    }
                    else
                    { 
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

            ProductOffers.Clear();
            ProductBids.Clear();
        }
    }
}