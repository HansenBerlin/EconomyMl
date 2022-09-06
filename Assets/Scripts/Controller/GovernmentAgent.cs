using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.Models.Meta;
using Assets.Scripts.Models.Population;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

namespace Assets.Scripts.Controller
{
    public class GovernmentAgent : Agent
    {
        private GovernmentModel _government;
        private PopulationModel _population;
        private NormalizationController _normCtr;

        public void Init(GovernmentModel government, PopulationModel population, NormalizationController normCtr)
        {
            _government = government;
            _population = population;
            _normCtr = normCtr;
            SetupObservations();
        }
        
        private void SetupObservations()
        {
            _normCtr.AddNew(nameof(_population.LastAvgAge), NormRange.One, _population.LastAvgAge);
            _normCtr.AddNew(nameof(_population.LastHappiness), NormRange.Two, _population.LastAvgAge);
            _normCtr.AddNew(nameof(_population.LastEmploymentRate), NormRange.One, _population.LastAvgAge);
            _normCtr.AddNew(nameof(_population.LastAvgCapital), NormRange.Two, _population.LastAvgAge);
            _normCtr.AddNew(nameof(_government.LastBalance), NormRange.Two, (float)_government.LastBalance);
            _normCtr.AddNew(nameof(_government.Capital), NormRange.Two, (float)_government.Capital);
            _normCtr.AddNew(nameof(_government.WorkerSalary), NormRange.One, _government.WorkerSalary);
            _normCtr.AddNew(nameof(_government.ConsumerTaxRate), NormRange.One, _government.ConsumerTaxRate);
            _normCtr.AddNew(nameof(_government.ProfitTaxRate), NormRange.One, _government.ProfitTaxRate);
            _normCtr.AddNew(nameof(_government.IncomeTaxRate), NormRange.One, _government.IncomeTaxRate);
            _normCtr.AddNew(nameof(_government.IncomeTaxInYear), NormRange.One, (float)_government.IncomeTaxInYear);
            _normCtr.AddNew(nameof(_government.ProfitTaxInMonth), NormRange.One, (float)_government.ProfitTaxInMonth);
            _normCtr.AddNew(nameof(_government.ConsumerTaxInYear), NormRange.One, (float)_government.ConsumerTaxInYear);
            _normCtr.AddNew(nameof(_government.PublicServicePaymentsInYear), NormRange.One, (float)_government.PublicServicePaymentsInYear);
            _normCtr.AddNew(nameof(_government.RetirementPaymentsInYear), NormRange.One, (float)_government.RetirementPaymentsInYear);
           
        }
        
        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(_normCtr.Normalize(nameof(_population.LastAvgAge), _population.LastAvgAge));
            sensor.AddObservation(_normCtr.Normalize(nameof(_population.LastHappiness), _population.LastHappiness));
            sensor.AddObservation(_normCtr.Normalize(nameof(_population.LastEmploymentRate), _population.LastEmploymentRate));
            sensor.AddObservation(_normCtr.Normalize(nameof(_population.LastAvgCapital), _population.LastAvgCapital));
            sensor.AddObservation(_normCtr.Normalize(nameof(_government.LastBalance), (float)_government.LastBalance));
            sensor.AddObservation(_normCtr.Normalize(nameof(_government.Capital), (float)_government.Capital));
            sensor.AddObservation(_normCtr.Normalize(nameof(_government.WorkerSalary), _government.WorkerSalary));
            sensor.AddObservation(_normCtr.Normalize(nameof(_government.ConsumerTaxRate), _government.ConsumerTaxRate));
            sensor.AddObservation(_normCtr.Normalize(nameof(_government.ProfitTaxRate), _government.ProfitTaxRate));
            sensor.AddObservation(_normCtr.Normalize(nameof(_government.IncomeTaxRate), _government.IncomeTaxRate));
            sensor.AddObservation(_normCtr.Normalize(nameof(_government.IncomeTaxInYear), (float)_government.IncomeTaxInYear));
            sensor.AddObservation(_normCtr.Normalize(nameof(_government.ProfitTaxInMonth), (float)_government.ProfitTaxInMonth));
            sensor.AddObservation(_normCtr.Normalize(nameof(_government.ConsumerTaxInYear), (float)_government.ConsumerTaxInYear));
            sensor.AddObservation(_normCtr.Normalize(nameof(_government.PublicServicePaymentsInYear), (float)_government.PublicServicePaymentsInYear));
            sensor.AddObservation(_normCtr.Normalize(nameof(_government.RetirementPaymentsInYear), (float)_government.RetirementPaymentsInYear));
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var setIncomeTax = (actionBuffers.ContinuousActions[0] + 1) / 4; // min 0, max 50%
            var setConsumerTax = (actionBuffers.ContinuousActions[1] + 1) / 4;
            var setProfitTax = (actionBuffers.ContinuousActions[2] + 1) / 2;  // min 0 max 100%
            var setUnemployedRatePayment = (actionBuffers.ContinuousActions[3] + 1) / 2;
            var setRetiredRatePayment = (actionBuffers.ContinuousActions[4] + 1) / 2;
            var changeWorkerPayments = 1 + actionBuffers.ContinuousActions[5];
            var changeUnemployedMinPayments = 1 + actionBuffers.ContinuousActions[6];
            var changeUnemployedMaxPayments = 1 + actionBuffers.ContinuousActions[7];
            changeWorkerPayments = changeWorkerPayments < 0.8f ? 0.8f : changeWorkerPayments > 1.2f ? 1.2f : changeWorkerPayments;
            changeUnemployedMinPayments = changeUnemployedMinPayments < 0.8f ? 0.8f : changeUnemployedMinPayments > 1.2f ? 1.2f : changeUnemployedMinPayments;
            changeUnemployedMaxPayments = changeUnemployedMaxPayments < 0.8f ? 0.8f : changeUnemployedMaxPayments > 1.2f ? 1.2f : changeUnemployedMaxPayments;

