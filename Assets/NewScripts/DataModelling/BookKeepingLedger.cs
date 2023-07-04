namespace NewScripts.DataModelling
{
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
}