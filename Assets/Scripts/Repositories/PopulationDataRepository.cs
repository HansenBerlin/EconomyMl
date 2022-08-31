using System.Collections.Generic;
using System.Linq;
using Enums;
using Models.Population;

namespace Repositories
{



    public class PopulationDataRepository
    {
        public readonly Dictionary<int, List<double>> PopulationAgeData = new();
        public readonly Dictionary<int, List<double>> ChildPerPersonCountData = new();
        private readonly Dictionary<int, List<IPersonBase>> _populationData = new();

        public void AddNewPersonsDataset(int year, List<IPersonBase> values)
        {
            _populationData.Add(year, values);
            PopulationAgeData.Add(year, values.Select(p => (double) p.Age).ToList());
            var childCountInAdultPopulationData = GetChildCountInAdultPopulation(values);
            ChildPerPersonCountData.Add(year, childCountInAdultPopulationData);
        }

        public List<double> GetAgeDistribution(int forYear)
        {
            return PopulationAgeData[forYear];
        }

        public List<double> GetAverageChildCountInAdultPopulation(int forYear)
        {
            return ChildPerPersonCountData[forYear];
        }

        /*public void GetChildCountInAdultPopulation(int forYear, int tillYear)
        {
            List<IPersonBase> searchInPersons = _populationData[forYear]
                .Where(p => p.AgeStatus > AgeStatus.UnderageChild).ToList();
    
            var plt = new ScottPlot.Plot(1920, 1080);
    
            int count = tillYear - forYear + 1;
            double[] zeroChildren = new double[count];
            double[] oneChild = new double[count];
            double[] twoChildren = new double[count];
            double[] threeChildrenOrMore = new double[count];
    
            for (int i = 0; i < searchInPersons.Count; i++)
            {
                var p = searchInPersons[i];
                if (p.ChildrenCount == 0)
                {
                    zeroChildren[0]++;
                }
                else if (p.ChildrenCount == 1)
                {
                    oneChild[0]++;
                }
                else if (p.ChildrenCount == 2)
                {
                    twoChildren[0]++;
                }
                else
                {
                    threeChildrenOrMore[0]++;
                }
            }
    
            double[] threeChildrenOrMore2 = new double[count];
            for (int i = 0; i < count; i++)
                threeChildrenOrMore2[i] = zeroChildren[i] + oneChild[i] + twoChildren[i] + threeChildrenOrMore[i];
            
            double[] twoChildren2 = new double[count];
            for (int i = 0; i < count; i++)
                twoChildren2[i] = zeroChildren[i] + oneChild[i] + twoChildren[i];
            
            double[] oneChild2 = new double[count];
            for (int i = 0; i < count; i++)
                oneChild2[i] = zeroChildren[i] + oneChild[i];
    
            plt.AddBar(threeChildrenOrMore2);
            plt.AddBar(twoChildren2);
            plt.AddBar(oneChild2);
            plt.AddBar(zeroChildren);
            plt.SetAxisLimits(yMin: 0);
    
            string path = ChartFactory.BuildPath(ChartType.Population, "childcounts");
    
            plt.SaveFig(path);
        }*/

        private List<double> GetChildCountInAdultPopulation(List<IPersonBase> pop)
        {
            List<IPersonBase> searchInPersons = pop
                .Where(p => p.AgeStatus > AgeStatus.UnderageChild).ToList();

            double[] values = new double[4];

            foreach (var p in searchInPersons)
            {
                switch (p.Children.Count)
                {
                    case 0:
                        values[0]++;
                        break;
                    case 1:
                        values[1]++;
                        break;
                    case 2:
                        values[2]++;
                        break;
                    default:
                        values[3]++;
                        break;
                }
            }

            return values.ToList();
        }


    }
}