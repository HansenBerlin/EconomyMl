namespace EconomyBase.Models.Population
{



    public class HumanResourceModel
    {
        private List<IPersonBase> _workers = new();
        private double _capacityUsed;
        private double _totalProductionCapacity;
        private double _desiredFutureProductionCapacity;
        private double _dropOutRate;

    }
}