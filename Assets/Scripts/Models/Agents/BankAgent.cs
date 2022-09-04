using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Models.Finance;
using Policies;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

namespace Models.Agents
{
    public class BankAgent : Agent
    {
        private decimal TotalAssets => _loans.Sum(x => x.TotalSumLeft) + _centralBankDeposit;
        private decimal TotalLiabilities => _accounts.Sum(x => x.Savings);
        public float PositiveInterestRate { get; private set; } = 0.05F;
        public float NegativeInterestRate { get; private set;} = 0.05F;
        private decimal EquityRatio => TotalAssets / (TotalAssets + TotalLiabilities);
        private decimal _centralBankDeposit = 1000000;
        private readonly List<LoanModel> _loans = new();
        private readonly List<BankAccountBase> _accounts = new();
        private CentralBankPolicy _policy;
        
        private CreditRating _minimumRatingForCredits;
        private int _lengthInMonthForNewCredits;
        private CentralBankAgent _centralBank;

        public void Init(CentralBankPolicy policy, CentralBankAgent centralBankAgent)
        {
            _policy = policy;
            _centralBank = centralBankAgent;
        }

        public void PaybackCredit(decimal sum)
        {
            _centralBankDeposit += sum;
        }
        
        public void RemoveCredit(LoanModel loan)
        {
            _loans.Remove(loan);
        }

        public void RemoveAccount(BankAccountBase account)
        {
            _accounts.Remove(account);
        }

        public BankAccountBase OpenBankAccount(decimal deposit)
        {
            var account = new BankAccountBase(deposit, this);
            _accounts.Add(account);
            return account;
        }

        public void PayInterst()
        {
            foreach (var a in _accounts)
            {
                _centralBankDeposit += a.UpdateSavingsByInterestRate(PositiveInterestRate, NegativeInterestRate);
            }
        }

        public LoanModel RequestLoan(decimal amount, CreditRating rating)
        {
            if (rating < _minimumRatingForCredits)
            {
                return new LoanModel();
            }
            var loan = new LoanModel(this, NegativeInterestRate, _lengthInMonthForNewCredits, amount);
            _loans.Add(loan);
            return loan;
        }

        private void ActionRequestFreshCapital()
        {
            var ownCapital = TotalAssets - TotalLiabilities;
            var capitalGap = (decimal)_policy.MinimumEquityRate * (TotalLiabilities + ownCapital) - ownCapital;
            if (capitalGap > 100000)
            {
                _centralBankDeposit += _centralBank.RequestFreshCapital(capitalGap);
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation((float)TotalAssets);
            sensor.AddObservation((float)TotalLiabilities);
            sensor.AddObservation(PositiveInterestRate);
            sensor.AddObservation(NegativeInterestRate);
            sensor.AddObservation((float)_centralBankDeposit);
            sensor.AddObservation((float)_minimumRatingForCredits);
            sensor.AddObservation(_lengthInMonthForNewCredits);
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var setRating = actionBuffers.DiscreteActions[0];
            var requestFreshCapital = actionBuffers.DiscreteActions[1];
            var setLoanRuntime = actionBuffers.DiscreteActions[2] + 6;
            float changeNegativeInterestRate = 1 + actionBuffers.ContinuousActions[0];
            float changePositiveInterestRate = 1 + actionBuffers.ContinuousActions[1];

            _minimumRatingForCredits = setRating != 7 ? (CreditRating)setRating : _minimumRatingForCredits;
            _lengthInMonthForNewCredits = setLoanRuntime;
            
            if (requestFreshCapital == 1)
            {
                ActionRequestFreshCapital();
            }

            var requestedRateNeg = changeNegativeInterestRate * NegativeInterestRate;
            var requestedRatePos = changePositiveInterestRate * PositiveInterestRate;
            requestedRateNeg = requestedRateNeg > _policy.LeaseInterestRate * 2 ? _policy.LeaseInterestRate * 2 : requestedRateNeg;
            PositiveInterestRate = requestedRatePos > _policy.LeaseInterestRate ? _policy.LeaseInterestRate : requestedRatePos;
            NegativeInterestRate = requestedRateNeg < requestedRatePos ? requestedRatePos * 1.5f : requestedRateNeg;
        }
    }
}