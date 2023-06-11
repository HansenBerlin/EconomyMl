using UnityEngine;

namespace NewScripts
{
    public class ProductOffer
    {
        public Company OfferedBy { get; set; }
        public Product Product { get; set; }

        public float Buy()
        {
            OfferedBy.LastSales++;
            OfferedBy.Capital += Product.Price;
            Product.Amount--;
            return Product.Price;
        }
    }
}