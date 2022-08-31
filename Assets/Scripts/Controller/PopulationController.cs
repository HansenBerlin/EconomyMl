using EconomyBase.Enums;
using EconomyBase.Factories;
using EconomyBase.Models;
using EconomyBase.Models.Market;
using EconomyBase.Models.Meta;
using EconomyBase.Models.Population;

namespace EconomyBase.Controller
{



    public class PopulationController
    {
        private readonly EnvironmentModel _env;
        private int Year => _env.Year;

        private readonly PopulationModel _populationModel;
        private readonly JobMarketController _jobController;
        private readonly PopulationFactory _factory;
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

        public void DailyUpdatePopulation(ICountryEconomyMarketsModel countryEconomyMarkets, int dayOfMonth)
        {
            var rng = StatisticalDistributionController.Rng;

            foreach (var person in _populationModel.AgeRangeAdult.OrderBy(_ => rng.Next()))
            {
                person.ActionBuyDailyStuff(countryEconomyMarkets, dayOfMonth);
            }
        }

        public void MonthlyUpdatePopulation(ICountryEconomyMarketsModel countryEconomyMarkets)
        {
            var rng = StatisticalDistributionController.Rng;

            foreach (var person in _populationModel.AgeRangeAdult.OrderBy(_ => rng.Next()))
            {
                person.ActionBuyMonthlyStuff(countryEconomyMarkets);
                person.ActionRethinkJobSituation(_jobController);
                person.UpdateExpenses();
            }
        }

        private decimal AverageIncome(IReadOnlyCollection<IPersonBase> searchIn)
        {
            double totalIncome = searchIn.Sum(w => (double) w.MonthlyIncome);
            var rt = searchIn.Count > 0 ? totalIncome / searchIn.Count : 0;
            return (decimal) rt;
        }

        public decimal AverageWorkerIncome()
        {
            var searchIn = _populationModel.Population.Where(p => p.JobStatus == JobStatus.Employed).ToList();
            double totalIncome = searchIn.Sum(w => (double) w.MonthlyIncome);
            var rt = searchIn.Count > 0 ? totalIncome / searchIn.Count : 0;
            return (decimal) rt;
        }

        public void YearlyUpdatePopulation()
        {
            var range = Enumerable.Range(0, _populationModel.PopulationCount).ToList();
            var avgIncome = AverageIncome(_populationModel.AgeRangeAdult);
            var tempChanges = new TempPopulationUpdateModel(_populationModel.Population);
            while (range.Count > 0)
            {
                int rn = StatisticalDistributionController.CreateRandom(0, range.Count);
                int indexpick = range[rn];
                var person = _populationModel.Population[indexpick];
                person.Update(avgIncome, tempChanges, _factory, _propabilityController);
                range.RemoveAt(rn);
            }

            _populationModel.Population.AddRange(tempChanges.Born);
            foreach (var dead in tempChanges.Died)
            {
                _populationModel.Population.Remove(dead);
            }


            _populationModel.UpdateData();
        }
    }
}