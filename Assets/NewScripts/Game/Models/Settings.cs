using NewScripts.Enums;

namespace NewScripts.Game.Models
{
    public class Settings
    {
        public decimal TotalMoneySupply { get; set; }
        public bool IsTraining { get; set; }
        public bool IsGovernmentTraining { get; set; }
        public bool WriteToDatabase { get; set; }
        public int FoodOutputMultiplier { get; } = 300;
        public int LuxuryOutputMultiplier { get; } = 20;
        public int FoodDemandModifier { get; } = 3;
        public int LuxuryDemandModifier { get; } = 1;
        public bool IsMusicOn { get; set; } = false;
        public bool IsIsometricCameraActive { get; set; } = false;
        public bool IsPaused { get; set; }
        public decimal LowerWageBoundary { get; set; } = 100;
        public decimal UpperWageBoundary { get; set; } = 400;
        public decimal LowerPriceBoundaryFood { get; set; } = 0.2M;
        public decimal UpperPriceBoundaryFood { get; set; } = 5;
        public decimal LowerPriceBoundaryLuxury { get; set; } = 1M;
        public decimal UpperPriceBoundaryLuxury { get; set; } = 50;
        public int LowerJobPositionsBoundary { get; set; } = 1;
        public int UpperJobPositionsBoundary { get; set; } = 50;
        public bool IsAutoPlay { get; set; } = false;
        
        public int DemandModifier(ProductType type)
        {
            return type == ProductType.Food ? FoodDemandModifier : LuxuryDemandModifier;
        }
        
        public int OutputMultiplier(ProductType type)
        {
            return type == ProductType.Food ? FoodOutputMultiplier : LuxuryOutputMultiplier;
        }
    }
}