using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.Models.Agents;
using Assets.Scripts.Models.Meta;
using Assets.Scripts.Repositories;
using Unity.MLAgents;

namespace Assets.Scripts.Models.Population
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
        public readonly List<double> DiedPercentageStat = new();
        public readonly List<double> BornPercentageStat = new();
        public readonly List<double> EmploymentRate = new();
        public float LastHappiness { get; private set; }
        public float LastEmploymentRate { get; private set; }
        public float LastAvgCapital { get; private set; }
        public float LastAvgAge { get; private set; }

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
            double statHappinessSum = 0;
            

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
                statHappinessSum += person.Happiness;
                

            }

            double statUnderagedTotal = AgeRangeChildren.Count;
            double statWorkerAgeTotal = AgeRangeWorker.Count;
            double statRetirementAgeTotal = AgeRangeRetired.Count;
            double statDiedTotal = Population.Count(p => p.AgeStatus == AgeStatus.Dead);
            double statTotalPopulation = Population.Count - statDiedTotal;
            //double statAverageDeathAge = deathAgeAggregate / deathReasonTotal;

            _populationData.AddNewPersonsDataset(Year, Population);

            Population.RemoveAll(p => p.AgeStatus == AgeStatus.Dead);

            TotalUnderAgeChildrenTrend.Add(statUnderagedTotal);
            TotalWorkerAgeTrend.Add(statWorkerAgeTotal);
            TotalRetiredAgeTrend.Add(statRetirementAgeTotal);
            TotalPopulationTrend.Add(statTotalPopulation);
            AverageAge.Add(statAgeSum / statTotalPopulation);
            _allTimeDeaths += statDiedTotal;
            AllTimeChildrenStat.Add(_allTimeChildren);
            AllTimeDeathStat.Add(_allTimeDeaths);
            AllTimePopulationTrendStat.Add(_allTimeChildren - _allTimeDeaths);
            DiedPercentageStat.Add(statDiedTotal / statTotalPopulation * 100);
            LastEmploymentRate = EmployedWorkers.Count / (EmployedWorkers.Count + UnemployedWorkers.Count) * 100;
            LastHappiness = (float)statHappinessSum / (float)statTotalPopulation;
            LastAvgAge = (float)statAgeSum / (float)statTotalPopulation;
            LastAvgCapital = (float) statCapitalSum / (float) statTotalPopulation;

            statsRecorder.Add("POP/TOTAL", (float)statTotalPopulation);
            statsRecorder.Add("POP/AVG AGE", (float)(statAgeSum / statTotalPopulation));
            statsRecorder.Add("POP/AVG CAPITAL", (float)(statCapitalSum / statTotalPopulation));
            statsRecorder.Add("POP/EMPL RATE", LastEmploymentRate);
            statsRecorder.Add("POP/HAPPINESS", LastHappiness);
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