namespace NewScripts.Game.Models
{
    public class Deal
    {
        public decimal Price { get; }
        public int Amount { get; }
        
        public Deal(decimal price, int amount)
        {
            Amount = amount;
            Price = price;
        }
    }
}