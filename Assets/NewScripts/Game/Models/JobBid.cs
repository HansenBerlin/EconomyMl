using NewScripts.Interfaces;

namespace NewScripts.Game.Models
{
    public class JobBid
    {
        public ICompany Employer { get; }
        public decimal Wage { get; }
        
        public JobBid(ICompany employer, decimal wage)
        {
            Employer = employer;
            Wage = wage;
        }
    }
}