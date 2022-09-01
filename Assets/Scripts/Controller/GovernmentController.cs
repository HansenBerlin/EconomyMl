using System.Linq;
using Models.Meta;
using Models.Population;

namespace Controller
{



    public class GovernmentController
    {
        private readonly GovernmentModel _government;
        private readonly PopulationModel _population;

        public GovernmentController(GovernmentModel government, PopulationModel population)
        {
            _government = government;
            _population = population;
        }

        public decimal PayIncomeTax(decimal baseAmount)
        {
            decimal tax = baseAmount * (decimal)_government.IncomeTaxRate;
            _government.Capital += tax;
            _government.IncomeTaxInMonth += tax;
            return tax;
        }

        public decimal PayConsumerTax(decimal baseAmount)
        {
            decimal tax = baseAmount * (decimal)_government.ConsumerTaxRate;
            _government.Capital += tax;
            _government.ConsumerTaxInMonth += tax;
            return tax;
        }

        public decimal PayProfitTax(decimal baseAmount)
        {
            decimal tax = baseAmount > 0 ? (decimal)_government.ProfitTaxRate * baseAmount : 0;
            _government.Capital += tax;
            _government.ProfitTaxInMonth += tax;
            return tax;
        }

        public void EndMonth()
        {
            _government.UpdateData();
            _government.Reset();
        }

        public decimal GetFederalMoneyForService(decimal costs)
        {
            _government.Capital -= costs;
            _government.PublicServicePaymentsInMonth += costs;
            return costs;
        }

        public void PayoutRetired()
        {
            decimal totalPaid = _population.AgeRangeRetired.Sum(p => p.Pay());
            _government.Capital -= totalPaid;
            _government.RetirementPaymentsInMonth += totalPaid;
        }

        public void PayoutUnemployed()
        {
            decimal totalPaid = _population.UnemployedWorkers.Sum(p => p.Pay());
            _government.Capital -= totalPaid;
            _government.UnemployedPaymentsInMonth += totalPaid;
        }

        public int RecalculateFederalWorkerDemand()
        {
            return (int) (_government.ServiceUnitsNeededPerPop * _population.PopulationCount);
        }

        public decimal GetMaxFederalWorkerPayment()
        {
            return (decimal)_government.WorkerSalary;
        }

        public bool InvestInEfficientFederalServices()
        {
            if (_government.Capital <= 0) return false;
            var rn = StatisticalDistributionController.CreateRandom(0, 49);
            return rn == 1;
        }
    }
}