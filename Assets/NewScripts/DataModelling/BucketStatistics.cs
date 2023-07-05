namespace NewScripts.DataModelling
{
    public class BucketStatistics
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }
        public bool IsBid { get; set; }
    }
}