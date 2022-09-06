using System.Collections.Generic;
using Assets.Scripts.Models.Agents;

namespace Assets.Scripts.Models.Population
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