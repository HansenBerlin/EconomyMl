using System.Collections.Generic;
using System.Linq;
using Controller.Data;
using Enums;
using Models;
using Policies;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

namespace Agents
{
    public class BankAgent : Agent
    {
        private readonly List<BankAccountModel> _accounts = new();
        private readonly List<LoanModel> _loans = new();
        private CentralBankAgent _centralBank;
        private decimal _centralBankDeposit = 12000000;
        private int _lengthInMonthForNewCredits = 12;
        private CreditRating _minimumRatingForCredits = CreditRating.A;
        private NormalizationController _normController;
        private CentralBankPolicy _policy;
        private decimal TotalAssets => _loans.Sum(x => x.TotalSumLeft) + _centralBankDeposit;
        private decimal TotalLiabilities => _accounts.Sum(x => x.Savings);
        public float PositiveInterestRate { get; private set; } = 0.05F;
        private float NegativeInterestRate { get; set; } = 0.05F;
        private float EquityRatio => (float) (OwnCapital / (OwnCapital + TotalLiabilities));
        private decimal OwnCapital => TotalAssets - TotalLiabilities;


        public void Init(CentralBankPolicy policy, CentralBankAgent centralBankAgent,
            NormalizationController normController)
        {
            _policy = policy;
            _centralBank = centralBankAgent;
            _normController = normController;
            _normController.AddNew(nameof(TotalAssets), NormRange.One, (float) TotalAssets);
            _normController.AddNew(nameof(TotalLiabilities), NormRange.One, (float) TotalLiabilities);
            _normController.AddNew(nameof(PositiveInterestRate), NormRange.One, PositiveInterestRate);
            _normController.AddNew(nameof(NegativeInterestRate), NormRange.One, NegativeInterestRate);
            _normController.AddNew(nameof(_centralBankDeposit), NormRange.One, (float) _centralBankDeposit);
            _normController.AddNew(nameof(_lengthInMonthForNewCredits), NormRange.One, _lengthInMonthForNewCredits);
            _normController.AddNew(nameof(EquityRatio), NormRange.One, EquityRatio);
        }

        public void MakeDecision()
        {
            RequestDecision();
            Academy.Instance.EnvironmentStep();
        }

        public void AddRewards()
        {
            float totalReward = 0;
            if (EquityRatio >= _policy.minimumEquityRate)
                totalReward += 0.5f;
            else
                totalReward -= 0.5f;
            AddReward(totalReward);

            EndEpisode();

            Academy.Instance.StatsRecorder.Add("BANK/ASSETS", (float) TotalAssets);
            Academy.Instance.StatsRecorder.Add("BANK/LIABILITIES", (float) TotalLiabilities);
            Academy.Instance.StatsRecorder.Add("BANK/EQUITYRATIO", EquityRatio);
            Academy.Instance.StatsRecorder.Add("BANK/FUNDS", (float) _centralBankDeposit);
            Academy.Instance.StatsRecorder.Add("BANK/INTERESTNEG", NegativeInterestRate);
            Academy.Instance.StatsRecorder.Add("BANK/INTERESTPOS", PositiveInterestRate);
        }

        public void PaybackCredit(decimal sum)
        {
            _centralBankDeposit += sum;
        }

        public void RemoveCredit(LoanModel loan)
        {
            _loans.Remove(loan);
        }

        public void RemoveAccount(BankAccountModel account)
        {
            _centralBankDeposit += account.Savings;
            _accounts.Remove(account);
        }

        public BankAccountModel OpenBankAccount(decimal deposit)
        {
            var account = new BankAccountModel(deposit, this);
            _accounts.Add(account);
            return account;
        }

        public void PayInterst()
        {
            foreach (var a in _accounts)
                _centralBankDeposit += a.UpdateSavingsByInterestRate(PositiveInterestRate, NegativeInterestRate);
        }

        public LoanModel RequestLoan(decimal amount, CreditRating rating)
        {
            if (rating < _minimumRatingForCredits) return new LoanModel();
            var loan = new LoanModel(this, NegativeInterestRate, _lengthInMonthForNewCredits, amount);
            _loans.Add(loan);
            return loan;
        }

        private void ActionRequestFreshCapital()
        {
            decimal ownCapital = TotalAssets - TotalLiabilities;
            decimal capitalGap = (decimal) _policy.minimumEquityRate * (TotalLiabilities + ownCapital) - ownCapital;
            if (capitalGap > 100000) _centralBankDeposit += _centralBank.RequestFreshCapital(capitalGap);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            for (var ci = 0; ci < (int) Rt.LastItem; ci++)
                sensor.AddObservation((int) _minimumRatingForCredits == ci ? 1.0f : 0.0f);
            sensor.AddObservation(_normController.Normalize(nameof(TotalAssets), (float) TotalAssets));
            sensor.AddObservation(_normController.Normalize(nameof(TotalLiabilities), (float) TotalLiabilities));
            sensor.AddObservation(_normController.Normalize(nameof(PositiveInterestRate), PositiveInterestRate));
            sensor.AddObservation(_normController.Normalize(nameof(NegativeInterestRate), NegativeInterestRate));
            sensor.AddObservation(_normController.Normalize(nameof(_centralBankDeposit), (float) _centralBankDeposit));
            sensor.AddObservation(_normController.Normalize(nameof(_lengthInMonthForNewCredits),
                _lengthInMonthForNewCredits));
            sensor.AddObservation(_normController.Normalize(nameof(EquityRatio), EquityRatio));
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            int setRating = actionBuffers.DiscreteActions[0];
            int requestFreshCapital = actionBuffers.DiscreteActions[1];
            int setLoanRuntime = actionBuffers.DiscreteActions[2] + 6;
            float changeNegativeInterestRate = 1 + actionBuffers.ContinuousActions[0];
            float changePositiveInterestRate = 1 + actionBuffers.ContinuousActions[1];

            _minimumRatingForCredits = setRating != 7 ? (CreditRating) setRating : _minimumRatingForCredits;
            _lengthInMonthForNewCredits = setLoanRuntime;

            if (requestFreshCapital == 1) ActionRequestFreshCapital();

            float requestedRateNeg = changeNegativeInterestRate * NegativeInterestRate;
            float requestedRatePos = changePositiveInterestRate * PositiveInterestRate;
            requestedRateNeg = requestedRateNeg > _policy.leaseInterestRate * 2
                ? _policy.leaseInterestRate * 2
                : requestedRateNeg;
            PositiveInterestRate = requestedRatePos > _policy.leaseInterestRate
                ? _policy.leaseInterestRate
                : requestedRatePos;
            NegativeInterestRate = requestedRateNeg < requestedRatePos ? requestedRatePos * 1.5f : requestedRateNeg;
        }

        private enum Rt
        {
            C,
            B,
            Bb,
            Bbb,
            A,
            AA,
            Aaa,
            LastItem
        }
    }
}