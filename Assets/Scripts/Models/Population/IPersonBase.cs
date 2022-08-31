using EconomyBase.Controller;
using EconomyBase.Controller.Actions;
using EconomyBase.Enums;
using EconomyBase.Factories;
using EconomyBase.Models.Market;
using EconomyBase.Models.Observations;

namespace EconomyBase.Models.Population
{
    public interface IPersonBase
    {
        void Fire();
        string Id { get; }
        JobStatus JobStatus { get; }
        int Age { get; }
        decimal Capital { get; }
        int UnderageChildrenCount { get; }
        bool StaysChildless { get; set; }
        DeathReason Death { get; set; }
        List<IPersonBase> Children { get; }
        AgeStatus AgeStatus { get; }
        JobModel Job { get; set; }
        decimal MonthlyIncome { get; }
        void UpdateCapital(decimal amount);
        void AddChild(IPersonBase child);
        void AddParents(string idA, string idB);
        decimal Pay();
        void Update(decimal avgIncome, TempPopulationUpdateModel tempPop, PopulationFactory factory, PopulationPropabilityController probController);
    }
}

