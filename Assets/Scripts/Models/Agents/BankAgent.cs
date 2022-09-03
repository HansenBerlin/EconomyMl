using System;
using System.Collections.Generic;
using Models.Finance;

namespace Models.Agents
{
    public class BankAgent
    {
        public float InterestRate { get; } = 0.05F;
        private decimal _capital = 1000000;
        private List<LoanModel> _loans = new();

        public void PayCredit(decimal sum)
        {
            _capital += sum;
        }
        
        public void RemoveCredit(LoanModel loan)
        {
            _loans.Remove(loan);
        }

        public LoanModel RequestLoan(decimal amount)
        {
            if (amount > _capital * 10 && _capital > 100000)
            {
                amount = _capital / 10;
            }
            else if (_capital < 100000)
            {
                return new LoanModel();
            }
            var loan = new LoanModel(this, InterestRate, 12, amount);
            _loans.Add(loan);
            return loan;
        }

    }
}