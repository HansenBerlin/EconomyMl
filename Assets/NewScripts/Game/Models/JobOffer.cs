using NewScripts.Game.Entities;

namespace NewScripts.Game.Models
{
    public class JobOffer
    {
        public Worker Worker { get; }
        public decimal Wage { get; }
        
        public JobOffer(Worker worker, decimal wage)
        {
            Worker = worker;
            Wage = wage;
        }
    }
}