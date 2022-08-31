using System;
using System.Collections.Generic;
using Enums;
using Models.Market;
using Models.Production;

namespace Controller
{



    public class ProductController
    {
        private int Month => _productData.Month;
        private int QuarterMonth => _productData.Month % 3 == 0 ? 3 : _productData.Month % 3;
        public IProductionTemplate Template { get; }
        public decimal ObsSupplyTrend => _supplyTrendData.Normalize(_productData.TotalSupply);
        public decimal ObsProfitTrend => _profitTrendData.Normalize(_productData.ProfitThisMonth);
        public decimal ObsPriceTrend => _priceTrendData.Normalize(_productData.Price);
        public decimal ObsSalesTrend => _salesTrendData.Normalize(_productData.SalesThisMonth);
        public decimal ObsProductionTrend => _productionTrendData.Normalize(_productData.ProductionThisMonth);
        public decimal ObsCppTrend => _costPerPieceTrendData.Normalize(_productData.CostPerPiece);



        public string Id = Guid.NewGuid().ToString();

        public ProductType Type { get; set; }
        //public decimal DailyProfit { get; set; }

        public decimal Price { get; private set; }
        public decimal Cpp => _productData.CostPerPiece;
        public decimal Profit => _productData.ProfitThisMonth;

        public long TotalSupply => _productData.TotalSupply;

        public long SalesThisMonth => _productData.SalesThisMonth;

        public decimal QuarterlySupplyAverage =>
            (_productData.SupplyQuarterlyCurrent + _productData.SupplyQuarterlyLast) / (QuarterMonth + 3);

        public decimal QuarterlyProductionAverage =>
            (_productData.ProdQuarterlyCurrent + _productData.ProdQuarterlyLast) / (QuarterMonth + 3);

        public decimal QuarterlySalesAverage => (_productData.SalesQuarterlyCurrent + _productData.SalesQuarterlyLast) /
                                                (QuarterMonth + 3);

        public decimal CapacityUsed => (_productData.CapacityQuarterlyCurrent + _productData.CapacityQuarterlyLast) /
                                       (QuarterMonth + 3);


        private readonly NormalizationModel _supplyTrendData;
        private readonly NormalizationModel _profitTrendData;
        private readonly NormalizationModel _priceTrendData;
        private readonly NormalizationModel _salesTrendData;
        private readonly NormalizationModel _productionTrendData;
        private readonly NormalizationModel _costPerPieceTrendData;

        private readonly ProductModel _productData;


        public ProductController(ProductType type, ProductModel productData, IProductionTemplate template)
        {
            Type = type;
            _productData = productData;
            Template = template;
            Price = productData.Price;
            _productData.MontlyData(UpdateEpisodeType.Initialize, new List<decimal> {Price});
            _supplyTrendData = new NormalizationModel(_productData.Stats.SupplyTotal);
            _profitTrendData = new NormalizationModel(_productData.Stats.ProfitTotal);
            _priceTrendData = new NormalizationModel(_productData.Stats.PriceTotal);
            _salesTrendData = new NormalizationModel(_productData.Stats.SalesTotal);
            _productionTrendData = new NormalizationModel(_productData.Stats.ProducedTotal);
            _costPerPieceTrendData = new NormalizationModel(_productData.Stats.CppTotal);
        }

        /*decimal NormalizeAvgProfitTrend()
        {
            
            var val = (_productData.ProfitThisMonth + _productData.ProfitAverageAllTime * (Month - 1)) / Month;
            //_profitAverageAllTime = (_profitThisMonth + _profitAverageAllTime * (month - 1)) / month;
            var ret = _profitTrendData.Normalize(_productData.ProfitThisMonth);
            return ret;
        }
        
        decimal NormalizeAvgPriceTrend()
        {
            
            var val = (_productData.Price + _productData.PriceAverageAllTime * (Month - 1)) / Month;
            //_priceAverageAllTime = (Price + _priceAverageAllTime * (month - 1)) / month;
            var ret = _priceTrendData.Normalize(_productData.Price);
            return ret;
        }
        
        decimal NormalizeAvgSupplyTrend()
        {
            
            var val = (_productData.TotalSupply + _productData.SupplyAverageAllTime * (Month - 1)) / Month;
            //SupplyAverageAllTime = (TotalSupply + SupplyAverageAllTime * (month - 1)) / month;
            var ret = _supplyTrendData.Normalize(_productData.TotalSupply);
            return ret;
        }
        
        decimal NormalizeAvgSalesTrend()
        {
            
            var val = (_productData.SalesThisMonth + _productData.SalesAverageAllTime * (Month - 1)) / Month;
            //SalesAverageAllTime = (SalesThisMonth + SalesAverageAllTime * (month - 1)) / month;
            var ret = _salesTrendData.Normalize(_productData.SalesThisMonth);
            return ret;
        }
        
        decimal NormalizeAvgProductionTrend()
        {
            var val = (_productData.ProductionThisMonth + _productData.ProductionAverageAllTime * (Month - 1)) / Month;
            //ProductionAverageAllTime = (ProductionThisMonth + ProductionAverageAllTime * (month - 1)) / month;
            var ret = _productionTrendData.Normalize(_productData.ProductionThisMonth);
            return ret;
        }
        
        decimal NormalizeAvgCppTrend()
        {
            var val = (_productData.CostPerPiece + _productData.CostPerPieceAverageAllTime * (Month - 1)) / Month;
            //_costPerPieceAverageAllTime = (CostPerPiece + _costPerPieceAverageAllTime * (month - 1)) / month;
            var ret = _costPerPieceData.Normalize(_productData.CostPerPiece);
            return ret;
        }*/



