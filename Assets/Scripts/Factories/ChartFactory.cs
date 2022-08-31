using EconomyBase.Enums;
using ScottPlot;
using ScottPlot.Statistics;
using ScottPlot.Styles;

namespace EconomyBase.Factories
{



    public static class ChartFactory
    {
        private const string Path = @"C:\Users\Hannes\source\repos\HWR\Studienarbeit\stats\";
        private static readonly IStyle Style = ScottPlot.Style.Default;

        private static string BuildPath(ChartType type, string title)
        {
            return type switch
            {
                ChartType.Businesses => $@"{Path}\business\{title}.png",
                ChartType.Economy => $@"{Path}\economy\{title}.png",
                ChartType.Population => $@"{Path}\population\{title}.png",
                _ => $@"{Path}\government\{title}.png"
            };
        }

        private static List<double>[] AdaptListLength(List<double>[] valueSets)
        {
            int maxLength = valueSets.Select(v => v.Count).Prepend(0).Max();
            foreach (var v in valueSets)
            {
                if (v.Count < maxLength)
                {
                    int add = maxLength - v.Count;
                    for (int i = 0; i < add; i++)
                    {
                        v.Insert(0, 0);
                    }
                }
            }

            return valueSets;
        }

        public static void CreateScatter(List<double>[] valueSets, ChartType type, string title,
            string[] axisTitles = null,
            int yLimitMin = 0, int xLimitMin = 0, int xLimitMax = 0, int yLimitMax = 0, bool smooth = true,
            bool isLogScale = false)
        {
            var plot = new Plot(1920, 1080);
            plot.Style(Style);
            valueSets = AdaptListLength(valueSets);

            double[] xAxis = CreateXAxis(valueSets[0].Count);
            plot.Title(title);
            bool hasLabels = axisTitles != null;
            for (int i = 0; i < valueSets.Length; i++)
            {
                var values = valueSets[i].ToArray();
                if (isLogScale)
                {
                    values = Tools.Log10(values);
                }

                if (hasLabels)
                {
                    var line = plot.AddScatter(xAxis, values, label: axisTitles[i]);
                    line.Smooth = smooth;
                }
                else
                {
                    var line = plot.AddScatter(xAxis, values);
                    line.Smooth = smooth;
                }
            }

            plot.Legend(hasLabels);
            if (xLimitMax != 0)
            {
                plot.SetAxisLimitsX(xLimitMin, xLimitMax);
            }

            if (yLimitMax != 0)
            {
                plot.SetAxisLimitsY(yLimitMin, yLimitMax);
            }



            string path = BuildPath(type, title);
            plot.SaveFig(path);
        }

        public static Plot CreateScatterPlot(List<double>[] valueSets, ChartType type, string title,
            string[] axisTitles = null,
            int yLimitMin = 0, int xLimitMin = 0, int xLimitMax = 0, int yLimitMax = 0, bool smooth = true,
            bool isLogScale = false)
        {
            var plot = new Plot(1920, 1080);
            plot.Style(Style);

            double[] xAxis = CreateXAxis(valueSets[0].Count);
            plot.Title(title);
            bool hasLabels = axisTitles != null;
            for (int i = 0; i < valueSets.Length; i++)
            {
                var values = valueSets[i].ToArray();
                if (isLogScale)
                {
                    values = Tools.Log10(values);
                }

                if (hasLabels)
                {
                    var line = plot.AddScatter(xAxis, values, label: axisTitles[i]);
                    line.Smooth = smooth;
                }
                else
                {
                    var line = plot.AddScatter(xAxis, values);
                    line.Smooth = smooth;
                }
            }

            plot.Legend(hasLabels);
            if (xLimitMax != 0)
            {
                plot.SetAxisLimitsX(xLimitMin, xLimitMax);
            }

            if (yLimitMax != 0)
            {
                plot.SetAxisLimitsY(yLimitMin, yLimitMax);
            }

            return plot;
        }

