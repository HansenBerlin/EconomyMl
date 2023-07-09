using System;
using System.Collections.Generic;
using System.Linq;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Entities;
using NewScripts.Game.Flow;
using NewScripts.Game.Models;
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
        public List<ICompany> Companys { get; } = new();
        public GlobalPolicies Policies { get; } = new();
        public FlowController FlowController { get; private set; }
        public Models.Settings Settings { get; } = new();
        public CompanyContainerPanelController CompanyContainerPanelController { get; private set; }
        public HouseholdAggregatorService HouseholdAggregator { get; private set; }
        public UiUpdateManager UiUpdateManager { get; private set; }
        public ReputationAggregatorFactory ReputationAggregatorFactory { get; } = new();
        public EconomyMetricsCalculator EconomyMetrics { get; private set; } = new();
        public Government Government { get; set; }


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

        public void AddInstances(Government government, CompanyContainerPanelController companyContainerPanelController, decimal startingLiquidityGovernment)
        {
            if (Companys.Count == 0)
            {
                throw new Exception("No companys found");
            }
            Government = government;
            Government.Init(Policies, Companys.Count, EconomyMetrics, new RewardNormalizer(), startingLiquidityGovernment, Settings);
            CompanyContainerPanelController = companyContainerPanelController;
            FlowController = new FlowController(Companys.Select(c => c.Id).ToList());
        }
    }
}