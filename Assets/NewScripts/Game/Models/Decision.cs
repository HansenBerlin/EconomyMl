namespace NewScripts.Game.Models
{
    public class Decision
    {
        public decimal PriceFood { get; set; } = 1;
        public decimal PriceLuxury { get; set; } = 10;
        public float RessourceDistribution { get; set; } = 1;
        public int WorkerChange { get; set; } = 0;
        public decimal Wage { get; set; } = 150;
        public bool AdjustWages { get; set; }
    }
}