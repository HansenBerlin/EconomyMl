using System.Collections.Generic;
using System.Linq;
using Agents;
using Controller.Data;
using Enums;
using Factories;
using Interfaces;
using Models;

namespace Controller.RepositoryController
{
    public class PopulationController
    {
        private readonly EnvironmentModel _env;
        private readonly PopulationFactory _factory;
        private readonly JobMarketController _jobController;

        private readonly PopulationModel _populationModel;
        private readonly PopulationPropabilityController _propabilityController;

        public PopulationController(EnvironmentModel env, PopulationModel populationModel,
            JobMarketController jobController, PopulationFactory factory,
            PopulationPropabilityController propabilityController)
        {
            _env = env;
            _populationModel = populationModel;
            _jobController = jobController;
            _factory = factory;
            _propabilityController = propabilityController;
        }

        private int Year => _env.Year;

        public void Setup()
        {
            var rng = StatisticalDistributionController.Rng;

            foreach (var person in _populationModel.AgeRangeAdult.OrderBy(_ => rng.Next()))
                person.SetupWorkState(_jobController);
        }


        public void MonthlyUpdatePopulation(ICountryEconomy countryEconomyMarkets, int month)
        {
            var rng = StatisticalDistributionController.Rng;

            foreach (var person in _populationModel.Population.OrderBy(_ => rng.Next()))
                person.RequestMonthlyDecisions(AverageWorkerIncome());
        }

        public void SetupMonth()
        {
            foreach (var person in _populationModel.Population)
                person.InitMonth();
        }

        private decimal AverageIncome(IReadOnlyCollection<PersonAgent> searchIn)
        {
            double totalIncome = searchIn.Sum(w => (double) w.MonthlyIncome);
            double rt = searchIn.Count > 0 ? totalIncome / searchIn.Count : 0;
            return (decimal) rt;
        }

        private decimal AverageWorkerIncome()
        {
            var searchIn = _populationModel.Population.Where(p => p.JobStatus == JobStatus.Employed).ToList();
            double totalIncome = searchIn.Sum(w => (double) w.MonthlyIncome);
            double rt = searchIn.Count > 0 ? totalIncome / searchIn.Count : 0;
            return (decimal) rt;
        }

        public void YearlyUpdatePopulation()
        {
            var range = Enumerable.Range(0, _populationModel.PopulationCount).ToList();
            decimal avgIncome = AverageWorkerIncome();
            var tempChanges = new TempPopulationUpdateModel(_populationModel.Population);
            while (range.Count > 0)
            {
                int rn = StatisticalDistributionController.CreateRandom(0, range.Count);
                int indexpick = range[rn];
                var person = _populationModel.Population[indexpick];
                person.YearlyAgentUpdate(avgIncome, tempChanges, _factory, _propabilityController);
                range.RemoveAt(rn);
            }

            _populationModel.Population.AddRange(tempChanges.Born);
            foreach (var dead in tempChanges.Died) _populationModel.Population.Remove(dead);
            _populationModel.UpdateData();
        }
    }
}