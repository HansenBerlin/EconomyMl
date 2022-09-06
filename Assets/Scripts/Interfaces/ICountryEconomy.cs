using Agents;
using Controller.RepositoryController;
using Enums;
using Models;

namespace Interfaces
{
    public interface ICountryEconomy
    {
        void RemoveBusiness(CompanyBaseAgent privateCompany, string productId);
        void AddProduct(ProductController product);
        ReceiptModel Buy(ProductRequestModel buyRequest);
        decimal AveragePrice(ProductType type);
        decimal MarketShare(ProductType type, string productId);
        void ResetProductMarkets();
        long GetTotalUnfulfilledDemand(ProductType type);
        void ReportDemand(long count, ProductType type);
        BankAccountModel OpenBankAccount(decimal amount, bool isSetup);

        void ReportStats(ProductType type, int workers, float capital, float moneyIn, float moneyOut,
            long production, long sales, float price, float cpp);
    }
}