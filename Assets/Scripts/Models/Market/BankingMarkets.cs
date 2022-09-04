using System.Collections.Generic;
using System.Linq;
using Enums;
using Models.Agents;
using Models.Finance;

namespace Models.Market
{
    public class BankingMarkets
    {
        private readonly List<BankAgent> _banks = new();

        public void AddBank(BankAgent bank)
        {
            _banks.Add(bank);
        }

        public LoanModel FindLoan(decimal amount, CreditRating rating)
        {
            var banks = _banks.OrderBy(b => b.NegativeInterestRate).ToList();
            if (banks.Count > 0)
            {
                var loan = banks[0].RequestLoan(amount, rating);
                if (loan.IsDeclined == false)
                {
                    return loan;
                }
            }

            return new LoanModel();
        }

        public BankAccountBase OpenBankAccount(decimal deposit)
        {
            var banks = _banks.OrderBy(b => b.PositiveInterestRate).ToList().First();
            return banks.OpenBankAccount(deposit);
        }
    }
}