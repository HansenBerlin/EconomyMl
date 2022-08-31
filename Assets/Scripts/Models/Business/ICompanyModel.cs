using EconomyBase.Enums;
using EconomyBase.Models.Population;

namespace EconomyBase.Models.Business
{



    public interface ICompanyModel
    {
        string Id { get; }
        ProductType TypeProduced { get; }
        ProductType ResourceTypeNeeded { get; }

        ProductType EnergyTypeNeeded { get; }

        //List<IPersonBase> Workers { get; }
        long EstimatedEnergyDemand { get; }
        long EstimatedResourceDemand { get; }
        void Reset(int month);
        bool IsRemoved();
        void ActionBuyResources(int daysLeft);
        void ActionBuyEnergy(int daysLeft);
        void ActionProduce();
        void MonthlyBookkeeping();
        void ActionAdaptPrices();
        void QuarterlyUpdate();
        void ActionAdaptProductionCapacity();
    }
}