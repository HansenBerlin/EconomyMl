using NewScripts.Enums;

namespace NewScripts.DataModelling
{
    public class HouseholdData
    {
        public decimal MoneyAvailableAdBidTime { get; set; }
        public decimal MoneyAtEndOfMonth { get; set; }
        public decimal RichTaxPaid { get; set; }
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