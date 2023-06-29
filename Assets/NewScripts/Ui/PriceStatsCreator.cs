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

        //private readonly List<GameObject> _offerBars = new();
        //private readonly List<decimal> _initialBidValues = new();
        //private readonly List<decimal> _initialOfferValues = new();
        private List<BucketStatistics> _buckets = new();
        private readonly Stack<List<ProductDistributionInfo>> _valueHistory = new();
        private int _bidders;
        private int _offerers;
        private List<ProductOffer> _fullfilledOffers = new();
        private List<ProductBid> _fullfilledBids = new();

        private void Awake()
        {
            var productMarket = productMarketGo.GetComponent<ProductMarket>();
            productMarket.updateEvent.AddListener(DeconstructOffersAndBids);
            InitGameObjects();
        }

        private void DeconstructOffersAndBids(List<ProductOffer> offers, List<ProductBid> bids, 
            List<ProductOffer> fullfilledOffers, List<ProductBid> fullfilledBids)
        {
            _fullfilledOffers = fullfilledOffers.OrderBy(x => x.Price).ToList();
            _fullfilledBids = fullfilledBids.OrderBy(x => x.Price).ToList();
            if (bids.Count == 0)
            {
                return;
            }

            _bidders = bids.Count;
            _offerers = offers.Count;
            //var values = bids.Select(bid => bid.Price).ToList();
            List<ProductDistributionInfo> initialValues = new();
            foreach (var bid in bids)
            {
                for (int i = 0; i < bid.Amount; i++)
                {
                    var distributionInfo = new ProductDistributionInfo(bid.Price, ProductDistributionType.Bid);
                    initialValues.Add(distributionInfo);
                }
            }

            foreach (var offer in offers)
            {
                for (int i = 0; i < offer.Amount; i++)
                {
                    var distributionInfo = new ProductDistributionInfo(offer.Price, ProductDistributionType.Offer);
                    initialValues.Add(distributionInfo);
                }
            }

            //_initialvalues = values;
            _valueHistory.Clear();
            _valueHistory.Push(initialValues);

            for (int i = 0; i < 1000 - bids.Count; i++)
            {
                //values.Add(0);
            }

            _buckets = GetBucketStatistics(initialValues, maxNumOfBuckets);
            textCurrentTotalCount.text = TotalCountText(initialValues.Count, _buckets);
            //_buckets = _buckets.OrderBy(x => x.Min).ToList();
            ShowStats();
        }


        private void InitGameObjects()
        {
            int count = showBoth ? maxNumOfBuckets * 2 : maxNumOfBuckets;
            for (int i = 0; i < count; i++)
            {
                GameObject instance = Instantiate(barPrefab, parent.transform, true);
                instance.GetComponent<Button>().onClick.AddListener(() =>
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
            heightMultiplier = 500 / (float) count * heightMultiplier;
            float width = showBoth ? 45 : 90;
            
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
                var rectTransform = instance.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(width, (int) (bucket.Count * heightMultiplier));
                rectTransform.anchoredPosition = new Vector2(i * 100, 0);
                instance.GetComponent<Image>().color = bucket.IsBid
                    ? new Color(0.271F, 0.153F, 0.627F)
                    : new Color(0.678F, 0.078F, 0.341F);
                
                foreach (Transform child in instance.transform.GetComponentsInChildren<Transform>())
                {
                    if (child.gameObject.name == "Fullfilled")
                    {
                        if (_fullfilledOffers.Count > 0 && bucket.IsBid)
                        {
                            child.gameObject.SetActive(true);
                            var values = _fullfilledOffers
                                .Where(x => x.Price >= bucket.Min && x.Price <= bucket.Max)
                                .ToList();
                            float sum = values.Select(x => x.Amount).Sum();
                                child.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, sum * heightMultiplier);
                        }
                        else if (_fullfilledBids.Count > 0 && bucket.IsBid == false)
                        {
                            var values = _fullfilledBids
                                .Where(x => x.Price >= bucket.Min && x.Price <= bucket.Max)
                                .ToList();
                            float sum = values.Select(x => x.Amount).Sum();
                            child.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, sum * heightMultiplier);
                        }
                        else
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }

                var texts = instance.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var text in texts)
                {
                    text.text = text.name switch
                    {
                        "Min" => $"{bucket.Min:0.##}",
                        "Max" => $"{bucket.Max:0.##}",
                        "Val" => bucket.Count.ToString(),
                        _ => text.text
                    };
                }

                instance.GetComponent<Button>().interactable = DistinctValuesInRange(bucket) > 1;
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