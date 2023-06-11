namespace NewScripts
{
    public class Worker
    {
        public float Money { get; set; } = 100;
        public int FoodDemand => FoodDemandModifier();
        private readonly int _foodDemand = 10;
        public int LuxuryDemand => LuxuryDemandModifier();
        private readonly int _luxuryDemand = 1;
        public int Health { get; set; } = 1000;
        public int CompanyId { get; set; }

        private int LuxuryDemandModifier()
        {
            return Money > 100 ? _luxuryDemand + 1 : Money > 1000 ? _luxuryDemand * 3 : int.MaxValue;
        }
        
        private int FoodDemandModifier()
        {
            return (int)(Health < 100 ? _foodDemand * 2 :  Health < 500 ? _foodDemand * 1.5 : _foodDemand);
        }
    }
}