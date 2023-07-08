using NewScripts.Enums;

namespace NewScripts.DataModelling
{
    public class HouseholdsAggregate : Aggregate
    {
        public decimal AveragePurchasingPower { get; private set; }
        public decimal AverageMoneyAtMonthEnd { get; private set; }
        public decimal AverageRichTaxPaid { get; private set; }
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
            AverageMoneyAtMonthEnd = (AverageMoneyAtMonthEnd * (Population - 1) + data.MoneyAtEndOfMonth) / Population;
            AverageRichTaxPaid = (AverageRichTaxPaid * (Population - 1) + data.RichTaxPaid) / Population;
        }
    }
}