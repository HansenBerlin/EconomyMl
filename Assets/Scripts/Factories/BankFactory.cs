using Assets.Scripts.Controller;
using Assets.Scripts.Models.Agents;
using Assets.Scripts.Policies;
using UnityEngine;

namespace Assets.Scripts.Factories
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
            var go =Instantiate(BankAgentPrefab);
            var bank = go.GetComponent<BankAgent>();
            bank.Init(_policy, _centralBank, new NormalizationController());
            return bank;
        }
    }
}