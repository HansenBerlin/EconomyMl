using System.Collections.Generic;
using Controller;
using Enums;
using Models.Business;

namespace Models.Market
{



    public interface ICountryEconomyMarketsModel
    {
        void AddBusiness(ICompanyModel privateCompany);
        void AddProduct(ProductController product);

        ReceiptModel Buy(ProductRequestModel buyRequest);
        decimal AveragePrice(ProductType type);
        decimal AveragePrice(ProductType type, string ownId);
        decimal TotalSupply(ProductType type);
        decimal EstimatedMonthlyDemand(ProductType forProduct);
        decimal MarketShare(ProductType type, string productId);
        void ResetProductMarkets();
        void ReportDemand(long count, ProductType type);
        void ReportProduction(long count, ProductType type);
        List<ProductType> FindMostDemandedByTrend();
    }
}