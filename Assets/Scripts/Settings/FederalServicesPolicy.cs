namespace EconomyBase.Settings
{



    public record FederalServicesPolicy(int ServiceUnitsPerPersonInPopulation, decimal FederalWorkerSalary,
        decimal IncomeTaxRate, decimal ProfitTaxRate, decimal ConsumerTaxRate)
    {
        public int ServiceUnitsPerPersonInPopulation { get; } = ServiceUnitsPerPersonInPopulation;
        public decimal IncomeTaxRate { get; } = IncomeTaxRate;
        public decimal ProfitTaxRate { get; } = ProfitTaxRate;
        public decimal ConsumerTaxRate { get; } = ConsumerTaxRate;
        public decimal FederalWorkerSalary { get; } = FederalWorkerSalary;
    }
}