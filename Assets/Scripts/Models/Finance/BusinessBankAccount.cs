using Models.Agents;
using Models.Business;

namespace Models.Finance
{
    public class BusinessBankAccount : BankAccountBase
    {
        private CompanyBaseAgent _customer;

        public BusinessBankAccount(decimal deposit, BankAgent bank) : base(deposit, bank)
        {
        }
    }
}