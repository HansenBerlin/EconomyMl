using System.Collections.Generic;
using System.Linq;
using Agents;
using Controller.Data;

namespace Models
{
    public class BankingMarkets
    {
        private readonly List<BankAgent> _banks = new();

        public void AddBank(BankAgent bank)
        {
            _banks.Add(bank);
        }

        public void PayOutInterestForSavings()
        {
            foreach (var b in _banks) b.PayInterst();
        }

        public BankAccountModel OpenBankAccount(decimal deposit, bool isSetup)
        {
            if (isSetup)
            {
                int rn = StatisticalDistributionController.CreateRandom(0, _banks.Count);
                return _banks[rn].OpenBankAccount(deposit);
            }

            var banks = _banks.OrderByDescending(b => b.PositiveInterestRate).ToList().First();
            return banks.OpenBankAccount(deposit);
        }

        public void AddRewards()
        {
            foreach (var b in _banks) b.AddRewards();
        }

        public void Decide()
        {
            foreach (var b in _banks) b.MakeDecision();
        }
    }
}