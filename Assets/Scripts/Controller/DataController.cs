using System.Collections.Generic;
using Assets.Scripts.Repositories;

namespace Assets.Scripts.Controller
{



    public class DataController
    {
        private readonly int _simulationDurationInYears;
        private readonly PopulationDataRepository _populationData;

        public DataController(PopulationDataRepository populationData, int simulationDurationInYears)
        {
            _populationData = populationData;
            _simulationDurationInYears = simulationDurationInYears;
        }

        public void Create()
        {
            var ageData1 = _populationData.GetAgeDistribution(0);
            var ageData2 = _populationData.GetAgeDistribution(_simulationDurationInYears / 2);
            var ageData3 = _populationData.GetAgeDistribution(_simulationDurationInYears);
            var popList = new List<List<double>> {ageData1, ageData2, ageData3};
            

            var childCountData1 = _populationData.GetAverageChildCountInAdultPopulation(0);
            var childCountData2 = _populationData.GetAverageChildCountInAdultPopulation(_simulationDurationInYears / 2);
            var childCountData3 = _populationData.GetAverageChildCountInAdultPopulation(_simulationDurationInYears);
            

        }
    }
}