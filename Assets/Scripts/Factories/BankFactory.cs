using Controller.Data;
using Models.Agents;
using Policies;
using UnityEngine;
using UnityEngine.Serialization;

namespace Factories
{
    public class BankFactory : MonoBehaviour
    {
        private CentralBankPolicy _policy;
        private CentralBankAgent _centralBank;
        [FormerlySerializedAs("BankAgentPrefab")] public GameObject bankAgentPrefab;
        
        public void Init(CentralBankPolicy policy, CentralBankAgent centralBank)
        {
            _policy = policy;
            _centralBank = centralBank;

        }

        public BankAgent Create()
        {
            var go =Instantiate(bankAgentPrefab);
            var bank = go.GetComponent<BankAgent>();
            bank.Init(_policy, _centralBank, new NormalizationController());
            return bank;
        }
    }
}