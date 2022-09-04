using Models.Agents;
using Policies;
using UnityEngine;

namespace Factories
{
    public class BankFactory : MonoBehaviour
    {
        private readonly CentralBankPolicy _policy;
        private readonly CentralBankAgent _centralBank;
        public GameObject BankAgentPrefab;
        
        public BankFactory(CentralBankPolicy policy)
        {
            _policy = policy;

        }

        public BankAgent Create()
        {
            var bank = BankAgentPrefab.GetComponent<BankAgent>();
            bank.Init(_policy, _centralBank);
            return bank;
        }
    }
}