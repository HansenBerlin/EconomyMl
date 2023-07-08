namespace NewScripts.DataModelling
{
    public class GovernmentLedger
    {
        public float InflationRate { get; set; }
        public float Gdp { get; set; }
        public float CompanyTaxRate { get; set; }
        public float RichTaxRate { get; set; }
        public float SubsidyRate { get; set; }
        public float FoodStamprate { get; set; }
        public float SocialWelfareRate { get; set; }
        public decimal MinimumWage { get; set; }
        public decimal Liquidity { get; set; }
        public int FoodSupply { get; set; }
    }
}