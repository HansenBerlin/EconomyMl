using System.Collections.Generic;
using System.Linq;
using Agents;
using Enums;

namespace Repositories
{
    public class PopulationDataRepository
    {
        private readonly Dictionary<int, List<PersonAgent>> _populationData = new();
        private readonly Dictionary<int, List<double>> ChildPerPersonCountData = new();
        private readonly Dictionary<int, List<double>> PopulationAgeData = new();

        public void AddNewPersonsDataset(int year, List<PersonAgent> values)
        {
            _populationData.Add(year, values);
            PopulationAgeData.Add(year, values.Select(p => (double) p.Age).ToList());
            var childCountInAdultPopulationData = GetChildCountInAdultPopulation(values);
            ChildPerPersonCountData.Add(year, childCountInAdultPopulationData);
        }

        private List<double> GetChildCountInAdultPopulation(List<PersonAgent> pop)
        {
            var searchInPersons = pop
                .Where(p => p.AgeStatus > AgeStatus.UnderageChild).ToList();

            var values = new double[4];

            foreach (var p in searchInPersons)
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

            return values.ToList();
        }
    }
}