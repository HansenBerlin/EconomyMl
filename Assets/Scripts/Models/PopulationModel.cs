using System.Collections.Generic;
using System.Linq;
using Agents;
using Enums;
using Repositories;
using Unity.MLAgents;

namespace Models
{
    public class PopulationModel
    {
        private readonly EnvironmentModel _env;
        private readonly PopulationDataRepository _populationData;
        private double _allTimeChildren;


        public PopulationModel(List<PersonAgent> initialPopulation, PopulationDataRepository populationData,
            EnvironmentModel env)
        {
            Population = initialPopulation;
            _populationData = populationData;
            _env = env;
        }

        private List<PersonAgent> EmployedWorkers => Population.Where(p => p.JobStatus <= JobStatus.Employed).ToList();

        public List<PersonAgent> UnemployedWorkers =>
            Population.Where(p => p.JobStatus <= JobStatus.Unemployed).ToList();

        private List<PersonAgent> AgeRangeWorker => Population.Where(p => p.AgeStatus == AgeStatus.WorkerAge).ToList();
        public List<PersonAgent> AgeRangeRetired => Population.Where(p => p.AgeStatus == AgeStatus.RetiredAge).ToList();
        public List<PersonAgent> AgeRangeAdult => Population.Where(p => p.AgeStatus > AgeStatus.UnderageChild).ToList();
        public List<PersonAgent> Population { get; }
        public int PopulationCount => Population.Count;
        private int Year => _env.Year;
        public float LastHappiness { get; private set; }
        public float LastEmploymentRate { get; private set; }
        public float LastAvgCapital { get; private set; }
        public float LastAvgAge { get; private set; }

        public void UpdateData()
        {
            var statsRecorder = Academy.Instance.StatsRecorder;

            double statAgeSum = 0;
            double statCapitalSum = 0;
            double statHappinessSum = 0;

            statsRecorder.Add("POP/INCOME AVG WORKER", (float) AverageIncome(AgeRangeWorker));

            foreach (var person in Population)
            {
                statAgeSum += person.Age;
                statCapitalSum += (double) person.Capital;
                statHappinessSum += person.Happiness;
            }

            double statDiedTotal = Population.Count(p => p.AgeStatus == AgeStatus.Dead);
            double statTotalPopulation = Population.Count - statDiedTotal;

            _populationData.AddNewPersonsDataset(Year, Population);

            Population.RemoveAll(p => p.AgeStatus == AgeStatus.Dead);

            LastEmploymentRate = EmployedWorkers.Count / (EmployedWorkers.Count + UnemployedWorkers.Count) * 100;
            LastHappiness = (float) statHappinessSum / (float) statTotalPopulation;
            LastAvgAge = (float) statAgeSum / (float) statTotalPopulation;
            LastAvgCapital = (float) statCapitalSum / (float) statTotalPopulation;

            statsRecorder.Add("POP/TOTAL", (float) statTotalPopulation);
            statsRecorder.Add("POP/AVG AGE", (float) (statAgeSum / statTotalPopulation));
            statsRecorder.Add("POP/AVG CAPITAL", (float) (statCapitalSum / statTotalPopulation));
            statsRecorder.Add("POP/EMPL RATE", LastEmploymentRate);
            statsRecorder.Add("POP/HAPPINESS", LastHappiness);
        }

        private decimal AverageIncome(IReadOnlyCollection<PersonAgent> searchIn)
        {
            double totalIncome = searchIn.Sum(w => (double) w.MonthlyIncome);
            double rt = searchIn.Count > 0 ? totalIncome / searchIn.Count : 0;
            return (decimal) rt;
        }
    }
}