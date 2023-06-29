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
        public int Month { get; }
        public int Year { get; }
        public BookKeepingLedger Books { get; set; }
        public ProductLedger Product { get; set; }
        public WorkersLedger Workers { get; set; }
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
        public int Sales { get; set; } 
        public int StockEndCheck { get; set; }
    }

    public class WorkersLedger
    {
        public WorkersLedger(int workersStart, decimal wageSet, decimal averageWage)
        {
            WorkersStart = workersStart;
            WageSet = wageSet;
            AverageWage = averageWage;
        }

        public int WorkersStart { get; }
        public decimal WageSet { get; }
        public int WorkersHired { get; set; }
        public int WorkersFired { get; set; }
        public int WorkersQuit { get; set; }
        public int OpenPositions { get; set; }
        public int WorkersPaid { get; set; }
        public int WorkersUnpaid { get; set; }
        public decimal AverageWage { get; }
    }
}