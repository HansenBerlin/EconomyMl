using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace Controller.Data
{

    public class CollectionNormalizationController
    {
        private readonly List<double> _data;

        public CollectionNormalizationController(List<double> data)
        {
            _data = data;
        }
        
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
    }
}