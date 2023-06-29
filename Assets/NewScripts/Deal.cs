namespace NewScripts
{
    public class Deal
    {
        public Deal(decimal price, int amount)
        {
            Amount = amount;
            Price = price;
        }

        public decimal Price { get; }
        public int Amount { get; }
    }
}