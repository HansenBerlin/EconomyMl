using System.Collections.Generic;
using Controller;
using Enums;
using Models.Business;
using Models.Finance;

namespace Models.Market
{



    public interface ICountryEconomy
    {
        void AddBusiness(CompanyBaseAgent privateCompany);
        void RemoveBusiness(CompanyBaseAgent privateCompany, string productId);
        void AddProduct(ProductController product);

        ReceiptModel Buy(ProductRequestModel buyRequest);
        decimal AveragePrice(ProductType type);
        decimal AveragePrice(ProductType type, string ownId);
        decimal TotalSupply(ProductType type);
        decimal MarketShare(ProductType type, string productId);
        void ResetProductMarkets();
        long GetTotalUnfulfilledDemand(ProductType type);
        void ReportDemand(long count, ProductType type);
        void ReportProduction(long count, ProductType type);
        List<ProductType> FindMostDemandedByTrend();
        LoanModel GetLoan(decimal amount, CreditRating rating);

        void ReportStats(ProductType type, int workers, float capital, float moneyIn, float moneyOut,
            long production, long sales, float price, float cpp);

    }
}