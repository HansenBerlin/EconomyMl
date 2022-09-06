using System;
using System.Collections.Generic;
using System.Linq;
using Controller.RepositoryController;
using Enums;
using Models.Business;
using Models.Finance;
using Models.Population;

namespace Models.Market
{



    public class CountryEconomy : ICountryEconomy
    {
        public string Id = Guid.NewGuid().ToString();

        private readonly List<ProductMarketModel> _productMarkets = new();
        private readonly List<CompanyBaseAgent> _businesses = new();
        private readonly JobMarketController _jobMarket;
        private readonly BankingMarkets _bankingMarkets;
        private readonly PopulationModel _populationModel;
        private readonly GovernmentAgent _government;
        private int _fossileEnergyLeft = 100000;
        public double WorkerAverageIncome = 0;

        public CountryEconomy(List<ProductMarketModel> productMarkets, JobMarketController jobMarket,
            PopulationModel populationModel, GovernmentAgent government, BankingMarkets bankingMarkets)
        {
            foreach (var m in productMarkets)
            {
                _productMarkets.Add(m);
            }

            _productMarkets = productMarkets;
            _jobMarket = jobMarket;
            _populationModel = populationModel;
            _government = government;
            _bankingMarkets = bankingMarkets;
        }

        public void AddBusiness(CompanyBaseAgent privateCompany)
        {
            _businesses.Add(privateCompany);
        }
        
        public void RemoveBusiness(CompanyBaseAgent privateCompany, string productId)
        {
            var market = FindMatchingMarket(privateCompany.TypeProduced);
            market.RemoveProduct(productId);
            _businesses.Remove(privateCompany);
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

        private ProductMarketModel FindMatchingMarket(ProductType type)
        {
            return _productMarkets.Where(x => x.Type == type).ToList().First();
        }

        public List<ProductType> FindMostDemandedByTrend()
        {
            List<ProductType> opportunities = new();
            foreach (var p in _productMarkets.Where(p =>
                         p.Type != ProductType.FederalService && p.Type != ProductType.None))
            {
                var demandTrend = p.GetLastQuarterDemandTrend();
                if (demandTrend > 1M)
                {
                    opportunities.Add(p.Type);
                }
            }

            return opportunities;
        }


        public ReceiptModel Buy(ProductRequestModel buyRequest)
        {
            var productMarket = FindMatchingMarket(buyRequest.Product);
            ReceiptModel receipt = new ReceiptModel();
            if (buyRequest.TotalSpendable < 0)
            {
                return receipt;
            }

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
        

        public decimal TotalSupply(ProductType type)
        {
            ProductMarketModel productMarket = FindMatchingMarket(type);
            return productMarket.TotalSupply;
        }

        public decimal MarketShare(ProductType type, string productId)
        {
            ProductMarketModel productMarket = FindMatchingMarket(type);
            return productMarket.GetMarketShare(productId);
        }

        public long GetTotalUnfulfilledDemand(ProductType type)
        {
            long demandTotal;
            ProductMarketModel productMarket = FindMatchingMarket(type);
            return productMarket.GetTotalUnfullfilledDemand();
        }

        public void ReportDemand(long count, ProductType type)
        {
            ProductMarketModel productMarket = FindMatchingMarket(type);
            productMarket.ReportDemand(count);
        }

        public void ReportProduction(long count, ProductType type)
        {
            ProductMarketModel productMarket = FindMatchingMarket(type);
            productMarket.ReportProduction(count);
        }

        public void ResetProductMarkets()
        {
            foreach (var m in _productMarkets)
            {
                m.Reset();
            }
        }
        
        public void ReportStats(ProductType type, int workers, float capital, float moneyIn, float moneyOut,
            long production, long sales, float price, float cpp)
        {
            
            ProductMarketModel productMarket = FindMatchingMarket(type);
            productMarket.ReportStats(workers, capital, moneyIn, moneyOut, production, sales, price, cpp);
        }
        
        

    }
}