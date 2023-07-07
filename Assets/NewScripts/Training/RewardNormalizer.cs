using System;
using System.Collections.Generic;
using System.Linq;

namespace NewScripts.Training
{
    public class RewardNormalizer
    {
        private readonly List<double> _globalHistoricValues;

        public RewardNormalizer()
        {
            _globalHistoricValues = new List<double> {0.0};
        }

        public void AddValue(double inputValue)
        {
            _globalHistoricValues.Add(inputValue);
        }

        public double Normalize(double inputValue)
        {
            double average = _globalHistoricValues.Average();

            double minValue = _globalHistoricValues.Min();
            double maxValue = _globalHistoricValues.Max();
            double range = maxValue - minValue;

            double normalizedValue = 0.0;

            if (range != 0)
            {
                double improvement = inputValue - (average + maxValue) / 2;
                //double improvement = inputValue - average;
                normalizedValue = improvement / (range / 2);
            }

            normalizedValue = Math.Max(-1, Math.Min(normalizedValue, 1));

            return normalizedValue;
        }
    }
}