using Policies;
using Repositories;
using Unity.MLAgents;

namespace Models
{
    public class GovernmentModel
    {
        private readonly GovernmentDataRepository _data;
        public readonly FederalServicesPolicy FedPolicy;
        public readonly FederalUnemployedPaymentPolicy WorkerPolicy;
        private decimal _gdp;
        public decimal LastBalance = 50000000;

        public GovernmentModel(PoliciesWrapper policies, GovernmentDataRepository data)
        {
            FedPolicy = policies.FederalPolicies;
            WorkerPolicy = policies.FederalUnemployedPaymentPolicies;
            _data = data;
        }

        public float IncomeTaxRate => FedPolicy.incomeTaxRate;
        public float ProfitTaxRate => FedPolicy.profitTaxRate;
        public float ConsumerTaxRate => FedPolicy.consumerTaxRate;
        public float WorkerSalary => FedPolicy.federalWorkerSalary;
        public decimal ServiceUnitsNeededPerPop => FedPolicy.serviceUnitsPerPersonInPopulation;
        public decimal Capital { get; set; } = 40000000;
        public decimal PublicServicePaymentsInYear { get; set; }
        public decimal RetirementPaymentsInYear { get; set; }
        public decimal UnemployedPaymentsInMonth { get; set; }
        public decimal IncomeTaxInYear { get; set; }
        public decimal ProfitTaxInMonth { get; set; }
        public decimal ConsumerTaxInYear { get; set; }

        public void UpdateData()
        {
            _data.Balance.Add((double) Capital);
            _data.ConsumerTaxes.Add((double) ConsumerTaxInYear);
            _data.IncomeTaxes.Add((double) IncomeTaxInYear);
            _data.ProfitTaxes.Add((double) ProfitTaxInMonth);
            Academy.Instance.StatsRecorder.Add("GOV/capital", (float) Capital);
            Academy.Instance.StatsRecorder.Add("GOV/consumertax", (float) ConsumerTaxInYear);
            Academy.Instance.StatsRecorder.Add("GOV/incometax", (float) IncomeTaxInYear);
            Academy.Instance.StatsRecorder.Add("GOV/profittax", (float) ProfitTaxInMonth);
            _data.PublicServiceCosts.Add((double) PublicServicePaymentsInYear);
            _data.RetiredCosts.Add((double) RetirementPaymentsInYear);
            _data.UnemployedCosts.Add((double) UnemployedPaymentsInMonth);
            _data.TotalIncome.Add((double) (ConsumerTaxInYear + IncomeTaxInYear + ProfitTaxInMonth));
            _data.TotalExpenses.Add((double) (PublicServicePaymentsInYear + RetirementPaymentsInYear +
                                              UnemployedPaymentsInMonth));
        }

        public void Reset()
        {
            LastBalance = Capital;
            ConsumerTaxInYear = 0;
            ProfitTaxInMonth = 0;
            IncomeTaxInYear = 0;
            PublicServicePaymentsInYear = 0;
            RetirementPaymentsInYear = 0;
            UnemployedPaymentsInMonth = 0;
        }
    }
}