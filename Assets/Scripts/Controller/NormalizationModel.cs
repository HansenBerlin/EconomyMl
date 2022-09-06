using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace Assets.Scripts.Controller
{




    public class NormalizationModel
    {
        private readonly List<double> _data;
        private decimal _min = -1;
        private decimal _max = 1;

        public NormalizationModel(List<double> data)
        {
            _data = data;
        }

        /*public decimal Normalize(decimal newValue, decimal baseValue = 0)
        {
            _min = newValue < _min ? newValue : _min;
            _max = newValue > _max ? newValue : _max;
            decimal average = (_min + _max) / 2;
            decimal range = (_max - _min) / 2;
            decimal normalized = range != 0 ? (newValue - average) / range : 0;
            return normalized;
        }*/



        public decimal Normalize(decimal valueNew)
        {
            if (_data.Count == 0 || _data.Sum() == 0)
            {
                return 0;
            }

            double mn = _data.Mean();
            double sd = _data.StandardDeviation();

            var value = ((double) valueNew - mn) / sd;
            value = double.IsNaN(value) ? 0 : value;
            value = double.IsInfinity(value) ? 0 : value;
            return (decimal) value;
        }



        /*public decimal Normalize(decimal newValue, decimal baseValue)
        {
            var change = baseValue == 0 ? newValue == 0 ? 0 : 1 : (newValue - baseValue) / baseValue;
            return change;
        }*/



        /*public decimal PercentChangeComparison(decimal salesChange, decimal productionChange, decimal supplyChange)
        {
            var potOverProduction = 
            
    
        }*/
    }
}