using System.Collections.Generic;
using System.Linq;
using Enums;
using Models.Agents;
using Models.Finance;

namespace Models.Market
{
    public class BankingMarkets
    {
        private List<BankAgent> _banks = new();

        public void AddBank(BankAgent bank)
        {
            _banks.Add(bank);
        }

        public LoanModel FindCheapestPossibleLoan(decimal amount, CreditRating rating)
        {
            var banks = _banks.OrderBy(b => b.InterestRate).ToList();
            if (banks.Count > 0)
            {
                var loan = banks[0].RequestLoan(amount);
                return loan;
            }

            return new LoanModel();
        }
    }
}