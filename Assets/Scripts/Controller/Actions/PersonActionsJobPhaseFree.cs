using Assets.Scripts.Controller.Rewards;
using Assets.Scripts.Enums;
using Assets.Scripts.Models.Observations;

namespace Assets.Scripts.Controller.Actions
{



    public class PersonActionsJobPhaseFree : IPersonAction
    {
        private readonly JobMarketController _jobMarket;
        //private readonly PersonRewardController _rewardController;
        //private readonly PersonObservations _observations;
        //private decimal AverageIncome => -_population.AverageWorkerIncome();

        public PersonActionsJobPhaseFree(JobMarketController jobMarket)
        {
            _jobMarket = jobMarket;
        }

        public void SearchForNewJob(PersonObservations observations, PersonRewardController rewardController, PersonController personController, decimal desiredSalary)
        {
            decimal salary = observations.Salary;
            decimal newSalary = UpdateJob(desiredSalary, salary, personController);

            rewardController.RewardForJobChange(salary, newSalary, false, false, observations);
        }

        private decimal UpdateJob(decimal desiredSalary, decimal salary, PersonController personController)
        {
            var job = _jobMarket.FindAvailableJob(desiredSalary);
            if (job.Status == JobPositionStatus.Taken)
            {
                personController.UpdateNewJob(job);
                return job.Salary;
            }

            return salary;

        }

        public void QuitJobAndStayUnemployed(PersonObservations observations, PersonRewardController rewardController, PersonController personController)
        {
            bool isUnemployed = observations.JobStatus == JobStatus.Employed;
            decimal salary = observations.Salary;
            personController.QuitJob();
            decimal salaryAfter = observations.Salary;
            rewardController.RewardForJobChange(salary, salaryAfter, isUnemployed, false, observations);
        }

        public void DoNothing(PersonObservations observations, PersonRewardController rewardController)
        {
            bool isUnemployed = observations.JobStatus == JobStatus.Unemployed;
            rewardController.RewardForJobChange(observations.Salary, observations.Salary, isUnemployed,
                true, observations);

        }
    }
}