using System.Collections.Generic;
using System.Linq;
using Enums;
using Models.Agents;
using Models.Meta;
using Repositories;
using Unity.MLAgents;

namespace Models.Population
{



    public class PopulationModel
    {
        private List<PersonAgent> EmployedWorkers => Population.Where(p => p.JobStatus <= JobStatus.Employed).ToList();

        public List<PersonAgent> UnemployedWorkers =>
            Population.Where(p => p.JobStatus <= JobStatus.Unemployed).ToList();

        private List<PersonAgent> AgeRangeChildren =>
            Population.Where(p => p.AgeStatus <= AgeStatus.UnderageChild).ToList();

        private List<PersonAgent> AgeRangeWorker => Population.Where(p => p.AgeStatus == AgeStatus.WorkerAge).ToList();
        public List<PersonAgent> AgeRangeRetired => Population.Where(p => p.AgeStatus == AgeStatus.RetiredAge).ToList();
        public List<PersonAgent> AgeRangeAdult => Population.Where(p => p.AgeStatus > AgeStatus.UnderageChild).ToList();
        public List<PersonAgent> Population { get; }
        public int PopulationCount => Population.Count;


        private readonly EnvironmentModel _env;

        private int Year => _env.Year;
        //private List<JobPositionModel> OpenJobPositions { get; set; }

        public readonly List<double> TotalPopulationTrend = new();
        public readonly List<double> AverageAge = new();
        public readonly List<double> AverageCapital = new();
        public readonly List<double> AverageIncomeWorkerAge = new();
        public readonly List<double> AverageIncomeRetiredAge = new();
        public readonly List<double> AverageIncomeEmployed = new();
        public readonly List<double> AverageIncomeUnemployed = new();
        public readonly List<double> AverageIncomeAdultAge = new();
        public readonly List<double> TotalWorkerAgeTrend = new();
        public readonly List<double> TotalRetiredAgeTrend = new();
        public readonly List<double> TotalUnderAgeChildrenTrend = new();
        public readonly List<double> AllTimeDeathStat = new();
        public readonly List<double> AllTimeChildrenStat = new();
        public readonly List<double> AllTimePopulationTrendStat = new();
        public readonly List<double> ChildrenStatAdded = new();
        public readonly List<double> ParentOfUnderagedAverageAge = new();
        public readonly List<double> AvgUnderageChildrenPerAdult = new();
        public readonly List<double> DiedPercentageStat = new();
        public readonly List<double> BornPercentageStat = new();
        public readonly List<double> EmploymentRate = new();

        private double _allTimeDeaths;
        private double _allTimeChildren;


        private readonly PopulationDataRepository _populationData;


        public PopulationModel(List<PersonAgent> initialPopulation, PopulationDataRepository populationData,
            EnvironmentModel env)
        {
            Population = initialPopulation;
            _populationData = populationData;
            _env = env;
        }

        public void UpdateData()
        {
            var statsRecorder = Academy.Instance.StatsRecorder;

            double statAgeSum = 0;
            double statCapitalSum = 0;
            double statUnderageChildrenAdultsSum = 0;
            double statParentsOfUnderageCount = 0;
            double statParentsOfUnderageAgeSum = 0;

            //AverageIncomeAdultAge.Add((double) AverageIncome(AgeRangeAdult));
            //AverageIncomeWorkerAge.Add((double) AverageIncome(AgeRangeWorker));
            //AverageIncomeRetiredAge.Add((double) AverageIncome(AgeRangeRetired));
            //AverageIncomeEmployed.Add((double) AverageIncome(EmployedWorkers));
            //AverageIncomeUnemployed.Add((double) AverageIncome(UnemployedWorkers));
            statsRecorder.Add("POP/INCOME AVG WORKER", (float)AverageIncome(AgeRangeWorker));

            foreach (var person in Population)
            {
                statAgeSum += person.Age;
                statCapitalSum += (double) person.Capital;
                statUnderageChildrenAdultsSum += person.UnderageChildrenCount;
                statParentsOfUnderageCount += person.UnderageChildrenCount > 0 ? 1 : 0;
                statParentsOfUnderageAgeSum += person.UnderageChildrenCount > 0 ? person.Age : 0;

            }

            double statUnderagedTotal = AgeRangeChildren.Count;
            double statWorkerAgeTotal = AgeRangeWorker.Count;
            double statRetirementAgeTotal = AgeRangeRetired.Count;
            double statDiedTotal = Population.Count(p => p.AgeStatus == AgeStatus.Dead);
            double statTotalPopulation = Population.Count - statDiedTotal;
            //double statAverageDeathAge = deathAgeAggregate / deathReasonTotal;

            _populationData.AddNewPersonsDataset(Year, Population);

            Population.RemoveAll(p => p.AgeStatus == AgeStatus.Dead);

            /*DeathReasonAge.Add(deathReasonAge);
            DeathReasonOther.Add(deathReasonOther);
            DeathReasonStarved.Add(deathReasonStarved);
            DeathReasonsTotal.Add(deathReasonTotal);
            DeathAverageAge.Add(statAverageDeathAge);
            if (deathReasonTotal - deathReasonAge - deathReasonOther - deathReasonStarved != 0)
                throw new Exception();*/

            TotalUnderAgeChildrenTrend.Add(statUnderagedTotal);
            TotalWorkerAgeTrend.Add(statWorkerAgeTotal);
            TotalRetiredAgeTrend.Add(statRetirementAgeTotal);
            TotalPopulationTrend.Add(statTotalPopulation);
            ParentOfUnderagedAverageAge.Add(statParentsOfUnderageAgeSum / statParentsOfUnderageCount);
            AverageAge.Add(statAgeSum / statTotalPopulation);
            AverageCapital.Add(statCapitalSum / statTotalPopulation);
            _allTimeDeaths += statDiedTotal;
            AllTimeChildrenStat.Add(_allTimeChildren);
            AllTimeDeathStat.Add(_allTimeDeaths);
            AvgUnderageChildrenPerAdult.Add(statUnderageChildrenAdultsSum / statParentsOfUnderageCount);
            AllTimePopulationTrendStat.Add(_allTimeChildren - _allTimeDeaths);
            DiedPercentageStat.Add(statDiedTotal / statTotalPopulation * 100);
            //AverageSkillLevel.Add(statSkillSum / statTotalPopulation);


            double employmentRate = (double) EmployedWorkers.Count / (EmployedWorkers.Count + UnemployedWorkers.Count) *
                                    100;

            statsRecorder.Add("POP/TOTAL", (float)statTotalPopulation);
            statsRecorder.Add("POP/AVG AGE", (float)(statAgeSum / statTotalPopulation));
            statsRecorder.Add("POP/AVG CAPITAL", (float)(statCapitalSum / statTotalPopulation));
            statsRecorder.Add("POP/EMPL RATE", (float)employmentRate);
            //EmploymentRate.Add(employmentRate);

        }

        private decimal AverageIncome(IReadOnlyCollection<PersonAgent> searchIn)
        {
            double totalIncome = searchIn.Sum(w => (double) w.MonthlyIncome);
            var rt = searchIn.Count > 0 ? totalIncome / searchIn.Count : 0;
            return (decimal) rt;
        }




    }
}