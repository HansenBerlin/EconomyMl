using UnityEngine;

namespace NewScripts
{
    public class Product
    {
        public ProductType ProductTypeInput;
        public int InputAmount;
        public ProductType ProductTypeOutput;
        public int OutputAmount;
    }

    public class ProductOffer
    {
        public Product Product;
        public Company Seller;
        public double Price;
        public int Amount;
    }
    
    public class ProductBid
    {
        public Product Product;
        public Worker Buyer;
        public double Price;
        public int Amount;
    }
    
    public class JobBid
    {
        public Company Employer;
        public double Price;
    }
    
    public class JobOffer
    {
        public Worker Worker;
        public double Price;
    }

    public enum ProductType
    {
        Food,
        Intermediate,
        Luxury,
        None
    }
}