using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;
using NewScripts.Common;
using NewScripts.DataModelling;
using NewScripts.Enums;
using NewScripts.Game.Entities;
using NewScripts.Game.Models;
using NewScripts.Game.Services;
using NewScripts.Interfaces;
using NewScripts.Ui.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui.Controller
{
    public class SupplyDemandStatsPanelController : MonoBehaviour
    {
        public int maxNumOfBuckets = 5;
        public bool showBoth = true;
        public GameObject barPrefab;
        public GameObject parent;
        public GameObject foodbutton;
        public GameObject luxurybutton;
        public GameObject workerButton;
        public TextMeshProUGUI textCurrentTotalCount;
        public int heightModifier = 290;

        private readonly List<GameObject> _bars = new();
        private List<BucketStatistics> _buckets = new();
        private readonly Stack<List<ProductDistributionInfo>> _valueHistory = new();
        private int _bidders;
        private List<ProductOffer> _offers = new();
        private List<ProductBid> _bids = new();
        private List<Deal> _successfulDeals = new();
        private bool _isInitDone;
        private MarketPanelSelection _currentSelection;
        private readonly BucketCreatorService _bucketCreatorService = new();
        

        private void Awake()
        {
            if (_isInitDone == false)
            {
                ServiceLocator.Instance.UiUpdateManager.updatedEvent.AddListener(DeconstructOffersAndBids);
                InitGameObjects();
                var data = ServiceLocator.Instance.FoodProductMarket.PriceAnalysisStats;
                if (data != null)
                {
                    DeconstructOffersAndBids(data);
                }
                _isInitDone = true;
            }
            else if (_valueHistory.Count > 0)
            {
                ShowStats();
            }
            else if(_bids.Count > 0)
            {
                PrepareData();
                ShowStats();
            }
        }

        private bool IsMatchingSelection(ProductType type)
        {
            switch (type)
            {
                case ProductType.Food when
                    _currentSelection == MarketPanelSelection.FoodMarket:
                case ProductType.Luxury when
                    _currentSelection == MarketPanelSelection.LuxuryMarket:
                default:
                    return false;
            }
        }
        
        

        private void DeconstructOffersAndBids(PriceAnalysisStatsModel data)
        {
            if (IsMatchingSelection(data.Type))
            {
                _successfulDeals = data.Deals.OrderBy(x => x.Price).ToList();
                _bids = data.Bids;
                _offers = data.Offers;
                if (enabled)
                {
                    PrepareData();
                    ShowStats();
                }
            }
        }

        private void PrepareData()
        {
            _bidders = _bids.Count;
            List<ProductDistributionInfo> initialValues = new();
            foreach (var bid in _bids)
            {
                for (int i = 0; i < bid.Amount; i++)
                {
                    var distributionInfo = new ProductDistributionInfo(bid.Price, ProductDistributionType.Bid);
                    initialValues.Add(distributionInfo);
                }
            }

            foreach (var offer in _offers)
            {
                for (int i = 0; i < offer.Amount; i++)
                {
                    var distributionInfo = new ProductDistributionInfo(offer.Price, ProductDistributionType.Offer);
                    initialValues.Add(distributionInfo);
                }
            }

            _valueHistory.Clear();
            _valueHistory.Push(initialValues);

            _buckets = _bucketCreatorService.GetBucketStatistics(initialValues, maxNumOfBuckets);
            textCurrentTotalCount.text = TotalCountText(initialValues.Count, _buckets);
        }


        private void InitGameObjects()
        {
            int count = showBoth ? maxNumOfBuckets * 2 : maxNumOfBuckets;
            for (int i = 0; i < count; i++)
            {
                GameObject instance = Instantiate(barPrefab, parent.transform, true);
                instance
                    .GetComponent<StatBar>().button
                    .GetComponent<Button>().onClick
                    .AddListener(() =>
                {
                    OnSingleBarClick(instance.GetInstanceID());
                });
                _bars.Add(instance);
            }
        }

        private float HeightMultiplier()
        {
            var count = _valueHistory.Peek().Count;
            float heightMultiplier = count / (float) _buckets
                .Select(x => x.Count)
                .Max();
            heightMultiplier = heightModifier / (float) count * heightMultiplier;
            return heightMultiplier;
        }

        private void ShowStats()
        {
            float heightMultiplier =  HeightMultiplier();
            
            for (int i = 0; i < _buckets.Count; i++)
            {
                var bucket = _buckets[i];
                GameObject instance = _bars[i];
                bucket.Id = instance.GetInstanceID();

                if (bucket.Count == 0)
                {
                    instance.SetActive(false);
                    continue;
                }

                instance.SetActive(true);
                var statBarScript = CreateStatBar(instance, bucket, heightMultiplier, i, out var rectTransform);

                var succesfulDealsInRange = _successfulDeals
                    .Where(x => x.Price >= bucket.Min && x.Price <= bucket.Max)
                    .ToList();
                
                float sumDeals = succesfulDealsInRange.Select(x => x.Amount).Sum();
                SetBarDimensions(succesfulDealsInRange, bucket, statBarScript, rectTransform, sumDeals, heightMultiplier);
                SetBarText(statBarScript, bucket);
            }

            if (_buckets.Count < maxNumOfBuckets * 2)
            {
                DeactivateBarsOutOfRange();
            }
        }

        private void DeactivateBarsOutOfRange()
        {
            int maxBuckets = showBoth ? maxNumOfBuckets * 2 : maxNumOfBuckets;
            int deactivateBars = maxBuckets - _buckets.Count;
            for (int i = maxBuckets - deactivateBars; i < maxBuckets; i++)
            {
                _bars[i].SetActive(false);
            }
        }

        private void SetBarText(StatBar statBarScript, BucketStatistics bucket)
        {
            statBarScript.rangeText.text = $"{bucket.Min:0.##} - {bucket.Max:0.##}";
            statBarScript.valueText.text = $"{bucket.Count}";

            statBarScript.button.GetComponent<Button>().interactable = DistinctValuesInRange(bucket) > 1;
        }

        private void SetBarDimensions(List<Deal> succesfulDealsInRange, BucketStatistics bucket, StatBar statBarScript,
            RectTransform rectTransform, float sumDeals, float heightMultiplier)
        {
            if (succesfulDealsInRange.Count > 0 && bucket.IsBid == false)
            {
                statBarScript.fullfilled.SetActive(true);
                float barWidth = rectTransform.sizeDelta.x;
                var fullfilledBarRect = statBarScript.fullfilled.GetComponent<RectTransform>();
                fullfilledBarRect.anchoredPosition = new Vector2(0, sumDeals * heightMultiplier + 90);
                fullfilledBarRect.sizeDelta = new Vector2(barWidth, 5);
            }
            else
            {
                statBarScript.fullfilled.SetActive(false);
            }
        }

        private StatBar CreateStatBar(GameObject instance, BucketStatistics bucket, float heightMultiplier, int i,
            out RectTransform rectTransform)
        {
            var statBarScript = instance.GetComponent<StatBar>();
            rectTransform = statBarScript.button.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, (int) (bucket.Count * heightMultiplier));
            rectTransform.anchoredPosition = new Vector2(i * 100, 0);
            statBarScript.button.GetComponent<Image>().color = bucket.IsBid
                ? Colors.LightGreen
                : Colors.Indigo;
            return statBarScript;
        }

        private void OnSingleBarClick(int instanceId)
        {
            var bucket = _buckets.First(x => x.Id == instanceId);
            if (bucket != null)
            {
                var values = _valueHistory
                    .Peek()
                    .Where(x => x.Value >= bucket.Min && x.Value <= bucket.Max)
                    .ToList();
                _valueHistory.Push(values);
                _buckets = _bucketCreatorService.GetBucketStatistics(values, maxNumOfBuckets);
                textCurrentTotalCount.text = TotalCountText(values.Count, _buckets);
                ShowStats();
            }
        }

        private string TotalCountText(int count, List<BucketStatistics> buckets)
        {
            var filteredBuckets = buckets.Where(x => x.Count > 0).ToList();
            string min = filteredBuckets[0].Min >= 100
                ? $"{filteredBuckets[0].Min:0}"
                : $"{filteredBuckets[0].Min:0.##}";
            string max;
            if (filteredBuckets.Count > 1)
            {
                max = filteredBuckets[^1].Max >= 100
                    ? $"{filteredBuckets[^1].Max:0}"
                    : $"{filteredBuckets[^1].Max:0.##}";
            }
            else
            {
                max = min;
            }

            return $"n:{count}/{_bidders} ({min} - {max})";
        }

        private int DistinctValuesInRange(BucketStatistics bucket)
        {
            var values = bucket.IsBid
                ? _valueHistory
                    .Peek()
                    .Where(x => x.Type == ProductDistributionType.Bid)
                    .Select(x => x.Value)
                : _valueHistory
                    .Peek()
                    .Where(x => x.Type == ProductDistributionType.Offer)
                    .Select(x => x.Value);

            int distinctValues = values
                .Where(x => x >= bucket.Min && x <= bucket.Max)
                .Distinct()
                .ToList()
                .Count;
            return distinctValues;
        }

        public void OnResetClick()
        {
            if (_valueHistory.Count == 1)
            {
                return;
            }

            _valueHistory.Pop();
            var values = _valueHistory.Peek();
            _buckets = _bucketCreatorService.GetBucketStatistics(values, maxNumOfBuckets);
            textCurrentTotalCount.text = TotalCountText(values.Count, _buckets);
            ShowStats();
        }
    }
}