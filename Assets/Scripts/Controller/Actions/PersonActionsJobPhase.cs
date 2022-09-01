using Controller.Rewards;
using Enums;
using Models.Observations;

namespace Controller.Actions
{



    public class PersonActionsJobPhase : IPersonAction
    {
        private readonly JobMarketController _jobMarket;
        private readonly PopulationController _population;
        //private readonly PersonRewardController _rewardController;
        //private readonly PersonObservations _observations;
        private decimal AverageIncome => -_population.AverageWorkerIncome();

        public PersonActionsJobPhase(JobMarketController jobMarket, PopulationController population)
        {
            _jobMarket = jobMarket;
            _population = population;
        }

        public void SearchForNewJobFromEmployedWithSlightlyIncreasedDemandedSalary(PersonObservations observations, PersonRewardController rewardController, PersonController personController)
        {
            decimal desiredSalary = observations.MonthlyIncome * 1.1M;
            decimal salary = observations.MonthlyIncome;
            decimal newSalary = UpdateJob(desiredSalary, personController);

            rewardController.RewardForJobChange(salary, newSalary, false, false);

        }

        public void SearchForNewJobFromEmployedWithHighIncreasedDemandedSalary(PersonObservations observations, PersonRewardController rewardController, PersonController personController)
        {
            decimal desiredSalary = observations.MonthlyIncome * 1.5M;
            decimal salary = observations.MonthlyIncome;
            decimal newSalary = UpdateJob(desiredSalary, personController);

            rewardController.RewardForJobChange(salary, newSalary, false, false);
        }

        public void SearchForNewJobFromUnemployedWithAverageDemandedSalary(PersonObservations observations, PersonRewardController rewardController, PersonController personController)
        {
            decimal desiredSalary = AverageIncome;
            decimal salary = observations.MonthlyIncome;
            decimal newSalary = UpdateJob(desiredSalary, personController);

            rewardController.RewardForJobChange(salary, newSalary, true, false);
        }

        public void SearchForNewJobFromUnemployedWithMinimumDemandedSalary(PersonObservations observations, PersonRewardController rewardController, PersonController personController)
        {
            decimal desiredSalary = observations.MonthlyIncome * 1.05M;
            decimal salary = observations.MonthlyIncome;
            decimal newSalary = UpdateJob(desiredSalary, personController);

            rewardController.RewardForJobChange(salary, newSalary, true, false);
        }

        public void SearchForNewJobFromUnemployedWithAboveAverageDemandedSalary(PersonObservations observations, PersonRewardController rewardController, PersonController personController)
        {
            decimal desiredSalary = AverageIncome * 1.2M;
            decimal salary = observations.MonthlyIncome;
            decimal newSalary = UpdateJob(desiredSalary, personController);

            rewardController.RewardForJobChange(salary, newSalary, true, false);
        }

        private decimal UpdateJob(decimal desiredSalary, PersonController personController)
        {
            var job = _jobMarket.FindAvailableJob(desiredSalary);
            if (job.Status == JobPositionStatus.Taken)
            {
                personController.UpdateNewJob(job);
            }

            return job.Salary;
        }

        public void QuitJobAndStayUnemployed(PersonObservations observations, PersonRewardController rewardController, PersonController personController)
        {
            bool isUnemployed = observations.JobStatus == JobStatus.Employed;
            decimal salary = observations.MonthlyIncome;
            personController.QuitJob();
            decimal salaryAfter = observations.MonthlyIncome;
            rewardController.RewardForJobChange(salary, salaryAfter, isUnemployed, false);
        }

        public void DoNothing(PersonObservations observations, PersonRewardController rewardController)
        {
            bool isUnemployed = (observations.JobStatus != JobStatus.Employed && observations.JobStatus != JobStatus.Retired);
            rewardController.RewardForJobChange(observations.MonthlyIncome, observations.MonthlyIncome, isUnemployed,
                true);

        }
    }
}