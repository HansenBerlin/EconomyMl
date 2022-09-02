using Controller.Rewards;
using Enums;
using Models.Observations;

namespace Controller.Actions
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

        public void SearchForNewJobFromEmployedWithSlightlyIncreasedDemandedSalary(PersonObservations observations, PersonRewardController rewardController, PersonController personController, decimal desiredSalary)
        {
            decimal salary = observations.MonthlyIncome;
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
            decimal salary = observations.MonthlyIncome;
            personController.QuitJob();
            decimal salaryAfter = observations.MonthlyIncome;
            rewardController.RewardForJobChange(salary, salaryAfter, isUnemployed, false, observations);
        }

        public void DoNothing(PersonObservations observations, PersonRewardController rewardController)
        {
            bool isUnemployed = observations.JobStatus == JobStatus.Unemployed;
            rewardController.RewardForJobChange(observations.MonthlyIncome, observations.MonthlyIncome, isUnemployed,
                true, observations);

        }
    }
}