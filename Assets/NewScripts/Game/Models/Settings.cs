using NewScripts.Enums;

namespace NewScripts.Game.Models
{
    public class Settings
    {
        public decimal TotalMoneySupply { get; set; }
        public bool IsTraining { get; set; }
        public bool WriteToDatabase { get; set; }
        public int FoodOutputMultiplier { get; } = 300;
        public int LuxuryOutputMultiplier { get; } = 20;
        public int FoodDemandModifier { get; } = 3;
        public int LuxuryDemandModifier { get; } = 1;
        public bool IsMusicOn { get; set; } = false;
        public bool IsIsometricCameraActive { get; set; } = false;
        public bool IsPaused { get; set; }
        
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