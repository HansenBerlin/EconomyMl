using UnityEngine;

namespace NewScripts
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
    
    public class JobBid
    {
        public ICompany Employer { get; }
        public decimal Wage { get; }
        
        public JobBid(ICompany employer, decimal wage)
        {
            Employer = employer;
            Wage = wage;
        }
    }
    
    public class JobOffer
    {
        public Worker Worker { get; }
        public decimal Wage { get; }
        
        public JobOffer(Worker worker, decimal wage)
        {
            Worker = worker;
            Wage = wage;
        }
    }

    public enum ProductType
    {
        Food,
        Luxury
    }
}