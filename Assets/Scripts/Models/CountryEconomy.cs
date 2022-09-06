using System;
using System.Collections.Generic;
using System.Linq;
using Agents;
using Controller.RepositoryController;
using Enums;
using Interfaces;

namespace Models
{
    public class CountryEconomy : ICountryEconomy
    {
        private readonly BankingMarkets _bankingMarkets;
        private readonly GovernmentAgent _government;
        private readonly List<ProductMarketModel> _productMarkets = new();
        public string Id = Guid.NewGuid().ToString();

        public CountryEconomy(List<ProductMarketModel> productMarkets, GovernmentAgent government,
            BankingMarkets bankingMarkets)
        {
            foreach (var m in productMarkets) _productMarkets.Add(m);

            _productMarkets = productMarkets;
            _government = government;
            _bankingMarkets = bankingMarkets;
        }

        public void RemoveBusiness(CompanyBaseAgent privateCompany, string productId)
        {
            var market = FindMatchingMarket(privateCompany.TypeProduced);
            market.RemoveProduct(productId);
        }

        public BankAccountModel OpenBankAccount(decimal amount, bool isSetup)
        {
            return _bankingMarkets.OpenBankAccount(amount, isSetup);
        }

        public void AddProduct(ProductController product)
        {
            var market = FindMatchingMarket(product.Type);
            market.AddProduct(product);
        }

        public ReceiptModel Buy(ProductRequestModel buyRequest)
        {
            var productMarket = FindMatchingMarket(buyRequest.Product);
            var receipt = new ReceiptModel();
            if (buyRequest.TotalSpendable < 0) return receipt;

            switch (buyRequest.SearchType)
            {
                case ProductRequestSearchType.MaxSpendable:
                    receipt = productMarket.BuyMaxProductsForMoney(buyRequest);
                    break;
                case ProductRequestSearchType.MaxAmount:
                    receipt = productMarket.BuyMaxProducts(buyRequest);
                    break;
                case ProductRequestSearchType.MaxAmountWithSpendingLimit:
                    receipt = productMarket.BuyMaxProductsForMaxAmountSpended(buyRequest);
                    break;
                default:
                    receipt = productMarket.BuyMaxProductsForMaxPricePerPiece(buyRequest);
                    break;
            }

            if (buyRequest.Product == ProductType.BaseProduct || buyRequest.Product == ProductType.LuxuryProduct)
            {
                decimal consumerTax = _government.PayConsumerTax(receipt.TotalPricePaid);
                receipt.TotalPricePaid += consumerTax;
            }

            return receipt;
        }

        public decimal AveragePrice(ProductType type)
        {
            var productMarket = FindMatchingMarket(type);
            return productMarket.AveragePrice;
        }

        public decimal MarketShare(ProductType type, string productId)
        {
            var productMarket = FindMatchingMarket(type);
            return productMarket.GetMarketShare(productId);
        }

        public long GetTotalUnfulfilledDemand(ProductType type)
        {
            var productMarket = FindMatchingMarket(type);
            return productMarket.GetTotalUnfullfilledDemand();
        }

        public void ReportDemand(long count, ProductType type)
        {
            var productMarket = FindMatchingMarket(type);
            productMarket.ReportDemand(count);
        }

        public void ResetProductMarkets()
        {
            foreach (var m in _productMarkets) m.Reset();
        }

        public void ReportStats(ProductType type, int workers, float capital, float moneyIn, float moneyOut,
            long production, long sales, float price, float cpp)
        {
            var productMarket = FindMatchingMarket(type);
            productMarket.ReportStats(workers, capital, moneyIn, moneyOut, production, sales, price, cpp);
        }

        private ProductMarketModel FindMatchingMarket(ProductType type)
        {
            return _productMarkets.Where(x => x.Type == type).ToList().First();
        }
    }
}