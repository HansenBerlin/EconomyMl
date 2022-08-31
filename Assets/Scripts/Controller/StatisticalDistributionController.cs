using MathNet.Numerics.Statistics;

namespace EconomyBase.Controller
{



    public static class StatisticalDistributionController
    {
        public static readonly Random Rng = new(0);

        private static readonly List<double> LifeExpectationRichSociety =
            ScottPlot.DataGen.RandomNormal(Rng, 1000, 80, 1, 20).ToList();

        private static readonly List<double> ReproductionRateRichSociety =
            ScottPlot.DataGen.RandomNormal(Rng, 100, 1.5).ToList();

        private static readonly List<double> NewParentAverageAge =
            ScottPlot.DataGen.RandomNormal(Rng, 100, 30, 6, 2).ToList();

        public static decimal ReproductionRate()
        {
            return (decimal) ScottPlot.DataGen.RandomNormalValue(Rng, 1.52, 0.5);
        }

        public static int CreateRandom(int startInclude, int endExclude)
        {
            return Rng.Next(startInclude, endExclude);
        }

        public static decimal NormalizedTrend(List<double> data, double currentValue)
        {
            double mn = MeanTrend(data);

            var value = (currentValue - mn) / mn;
            value = double.IsNaN(value) ? 0 : value;
            value = double.IsInfinity(value) ? 0 : value;

            return (decimal) Math.Round(value, 2);

        }

        public static double MeanTrend(List<double> data)
        {
            if (data.Count == 0 || data.Sum() == 0)
            {
                return 0;
            }

            int indexStart = data.Count < 6 ? 0 : data.Count - 6;
            int itemsCount = data.Count < 6 ? data.Count : 6;

            var dataCut = data.GetRange(indexStart, itemsCount);
            double mn = dataCut.Mean();

            return Math.Round(mn, 2);

        }

    }
}