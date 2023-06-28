using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NewScripts.Ui
{
    public class PriceStatsCreator : MonoBehaviour
    {
        public int maxNumOfBuckets = 10;
        public GameObject barPrefab;
        public GameObject parent;
        public GameObject productMarketGo;
        public TextMeshProUGUI textCurrentTotalCount;
        
        private readonly List<GameObject> _bars = new();
        private List<decimal> _initialvalues = new();
        private List<BucketStatistics> _buckets = new();
        private readonly Stack<List<decimal>> _valueHistory = new();

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
            
            var values = bids.Select(bid => bid.Price).ToList();
            _initialvalues = values;
            _valueHistory.Clear();
            _valueHistory.Push(_initialvalues);

            for (int i = 0; i < 1000 - bids.Count; i++)
            {
                //values.Add(0);
            }

            _buckets = GetBucketStatistics(values, maxNumOfBuckets);
            textCurrentTotalCount.text = TotalCountText(values.Count, _buckets);
            ShowStats();
        }


        private void InitGameObjects()
        {
            for (int i = 0; i < maxNumOfBuckets; i++)
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
            float heightMultiplier = _initialvalues.Count / (float) _buckets.Select(x => x.Count).Max();
            for (int i = 0; i < _buckets.Count; i++)
            {
                var bucket = _buckets[i];
                GameObject instance = _bars[i];
                
                if (bucket.Count == 0)
                {
                    instance.SetActive(false);
                    continue;
                }

                instance.SetActive(true);
                bucket.Id = instance.GetInstanceID();
                var rectTransform = instance.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(90, (int) (bucket.Count * heightMultiplier / 2));
                rectTransform.anchoredPosition = new Vector2(i * 100, 0);
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

                instance.GetComponent<Button>().interactable = DistinctValuesInRange(bucket.Min, bucket.Max) != 1;
            }
        }

        private void OnSingleBarClick(int instanceId)
        {
            var bucket = _buckets.First(x => x.Id == instanceId);
            if (bucket != null)
            {
                var values = _initialvalues.Where(x => x >= bucket.Min && x <= bucket.Max).ToList();
                _valueHistory.Push(values);
                _buckets = GetBucketStatistics(values, maxNumOfBuckets);
                textCurrentTotalCount.text = TotalCountText(values.Count, _buckets);
                ShowStats();
            }
        }

        private string TotalCountText(int count, List<BucketStatistics> buckets)
        {
            var filteredBuckets = buckets.Where(x => x.Count > 0).ToList();
            string min = filteredBuckets[0].Min >= 100 ? $"{filteredBuckets[0].Min:0}" : $"{filteredBuckets[0].Min:0.##}";
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
            return $"n:{count} ({min} - {max})";
        }

        private int DistinctValuesInRange(decimal min, decimal max)
        {
            var values = _initialvalues.Where(x => x >= min && x <= max).ToList();
            int distinctValues = values.Distinct().ToList().Count;
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

        private List<BucketStatistics> GetBucketStatistics(List<decimal> values, int numBuckets)
        {
            List<BucketStatistics> buckets = new List<BucketStatistics>();

            int distinctValues = values.Distinct().ToList().Count;
            if (distinctValues < numBuckets)
            {
                numBuckets = distinctValues;
            }

            decimal minValue = values.Min();
            decimal maxValue = values.Max();
            decimal range = (maxValue - minValue) / numBuckets;

            for (int i = 0; i < maxNumOfBuckets; i++)
            {
                decimal min = minValue + i * range;
                BucketStatistics bucket = new BucketStatistics
                {
                    Count = 0,
                    Min = min,
                    Max = min + range
                };
                buckets.Add(bucket);
            }

            foreach (decimal value in values)
            {
                int bucketIndex = (int) ((value - minValue) / range);
                bucketIndex = bucketIndex > 9 ? 9 : bucketIndex;
                buckets[bucketIndex].Count++;
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
    }
}