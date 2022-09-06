using Agents;
using Controller.RepositoryController;
using Enums;
using Interfaces;

namespace Controller.Agents
{
    public class PersonJobAction : IPersonAction
    {
        private readonly JobMarketController _jobMarket;
        private PersonObservations _observations;
        private PersonController _personController;
        private PersonRewardController _rewardController;

        public PersonJobAction(JobMarketController jobMarket)
        {
            _jobMarket = jobMarket;
        }

        public void Init(PersonObservations observations, PersonRewardController rewardController,
            PersonController personController)
        {
            _observations = observations;
            _rewardController = rewardController;
            _personController = personController;
        }

        public void SearchForNewJob(decimal desiredSalary)
        {
            decimal salary = _observations.Salary;
            decimal newSalary = UpdateJob(desiredSalary, salary);

            _rewardController.RewardForJobChange(salary, newSalary, false, false);
        }

        private decimal UpdateJob(decimal desiredSalary, decimal salary)
        {
            var job = _jobMarket.FindAvailableJob(desiredSalary);
            if (job.Status == JobPositionStatus.Taken)
            {
                _personController.UpdateNewJob(job);
                return job.Salary;
            }

            return salary;
        }

        public void QuitJobAndStayUnemployed()
        {
            bool isUnemployed = _observations.JobStatus == JobStatus.Employed;
            decimal salary = _observations.Salary;
            _personController.QuitJob();
            decimal salaryAfter = _observations.Salary;
            _rewardController.RewardForJobChange(salary, salaryAfter, isUnemployed, false);
        }

        public void DoNothing()
        {
            bool isUnemployed = _observations.JobStatus == JobStatus.Unemployed;
            _rewardController.RewardForJobChange(_observations.Salary, _observations.Salary, isUnemployed,
                true);
        }
    }
}