using Enums;

namespace Models.Market
{



    public class ProductRequestModel
    {
        public decimal MaxPrice { get; }
        public int MaxAmount { get; }
        public decimal TotalSpendable { get; }
        public ProductRequestSearchType SearchType { get; }
        public ProductType Product { get; }

        public ProductRequestModel(ProductType product, ProductRequestSearchType searchType, decimal maxPrice = 0,
            int maxAmount = 0, decimal totalSpendable = 0)
        {
            Product = product;
            SearchType = searchType;
            MaxPrice = maxPrice;
            MaxAmount = maxAmount;
            TotalSpendable = totalSpendable;
            //if (searchType == ProductRequestSearchType.MaxAmount && maxAmount == 0)
            //    throw new Exception();
            //if (searchType == ProductRequestSearchType.MaxSpendable && totalSpendable == 0)
            //    throw new Exception();
            //if (searchType == ProductRequestSearchType.MaxAmountForMaxPrice && (maxAmount == 0 || maxPrice == 0))
            //    throw new Exception();
            //if (searchType == ProductRequestSearchType.MaxAmountWithSpendingLimit && (maxAmount == 0 || totalSpendable == 0))
            //    throw new Exception();

        }
    }
}