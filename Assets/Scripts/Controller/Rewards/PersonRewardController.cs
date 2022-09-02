using Enums;
using Models.Observations;

namespace Controller.Rewards
{
    public class PersonRewardController
    {
        private float minC;
        private float maxC;
        private float minJ;
        private float maxJ;
        private float minB;
        private float maxB;
        private float minL;
        private float maxL;
        
        private float NormalizeCombined(float value)
        {
            minC = value < minC ? value : minC;
            maxC = value > maxC ? value : maxC;
            float norm = 2 * ((value - minC) / (maxC - minC)) - 1;
            return norm;
        }
        
        private float NormalizeWork(float value)
        {
            minJ = value < minJ ? value : minJ;
            maxJ = value > maxJ ? value : maxJ;
            float norm = 2 * ((value - minJ) / (maxJ - minJ)) - 1;
            return norm;
        }
        
        private float NormalizeBase(float value)
        {
            minB = value < minB ? value : minB;
            maxB = value > maxB ? value : maxB;
            float norm = 2 * ((value - minB) / (maxB - minB)) - 1;
            return norm;
        }
        
        private float NormalizeLux(float value)
        {
            minL = value < minL ? value : minL;
            maxL = value > maxL ? value : maxL;
            float norm = 2 * ((value - minL) / (maxL - minL)) - 1;
            return norm;
        }
        
        public float CombinedReward(PersonObservations observations)
        {
            var capitalFactor = observations.Capital > 0 ? 100F : -200F;
            var capitalFactor2 = observations.Capital > 100000 ? 1000F : 0F;
            float expenseFactor = (float)(observations.MonthlyIncome - observations.MonthlyExpenses);
            var jobFactor = observations.JobStatus != JobStatus.Unemployed ? 1000F : -500F;
            var baseDemandFulfilled = observations.UnsatisfiedBaseDemand * -10;
            var luxury = observations.LuxuryProducts < 100 ? observations.LuxuryProducts * 100 : 100;
            observations.JobReward = 0;
            observations.BaseBuyReward = 0;
            observations.LuxuryBuyReward = 0;
            observations.CapitalReward = 0;
            return NormalizeCombined(capitalFactor + capitalFactor2 + jobFactor + expenseFactor + baseDemandFulfilled + luxury);
        }
        
        public void RewardForBaseProductSatisfaction(int amountBought, int demanded, PersonObservations observations)
        {
            observations.BaseBuyReward = NormalizeBase(demanded - amountBought == 0 ? 0.2F : -0.1F);
        }
        
        public void RewardForLuxuryProductSatisfaction(int amountBought, int demanded, PersonObservations observations)
        {
            var demandFactor = demanded - amountBought == 0 ? 0.1F : 0F;
            var amountFactor = amountBought * 0.5F / (observations.LuxuryProducts + 1);
            var overBoughFactor = observations.Capital < 0 ? -0.2F : 0.05F;
            observations.LuxuryBuyReward = NormalizeLux(demandFactor + amountFactor + overBoughFactor);
        }

        public void RewardForJobChange(decimal salaryBefore, decimal salaryAfter, bool isUnemployed, bool isDecisionSkipped, PersonObservations observations)
        {
            if (isDecisionSkipped && isUnemployed)
            {
                observations.JobReward = -0.2F;
            }
            if (isDecisionSkipped && isUnemployed == false)
            {
                observations.JobReward = -0.01F;
            }
            if (salaryBefore > salaryAfter)
            {
                if (isUnemployed)
                {
                    float val = (float) (salaryAfter / salaryBefore - 1) + 0.2F;
                    observations.JobReward = NormalizeWork(val);
                }
                else
                {
                    float val = (float)(salaryAfter / salaryBefore - 1);
                    observations.JobReward = NormalizeWork(val);
                }
            }
            if (salaryBefore < salaryAfter)
            {
                float val = (float)(salaryAfter / salaryBefore - 1);
                observations.JobReward = NormalizeWork(val);
            }
            
        }
    }
}