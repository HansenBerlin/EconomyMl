using EconomyBase.Models.Population;

namespace EconomyBase.Models
{



    public class TempPopulationUpdateModel
    {
        public TempPopulationUpdateModel(List<IPersonBase> current)
        {
            Current = current;
        }

        public List<IPersonBase> Died { get; } = new();
        public List<IPersonBase> Born { get; } = new();
        public List<IPersonBase> Retired { get; } = new();
        public List<IPersonBase> Current { get; }


    }
}