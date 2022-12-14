using System;
using System.Collections.Generic;
using Factories;

namespace Controller.Data
{
    public class PopulationPropabilityController
    {
        private readonly PopulationDataTemplateModel _dataTemplate;
        private List<double> _qualificationDistribution;

        public PopulationPropabilityController(PopulationDataTemplateModel dataTemplate)
        {
            _dataTemplate = dataTemplate;
            AgeDistribution = _dataTemplate.CreateAgeDistributionTemplate();
            _qualificationDistribution = _dataTemplate.CreateQualificationStructure();
        }

        public List<int> AgeDistribution { get; }

        public bool IsDead(int age)
        {
            int index = age == 0 ? 0 : age > 95 ? 20 : (int) Math.Floor((double) age / 5) + 1;
            double deathPropability = _dataTemplate.CreateDeathPropabilityDistribution()[index];
            int rn = StatisticalDistributionController.CreateRandom(0, 10001);
            return rn < deathPropability * 100;
        }


        public decimal[] InitialIncomeAndCapital(int age)
        {
            int rnIndex = StatisticalDistributionController.CreateRandom(1, 10);
            var distAge = _dataTemplate.IncomeDistributionByAge(age);
            var distCapital = _dataTemplate.CapitalDistribution();

            double lowAge = distAge[rnIndex - 1];
            double lowC = distCapital[rnIndex - 1];
            double highAge = distAge[rnIndex];
            double highC = distCapital[rnIndex];
            double ageModifier = (age - 42) * 0.625 / 100;

            decimal income = StatisticalDistributionController.CreateRandom((int) lowAge, (int) highAge);
            decimal capital = StatisticalDistributionController.CreateRandom((int) lowC, (int) highC);
            income += income * (decimal) ageModifier;
            return new[] {income, capital};
        }
    }
}