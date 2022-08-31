using EconomyBase.Controller;
using EconomyBase.Controller.Actions;
using EconomyBase.Controller.Rewards;
using EconomyBase.Enums;
using EconomyBase.Factories;
using EconomyBase.Models.Market;
using EconomyBase.Models.Observations;
using EconomyBase.Models.Population;
using EconomyBase.Settings;

namespace EconomyBase.Models.Agents
{




    public class PersonAgent : IPersonBase
    {
        private readonly PersonController _controller;
        private readonly PersonRewardController _rewardController;

        public bool IsDummy { get; protected init; }
        public string Id { get; } = Guid.NewGuid().ToString();
        private string _parentAId;
        private string _parentBId;
        public JobStatus JobStatus => _observations.JobStatus;
        public int Age => _observations.Age;
        public decimal Capital => _observations.Capital;
        public decimal MonthlyIncome => _observations.MonthlyIncome;
        public int UnderageChildrenCount => Children.Count(c => c.AgeStatus == AgeStatus.UnderageChild);
        public bool StaysChildless { get; set; }
        public DeathReason Death { get; set; } = DeathReason.HasNotDied;
        public List<IPersonBase> Children { get; } = new();
        public AgeStatus AgeStatus => _observations.AgeStatus;
        public JobModel Job { get; set; }

        private PersonActionsJobPhase _jobActions;
        private PersonActionsBuyBaseProductPhase _baseBuyActions;
        private PersonActionsBuyLuxuryProductPhase _luxuryBuyActions;
        private readonly PersonObservations _observations;

        public PersonAgent(string parentAId, string parentBId, PersonObservations observations,
            PersonController controller, PersonRewardController rewardController)
        {
            _observations = observations;
            _controller = controller;
            _rewardController = rewardController;
            _controller.Setup(this, _observations, _rewardController);
            var actions = _controller.InitActions();
            _jobActions = actions[0] as PersonActionsJobPhase;
            _baseBuyActions = actions[1] as PersonActionsBuyBaseProductPhase;
            _luxuryBuyActions = actions[2] as PersonActionsBuyLuxuryProductPhase;
            AddParents(parentAId, parentBId);
            StaysChildless = StatisticalDistributionController.CreateRandom(0, 10) == 1;
        }

        public PersonAgent()
        {
        }

        public void Update(decimal avgIncome, TempPopulationUpdateModel tempPop, PopulationFactory factory,
            PopulationPropabilityController probController)
        {
            _controller.Update(avgIncome, tempPop, factory, probController);
        }

        public void UpdateCapital(decimal amount)
        {
            _observations.Capital += amount;
        }

        public void AddChild(IPersonBase child)
        {
            Children.Add(child);
        }

        public void AddParents(string idA, string idB)
        {
            _parentAId = idA;
            _parentBId = idB;
        }

        public void Fire()
        {
            _controller.RemoveJobWhenFired();
        }

        public decimal Pay()
        {
            return _controller.Pay();
        }
    }
}