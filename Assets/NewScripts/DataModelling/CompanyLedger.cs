namespace NewScripts.DataModelling
{
    public class CompanyLedger
    {
        public CompanyLedger(int companyId, string companyName, int month, int year, int lifetime)
        {
            CompanyId = companyId;
            CompanyName = companyName;
            Month = month;
            Year = year;
            Lifetime = lifetime;
        }

        public int CompanyId { get; }
        public string CompanyName { get; }
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
}