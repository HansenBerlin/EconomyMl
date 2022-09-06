using Enums;

namespace Models
{
    public class ProductRequestModel
    {
        public ProductRequestModel(ProductType product, ProductRequestSearchType searchType, decimal maxPrice = 0,
            long maxAmount = 0, decimal totalSpendable = 0)
        {
            Product = product;
            SearchType = searchType;
            MaxPrice = maxPrice;
            MaxAmount = maxAmount;
            TotalSpendable = totalSpendable;
        }

        public decimal MaxPrice { get; }
        public long MaxAmount { get; }
        public decimal TotalSpendable { get; }
        public ProductRequestSearchType SearchType { get; }
        public ProductType Product { get; }
    }
}