using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
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
        private readonly List<ICompanyModel> _businesses = new();
        private readonly JobMarketController _jobMarket;
        private readonly BankingMarkets _bankingMarkets;
        private readonly PopulationModel _populationModel;
        private readonly GovernmentController _government;
        private int _fossileEnergyLeft = 100000;
        public double WorkerAverageIncome = 0;

        public CountryEconomy(List<ProductMarketModel> productMarkets, JobMarketController jobMarket,
            PopulationModel populationModel, GovernmentController government, BankingMarkets bankingMarkets)
        {
            foreach (var m in productMarkets)
            {
                _productMarkets.Add(m);
            }

            _productMarkets = productMarkets;
            _jobMarket = jobMarket;
            _populationModel = populationModel;
            _government = government;
        }

        public void AddBusiness(ICompanyModel privateCompany)
        {
            _businesses.Add(privateCompany);
        }

        public LoanModel GetLoan(decimal amount, CreditRating rating)
        {
            return _bankingMarkets.FindCheapestPossibleLoan(amount, rating);
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
                throw new Exception();
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

        public decimal GetProductSupplyAndSalesTrend(ProductType type)
        {

            var productMarket = FindMatchingMarket(type);
            return productMarket.AveragePrice;
        }

        public decimal AveragePrice(ProductType type, string ownId)
        {
            var productMarket = FindMatchingMarket(type);
            return productMarket.AveragePrice;
        }

        public decimal TotalSupply(ProductType type)
        {
            ProductMarketModel productMarket = FindMatchingMarket(type);
            return productMarket.TotalSupply;
        }

        public decimal EstimatedMonthlyDemand(ProductType forProduct)
        {
            long estDemand = 0;
            foreach (var b in _businesses)
            {
                if (b.EnergyTypeNeeded == forProduct)
                {
                    estDemand += b.EstimatedEnergyDemand;
                }

                if (b.ResourceTypeNeeded == forProduct)
                {
                    estDemand += b.EstimatedResourceDemand;
                }
            }

            if (forProduct is ProductType.BaseProduct)
            {
                estDemand += (long) (_populationModel.PopulationCount * 1.6 * 30);
            }

            if (forProduct is ProductType.LuxuryProduct)
            {
                estDemand += (long) (_populationModel.PopulationCount * 1.2);
            }

            return estDemand;
        }

        public decimal MarketShare(ProductType type, string productId)
        {
            ProductMarketModel productMarket = FindMatchingMarket(type);
            return productMarket.GetMarketShare(productId);
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

    }
}