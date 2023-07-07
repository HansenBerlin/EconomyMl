using NewScripts.Enums;

namespace NewScripts.DataModelling
{
    public class Aggregate
    {
        protected Aggregate(int month, int year)
        {
            Month = month;
            Year = year;
        }

        public int Month { get; }
        public int Year { get; }
    }
    
    public class CompaniesAggregate : Aggregate
    {
        public decimal AverageWageOffer { get; private set; }
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
        }
    }
    
    public class HouseholdsAggregate : Aggregate
    {
        public decimal AveragePurchasingPower { get; private set; }
        public double AverageDemandFood { get; private set; }
        public double AverageDemandLuxury { get; private set; }
        public double OverallEmploymentRate => _overallEmploymentRate * 100;
        public double ShortTimeWorkingRate => _shortTimeWorkingRate * 100;
        public double FullyEmployedWorkingRate => _fullyEmployedWorkingRate * 100;
        public decimal AveragePriceBidFood { get; private set; }
        public decimal AveragePriceBidLuxury { get; private set; }
        public decimal AverageFulltimeWage { get; private set; }
        public decimal AverageShortWorkWage { get; private set; }
        public decimal AverageReservationWage { get; private set; }
        public int AverageFoodInventoryBeforeBuying { get; private set; }
        public int AverageLuxuryInventoryBeforeBuying { get; private set; }
        private int Population { get; set; }
        private int EmployedPopulation { get; set; }
        private double _overallEmploymentRate;
        private double _shortTimeWorkingRate;
        private double _fullyEmployedWorkingRate;
        
        public HouseholdsAggregate(int month, int year) : base(month, year) { }
        
        public void UpdateHouseholdData(HouseholdData data)
        {
            Population++;
            EmployedPopulation += data.JobStatus != WorkerJobStatus.Unemployed ? 1 : 0;
            AveragePurchasingPower = (AveragePurchasingPower * (Population - 1) + data.MoneyAvailableAdBidTime) / Population;
            AverageDemandFood = (AverageDemandFood * (Population - 1) + (data.DemandFood > 0 && data.PriceBidFood > 0 ? data.DemandFood : 0)) / Population;
            AverageDemandLuxury = (AverageDemandLuxury * (Population - 1) + (data.DemandLuxury > 0 && data.PriceBidLuxury > 0 ? data.DemandLuxury : 0)) / Population;
            _overallEmploymentRate = (_overallEmploymentRate * (Population - 1) + (data.JobStatus != WorkerJobStatus.Unemployed ? 1 : 0)) / Population;
            _fullyEmployedWorkingRate = (_fullyEmployedWorkingRate * (Population - 1) + (data.JobStatus == WorkerJobStatus.FullyEmployed ? 1 : 0)) / Population;
            _shortTimeWorkingRate = (_shortTimeWorkingRate * (Population - 1) + (data.JobStatus == WorkerJobStatus.ShortTimeWork ? 1 : 0)) / Population;
            if (data.JobStatus == WorkerJobStatus.FullyEmployed)
            {
                AverageFulltimeWage = (AverageFulltimeWage * (EmployedPopulation - 1) + data.RealWage) / EmployedPopulation;
            }
            if (data.JobStatus == WorkerJobStatus.ShortTimeWork)
            {
                AverageShortWorkWage = (AverageShortWorkWage * (EmployedPopulation - 1) + data.RealWage / 2) / EmployedPopulation;
            }
            AveragePriceBidFood = (AveragePriceBidFood * (Population - 1) + (data.PriceBidFood > 0 && data.DemandFood > 0 ? data.PriceBidFood : 0)) / Population;
            AveragePriceBidLuxury = (AveragePriceBidLuxury * (Population - 1) + (data.PriceBidLuxury > 0 && data.DemandLuxury > 0 ? data.PriceBidLuxury : 0)) / Population;
            AverageReservationWage = (AverageReservationWage * (Population - 1) + data.ReservationWage) / Population;
            AverageFoodInventoryBeforeBuying = (AverageFoodInventoryBeforeBuying * (Population - 1) + data.FoodInventoryBeforeBuying) / Population;
            AverageLuxuryInventoryBeforeBuying = (AverageLuxuryInventoryBeforeBuying * (Population - 1) + data.LuxuryInventoryBeforeBuying) / Population;
        }
    }

    public class HouseholdData
    {
        public decimal MoneyAvailableAdBidTime { get; set; }
        public decimal PriceBidFood { get; set; }
        public decimal PriceBidLuxury { get; set; }
        public decimal RealWage { get; set; }
        public decimal ReservationWage { get; set; }
        public int DemandFood { get; set; }
        public int DemandLuxury { get; set; }
        public int FoodInventoryBeforeBuying { get; set; }
        public int LuxuryInventoryBeforeBuying { get; set; }
        public WorkerJobStatus JobStatus { get; set; }
    }
}