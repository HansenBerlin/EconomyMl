using System.Collections.Generic;
using Enums;
using Repositories;

namespace Models
{
    public class ProductModel
    {
        public readonly ProductDataRepository Stats;


        public ProductModel(ProductDataRepository stats, ProductType type,
            decimal initialPrice)
        {
            Stats = stats;
            Type = type;
            Price = initialPrice;
        }

        public decimal Price { get; private set; }
        public decimal CostPerPiece { get; private set; }
        public long TotalSupply { get; set; } = 0;
        public long SalesThisMonth { get; set; }
        public long ProductionThisMonth { get; set; }
        public decimal ProfitThisMonth { get; set; }
        public long LastSales { get; private set; }
        public long LastProd { get; private set; }
        public decimal LastProfit { get; private set; }
        private decimal CurrentCapacityUsed { get; set; }
        private decimal SupplyQuarterlyCurrent { get; set; }
        private decimal ProdQuarterlyCurrent { get; set; }
        private decimal SalesQuarterlyCurrent { get; set; }
        private decimal CapacityQuarterlyCurrent { get; set; }
        public ProductType Type { get; }

        public void MontlyData(UpdateEpisodeType type, List<decimal> data = null)
        {
            if (type == UpdateEpisodeType.Initialize)
            {
                Price = data[0];
                CurrentCapacityUsed = 1;
            }
            else if (type == UpdateEpisodeType.Update)
            {
                SupplyQuarterlyCurrent += TotalSupply;
                ProdQuarterlyCurrent += ProductionThisMonth;
                SalesQuarterlyCurrent += SalesThisMonth;
                CapacityQuarterlyCurrent += CurrentCapacityUsed;

                LastProd = ProductionThisMonth;
                LastProfit = ProfitThisMonth;
                LastSales = SalesThisMonth;
                CostPerPiece = data[0];
                CurrentCapacityUsed = data[1];
                Price = data[8];

                Stats.PriceTrend.Add((double) data[2]);
                Stats.SupplyTrend.Add((double) data[3]);
                Stats.ProfitTrend.Add((double) data[4]);
                Stats.SalesTrend.Add((double) data[5]);
                Stats.ProductionTrend.Add((double) data[6]);
                Stats.CppTrend.Add((double) data[7]);

                Stats.PriceTotal.Add((double) Price);
                Stats.SupplyTotal.Add(TotalSupply);
                Stats.ProducedTotal.Add(ProductionThisMonth);
                Stats.SalesTotal.Add(SalesThisMonth);
                Stats.ProfitTotal.Add((double) ProfitThisMonth);
                Stats.CppTotal.Add((double) CostPerPiece);
            }
            else if (type == UpdateEpisodeType.Reset)
            {
                ProductionThisMonth = 0;
                SalesThisMonth = 0;
                ProfitThisMonth = 0;
            }
        }

        public void QuarterlyData(UpdateEpisodeType type)
        {
            if (type == UpdateEpisodeType.Update)
            {
            }
            else if (type == UpdateEpisodeType.Reset)
            {
                SupplyQuarterlyCurrent = 0;
                ProdQuarterlyCurrent = 0;
                SalesQuarterlyCurrent = 0;
                CapacityQuarterlyCurrent = 0;
            }
        }
    }
}