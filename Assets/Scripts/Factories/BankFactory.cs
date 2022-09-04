using Models.Agents;
using Policies;
using UnityEngine;

namespace Factories
{
    public class BankFactory : MonoBehaviour
    {
        private CentralBankPolicy _policy;
        public GameObject BankAgentPrefab;
        
        public BankFactory(CentralBankPolicy policy)
        {
            _policy = policy;

        }

        public BankAgent Create()
        {
            var bank = BankAgentPrefab.GetComponent<BankAgent>();
            bank.Init(_policy);
            return bank;
        }
    }
}