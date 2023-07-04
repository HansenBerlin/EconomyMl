using NewScripts.Enums;
using NewScripts.Game.Entities;

namespace NewScripts.Game.Models
{
    public class ProductBid
    {
        public ProductType Product { get; }
        public Worker Buyer { get; }
        public decimal Price { get; }
        //public double MaxSpending { get; set; }
        public int Amount { get; set; }
        
        public ProductBid(ProductType product, Worker buyer, decimal price, int amount)
        {
            Product = product;
            Buyer = buyer;
            Price = price;
            Amount = amount;
            //MaxSpending = maxSpending;
        }
    }
}