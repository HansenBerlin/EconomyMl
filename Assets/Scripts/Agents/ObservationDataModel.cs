using Enums;

namespace Agents
{
    public class ObservationDataModel
    {
        public ObservationDataModel(NormRange range, float initValue)
        {
            Range = range;
            MinValue = initValue;
            MaxValue = initValue;
        }

        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public NormRange Range { get; }
    }
}