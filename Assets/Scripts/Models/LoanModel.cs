using Agents;

namespace Models
{
    public class LoanModel
    {
        private readonly BankAgent _bank;

        public LoanModel(BankAgent bank, float interestRate, int runsForMonth, decimal totalSumLeft)
        {
            TotalSumLeft = totalSumLeft;
            MonthLeft = runsForMonth;
            YearlyInterestRate = interestRate;
            _bank = bank;
        }

        public LoanModel()
        {
            IsDeclined = true;
        }

        public int MonthLeft { get; private set; }
        public decimal TotalSumLeft { get; private set; }
        private float YearlyInterestRate { get; }
        public bool IsDeclined { get; }

        public decimal MakeMonthlyPayment()
        {
            if (MonthLeft == 0)
            {
                _bank.RemoveCredit(this);
                return 0;
            }

            decimal monthlySum = TotalSumLeft / MonthLeft;
            decimal monthlyInterest = monthlySum * ((decimal) YearlyInterestRate / 12);
            decimal payment = monthlyInterest + monthlySum;
            TotalSumLeft -= monthlySum;
            MonthLeft--;
            _bank.PaybackCredit(payment);
            return payment;
        }

        public void RemoveFromBank()
        {
            _bank.RemoveCredit(this);
        }
    }
}