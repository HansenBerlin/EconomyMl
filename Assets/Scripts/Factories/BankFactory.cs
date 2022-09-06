using Agents;
using Controller.Data;
using Policies;
using UnityEngine;

namespace Factories
{
    public class BankFactory : MonoBehaviour
    {
        public GameObject bankAgentPrefab;
        private CentralBankAgent _centralBank;
        private CentralBankPolicy _policy;

        public void Init(CentralBankPolicy policy, CentralBankAgent centralBank)
        {
            _policy = policy;
            _centralBank = centralBank;
        }

        public BankAgent Create()
        {
            var go = Instantiate(bankAgentPrefab);
            var bank = go.GetComponent<BankAgent>();
            bank.Init(_policy, _centralBank, new NormalizationController());
            return bank;
        }
    }
}