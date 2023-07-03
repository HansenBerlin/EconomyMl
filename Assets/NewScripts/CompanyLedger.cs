namespace NewScripts
{
    public class CompanyData
    {
        public CompanyData(int companyId, int month, int year, int lifetime)
        {
            CompanyId = companyId;
            Month = month;
            Year = year;
            Lifetime = lifetime;
        }

        public int CompanyId { get; }
        public int Lifetime { get; }
        public int Reputation { get; set; }
        public int Month { get; }
        public int Year { get; }
        public BookKeepingLedger Books { get; set; }
        public ProductLedger Food { get; set; }
        public ProductLedger Luxury { get; set; }
        public WorkersLedger Workers { get; set; }
        public DecisionLedger Decision { get; set; }
    }

    public class BookKeepingLedger
    {
        public BookKeepingLedger(decimal liquidityStart)
        {
            LiquidityStart = liquidityStart;
        }

        public decimal LiquidityStart { get; }
        public decimal LiquidityEndCheck { get; set; }
        public decimal Income { get; set;  }
        public decimal TaxPayments { get; set; }
        public decimal WagePayments { get; set; }
    }

    public class ProductLedger
    {
        public ProductLedger(decimal priceSet, int stockStart)
        {
            PriceSet = priceSet;
            StockStart = stockStart;
        }

        public decimal PriceSet { get; }
        public int StockStart { get; }
        public int Production { get; set; }
        public int Destroyed { get; set; }
        public int Sales { get; set; } 
        public int StockEndCheck { get; set; }
    }

    public class WorkersLedger
    {
        public WorkersLedger(int startCount, decimal offeredWage, decimal averageWage)
        {
            StartCount = startCount;
            OfferedWage = offeredWage;
            AverageWage = averageWage;
        }

        public int StartCount { get; }
        public int EndCount { get; set; }
        public decimal OfferedWage { get; }
        public int Hired { get; set; }
        public int FiredByDecision { get; set; }
        public int FiredByLackOfFunds { get; set; }
        public int Quit { get; set; }
        public int OpenPositions { get; set; }
        public int ReducedPaidCount { get; set; }
        public int UnpaidCount { get; set; }
        public decimal AverageWage { get; }
    }

    public class DecisionLedger
    {
        public DecisionLedger(Decision decision)
        {
            FireWorkers = decision.WorkerChange < 0 ? decision.WorkerChange * -1 : 0;
            OpenPositions = decision.WorkerChange > 0 ? decision.WorkerChange : 0;
            SetWorkerWage = decision.Wage;
            SetFoodPrice = decision.PriceFood;
            SetLuxuryPrice = decision.PriceLuxury;
        }

        public int FireWorkers { get; }
        public int OpenPositions { get; }
        public decimal SetWorkerWage { get; }
        public decimal SetFoodPrice { get; }
        public decimal SetLuxuryPrice { get; }
    }
}