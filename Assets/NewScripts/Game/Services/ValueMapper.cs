using System;

namespace NewScripts.Game.Services
{
    public static class ValueMapper
    {
        public static (float subsidyRate, float socialWelfareRate, float foodStampRate) MapPolicyActions(
            float subsidityDecision, float socialWelfareDecision, float foodStampDecision)
        {
            subsidityDecision = (subsidityDecision + 1f) * 0.5f;
            socialWelfareDecision = (socialWelfareDecision + 1f) * 0.5f;
            foodStampDecision = (foodStampDecision + 1f) * 0.5f;
            float sum = Math.Abs(subsidityDecision) + Math.Abs(socialWelfareDecision) + Math.Abs(foodStampDecision);

            float subsidyRate = subsidityDecision / sum;
            float socialWlfareRate = socialWelfareDecision / sum;
            float foodStampRate = foodStampDecision / sum;
            return (subsidyRate, socialWlfareRate, foodStampRate);
        }
        
        public static float MapValue(float value, float minValue, float maxValue)
        {
            float mappedValue = (value + 1f) * 0.5f * (maxValue - minValue) + minValue;
            return mappedValue;
        }
    }
}