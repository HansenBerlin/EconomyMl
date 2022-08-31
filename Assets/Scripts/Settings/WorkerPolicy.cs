namespace EconomyBase.Settings
{



    public record WorkerPolicy(decimal GuaranteedIncome, decimal UnemployedSupportRate, decimal UnemployedSupportMax,
        decimal UnemployedSupportMin)
    {
        public decimal GuaranteedIncome { get; } = GuaranteedIncome;
        public decimal UnemployedSupportRate { get; } = UnemployedSupportRate;
        public decimal UnemployedSupportMax { get; } = UnemployedSupportMax;
        public decimal UnemployedSupportMin { get; } = UnemployedSupportMin;
    }
}