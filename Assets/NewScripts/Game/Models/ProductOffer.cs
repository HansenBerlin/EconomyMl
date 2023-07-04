using NewScripts.Enums;
using NewScripts.Interfaces;

namespace NewScripts.Game.Models
{
    public class ProductOffer
    {
        public ProductType Product { get; }
        public ICompany Seller { get; }
        public decimal Price { get; }
        public int Amount { get; set; }
        
        public ProductOffer(ProductType product, ICompany seller, decimal price, int amount)
        {
            Product = product;
            Seller = seller;
            Price = price;
            Amount = amount;
        }
    }
}