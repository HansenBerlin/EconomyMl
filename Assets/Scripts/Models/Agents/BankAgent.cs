using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
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
        private float NegativeInterestRate { get; set;} = 0.05F;
        private float EquityRatio => (float)(TotalAssets / (TotalAssets + TotalLiabilities));
        private decimal _centralBankDeposit = 1000000;
        private readonly List<LoanModel> _loans = new();
        private readonly List<BankAccountModel> _accounts = new();
        private CentralBankPolicy _policy;
        
        private CreditRating _minimumRatingForCredits = CreditRating.A;
        private int _lengthInMonthForNewCredits = 12;
        private CentralBankAgent _centralBank;
        private NormalizationController _normController;
        private enum Rt { C, B, BB, BBB, A, AA, AAA, LastItem }

        

        public void Init(CentralBankPolicy policy, CentralBankAgent centralBankAgent, NormalizationController normController)
        {
            _policy = policy;
            _centralBank = centralBankAgent;
            _normController = normController;
            _normController.AddNew(nameof(TotalAssets), NormRange.One, (float)TotalAssets);
            _normController.AddNew(nameof(TotalLiabilities), NormRange.One, (float)TotalLiabilities);
            _normController.AddNew(nameof(PositiveInterestRate), NormRange.One, PositiveInterestRate);
            _normController.AddNew(nameof(NegativeInterestRate), NormRange.One, NegativeInterestRate);
            _normController.AddNew(nameof(_centralBankDeposit), NormRange.One, (float)_centralBankDeposit);
            _normController.AddNew(nameof(_lengthInMonthForNewCredits), NormRange.One, _lengthInMonthForNewCredits);
            _normController.AddNew(nameof(EquityRatio), NormRange.One, (float)EquityRatio);
        }

        public void AddRewards()
        {
            if (TotalAssets > TotalLiabilities)
            {
                AddReward(0.1f);
            }
            if (EquityRatio >= _policy.MinimumEquityRate)
            {
                AddReward(0.2f);
            }
            else
            {
                AddReward(-0.2f);
            }
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
            for (int ci = 0; ci < (int)Rt.LastItem; ci++)
            {
                sensor.AddObservation((int)_minimumRatingForCredits == ci ? 1.0f : 0.0f);
            }
            sensor.AddObservation(_normController.Normalize(nameof(TotalAssets), (float)TotalAssets));
            sensor.AddObservation(_normController.Normalize(nameof(TotalLiabilities), (float)TotalLiabilities));
            sensor.AddObservation(_normController.Normalize(nameof(PositiveInterestRate), PositiveInterestRate));
            sensor.AddObservation(_normController.Normalize(nameof(NegativeInterestRate), NegativeInterestRate));
            sensor.AddObservation(_normController.Normalize(nameof(_centralBankDeposit), (float)_centralBankDeposit));
            sensor.AddObservation(_normController.Normalize(nameof(_lengthInMonthForNewCredits), _lengthInMonthForNewCredits));
            sensor.AddObservation(_normController.Normalize(nameof(EquityRatio), (float)EquityRatio));
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