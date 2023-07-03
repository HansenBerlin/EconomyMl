namespace NewScripts
{
    public class Decision
    {
        public decimal PriceFood { get; }
        public decimal PriceLuxury { get; }
        public float RessourceDistribution { get; }
        public int WorkerChange { get; }
        public decimal Wage { get; }
        public bool AdjustWages { get; }
        
        public Decision(decimal priceFood, decimal priceLuxury, float ressourceDistribution, int workerChange,
            decimal wage, bool adjustWages)
        {
            PriceFood = priceFood;
            PriceLuxury = priceLuxury;
            RessourceDistribution = ressourceDistribution;
            WorkerChange = workerChange;
            Wage = wage;
            AdjustWages = adjustWages;    
        }
    }
}