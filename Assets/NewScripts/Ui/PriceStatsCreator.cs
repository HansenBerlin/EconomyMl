using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui
{
    public class PriceStatsCreator : MonoBehaviour
    {
        public int maxNumOfBuckets = 5;
        public bool showBoth = true;
        public GameObject barPrefab;
        public GameObject parent;
        public GameObject productMarketGo;
        public TextMeshProUGUI textCurrentTotalCount;

        private readonly List<GameObject> _bars = new();
        private List<BucketStatistics> _buckets = new();
        private readonly Stack<List<ProductDistributionInfo>> _valueHistory = new();
        private int _bidders;
        private List<ProductOffer> _offers = new();
        private List<ProductBid> _bids = new();
        private List<Deal> _successfulDeals = new();
        private bool _isInitDone;
        

        private void Awake()
        {
            if (_isInitDone == false)
            {
                var productMarket = productMarketGo.GetComponent<ProductMarket>();
                productMarket.updateEvent.AddListener(DeconstructOffersAndBids);
                InitGameObjects();
                _isInitDone = true;
            }
            if (_valueHistory.Count > 0)
            {
                ShowStats();
            }
            else if(_bids.Count > 0)
            {
                PrepareData();
                ShowStats();
            }
        }

        private void DeconstructOffersAndBids(List<ProductOffer> offers, List<ProductBid> bids, List<Deal> successfulDeals)
        {
            _successfulDeals = successfulDeals.OrderBy(x => x.Price).ToList();
            _bids = bids;
            _offers = offers;
            if (enabled)
            {
                PrepareData();
                ShowStats();
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

            _buckets = GetBucketStatistics(initialValues, maxNumOfBuckets);
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

        private void ShowStats()
        {
            var count = _valueHistory.Peek().Count;
            float heightMultiplier = count / (float) _buckets
                .Select(x => x.Count)
                .Max();
            heightMultiplier = 650 / (float) count * heightMultiplier;
            //float width = showBoth ? 45 : 90;
            
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
                var statBarScript = instance.GetComponent<StatBar>();
                var rectTransform = statBarScript.button.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(100, (int) (bucket.Count * heightMultiplier));
                rectTransform.anchoredPosition = new Vector2(i * 100, 0);
                statBarScript.button.GetComponent<Image>().color = bucket.IsBid
                    ? new Color(0.271F, 0.153F, 0.627F)
                    : new Color(0.678F, 0.078F, 0.341F);
                
                var succesfulDealsInRange = _successfulDeals
                    .Where(x => x.Price >= bucket.Min && x.Price <= bucket.Max)
                    .ToList();
                
                float sumDeals = succesfulDealsInRange.Select(x => x.Amount).Sum();
                
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

                statBarScript.rangeText.text = $"{bucket.Min:0.##} - {bucket.Max:0.##}";
                //statBarScript.valueText.text = $"{bucket.Count}({sumDeals / bucket.Count:0.##})";
                statBarScript.valueText.text = $"{bucket.Count}";
                
                statBarScript.button.GetComponent<Button>().interactable = DistinctValuesInRange(bucket) > 1;
            }

            if (_buckets.Count < maxNumOfBuckets * 2)
            {
                int maxBuckets = showBoth ? maxNumOfBuckets * 2 : maxNumOfBuckets;
                int deactivateBars = maxBuckets - _buckets.Count;
                for (int i = maxBuckets - deactivateBars; i < maxBuckets; i++)
                {
                    _bars[i].SetActive(false);
                }
            }
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
                _buckets = GetBucketStatistics(values, maxNumOfBuckets);
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
            _buckets = GetBucketStatistics(values, maxNumOfBuckets);
            textCurrentTotalCount.text = TotalCountText(values.Count, _buckets);
            ShowStats();
        }

        private List<BucketStatistics> GetBucketStatistics(List<ProductDistributionInfo> data, int numBuckets)
        {
            List<decimal> values = data.Select(x => x.Value).ToList();

            int distinctValues = values
                .Distinct()
                .ToList()
                .Count;

            if (distinctValues < numBuckets)
            {
                numBuckets = distinctValues;
            }

            decimal minValue = values.Min();
            decimal maxValue = values.Max();

            decimal range;
            if (minValue == maxValue)
            {
                range = values[0] / numBuckets;
            }
            else
            {
                range = (maxValue - minValue) / numBuckets;
            }

            List<BucketStatistics> bidBuckets = CreateEmptyBuckets(numBuckets, true, minValue, range);
            List<BucketStatistics> offerBuckets = CreateEmptyBuckets(numBuckets, false, minValue, range);


            foreach (var item in data)
            {
                int bucketIndex = (int) ((item.Value - minValue) / range);
                bucketIndex = bucketIndex >= numBuckets ? numBuckets - 1 : bucketIndex;
                if (item.Type == ProductDistributionType.Bid)
                {
                    bidBuckets[bucketIndex].Count++;
                }
                else
                {
                    offerBuckets[bucketIndex].Count++;
                }
            }

            List<BucketStatistics> buckets = new();
            buckets.AddRange(bidBuckets);
            buckets.AddRange(offerBuckets);
            buckets = buckets.OrderBy(x => x.Min).ToList();

            return buckets;
        }


        private List<BucketStatistics> CreateEmptyBuckets(int count, bool isBidder, decimal minValue, decimal range)
        {
            List<BucketStatistics> buckets = new();
            for (int i = 0; i < count; i++)
            {
                decimal min = minValue + i * range;
                BucketStatistics bucket = new BucketStatistics
                {
                    Count = 0,
                    Min = min,
                    Max = min + range,
                    IsBid = isBidder
                };
                buckets.Add(bucket);
            }

            return buckets;
        }
    }

    public class BucketStatistics
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public bool IsBid { get; set; }
    }

    public class ProductDistributionInfo
    {
        public ProductDistributionInfo(decimal value, ProductDistributionType type)
        {
            Value = value;
            Type = type;
        }

        public decimal Value { get; }
        public ProductDistributionType Type { get; }
    }

    public enum ProductDistributionType
    {
        Bid,
        Offer
    }
}