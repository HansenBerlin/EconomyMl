using System.Collections.Generic;
using System.Linq;
using NewScripts.DataModelling;
using NewScripts.Enums;

namespace NewScripts.Ui.Controller
{
    public class BucketCreatorService
    {
        public List<BucketStatistics> GetBucketStatistics(List<ProductDistributionInfo> data, int numBuckets)
        {
            List<decimal> values = data.Select(x => x.Value).ToList();

            int distinctValues = values
                .Distinct()
                .ToList()
                .Count;

            numBuckets = distinctValues < numBuckets ? distinctValues : numBuckets;

            decimal minValue = values.Min();
            decimal maxValue = values.Max();

            decimal range = minValue == maxValue ? values[0] / numBuckets : (maxValue - minValue) / numBuckets;
            List<BucketStatistics> bidBuckets = CreateEmptyBuckets(numBuckets, true, minValue, range);
            List<BucketStatistics> offerBuckets = CreateEmptyBuckets(numBuckets, false, minValue, range);

            foreach (var item in data)
            {
                IncrementBucketCount(numBuckets, item, minValue, range, bidBuckets, offerBuckets);
            }

            return MergeBuckets(bidBuckets, offerBuckets);
        }
        
        private List<BucketStatistics> MergeBuckets(List<BucketStatistics> bidBuckets, List<BucketStatistics> offerBuckets)
        {
            List<BucketStatistics> buckets = new();
            buckets.AddRange(bidBuckets);
            buckets.AddRange(offerBuckets);
            buckets = buckets.OrderBy(x => x.Min).ToList();
            return buckets;
        }

        private void IncrementBucketCount(int numBuckets, ProductDistributionInfo item, decimal minValue, decimal range,
            List<BucketStatistics> bidBuckets, List<BucketStatistics> offerBuckets)
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
}