            _government._fedPolicy.IncomeTaxRate = setIncomeTax;
            _government._fedPolicy.ConsumerTaxRate = setConsumerTax;
            _government._fedPolicy.ProfitTaxRate = setProfitTax;
            _government._fedPolicy.FederalWorkerSalary *= changeWorkerPayments;
            _government._workerPolicy.UnemployedSupportRate = setUnemployedRatePayment;
            _government._workerPolicy.RetirementSupportRate = setRetiredRatePayment;
            _government._workerPolicy.UnemployedSupportMin *= changeUnemployedMinPayments;
            _government._workerPolicy.UnemployedSupportMax *= changeUnemployedMaxPayments;
            _government._workerPolicy.UnemployedSupportMax = 
                _government._workerPolicy.UnemployedSupportMax < _government._workerPolicy.UnemployedSupportMin
                ? _government._workerPolicy.UnemployedSupportMin : _government._workerPolicy.UnemployedSupportMax;
        }

        public void MakeDecision()
        {
            RequestDecision();
            Academy.Instance.EnvironmentStep();
        }
        
        public void EndYear()
        {
            var happinessReward = _population.LastHappiness / _population.PopulationCount;
            var fundsReward = _government.LastBalance < _government.Capital ? 0.2 : -0.2;
            var enemployedReward = _population.LastEmploymentRate > 0.9 ? 0.3 
                : _population.LastEmploymentRate > 0.8 ? 0.1 
                : _population.LastEmploymentRate > 0.4 ? -0.2 : -0.5;
            var totalReward = (float) (happinessReward + fundsReward + enemployedReward);
            AddReward(totalReward);
            EndEpisode();

            _government.UpdateData();
            _government.Reset();
        }

        public decimal PayIncomeTax(decimal baseAmount)
        {
            decimal tax = baseAmount * (decimal)_government.IncomeTaxRate;
            _government.Capital += tax;
            _government.IncomeTaxInYear += tax;
            return tax;
        }

        public decimal PayConsumerTax(decimal baseAmount)
        {
            decimal tax = baseAmount * (decimal)_government.ConsumerTaxRate;
            _government.Capital += tax;
            _government.ConsumerTaxInYear += tax;
            return tax;
        }

        public decimal PayProfitTax(decimal baseAmount)
        {
            decimal tax = baseAmount > 0 ? (decimal)_government.ProfitTaxRate * baseAmount : 0;
            _government.Capital += tax;
            _government.ProfitTaxInMonth += tax;
            return tax;
        }

        public decimal GetFederalMoneyForService(decimal costs)
        {
            _government.Capital -= costs;
            _government.PublicServicePaymentsInYear += costs;
            return costs;
        }

        public void PayoutRetired()
        {
            decimal totalPaid = _population.AgeRangeRetired.Sum(p => p.Pay());
            _government.Capital -= totalPaid;
            _government.RetirementPaymentsInYear += totalPaid;
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