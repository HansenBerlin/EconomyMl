using Controller.Rewards;
using Enums;
using Models.Observations;

namespace Controller.Actions
{



    public class PersonActionsJobPhase : IPersonAction
    {
        private readonly JobMarketController _jobMarket;
        private readonly PopulationController _population;
        private readonly PersonRewardController _rewardController;
        private readonly PersonController _personController;
        private readonly PersonObservations _observations;
        private decimal AverageIncome => -_population.AverageWorkerIncome();

        public PersonActionsJobPhase(JobMarketController jobMarket, PopulationController population,
            PersonRewardController rewardController, PersonController personController, PersonObservations observations)
        {
            _jobMarket = jobMarket;
            _population = population;
            _rewardController = rewardController;
            _personController = personController;
            _observations = observations;
        }

        public void SearchForNewJobFromEmployedWithSlightlyIncreasedDemandedSalary()
        {
            decimal desiredSalary = _observations.MonthlyIncome * 1.1M;
            decimal salary = _observations.MonthlyIncome;
            decimal newSalary = UpdateJob(desiredSalary);

            _rewardController.RewardForJobChange(salary, newSalary, false, false);

        }

        public void SearchForNewJobFromEmployedWithHighIncreasedDemandedSalary()
        {
            decimal desiredSalary = _observations.MonthlyIncome * 1.5M;
            decimal salary = _observations.MonthlyIncome;
            decimal newSalary = UpdateJob(desiredSalary);

            _rewardController.RewardForJobChange(salary, newSalary, false, false);
        }

        public void SearchForNewJobFromUnemployedWithAverageDemandedSalary()
        {
            decimal desiredSalary = AverageIncome;
            decimal salary = _observations.MonthlyIncome;
            decimal newSalary = UpdateJob(desiredSalary);

            _rewardController.RewardForJobChange(salary, newSalary, true, false);
        }

        public void SearchForNewJobFromUnemployedWithMinimumDemandedSalary()
        {
            decimal desiredSalary = _observations.MonthlyIncome * 1.05M;
            decimal salary = _observations.MonthlyIncome;
            decimal newSalary = UpdateJob(desiredSalary);

            _rewardController.RewardForJobChange(salary, newSalary, true, false);
        }

        public void SearchForNewJobFromUnemployedWithAboveAverageDemandedSalary()
        {
            decimal desiredSalary = AverageIncome * 1.2M;
            decimal salary = _observations.MonthlyIncome;
            decimal newSalary = UpdateJob(desiredSalary);

            _rewardController.RewardForJobChange(salary, newSalary, true, false);
        }

        private decimal UpdateJob(decimal desiredSalary)
        {
            var job = _jobMarket.FindAvailableJob(desiredSalary);
            if (job.Status == JobPositionStatus.Taken)
            {
                _personController.UpdateNewJob(job);
            }

            return job.Salary;
        }

        public void QuitJobAndStayUnemployed()
        {
            bool isUnemployed = _observations.JobStatus == JobStatus.Employed;
            decimal salary = _observations.MonthlyIncome;
            _personController.QuitJob();
            decimal salaryAfter = _observations.MonthlyIncome;
            _rewardController.RewardForJobChange(salary, salaryAfter, isUnemployed, false);
        }

        public void DoNothing()
        {
            bool isUnemployed = _observations.JobStatus == JobStatus.Employed;
            _rewardController.RewardForJobChange(_observations.MonthlyIncome, _observations.MonthlyIncome, isUnemployed,
                true);

        }
    }
}