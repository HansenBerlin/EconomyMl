namespace NewScripts
{
    public class JobContract
    {
        private readonly Worker _worker;
        private readonly Company _employer;
        private readonly double _wage;

        public JobContract(Worker worker, Company company, double wage)
        {
            _worker = worker;
            _employer = company;
            _wage = wage;
            _employer.AddContract(this);
            _worker.AddContract(this);
        }

        public void PayWorker()
        {
            _worker.Money += _wage;
            _employer.Liquidity -= _wage;
        }
    }
}