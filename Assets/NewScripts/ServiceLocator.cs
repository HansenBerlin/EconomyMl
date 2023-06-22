using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Unity.VisualScripting;
using UnityEngine;

namespace NewScripts
{
    public class ServiceLocator : MonoBehaviour
    {
        public string SessionId { get; } = Guid.NewGuid().ToString();
        public static ServiceLocator Instance { get; private set; }
        public ProductMarket ProductMarket { get; private set; }
        public LaborMarket LaborMarket { get; private set; }
        public CompanyInfoPopup PopupInfoService { get; private set; }
        public List<Company> Companys { get; set; } = new();
        public FlowController FlowController { get; } = new();
        public SetupSettings Settings { get; } = new();
        public StatsSink Stats { get; private set; }
        public long stepsCompany;
        public long stepsWorker;
        
        


        private void Awake()
        {
           
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            ProductMarket = GetComponentInChildren<ProductMarket>();
            LaborMarket = GetComponentInChildren<LaborMarket>();
            PopupInfoService = GetComponentInChildren<CompanyInfoPopup>();
            Stats = GetComponentInChildren<StatsSink>();
        }
        
        
    }
}