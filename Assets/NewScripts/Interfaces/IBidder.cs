using NewScripts.Enums;

namespace NewScripts.Interfaces
{
    public interface IBidder
    {
        void FullfillBid(ProductType product, int count, decimal price);
    }
}