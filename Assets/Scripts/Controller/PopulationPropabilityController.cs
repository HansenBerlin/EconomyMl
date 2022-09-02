using System;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{



    public class PopulationPropabilityController : MonoBehaviour
    {
        public GameObject DataTemplateModel;
        private PopulationDataTemplateModel _dataTemplate;
        public List<int> AgeDistribution { get; private set; }
        private List<double> _qualificationDistribution;

        public void Awake()
        {
            _dataTemplate = DataTemplateModel.GetComponent<PopulationDataTemplateModel>();
            AgeDistribution = _dataTemplate.CreateAgeDistributionTemplate();
            _qualificationDistribution = _dataTemplate.CreateQualificationStructure();
        }

        public bool IsDead(int age)
        {
            int index = age == 0 ? 0 : age > 95 ? 20 : (int) Math.Floor((double) age / 5) + 1;
            double deathPropability = _dataTemplate.CreateDeathPropabilityDistribution()[index];
            int rn = StatisticalDistributionController.CreateRandom(0, 10001);
            return rn < deathPropability * 100;
        }


        public decimal[] InitialIncomeAndCapital(int age)
        {
            var rnIndex = StatisticalDistributionController.CreateRandom(1, 10);
            List<double> distAge = _dataTemplate.IncomeDistributionByAge(age);
            List<double> distCapital = _dataTemplate.CapitalDistribution();

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