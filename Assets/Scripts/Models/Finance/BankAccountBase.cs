using System.Collections.Generic;
using System.Linq;
using System.Net;
using Models.Agents;

namespace Models.Finance
{
    public class BankAccountBase
    {
        private readonly BankAgent _bank;
        public decimal Savings { get; private set; }
        private List<LoanModel> Loans = new();

        public BankAccountBase(decimal deposit, BankAgent bank)
        {
            _bank = bank;
            Savings = deposit;
        }

        public decimal UpdateSavingsByInterestRate(float positiveRate, float negativeRate)
        {
            if (Savings > 0)
            {
                decimal additionalSavings = Savings * (decimal)(positiveRate / 12);
                Savings += additionalSavings;
                return additionalSavings * -1;
            }
            decimal payments = Savings * (decimal)(negativeRate / 12);
            Savings += payments;
            return payments * -1;
        }

        public decimal CloseAccount()
        {
            foreach (var l in Loans)
            {
                if (Savings <= 0)
                {
                    l.RemoveFromBank();
                    continue;
                }
                while (l.MonthLeft > 0 && Savings > 0)
                {
                    Savings -= l.MakeMonthlyPayment();
                }
            }
            _bank.RemoveAccount(this);
            Loans.Clear();
            return Savings;
        }
    }
}