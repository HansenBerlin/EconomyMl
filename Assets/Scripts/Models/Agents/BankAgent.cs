using System.Collections.Generic;
using Models.Finance;

namespace Models.Agents
{
    public class BankAgent 
    {
        public float InterestRate { get; }
        private long _capital = 100000000000;
        private List<LoanModel> _loans = new();

        public void PayCredit(decimal sum)
        {
            _capital += (long)sum;
        }
        
        public void RemoveCredit(LoanModel loan)
        {
            _loans.Remove(loan);
        }

        public LoanModel RequestLoan(decimal amount)
        {
            var loan = new LoanModel(this, InterestRate, 12, (long)amount);
            _loans.Add(loan);
            return loan;
        }

    }
}