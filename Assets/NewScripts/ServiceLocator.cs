using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace NewScripts
{
    public class ServiceLocator : MonoBehaviour
    {
        public static ServiceLocator Instance { get; private set; }
        public ProductMarket ProductMarketService { get; private set; }
        public LaborMarket LaborMarketService { get; private set; }
        public List<Company> Companys { get; private set; } = new();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            ProductMarketService = GetComponentInChildren<ProductMarket>();
            LaborMarketService = GetComponentInChildren<LaborMarket>();
        }

        
        
    }
}