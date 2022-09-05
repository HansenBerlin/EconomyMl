using Controller;
using Models.Agents;
using Policies;
using UnityEngine;

namespace Factories
{
    public class BankFactory : MonoBehaviour
    {
        private CentralBankPolicy _policy;
        private CentralBankAgent _centralBank;
        public GameObject BankAgentPrefab;
        
        public void Init(CentralBankPolicy policy, CentralBankAgent centralBank)
        {
            _policy = policy;
            _centralBank = centralBank;

        }

        public BankAgent Create()
        {
            var bank = BankAgentPrefab.GetComponent<BankAgent>();
            bank.Init(_policy, _centralBank, new NormalizationController());
            return bank;
        }
    }
}