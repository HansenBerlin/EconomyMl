using Repositories;
using Settings;
using Unity.MLAgents;

namespace Models.Meta
{



    public class GovernmentModel
    {
        public readonly FederalServicesPolicy _fedPolicy;
        public readonly FederalUnemployedPaymentPolicy _workerPolicy;
        private decimal Gdp;
        public float IncomeTaxRate => _fedPolicy.IncomeTaxRate;
        public float ProfitTaxRate => _fedPolicy.ProfitTaxRate;
        public float ConsumerTaxRate => _fedPolicy.ConsumerTaxRate;
        public float WorkerSalary => _fedPolicy.FederalWorkerSalary;
        public float MinUnemploymentSupport => _workerPolicy.UnemployedSupportMin;
        public float MaxUnemploymentSupport => _workerPolicy.UnemployedSupportMax;
        public float UnemploymentSupportQuote => _workerPolicy.UnemployedSupportRate;
        public float RetirementSupportSupportQuote => _workerPolicy.RetirementSupportRate;
        public decimal ServiceUnitsNeededPerPop => _fedPolicy.ServiceUnitsPerPersonInPopulation;
        public decimal Capital { get; set; } = 40000000;
        public decimal RetirementFundCapital { get; set; }
        public decimal PublicServicePaymentsInYear { get; set; }
        public decimal RetirementPaymentsInYear { get; set; }
        public decimal UnemployedPaymentsInMonth { get; set; }
        public decimal IncomeTaxInYear { get; set; }
        public decimal ProfitTaxInMonth { get; set; }
        public decimal ConsumerTaxInYear { get; set; }
        public decimal LastBalance = 50000000;

        private readonly GovernmentDataRepository _data;

        public GovernmentModel(PoliciesWrapper policies, GovernmentDataRepository data)
        {
            _fedPolicy = policies.FederalPolicies;
            _workerPolicy = policies.federalUnemployedPaymentPolicies;
            _data = data;
        }

        public void UpdateData()
        {
            _data.Balance.Add((double) Capital);
            _data.ConsumerTaxes.Add((double) ConsumerTaxInYear);
            _data.IncomeTaxes.Add((double) IncomeTaxInYear);
            _data.ProfitTaxes.Add((double) ProfitTaxInMonth);
            Academy.Instance.StatsRecorder.Add("GOV/capital", (float)Capital);
            Academy.Instance.StatsRecorder.Add("GOV/consumertax", (float)ConsumerTaxInYear);
            Academy.Instance.StatsRecorder.Add("GOV/incometax", (float)IncomeTaxInYear);
            Academy.Instance.StatsRecorder.Add("GOV/profittax", (float)ProfitTaxInMonth);
            _data.PublicServiceCosts.Add((double) PublicServicePaymentsInYear);
            _data.RetiredCosts.Add((double) RetirementPaymentsInYear);
            _data.UnemployedCosts.Add((double) UnemployedPaymentsInMonth);
            _data.TotalIncome.Add((double) (ConsumerTaxInYear + IncomeTaxInYear + ProfitTaxInMonth));
            _data.TotalExpenses.Add((double) (PublicServicePaymentsInYear + RetirementPaymentsInYear + UnemployedPaymentsInMonth));
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