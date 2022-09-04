using Enums;
using Models.Observations;
using UnityEngine;

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

        public void Reset()
        {
            minC = 0;
            maxC = 0;
            minJ = 0;
            maxJ = 0;
            minB = 0;
            maxB = 0;
            minL = 0;
            maxL = 0;
        }
        
        private float NormalizeCombined(float value)
        {
            minC = value < minC ? value : minC;
            maxC = value > maxC ? value : maxC;
            if (maxC - minC == 0)
            {
                return 0;
            }
            float norm = 2 * ((value - minC) / (maxC - minC)) - 1;
            return norm;
        }
        
        private float NormalizeWork(float value)
        {
            minJ = value < minJ ? value : minJ;
            maxJ = value > maxJ ? value : maxJ;
            if (maxJ - minJ == 0)
            {
                return 0;
            }
            float norm = 2 * ((value - minJ) / (maxJ - minJ)) - 1;
            return norm;
        }
        
        private float NormalizeBase(float value)
        {
            minB = value < minB ? value : minB;
            maxB = value > maxB ? value : maxB;
            if (maxB - minB == 0)
            {
                return 0;
            }
            float norm = 2 * ((value - minB) / (maxB - minB)) - 1;
            return norm;
        }
        
        private float NormalizeLux(float value)
        {
            minL = value < minL ? value : minL;
            maxL = value > maxL ? value : maxL;
            if (maxL - minL == 0)
            {
                return 0;
            }
            float norm = 2 * ((value - minL) / (maxL - minL)) - 1;
            return norm;
        }
        
        public float CombinedReward(PersonObservations observations)
        {
            var capitalFactor = observations.Capital > 0 ? 0.5f : -0.5F;
            var capitalFactor2 = observations.Capital > 100000 ? 0.5F : 0F;
            float expenseFactor = (float)(observations.MonthlyExpensesAccumulatedForYear > - observations.MonthlyIncomeAccumulatedForYear ? -0.5 : 0.5);
            var jobFactor = observations.JobStatus == JobStatus.Unemployed ? -0.2F : observations.JobStatus == JobStatus.Employed ? 0.4F : 0;
            float baseDemandFulfilled = observations.UnsatisfiedBaseDemand > 0 ? - 0.2f : 0.5f;
            float luxury = observations.LuxuryProducts > 12 ? 0.2f : observations.LuxuryProducts > 0 ? 0.1f : -0.05f;
            
            var val = NormalizeCombined(capitalFactor + capitalFactor2 + jobFactor + expenseFactor + baseDemandFulfilled + luxury +
                                        observations.JobReward / 12 + observations.BaseBuyReward / 12 + observations.LuxuryBuyReward / 12);
            return val;
        }
        
        public void RewardForBaseProductSatisfaction(long amountBought, long demanded, PersonObservations observations)
        {
            observations.BaseBuyReward += NormalizeBase(demanded - amountBought == 0 ? 0.5F : -0.5F);
        }
        
        public void RewardForLuxuryProductSatisfaction(long amountBought, long demanded, PersonObservations observations)
        {
            var demandFactor = demanded - amountBought == 0 ? 1F : 0F;
            var overBoughFactor = observations.Capital < 0 ? -1F : 0F;
            var val = NormalizeLux(demandFactor + overBoughFactor);
            observations.LuxuryBuyReward += val;
        }

        public void RewardForJobChange(decimal salaryBefore, decimal salaryAfter, bool isUnemployed, bool isDecisionSkipped, PersonObservations observations)
        {
            if (isDecisionSkipped && isUnemployed)
            {
                observations.JobReward += -0.5F;
            }
            
            if (salaryBefore > salaryAfter)
            {
                if (isUnemployed)
                {
                    float val = (float) (salaryAfter / salaryBefore - 1) + 0.2F;
                    observations.JobReward += NormalizeWork(val);
                }
                else
                {
                    float val = (float)(salaryAfter / salaryBefore - 1);
                    observations.JobReward += NormalizeWork(val);
                }
            }
            if (salaryBefore < salaryAfter)
            {
                float val = (float)(salaryAfter / salaryBefore - 1);
                observations.JobReward += NormalizeWork(val);
            }
            
        }
    }
}