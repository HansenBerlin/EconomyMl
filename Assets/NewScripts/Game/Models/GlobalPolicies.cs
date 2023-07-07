namespace NewScripts.Game.Models
{
    public class GlobalPolicies
    {
        public decimal MinimumWage { get; set; } = 50;
        public float TaxRate { get; set; } = 0.5F;
        public float SubsidyRate { get; set; } = 0.2F;
        public float FoodStamprate { get; set; } = 0.3F;
        public float SocialWelfareRate { get; set; } = 0.5F;
    }
}