        public static void CreateDistribution(List<double> values, string title, string xLabel, string yLabel,
            ChartType type)
        {
            var plt = new Plot(600, 400);
            int min = (int) values.Min();
            int max = (int) values.Max();
            (double[] counts, double[] binEdges) = Common.Histogram(values.ToArray(), min, max, 1);
            double[] leftEdges = binEdges.Take(binEdges.Length - 1).ToArray();
            var bar = plt.AddBar(counts, leftEdges);
            var barWidth = (values.Max() - values.Min()) / values.Count;
            bar.BarWidth = 1;
            plt.YAxis.Label(xLabel);
            plt.XAxis.Label(yLabel);
            //plt.SetAxisLimits(yMin: 0);
            plt.SetAxisLimitsX(0, 100);
            //plot.SetAxisLimitsY(yLimitMin, yLimitMax);
            string path = BuildPath(type, title);
            plt.SaveFig(path);

        }

        public static void CreatePopulationPlot(List<List<double>> values, string title, ChartType type,
            List<string> tickTitles)
        {
            var plt = new Plot(1920, 1080);
            plt.Style(Style);

            var populations = new Population[values.Count];
            string[] populationNames = tickTitles.ToArray();

            for (int i = 0; i < values.Count; i++)
            {
                populations[i] = new Population(values[i].ToArray());
            }

            plt.AddPopulations(populations);
            plt.XAxis.Grid(false);
            plt.XTicks(populationNames);
            string path = BuildPath(type, title);
            plt.SaveFig(path);

        }

        public static void CreatePieChart(List<double> values, string title, ChartType type, List<string> labels)
        {
            var plt = new Plot(1080, 1920);
            plt.Style(Style);

            var pie = plt.AddPie(values.ToArray());
            pie.ShowPercentages = true;
            pie.SliceLabels = labels.ToArray();
            pie.ShowLabels = true;
            string path = BuildPath(type, title);
            plt.SaveFig(path);
        }

        public static ChartModel CalculateRowAndColCount(int length)
        {
            int mod = length % 3;
            int lengthNew = length > 4 ? length - mod + 3 : length;
            int chartsCount = (int) Math.Ceiling((double) lengthNew / 6);

            if (chartsCount <= 1)
                return length switch
                {
                    2 => new ChartModel(1, 2, 1),
                    3 => new ChartModel(1, 3, 1),
                    4 => new ChartModel(2, 2, 1),
                    5 => new ChartModel(2, 3, 1),
                    _ => new ChartModel(1, 1, 1)
                };
            var chartsModel = new ChartModel(2, 3, chartsCount);
            return chartsModel;

        }

        public static void Multiplot(List<Plot> plots, string title, ChartType type)
        {
            var plotsModel = CalculateRowAndColCount(plots.Count);

            for (int i = 0; i < plotsModel.ChartCount; i++)
            {
                var mp = new MultiPlot(1920, 1080, plotsModel.RowCount, plotsModel.ColumnCount);

                for (int j = 0; j < plotsModel.RowCount * plotsModel.ColumnCount; j++)
                {
                    int index = j + (i * 6);
                    if (index < plots.Count)
                    {
                        mp.subplots[j] = plots[index];
                    }
                    else
                    {
                        mp.subplots[j] = new Plot();
                    }
                }

                string path = BuildPath(type, $"{title}-{i}");
                mp.SaveFig(path);
            }
        }


        private static double[] CreateXAxis(int length)
        {
            List<double> values = new();
            for (int i = 0; i < length; i++)
            {
                values.Add(i + 1);
            }

            return values.ToArray();

        }
    }

    public class ChartModel
    {
        public ChartModel(int rowCount, int columnCount, int chartCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;
            ChartCount = chartCount;
        }

        public int RowCount { get; }
        public int ColumnCount { get; }
        public int ChartCount { get; }

    }
}