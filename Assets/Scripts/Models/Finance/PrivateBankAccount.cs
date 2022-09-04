using Models.Agents;

namespace Models.Finance
{
    public class PrivateBankAccount : BankAccountBase
    {
        private PersonAgent _customer;

        public PrivateBankAccount(decimal deposit, BankAgent bank) : base(deposit, bank)
        {
        }
    }
}