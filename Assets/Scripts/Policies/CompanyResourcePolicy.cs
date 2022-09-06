namespace Policies
{
    public class CompanyResourcePolicy
    {
        public CompanyResourcePolicy(decimal maxSalary, decimal minSalary, int initialWorkers, decimal initialBalance,
            int initialresources)
        {
            MaxSalary = maxSalary;
            MinSalary = minSalary;
            InitialWorkers = initialWorkers;
            InitialBalance = initialBalance;
            InitialResources = initialresources;
        }

        public decimal MaxSalary { get; set; }
        public decimal MinSalary { get; set; }
        public int InitialWorkers { get; set; }
        public decimal InitialBalance { get; set; }
        public int InitialResources { get; set; }
    }
}