using Enums;
using Models.Observations;
using UnityEngine;

namespace Controller.Rewards
{
    public class PersonRewardController : MonoBehaviour
    {
        private readonly PersonObservations _observations;

        public PersonRewardController(PersonObservations observations)
        {
            _observations = observations;
        }

        public float CombinedReward()
        {
            var capitalFactor = _observations.Capital > 0 ? 0.1F : -0.2F;
            var expenseFactor = _observations.MonthlyExpenses > _observations.MonthlyIncome ? -0.1F : 0.1F;
            var jobFactor = _observations.JobStatus != JobStatus.Unemployed ? 0.2F : -0.2F;
            _observations.JobReward = 0;
            _observations.BaseBuyReward = 0;
            _observations.LuxuryBuyReward = 0;
            _observations.CapitalReward = 0;
            return capitalFactor + jobFactor + expenseFactor;
        }
        
        public void RewardForBaseProductSatisfaction(int amountBought, int demanded)
        {
            _observations.BaseBuyReward = demanded - amountBought == 0 ? 0.1F : 0;
        }
        
        public void RewardForLuxuryProductSatisfaction(int amountBought, int demanded)
        {
            var demandFactor = demanded - amountBought == 0 ? 0.1F : 0F;
            var amountFactor = amountBought * 0.5F / (_observations.LuxuryProducts + 1);
            var overBoughFactor = _observations.Capital < 0 ? 0.5F : 0;
            _observations.LuxuryBuyReward = demandFactor + amountFactor + overBoughFactor;
        }

        public void RewardForJobChange(decimal salaryBefore, decimal salaryAfter, bool isUnemployed, bool isDecisionSkipped)
        {
            if (isDecisionSkipped && isUnemployed)
            {
                _observations.JobReward = -0.1F;
            }
            if (isDecisionSkipped && isUnemployed == false)
            {
                _observations.JobReward = +0.01F;
            }
            if (salaryBefore > salaryAfter)
            {
                if (isUnemployed)
                {
                    _observations.JobReward = (float)(salaryAfter / salaryBefore - 1) + 0.1F;
                }
                _observations.JobReward = (float)(salaryAfter / salaryBefore - 1);
            }
            if (salaryBefore < salaryAfter)
            {
                _observations.JobReward = (float)(salaryBefore / salaryAfter - 1);
            }
            
        }
    }
}