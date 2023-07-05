using NewScripts.Enums;

namespace NewScripts.DataModelling
{
    public class ProductDistributionInfo
    { 
        public decimal Value { get; }
        public ProductDistributionType Type { get; }
        public ProductDistributionInfo(decimal value, ProductDistributionType type)
        {
            Value = value;
            Type = type;
        }
    }
}