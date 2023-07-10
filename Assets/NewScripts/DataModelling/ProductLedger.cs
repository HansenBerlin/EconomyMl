namespace NewScripts.DataModelling
{
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
        public decimal Income { get; set; } 
        public int StockEndCheck { get; set; }
    }
}