using Assets.Scripts.Models.Agents;

namespace Assets.Scripts.Models.Finance
{
    public class LoanModel
    {
        public int MonthLeft { get; private set; }
        public decimal TotalSumLeft { get; private set; }
        //public long MonthlyPayment { get; private set; }
        public float YearlyInterestRate { get; private set; }
        private BankAgent _bank;
        public bool IsDeclined { get; }

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

        public decimal MakeMonthlyPayment()
        {
            if (MonthLeft == 0)
            {
                _bank.RemoveCredit(this);
                return 0;
            }

            decimal monthlySum = TotalSumLeft / MonthLeft;
            decimal monthlyInterest = monthlySum * ((decimal)YearlyInterestRate / 12);
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