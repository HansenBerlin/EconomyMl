using Enums;
using Models.Observations;

namespace Controller.Agents
{
    public class PersonRespawnController
    {
        public decimal Capital { get; }
        private decimal Salary { get; }
        private decimal DesiredSalary { get; }

        public PersonRespawnController(PersonObservations observations)
        {
            Capital = observations.Capital;
            Salary = observations.Salary;
            DesiredSalary = observations.DesiredSalary;
        }

        public void Reset(PersonObservations observations)
        {
            observations.Salary = Salary / 2;
            observations.DesiredSalary = DesiredSalary;
            observations.JobStatus = JobStatus.Unemployed;
            observations.JobReward = 0;
            observations.LuxuryProducts = 0;
            observations.BaseBuyReward = 0;
            observations.LastMonthExpenses = 0;
            observations.LuxuryBuyReward = 0;
            observations.ThisMonthExpenses = 0;
            observations.UnsatisfiedBaseDemand = 0;
            observations.MonthlyExpensesAccumulatedForYear = 0;
            observations.MonthlyIncomeAccumulatedForYear = 0;
        }
    }
}