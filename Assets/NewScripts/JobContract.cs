using Unity.MLAgents;

namespace NewScripts
{
    public class JobContract
    {
        private Worker Worker { get; }
        public ICompany Employer { get; }
        public decimal Wage { get; private set; }
        public int RunsFor { get; set; }
        public bool IsForceReduced { get; set; }

        public JobContract(Worker worker, ICompany company, decimal wage)
        {
            Worker = worker;
            Employer = company;
            Wage = wage;
            Employer.AddContract(this);
            Worker.AddContract(this);
        }

        public void PayWorker()
        {
            Worker.Money += Wage;
            Employer.Liquidity -= Wage;
            IsForceReduced = false;
        }

        public void ReduceWage()
        {
            Wage = Wage / 2 > 20 ? Wage / 2 : 20;
            IsForceReduced = true;
        }
        
        
        
        public void QuitContract(bool isQuitByEmployer = false)
        {
            Academy.Instance.StatsRecorder.Add("Contract/WorkQuit", ++ServiceLocator.Instance.LaborMarket.CountRemoved);

            Employer.RemoveContract(this);
            Worker.RemoveJobContract(this, isQuitByEmployer);
        }
    }
}