using EconomyBase.Models.Observations;
using EconomyBase.Models.Population;

namespace EconomyBase.Controller.Rewards
{



    public class PersonRewardController
    {
        private readonly PersonObservations _observations;

        public PersonRewardController(PersonObservations observations)
        {
            _observations = observations;
        }

        public void RewardForBaseProductSatisfaction(int amountBought, int demanded)
        {

        }

        public void RewardForJobChange(decimal salaryBefore, decimal salaryAfter, bool wasUnemployed,
            bool isDecisionSkipped)
        {
            // gleich: negativ

        }


    }
}