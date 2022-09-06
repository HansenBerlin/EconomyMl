using Enums;

namespace Models
{
    public class ObservationDataModel
    {
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public NormRange Range { get; }

        public ObservationDataModel(NormRange range, float initValue)
        {
            Range = range;
            MinValue = initValue;
            MaxValue = initValue;
        }
    }
}