using System.Collections.Generic;
using Models.Agents;

namespace Controller
{
    public class BankController
    {
        private List<BankAgent> _banks;

        public void AddBank(BankAgent bank)
        {
            _banks.Add(bank);
        }

        public void PayOutInterestForSavings()
        {
            foreach (var b in _banks)
            {
                b.PayInterst();
            }
        }
    }
}