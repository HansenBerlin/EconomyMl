using System.Collections.Generic;
using System.Linq;
using NewScripts.Enums;
using NewScripts.Game.Models;

namespace NewScripts.DataModelling
{
    public class PriceAnalysisStatsModel
    {
        public List<ProductOffer> Offers { get; }
        public List<ProductBid> Bids { get; }
        public List<Deal> Deals { get; set; }
        public ProductType Type { get; }
        public PriceAnalysisStatsModel(List<ProductOffer> offers, List<ProductBid> bids, ProductType type)
        {
            Type = type;
            (List<ProductOffer> offersCopy, List<ProductBid> bidsCopy) = DeepCopy(offers, bids);
            Offers = offersCopy;
            Bids = bidsCopy;
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