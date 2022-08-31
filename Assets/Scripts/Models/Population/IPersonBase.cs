using System.Collections.Generic;
using Controller;
using Enums;
using Factories;
using Models.Meta;

namespace Models.Population
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
        void ResetMasking();

        void YearlyAgentUpdate(decimal avgIncome, TempPopulationUpdateModel tempPop, PopulationFactory factory, PopulationPropabilityController probController);
    }
}

