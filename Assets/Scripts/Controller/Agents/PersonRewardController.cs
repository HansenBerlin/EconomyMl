using Agents;
using Controller.Data;
using Enums;

namespace Controller.Agents
{
    public class PersonRewardController
    {
        private readonly NormalizationController _normController;
        private readonly PersonObservations _observations;

        public PersonRewardController(NormalizationController normController, PersonObservations observations)
        {
            _normController = normController;
            _observations = observations;
            _normController.AddNew(nameof(_observations.Happiness), NormRange.Two, 0);
            _normController.AddNew(nameof(_observations.BaseBuyReward), NormRange.Two, 0);
            _normController.AddNew(nameof(_observations.LuxuryBuyReward), NormRange.Two, 0);
            _normController.AddNew(nameof(_observations.JobReward), NormRange.Two, 0);
        }

        public float CombinedReward()
        {
            float capitalFactor = _observations.Capital > 0 ? 0.5f : -0.5F;
            float capitalFactor2 = _observations.Capital > 100000 ? 0.5F : 0F;
            var expenseFactor = (float) (_observations.MonthlyExpensesAccumulatedForYear >
                                         _observations.MonthlyIncomeAccumulatedForYear
                ? -0.5
                : 0.5);
            float jobFactor = _observations.JobStatus switch
            {
                JobStatus.Unemployed => -0.2F,
                JobStatus.Employed => 0.4F,
                _ => 0
            };
            float baseDemandFulfilled = _observations.UnsatisfiedBaseDemand > 0 ? -0.2f : 0.5f;
            float luxury = _observations.LuxuryProducts > 12 ? 0.2f : _observations.LuxuryProducts > 0 ? 0.1f : -0.05f;

            float val = capitalFactor + capitalFactor2 + jobFactor + expenseFactor + baseDemandFulfilled + luxury +
                        _observations.JobReward / 12 + _observations.BaseBuyReward / 12 +
                        _observations.LuxuryBuyReward / 12;
            float valNorm = _normController.Normalize(nameof(_observations.Happiness), val);
            return valNorm;
        }

        public void RewardForBaseProductSatisfaction(long amountBought, long demanded)
        {
            float val = demanded - amountBought == 0 ? 0.5F : -0.5F;
            _observations.BaseBuyReward += _normController.Normalize(nameof(_observations.BaseBuyReward), val);
        }

        public void RewardForLuxuryProductSatisfaction(long amountBought, long demanded)
        {
            float demandFactor = demanded - amountBought == 0 ? 1F : 0F;
            float overBoughFactor = _observations.Capital < 0 ? -1F : 0F;
            float val = _normController.Normalize(nameof(_observations.LuxuryBuyReward),
                demandFactor + overBoughFactor);
            _observations.LuxuryBuyReward += val;
        }

        public void RewardForJobChange(decimal salaryBefore, decimal salaryAfter, bool isUnemployed,
            bool isDecisionSkipped)
        {
            float reward = 0;
            if (isDecisionSkipped && isUnemployed) reward = -0.5F;

            if (salaryBefore > salaryAfter)
            {
                var val = (float) (salaryAfter / salaryBefore - 1);
                val += isUnemployed ? 2f : 0;
                reward += val;
            }

            if (salaryBefore == 0 && salaryAfter > 0)
            {
                reward += 0.05f;
            }
            else
            {
                var val = (float) (salaryAfter / salaryBefore - 1);
                reward += val;
            }

            _observations.JobReward += _normController.Normalize(nameof(_observations.JobReward), reward);
        }
    }
}