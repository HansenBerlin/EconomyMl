using UnityEngine;

namespace NewScripts
{
    public class Product
    {
        public ProductType ProductTypeInput;
        public ProductType ProductTypeOutput;
        public float Price;
        public int Amount;
    }

    public enum ProductType
    {
        Food,
        Intermediate,
        Luxury,
        None
    }
}