namespace NewScripts
{
    public class Worker
    {
        public float Money { get; set; } = 50;
        public int FoodDemand => FoodDemandModifier();
        private readonly int _foodDemand = 10;
        public int LuxuryDemand => LuxuryDemandModifier();
        private readonly int _luxuryDemand = 1;
        public int Health { get; set; } = 1000;
        public int CompanyId { get; set; }
        public bool IsCeo { get; set; }

        private int LuxuryDemandModifier()
        {
            return Health < 100 
                ? _luxuryDemand 
                : Money > 100 
                    ? _luxuryDemand + 1 
                    : Money > 1000 
                        ? _luxuryDemand * 3 
                        : int.MaxValue;
            return _luxuryDemand;
        }
        
        private int FoodDemandModifier()
        {
            return _foodDemand;
            //return (int)(Health < 100 ? _foodDemand * 2 :  Health < 500 ? _foodDemand * 1.5 : _foodDemand);
        }
    }
}