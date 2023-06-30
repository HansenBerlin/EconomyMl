namespace NewScripts
{
    public class HouseholdsAggregate
    {
        public HouseholdsAggregate(int month, int year)
        {
            Month = month;
            Year = year;
        }

        public int Month { get; }
        public int Year { get; }
        public decimal AveragePurchasingPower { get; private set; }
        public double AverageDemand { get; private set; }
        public double EmploymentRate { get; private set; }
        public int Population { get; private set; }
        
        public void Update(HouseholdLedger data)
        {
            Population++;
            AveragePurchasingPower = (AveragePurchasingPower * (Population - 1) + data.MoneyAvailable) / Population;
            AverageDemand = (AverageDemand * (Population - 1) + data.Demand) / Population;
            EmploymentRate = (EmploymentRate * (Population - 1) + (data.IsEmployed ? 1 : 0)) / Population;
        }
    }

    public class HouseholdLedger
    {
        public HouseholdLedger(bool isEmployed, int demand, decimal moneyAvailable)
        {
            IsEmployed = isEmployed;
            Demand = demand;
            MoneyAvailable = moneyAvailable;
        }

        public decimal MoneyAvailable { get; }
        public int Demand { get; }
        public bool IsEmployed { get; }
    }
}