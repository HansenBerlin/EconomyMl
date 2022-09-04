using Enums;
using Models.Observations;

namespace Controller
{
    public class PersonRespawnController
    {
        private decimal Capital { get; set; }
        private decimal Salary { get; set; }

        private decimal DesiredSalary { get; set; }
       
        public long LuxuryProducts { get; set; }

        private JobStatus JobStatus { get; set; }
        private int Age { get; set; }

        public PersonRespawnController(PersonObservations observations)
        {
            Capital = observations.Capital;
            Salary = observations.Salary;
            DesiredSalary = observations.DesiredSalary;
            JobStatus = observations.JobStatus;
            Age = observations.Age;
        }

        public void Reset(PersonObservations observations)
        {
            observations.Capital = Capital / 2;
            observations.Salary = Salary / 2;
            observations.DesiredSalary = DesiredSalary;
            observations.JobStatus = JobStatus.Unemployed;
            //observations.Age = Age;
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