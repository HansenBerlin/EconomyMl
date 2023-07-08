namespace NewScripts.DataModelling
{
    public class CompaniesAggregate : Aggregate
    {
        public decimal AverageWageOffer { get; private set; }
        public decimal AverageTaxPaid { get; private set; }
        public decimal AveragePriceOfferFood { get; private set; }
        public double AverageSupplyFood { get; private set; }
        public int AverageStockFood { get; private set; }
        public int AverageSalesFood { get; private set; }
        public decimal AveragePriceOfferLuxury { get; private set; }
        public double AverageSupplyLuxury { get; private set; }
        public int AverageStockLuxury { get; private set; }
        public int AverageSalesLuxury { get; private set; }
        public decimal AverageLiquidity { get; private set; }
        public double AverageReputation => (_averageReputation + 1) * 100;
        public int AverageLifetime { get; private set; }
        public int AverageOpenPositions { get; private set; }
        public int AverageFiredWorkersTotal { get; private set; }
        public int AverageFiredWorkersByDecision { get; private set; }
        public int AverageFiredWorkersByLackOfFunds { get; private set; }
        private int Companys { get; set; }
        private double _averageReputation;
        
        public CompaniesAggregate(int month, int year) : base(month, year) { }
        
        public void UpdateCompanyData(CompanyLedger ledger)
        {
            Companys++;
            AveragePriceOfferFood = (AveragePriceOfferFood * (Companys - 1) + ledger.Food.PriceSet) / Companys;
            AverageSupplyFood = (AverageSupplyFood * (Companys - 1) + ledger.Food.Production) / Companys;
            AverageStockFood = (AverageStockFood * (Companys - 1) + ledger.Food.StockStart) / Companys;
            AverageSalesFood = (AverageSalesFood * (Companys - 1) + ledger.Food.Sales) / Companys;
            AveragePriceOfferLuxury = (AveragePriceOfferLuxury * (Companys - 1) + ledger.Luxury.PriceSet) / Companys;
            AverageSupplyLuxury = (AverageSupplyLuxury * (Companys - 1) + ledger.Luxury.Production) / Companys;
            AverageStockLuxury = (AverageStockLuxury * (Companys - 1) + ledger.Luxury.StockStart) / Companys;
            AverageSalesLuxury = (AverageSalesLuxury * (Companys - 1) + ledger.Luxury.Sales) / Companys;
            
            AverageWageOffer = (AverageWageOffer * (Companys - 1) + ledger.Workers.OfferedWage) / Companys;
            AverageLiquidity = (AverageLiquidity * (Companys - 1) + ledger.Books.LiquidityStart) / Companys;
            _averageReputation = (_averageReputation * (Companys - 1) + ledger.Reputation) / Companys;
            AverageLifetime = (AverageLifetime * (Companys - 1) + ledger.Lifetime) / Companys;
            AverageOpenPositions = (AverageOpenPositions * (Companys - 1) + ledger.Workers.OpenPositions) / Companys;
            AverageFiredWorkersTotal = (AverageFiredWorkersTotal * (Companys - 1) + ledger.Workers.Quit + ledger.Workers.FiredByDecision + ledger.Workers.FiredByLackOfFunds) / Companys;
            AverageFiredWorkersByDecision = (AverageFiredWorkersByDecision * (Companys - 1) + ledger.Workers.FiredByDecision) / Companys;
            AverageFiredWorkersByLackOfFunds = (AverageFiredWorkersByLackOfFunds * (Companys - 1) + ledger.Workers.FiredByLackOfFunds) / Companys;
            AverageTaxPaid = (AverageTaxPaid * (Companys - 1) + ledger.Books.TaxPayments) / Companys;
        }
    }
}