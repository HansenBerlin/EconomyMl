using System.Collections.Generic;
using Models.Agents;

namespace Models.Population
{
    public class HumanResourceModel
    {
        private List<PersonAgent> _workers = new();
        private double _capacityUsed;
        private double _totalProductionCapacity;
        private double _desiredFutureProductionCapacity;
        private double _dropOutRate;

    }
}