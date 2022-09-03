using System;
using System.Collections.Generic;
using Models.Finance;

namespace Models.Agents
{
    public class BankAgent 
    {
        public float InterestRate { get; }
        private decimal _capital = 100000;
        private List<LoanModel> _loans = new();

        public void PayCredit(decimal sum)
        {
            try
            {
                _capital += sum;

            }
            catch (OverflowException e)
            {
                Console.WriteLine(e);
            }
        }
        
        public void RemoveCredit(LoanModel loan)
        {
            _loans.Remove(loan);
        }

        public LoanModel RequestLoan(decimal amount)
        {
            var loan = new LoanModel(this, InterestRate, 12, amount);
            _loans.Add(loan);
            return loan;
        }

    }
}