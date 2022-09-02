using Enums;

namespace Models.Business
{



    public interface ICompanyModelAgent
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
        void ActionProduce(int percentProduction);
        void MonthlyBookkeeping();
        void ActionAdaptPrices();
        void QuarterlyUpdate();
        void ActionAdaptProductionCapacity();
    }
}