namespace NewScripts
{
    public class InventoryItem
    {
        public ProductType Product { get; set; }
        public int Count { get; set; }
        public double AvgPaid { get; set; }

        private long _totalBought = 0;

        public void Add(int count, double price)
        {
            AvgPaid = (AvgPaid * _totalBought + price * count) / _totalBought + count;
            Count += count;
            _totalBought += count;
        }

        public void Consume(int count)
        {
            Count -= count;
        }
    }
}