using NewScripts.Enums;
using Unity.MLAgents;

namespace NewScripts
{
    public class JobContract
    {
        public ICompany Employer { get; }
        public decimal Wage { get; set; }
        public int RunsFor { get; set; }
        public bool IsForceReduced { get; private set; }
        private Worker Worker { get; }

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
            Academy.Instance.StatsRecorder.Add("New/Worker-Payment-Regular", (float)Wage);
            Worker.Money += Wage;
            Employer.Liquidity -= Wage;
            IsForceReduced = false;
        }

        public void PayReducedWage()
        {
            Academy.Instance.StatsRecorder.Add("New/Worker-Payment-Reduced", (float)Wage/2);
            decimal reducedWage = Wage / 2;
            Worker.Money += reducedWage;
            Employer.Liquidity -= reducedWage;
            IsForceReduced = true;
        }

        public void QuitContract(WorkerFireReason reason)
        {
            Academy.Instance.StatsRecorder.Add("Contract/WorkQuit", ++ServiceLocator.Instance.LaborMarket.CountRemoved);

            Employer.RemoveContract(this, reason);
            Worker.RemoveJobContract(this, reason);
            IsForceReduced = false;
            ServiceLocator.Instance.LaborMarket.RemoveContract(this);
        }
    }
}