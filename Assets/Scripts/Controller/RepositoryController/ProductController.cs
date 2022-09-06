using System;
using System.Collections.Generic;
using Controller.Data;
using Enums;
using Interfaces;
using Models;

namespace Controller.RepositoryController
{
    public class ProductController
    {
        private readonly CollectionNormalizationController _costPerPieceTrendData;
        private readonly CollectionNormalizationController _priceTrendData;
        private readonly ProductModel _productData;
        private readonly CollectionNormalizationController _productionTrendData;
        private readonly CollectionNormalizationController _profitTrendData;
        private readonly CollectionNormalizationController _salesTrendData;

        private readonly CollectionNormalizationController _supplyTrendData;
        public readonly string Id = Guid.NewGuid().ToString();

        public ProductController(ProductType type, ProductModel productData, IProductionTemplate template)
        {
            Type = type;
            _productData = productData;
            Template = template;
            Price = productData.Price;
            _productData.MontlyData(UpdateEpisodeType.Initialize, new List<decimal> {Price});
            _supplyTrendData = new CollectionNormalizationController(_productData.Stats.SupplyTotal);
            _profitTrendData = new CollectionNormalizationController(_productData.Stats.ProfitTotal);
            _priceTrendData = new CollectionNormalizationController(_productData.Stats.PriceTotal);
            _salesTrendData = new CollectionNormalizationController(_productData.Stats.SalesTotal);
            _productionTrendData = new CollectionNormalizationController(_productData.Stats.ProducedTotal);
            _costPerPieceTrendData = new CollectionNormalizationController(_productData.Stats.CppTotal);
        }

        private decimal ObsSupplyTrend => _supplyTrendData.Normalize(_productData.TotalSupply);
        private decimal ObsPriceTrend => _priceTrendData.Normalize(_productData.Price);
        private decimal ObsCppTrend => _costPerPieceTrendData.Normalize(_productData.CostPerPiece);
        public IProductionTemplate Template { get; }
        public decimal ObsProfitTrend => _profitTrendData.Normalize(_productData.ProfitThisMonth);
        public decimal ObsSalesTrend => _salesTrendData.Normalize(_productData.SalesThisMonth);
        public decimal ObsProductionTrend => _productionTrendData.Normalize(_productData.ProductionThisMonth);
        public ProductType Type { get; }
        public decimal Price { get; private set; }
        public decimal Profit => _productData.ProfitThisMonth;
        public decimal ProfitLastMonth => _productData.LastProfit;
        public long TotalSupply => _productData.TotalSupply;
        public long SalesThisMonth => _productData.SalesThisMonth;
        public long SalesLastMonth => _productData.LastSales;
        public long ProductionThisMonth => _productData.ProductionThisMonth;
        public long ProductionLastMonth => _productData.LastProd;

        public void UpdatePrice(decimal newPrice)
        {
            Price = newPrice;
        }

        public void Update(EpisodeCut timeFrame, decimal cpp = 0, decimal capUsed = 0)
        {
            switch (timeFrame)
            {
                case EpisodeCut.Month:
                {
                    var data = new List<decimal>
                    {
                        cpp, capUsed, ObsPriceTrend, ObsSupplyTrend, ObsProfitTrend,
                        ObsSalesTrend, ObsProductionTrend, ObsCppTrend, Price
                    };
                    _productData.MontlyData(UpdateEpisodeType.Update, data);
                    _productData.MontlyData(UpdateEpisodeType.Reset);
                    break;
                }
                case EpisodeCut.Quarter:
                    _productData.QuarterlyData(UpdateEpisodeType.Reset);
                    break;
            }
        }

        public ReceiptModel BuyMaxAmount(long amount, decimal maxPrice)
        {
            if (amount > _productData.TotalSupply) amount = _productData.TotalSupply;
            if (_productData.Price > maxPrice) amount = 0;
            _productData.TotalSupply -= amount;
            _productData.SalesThisMonth += amount;
            _productData.ProfitThisMonth += _productData.Price * amount;

            return new ReceiptModel
            {
                TotalPricePaid = _productData.Price * amount,
                AmountBought = amount
            };
        }

        public ReceiptModel BuyMaxAmount(long amount)
        {
            if (amount > _productData.TotalSupply) amount = _productData.TotalSupply;
            _productData.TotalSupply -= amount;
            _productData.SalesThisMonth += amount;
            _productData.ProfitThisMonth += _productData.Price * amount;

            return new ReceiptModel
            {
                TotalPricePaid = _productData.Price * amount,
                AmountBought = amount
            };
        }

        public ReceiptModel BuyFor(decimal maxMoney)
        {
            var buy = (long) (maxMoney / _productData.Price);
            if (buy > _productData.TotalSupply) buy = _productData.TotalSupply;
            _productData.TotalSupply -= buy;
            _productData.SalesThisMonth += buy;
            _productData.ProfitThisMonth += _productData.Price * buy;

            return new ReceiptModel
            {
                TotalPricePaid = _productData.Price * buy,
                AmountBought = buy
            };
        }

        public ReceiptModel BuyFor(decimal maxMoney, long amount)
        {
            var buy = (long) (maxMoney / _productData.Price);
            if (buy > _productData.TotalSupply && amount > _productData.TotalSupply) buy = buy > amount ? amount : buy;
            if (buy > amount) buy = amount;
            if (_productData.TotalSupply < buy) buy = _productData.TotalSupply;
            _productData.TotalSupply -= buy;
            _productData.SalesThisMonth += buy;
            _productData.ProfitThisMonth += _productData.Price * buy;

            return new ReceiptModel
            {
                TotalPricePaid = _productData.Price * buy,
                AmountBought = buy
            };
        }

        public void AddNew(int count)
        {
            _productData.TotalSupply += count;
            _productData.ProductionThisMonth += count;
        }
    }
}