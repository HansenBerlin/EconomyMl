using System.Collections.Generic;
using Assets.Scripts.Models.Agents;

namespace Assets.Scripts.Models.Meta
{



    public class TempPopulationUpdateModel
    {
        public TempPopulationUpdateModel(List<PersonAgent> current)
        {
            Current = current;
        }

        public List<PersonAgent> Died { get; } = new();
        public List<PersonAgent> Born { get; } = new();
        public List<PersonAgent> Retired { get; } = new();
        public List<PersonAgent> Current { get; }


    }
}