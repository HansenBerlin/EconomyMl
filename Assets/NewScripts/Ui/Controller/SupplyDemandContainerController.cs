using System;
using NewScripts.Common;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui.Controller
{
    public class SupplyDemandContainerController : MonoBehaviour
    {
        public Button foodButtonGo;
        public Button luxuryButtonGo;
        public Button workerButtonGo;
        public Button backButtonGo;
        
        public GameObject foodPanelGo;
        public GameObject luxuryPanelGo;
        public GameObject workerPanelGo;
        
        public RawImage bidLegendImage;
        public RawImage offerLegendImage;
        
        public TextMeshProUGUI headerText;
        private ProductType _currentProductType;
        private PriceAnalysisStatsModel _foodStats;
        private PriceAnalysisStatsModel _luxuryStats;
        
        private void Awake()
        {
            offerLegendImage.color = Colors.Indigo;
            bidLegendImage.color = Colors.LightGreen;
            var uiService = ServiceLocator.Instance.UiUpdateManager;
            foodPanelGo.GetComponent<SupplyDemandStatsPanelController>().InitGameObjects(ProductType.Food);
            luxuryPanelGo.GetComponent<SupplyDemandStatsPanelController>().InitGameObjects(ProductType.Luxury);
            
            uiService.foodPricesupdateEvent.AddListener((x) =>
            {
                _foodStats = x;
                if (_currentProductType == ProductType.Food)
                {
                    foodPanelGo.GetComponent<SupplyDemandStatsPanelController>().DeconstructOffersAndBids(x);
                }
            });
            uiService.luxuryPricesupdateEvent.AddListener(x =>
            {
                _luxuryStats = x;
                if (_currentProductType == ProductType.Luxury)
                {
                    luxuryPanelGo.GetComponent<SupplyDemandStatsPanelController>().DeconstructOffersAndBids(x);
                }
            });
            
            foodPanelGo.GetComponent<SupplyDemandStatsPanelController>()
                .DeconstructOffersAndBids(ServiceLocator.Instance.FoodProductMarket.PriceAnalysisStats);

            foodButtonGo.onClick.AddListener(() =>
            {
                if (_currentProductType != ProductType.Food)
                {
                    _currentProductType = ProductType.Food;
                    foodPanelGo.SetActive(true);
                    luxuryPanelGo.SetActive(false);
                    //workerPanelGo.SetActive(false);
                    headerText.text = "Food Prices";
                    foodPanelGo.GetComponent<SupplyDemandStatsPanelController>().DeconstructOffersAndBids(_foodStats);
                    //foodPanelGo.GetComponent<SupplyDemandStatsPanelController>().UpdateBreadcrumbText();
                }
            });
            luxuryButtonGo.onClick.AddListener(() =>
            {
                if (_currentProductType != ProductType.Luxury)
                {
                    _currentProductType = ProductType.Luxury;
                    foodPanelGo.SetActive(false);
                    luxuryPanelGo.SetActive(true);
                    //workerPanelGo.SetActive(false);
                    headerText.text = "Luxury Prices";
                    luxuryPanelGo.GetComponent<SupplyDemandStatsPanelController>().DeconstructOffersAndBids(_luxuryStats);
                }
                //luxuryPanelGo.GetComponent<SupplyDemandStatsPanelController>().UpdateBreadcrumbText();
            });
            workerButtonGo.onClick.AddListener(() =>
            {
                //foodPanelGo.SetActive(false);
                //luxuryPanelGo.SetActive(false);
                //workerPanelGo.SetActive(true);
                //headerText.text = "Worker Prices";
            });
            backButtonGo.onClick.AddListener(() =>
            {
                if (_currentProductType == ProductType.Food)
                {
                    foodPanelGo.GetComponent<SupplyDemandStatsPanelController>().BackButtonClicked();
                }
                else if (_currentProductType == ProductType.Luxury)
                {
                    luxuryPanelGo.GetComponent<SupplyDemandStatsPanelController>().BackButtonClicked();
                }
            });
        }
    }
}