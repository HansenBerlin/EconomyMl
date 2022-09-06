using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;
using ScottPlot;

namespace Controller.Data
{
    public static class StatisticalDistributionController
    {
        public static readonly Random Rng = new(0);

        public static decimal ReproductionRate()
        {
            return (decimal) DataGen.RandomNormalValue(Rng, 1.52, 0.5);
        }

        public static int CreateRandom(int startInclude, int endExclude)
        {
            return Rng.Next(startInclude, endExclude);
        }

        public static decimal NormalizedTrend(List<double> data, double currentValue)
        {
            double mn = MeanTrend(data);

            double value = (currentValue - mn) / mn;
            value = double.IsNaN(value) ? 0 : value;
            value = double.IsInfinity(value) ? 0 : value;

            return (decimal) Math.Round(value, 2);
        }

        public static double MeanTrend(List<double> data)
        {
            if (data.Count == 0 || data.Sum() == 0) return 0;

            int indexStart = data.Count < 6 ? 0 : data.Count - 6;
            int itemsCount = data.Count < 6 ? data.Count : 6;
            var dataCut = data.GetRange(indexStart, itemsCount);
            double mn = dataCut.Mean();

            return Math.Round(mn, 2);
        }
    }
}