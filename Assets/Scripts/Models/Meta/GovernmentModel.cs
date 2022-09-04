using Repositories;
using Settings;
using Unity.MLAgents;

namespace Models.Meta
{



    public class GovernmentModel
    {
        public readonly FederalServicesPolicy _policy;
        private decimal Gdp;
        public float IncomeTaxRate => _policy.IncomeTaxRate;
        public float ProfitTaxRate => _policy.ProfitTaxRate;
        public float ConsumerTaxRate => _policy.ConsumerTaxRate;
        public float WorkerSalary => _policy.FederalWorkerSalary;
        public decimal ServiceUnitsNeededPerPop => _policy.ServiceUnitsPerPersonInPopulation;
        public decimal Capital { get; set; } = 50000000;
        public decimal RetirementFundCapital { get; set; }
        public decimal PublicServicePaymentsInMonth { get; set; }
        public decimal RetirementPaymentsInMonth { get; set; }
        public decimal UnemployedPaymentsInMonth { get; set; }
        public decimal IncomeTaxInMonth { get; set; }
        public decimal ProfitTaxInMonth { get; set; }
        public decimal ConsumerTaxInMonth { get; set; }
        public decimal LastBalance = 50000000;

        private readonly GovernmentDataRepository _data;

        public GovernmentModel(FederalServicesPolicy policy, GovernmentDataRepository data)
        {
            _policy = policy;
            _data = data;
        }

        public void UpdateData()
        {
            _data.Balance.Add((double) Capital);
            _data.ConsumerTaxes.Add((double) ConsumerTaxInMonth);
            _data.IncomeTaxes.Add((double) IncomeTaxInMonth);
            _data.ProfitTaxes.Add((double) ProfitTaxInMonth);
            Academy.Instance.StatsRecorder.Add("GOV/capital", (float)Capital);
            Academy.Instance.StatsRecorder.Add("GOV/consumertax", (float)ConsumerTaxInMonth);
            Academy.Instance.StatsRecorder.Add("GOV/incometax", (float)IncomeTaxInMonth);
            Academy.Instance.StatsRecorder.Add("GOV/profittax", (float)ProfitTaxInMonth);
            _data.PublicServiceCosts.Add((double) PublicServicePaymentsInMonth);
            _data.RetiredCosts.Add((double) RetirementPaymentsInMonth);
            _data.UnemployedCosts.Add((double) UnemployedPaymentsInMonth);
            _data.TotalIncome.Add((double) (ConsumerTaxInMonth + IncomeTaxInMonth + ProfitTaxInMonth));
            _data.TotalExpenses.Add((double) (PublicServicePaymentsInMonth + RetirementPaymentsInMonth + UnemployedPaymentsInMonth));
        }

        public void Reset()
        {
            LastBalance = Capital;
            ConsumerTaxInMonth = 0;
            ProfitTaxInMonth = 0;
            IncomeTaxInMonth = 0;
            PublicServicePaymentsInMonth = 0;
            RetirementPaymentsInMonth = 0;
            UnemployedPaymentsInMonth = 0;
        }



    }
}