using System.Collections.Generic;
using System.Linq;
using Controller.Data;
using Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Factories
{
    public class PopulationDataTemplateModel : MonoBehaviour
    {
        [FormerlySerializedAs("_type")]
        [Tooltip("Enum of type DevelopingCountrySociety, EmergingCountrySociety or IndustrialCountrySociety")]
        public DemographyType type;

        [FormerlySerializedAs("_initialPopulationCount")]
        public int initialPopulationCount;


        public List<int> CreateAgeDistributionTemplate()
        {
            List<int> distribution = new();
            if (type != DemographyType.IndustrialCountrySociety) return distribution;
            double[] distributionPerDecade = {9.1, 9.3, 11.8, 12.8, 12.6, 16.2, 12.4, 9.3, 3.7, 2.7};
            for (var i = 0; i < distributionPerDecade.Length; i++)
            {
                var paritalCount = (int) (initialPopulationCount * distributionPerDecade[i] / 100);
                for (var j = 0; j < paritalCount; j++)
                {
                    int min = i * 10;
                    int max = (i + 1) * 10;
                    int randomAge = StatisticalDistributionController.CreateRandom(min, max);
                    distribution.Add(randomAge);
                }
            }

            return distribution;
        }

        public List<double> CreateDeathPropabilityDistribution()
        {
            if (type == DemographyType.IndustrialCountrySociety)
                return new[]
                {
                    0.3, 0.01, 0.01, 0.01, 0.02,
                    0.03, 0.03, 0.045, 0.075, 0.125,
                    0.19, 0.315, 0.535, 0.9, 1.4,
                    2.1, 3.9, 6, 11.8, 22.1,
                    35.6
                }.ToList();

            return new List<double>();
        }

        public List<double> CreateQualificationStructure()
        {
            if (type == DemographyType.IndustrialCountrySociety) return new double[] {16, 64, 20}.ToList();

            return new List<double>();
        }

        public List<double> CapitalDistribution()
        {
            if (type == DemographyType.IndustrialCountrySociety)
                return new double[] {-13000, 0, 1800, 7500, 18000, 40000, 80000, 130000, 210000, 610000}.ToList();

            return new List<double>();
        }

        public List<double> IncomeDistributionByAge(int age)
        {
            if (type != DemographyType.IndustrialCountrySociety) return new List<double>();
            return age switch
            {
                < 25 => new double[] {770, 1000, 1200, 1400, 1600, 1800, 2100, 2400, 3000, 7000}.ToList(),
                >= 25 and < 50 => new double[] {900, 1200, 1400, 1700, 1900, 2100, 2400, 2750, 3300, 7000}.ToList(),
                >= 50 and < 65 => new double[] {1050, 1400, 1700, 1950, 2200, 2500, 2800, 3300, 4100, 7000}.ToList(),
                _ => new double[] {1050, 1300, 1450, 1600, 1800, 2000, 2250, 2600, 3300, 7000}.ToList()
            };
        }
    }
}