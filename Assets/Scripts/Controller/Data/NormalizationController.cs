using System.Collections.Generic;
using Enums;
using Models;

namespace Controller.Data
{
    public class NormalizationController
    {
        private readonly Dictionary<string, ObservationDataModel> _observationHistory = new();

        public void AddNew(string observationName, NormRange range, float initValue)
        {
            var obsModel = new ObservationDataModel(range, initValue);
            _observationHistory.Add(observationName, obsModel);
        }
        
        public float Normalize(string obsName, float value)
        {
            var obsModel = _observationHistory[obsName];
            obsModel.MinValue = value < obsModel.MinValue ? value : obsModel.MinValue;
            obsModel.MaxValue = value > obsModel.MaxValue ? value : obsModel.MaxValue;
            return obsModel.Range == NormRange.One
                ? GetRangeOne(obsModel.MinValue, obsModel.MaxValue, value)
                : GetRangeTwo(obsModel.MinValue, obsModel.MaxValue, value);
        }

        private float GetRangeTwo(float min, float max, float value)
        {
            if (max - min == 0)
            {
                return 0;
            }
            float norm = 2 * ((value - min) / (max - min)) - 1;
            return norm;
        }
        
        private float GetRangeOne(float min, float max, float value)
        {
            if (max - min == 0)
            {
                return 0.5f;
            }
            float norm = (value - min) / (max - min);
            return norm;
        }
    }
}