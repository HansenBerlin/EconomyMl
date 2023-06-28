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
        public int maxNumOfBuckets = 10;
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

        private void Awake()
        {
            var productMarket = productMarketGo.GetComponent<ProductMarket>();
            productMarket.updateEvent.AddListener(DeconstructOffersAndBids);
            InitGameObjects();
        }

        private void DeconstructOffersAndBids(List<ProductOffer> offers, List<ProductBid> bids)
        {
            if (bids.Count == 0)
            {
                return;
            }

            _bidders = bids.Count;
            _offerers = offers.Count;
            //var values = bids.Select(bid => bid.Price).ToList();
            List<decimal> initialBidValues = new();
            List<decimal> initialOfferValues = new();
            foreach (var bid in bids)
            {
                for (int i = 0; i < bid.Amount; i++)
                {
                    initialBidValues.Add(bid.Price);
                }
            }

            foreach (var offer in offers)
            {
                for (int i = 0; i < offer.Amount; i++)
                {
                    initialOfferValues.Add(offer.Price);
                }
            }

            //_initialvalues = values;
            _valueHistory.Clear();
            var data = new StatsData(initialBidValues, initialOfferValues);
            _valueHistory.Push(data);

            for (int i = 0; i < 1000 - bids.Count; i++)
            {
                //values.Add(0);
            }

            _buckets = GetBucketStatistics(initialBidValues, maxNumOfBuckets, true);
            textCurrentTotalCount.text = TotalCountText(initialBidValues.Count, _buckets);
            if (showBoth)
            {
                var offerBuckets = GetBucketStatistics(initialOfferValues, maxNumOfBuckets, false);
                _buckets.AddRange(offerBuckets);
            }

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
            var initialBidValues = _valueHistory.Peek().PriceBids;
            var initialOfferValues = _valueHistory.Peek().PriceOffers;
            int relevantCount = initialBidValues.Count > initialOfferValues.Count
                ? initialBidValues.Count
                : initialOfferValues.Count;
            float heightMultiplier = relevantCount / (float) _buckets.Select(x => x.Count).Max();
            heightMultiplier = 500 / (float) relevantCount * heightMultiplier;
            float width = showBoth ? 45 : 90;

            _buckets = _buckets.OrderBy(x => x.Min).ToList();

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

                instance.GetComponent<Button>().interactable = DistinctValuesInRange(bucket) != 1;
            }
        }

        private void OnSingleBarClick(int instanceId)
        {
            var bucket = _buckets.First(x => x.Id == instanceId);
            if (bucket != null)
            {
                var bidValues = _valueHistory
                    .Peek().PriceBids
                    .Where(x => x >= bucket.Min && x <= bucket.Max)
                    .ToList();
                var offerValues = _valueHistory
                    .Peek().PriceOffers
                    .Where(x => x >= bucket.Min && x <= bucket.Max)
                    .ToList();
                var data = new StatsData(bidValues, offerValues);
                _valueHistory.Push(data);
                _buckets = GetBucketStatistics(bidValues, maxNumOfBuckets, true);
                var offerBuckets = GetBucketStatistics(offerValues, maxNumOfBuckets, false);
                textCurrentTotalCount.text = TotalCountText(bidValues.Count, _buckets);
                _buckets.AddRange(offerBuckets);
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
                ? _valueHistory.Peek().PriceBids
                : _valueHistory.Peek().PriceOffers;

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
            _buckets = GetBucketStatistics(values.PriceBids, maxNumOfBuckets, true);
            textCurrentTotalCount.text = TotalCountText(values.PriceBids.Count, _buckets);
            var offerBuckets = GetBucketStatistics(values.PriceBids, maxNumOfBuckets, false);
            _buckets.AddRange(offerBuckets);
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

            bidBuckets = bidBuckets.OrderBy(x => x.Count).ToList();

            return bidBuckets;
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