        public void UpdatePrice(decimal newPrice)
        {
            if (newPrice == 0)
                throw new Exception();
            Price = newPrice;
        }

        public void Update(EpisodeCut timeFrame, decimal cpp = 0, decimal capUsed = 0)
        {
            switch (timeFrame)
            {
                case EpisodeCut.Day:
                    _productData.DailyData(UpdateEpisodeType.Update);
                    _productData.DailyData(UpdateEpisodeType.Reset);
                    break;
                case EpisodeCut.Month:
                {
                    List<decimal> data = new List<decimal>
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

        public ReceiptModel BuyMaxAmount(int amount, decimal maxPrice)
        {
            if (amount > _productData.TotalSupply)
            {
                amount = (int) _productData.TotalSupply;
            }

            if (_productData.Price > maxPrice)
            {
                amount = 0;
            }

            if (amount < 0)
                throw new Exception();

            _productData.TotalSupply -= amount;
            if (_productData.TotalSupply < 0)
                throw new Exception();
            _productData.SalesThisMonth += amount;

            _productData.ProfitThisMonth += _productData.Price * amount;
            //_productData.DailyProfit += _productData.Price * amount;

            return new ReceiptModel()
            {
                TotalPricePaid = _productData.Price * amount,
                AmountBought = amount
            };
        }

        public ReceiptModel BuyMaxAmount(int amount)
        {
            if (amount > _productData.TotalSupply)
            {
                amount = (int) _productData.TotalSupply;
            }

            if (amount < 0)
                throw new Exception();

            _productData.TotalSupply -= amount;
            if (_productData.TotalSupply < 0)
                throw new Exception();
            _productData.SalesThisMonth += amount;
            //_salesLastMonth += amount;
            _productData.ProfitThisMonth += _productData.Price * amount;
            //_profitLastMonth += Price * amount;
            //_productData.DailyProfit += _productData.Price * amount;

            return new ReceiptModel()
            {
                TotalPricePaid = _productData.Price * amount,
                AmountBought = amount,
            };
        }

        public ReceiptModel BuyFor(decimal maxMoney)
        {

            int buy = (int) (maxMoney / _productData.Price);
            if (buy > _productData.TotalSupply)
            {
                buy = (int) _productData.TotalSupply;
            }

            if (buy < 0)
                throw new Exception();

            _productData.TotalSupply -= buy;
            if (_productData.TotalSupply < 0)
                throw new Exception();
            _productData.SalesThisMonth += buy;
            //_salesLastMonth += buy;
            _productData.ProfitThisMonth += _productData.Price * buy;
            //_profitLastMonth += Price * buy;
            //_productData.DailyProfit += _productData.Price * buy;

            return new ReceiptModel()
            {
                TotalPricePaid = _productData.Price * buy,
                AmountBought = buy
            };
        }

        public ReceiptModel BuyFor(decimal maxMoney, int amount)
        {
            int buy = (int) (maxMoney / _productData.Price);
            if (buy > _productData.TotalSupply && amount > _productData.TotalSupply)
            {
                buy = buy > amount ? amount : buy;
            }

            if (buy > amount)
            {
                buy = amount;
            }

            if (_productData.TotalSupply < buy)
            {
                buy = (int) _productData.TotalSupply;
            }

            if (buy < 0)
                throw new Exception();


            _productData.TotalSupply -= buy;
            if (_productData.TotalSupply < 0)
                throw new Exception();
            _productData.SalesThisMonth += buy;
            //_salesLastMonth += buy;
            _productData.ProfitThisMonth += _productData.Price * buy;
            //_profitLastMonth += Price * buy;
            //_productData.DailyProfit += _productData.Price * buy;

            return new ReceiptModel()
            {
                TotalPricePaid = _productData.Price * buy,
                AmountBought = buy
            };
        }

        public void AddNew(int count)
        {
            if (count < 0)
                throw new Exception();
            _productData.TotalSupply += count;
            //_productionAverageAllTime += count;
            _productData.ProductionThisMonth += count;
        }
    }
}