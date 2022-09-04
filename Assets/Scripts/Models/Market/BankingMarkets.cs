using System.Collections.Generic;
using System.Linq;
using Controller;
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

        public BankAccountModel OpenBankAccount(decimal deposit, bool isSetup)
        {
            if (isSetup)
            {
                var rn = StatisticalDistributionController.CreateRandom(0, _banks.Count);
                return _banks[rn].OpenBankAccount(deposit);
            }
            var banks = _banks.OrderByDescending(b => b.PositiveInterestRate).ToList().First();
            return banks.OpenBankAccount(deposit);
        }
    }
}