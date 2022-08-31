using System;
using System.Collections.Generic;
using Controller;
using Enums;
using Models.Meta;
using Repositories;

namespace Models.Production
{



    public class ProductModel : IEpisodeUpdateController
    {
        public readonly ProductDataRepository Stats;
        private readonly EnvironmentModel _environment;
        public int Year => _environment.Year;
        public int Month => _environment.Month;
        public int Day => _environment.Day;

        public decimal Price { get; private set; }

        //public decimal PriceAverageAllTime { get; private set; }
        public decimal CostPerPiece { get; private set; }

        //public decimal CostPerPieceAverageAllTime { get; private set; }
        public long TotalSupply { get; set; } = 0;

        //public long SupplyAverageAllTime { get; private set; }
        public long SalesThisMonth { get; set; }

        //public long SalesAverageAllTime { get; private set; }
        public long ProductionThisMonth { get; set; }

        //public long ProductionAverageAllTime { get; private set; }
        public decimal ProfitThisMonth { get; set; }

        //public decimal ProfitAverageAllTime { get; private set; }
        public long LastSales { get; private set; }
        public long LastProd { get; private set; }
        public decimal LastCpp { get; private set; }
        public decimal LastPrice { get; private set; }
        public long LastSupply { get; private set; }
        public decimal LastProfit { get; private set; }
        public decimal LastCapacityUsed { get; private set; }
        public decimal CurrentCapacityUsed { get; private set; }
        public decimal SupplyQuarterlyLast { get; private set; }
        public decimal ProdQuarterlyLast { get; private set; }
        public decimal SalesQuarterlyLast { get; private set; }
        public decimal CapacityQuarterlyLast { get; private set; }
        public decimal SupplyQuarterlyCurrent { get; private set; }
        public decimal ProdQuarterlyCurrent { get; private set; }
        public decimal SalesQuarterlyCurrent { get; private set; }
        public decimal CapacityQuarterlyCurrent { get; private set; }
        public ProductType Type { get; }


        public ProductModel(ProductDataRepository stats, EnvironmentModel environment, ProductType type,
            decimal initialPrice)
        {
            Stats = stats;
            _environment = environment;
            Type = type;
            Price = initialPrice;
        }

        public void DailyData(UpdateEpisodeType type)
        {
            /*if (type == UpdateEpisodeType.Update)
            {
                ProfitThisMonth += DailyProfit;
            }
            else if (type == UpdateEpisodeType.Reset)
            {
                DailyProfit = 0;
            }*/
        }




        public void YearlyData(UpdateEpisodeType type)
        {
            throw new NotImplementedException();
        }

        public void MontlyData(UpdateEpisodeType type, List<decimal> data = null)
        {
            if (type == UpdateEpisodeType.Initialize)
            {
                Price = data[0];
                CurrentCapacityUsed = 1;
            }
            else if (type == UpdateEpisodeType.Update)
            {
                /*PriceAverageAllTime = (Price + PriceAverageAllTime * (Month - 1)) / Month;
                SupplyAverageAllTime = (TotalSupply + SupplyAverageAllTime * (Month - 1)) / Month;
                ProductionAverageAllTime = (ProductionThisMonth + ProductionAverageAllTime * (Month - 1)) / Month;
                SalesAverageAllTime = (SalesThisMonth + SalesAverageAllTime * (Month - 1)) / Month;
                ProfitAverageAllTime = (ProfitThisMonth + ProfitAverageAllTime * (Month - 1)) / Month;
                CostPerPieceAverageAllTime = (CostPerPiece + CostPerPieceAverageAllTime * (Month - 1)) / Month;*/

                SupplyQuarterlyCurrent += TotalSupply;
                ProdQuarterlyCurrent += ProductionThisMonth;
                SalesQuarterlyCurrent += SalesThisMonth;
                CapacityQuarterlyCurrent += CurrentCapacityUsed;

                LastSupply = TotalSupply;
                LastCpp = CostPerPiece;
                LastPrice = Price;
                LastProd = ProductionThisMonth;
                LastProfit = ProfitThisMonth;
                LastSales = SalesThisMonth;
                LastCapacityUsed = CurrentCapacityUsed;
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
                Stats.SupplyTotal.Add((double) TotalSupply);
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
                SupplyQuarterlyLast = SupplyQuarterlyCurrent;
                ProdQuarterlyLast = ProdQuarterlyCurrent;
                SalesQuarterlyLast = SalesQuarterlyCurrent;
                CapacityQuarterlyLast = CapacityQuarterlyCurrent;
                SupplyQuarterlyCurrent = 0;
                ProdQuarterlyCurrent = 0;
                SalesQuarterlyCurrent = 0;
                CapacityQuarterlyCurrent = 0;
            }
        }
    }
}