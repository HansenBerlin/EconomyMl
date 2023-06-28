namespace NewScripts
{
    public class InventoryItem
    {
        public ProductType Product { get; set; }
        public int Count { get; set; }
        public decimal AvgPaid { get; set; } = 1;

        private long _totalBought;

        public void Add(int count, decimal price)
        {
            if (count == 0 || _totalBought + count == 0)
            {
                return;
            }
            AvgPaid = (AvgPaid * _totalBought + price * count) / (_totalBought + count);
            Count += count;
            _totalBought += count;
        }

        public void Consume(int count)
        {
            Count = Count - count < 0 ? 0 : Count - count;
        }
    }
}