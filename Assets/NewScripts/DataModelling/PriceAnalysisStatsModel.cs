using System.Collections.Generic;
using System.Linq;
using NewScripts.Game.Models;

namespace NewScripts.DataModelling
{
    public class PriceAnalysisStatsModel
    {
        public PriceAnalysisStatsModel(List<ProductOffer> offers, List<ProductBid> bids)
        {
            (List<ProductOffer> offersCopy, List<ProductBid> bidsCopy) = DeepCopy(offers, bids);
            Offers = offersCopy;
            Bids = bidsCopy;
        }

        public List<ProductOffer> Offers { get; }
        public List<ProductBid> Bids { get; }
        public List<Deal> Deals { get; set; }
        
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