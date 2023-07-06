using System;
using System.Collections.Generic;
using System.Linq;
using NewScripts.Enums;
using NewScripts.Game.Entities;
using NewScripts.Game.Flow;
using NewScripts.Interfaces;
using NewScripts.Training;
using NewScripts.Ui.Controller;
using UnityEngine;

namespace NewScripts.Game.Services
{
    public class ServiceLocator : MonoBehaviour
    {
        public string SessionId { get; } = Guid.NewGuid().ToString();
        public static ServiceLocator Instance { get; private set; }
        public ProductMarket FoodProductMarket { get; private set; }
        public ProductMarket LuxuryProductMarket { get; private set; }
        public LaborMarket LaborMarket { get; private set; }
        public List<ICompany> Companys { get; set; } = new();
        public FlowController FlowController { get; set; }
        public Models.Settings Settings { get; } = new();
        public CompanyContainerPanelController CompanyContainerPanelController { get; set; }
        public HouseholdAggregatorService HouseholdAggregator { get; private set; }
        public UiUpdateManager UiUpdateManager { get; private set; }
        public ReputationAggregatorFactory ReputationAggregatorFactory { get; } = new();


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            FoodProductMarket = new ProductMarket(ProductType.Food, 1);
            LuxuryProductMarket = new ProductMarket(ProductType.Luxury, 10);
            LaborMarket = new LaborMarket();
            
            HouseholdAggregator = GetComponentInChildren<HouseholdAggregatorService>();
            UiUpdateManager = GetComponentInChildren<UiUpdateManager>();
            GetComponentInChildren<FooterMenuController>().RegisterEvents();
        }

        public void InitFlowController()
        {
            if (Companys.Count == 0)
            {
                throw new Exception("No companys found");
            }
            FlowController = new FlowController(Companys.Select(c => c.Id).ToList());
        }
    }
}