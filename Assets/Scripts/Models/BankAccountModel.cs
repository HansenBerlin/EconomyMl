using System.Collections.Generic;
using System.Linq;
using Agents;
using Enums;

namespace Models
{
    public class BankAccountModel
    {
        private readonly BankAgent _bank;
        private readonly List<LoanModel> _loans = new();

        public BankAccountModel(decimal deposit, BankAgent bank)
        {
            _bank = bank;
            Savings = deposit;
        }

        public decimal Savings { get; private set; }
        public decimal LoansSum => _loans.Sum(x => x.TotalSumLeft);

        public decimal UpdateSavingsByInterestRate(float positiveRate, float negativeRate)
        {
            if (Savings > 0)
            {
                decimal additionalSavings = Savings * (decimal) (positiveRate / 12);
                Savings += additionalSavings;
                return additionalSavings * -1;
            }

            decimal payments = Savings * (decimal) (negativeRate / 12);
            Savings += payments;
            return payments * -1;
        }

        public bool IsLoanAdded(decimal amount, CreditRating rating)
        {
            var loan = _bank.RequestLoan(amount, rating);
            if (loan.IsDeclined == false)
            {
                _loans.Add(loan);
                Savings += loan.TotalSumLeft;
                return true;
            }

            return false;
        }

        public decimal MonthlyPaymentForLoans()
        {
            decimal totalPaid = 0;
            for (int i = _loans.Count - 1; i >= 0; i--)
            {
                var loan = _loans[i];
                decimal paid = loan.MakeMonthlyPayment();
                totalPaid += paid;
                Savings -= paid;
                if (loan.MonthLeft == 0) _loans.Remove(loan);
            }

            return totalPaid;
        }

        public void Deposit(decimal sum)
        {
            Savings += sum;
        }

        public decimal Withdraw(decimal sum)
        {
            Savings -= sum;
            return sum;
        }

        public decimal CloseAccount()
        {
            foreach (var l in _loans)
            {
                if (Savings <= 0)
                {
                    l.RemoveFromBank();
                    continue;
                }

                while (l.MonthLeft > 0 && Savings > 0) Savings -= l.MakeMonthlyPayment();
            }

            _bank.RemoveAccount(this);
            _loans.Clear();
            return Savings;
        }
    }
}