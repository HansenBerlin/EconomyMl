﻿using Controller;
using Enums;
using Settings;

namespace Models.Observations
{



    public class PersonObservations
    {
        private readonly PoliciesWrapper _policies;
        private readonly JobMarketController _jobMarket;

        public decimal Capital { get; set; }
        public decimal Salary { get; set; }
        public decimal MonthlyExpensesAccumulatedForYear { get; set; }
        public decimal MonthlyIncomeAccumulatedForYear { get; set; }
        public decimal DesiredSalary { get; set; }
        public decimal SatisfactionRate { get; set; }
        public long UnsatisfiedBaseDemand { get; set; }
        public long LuxuryProducts { get; set; }
        public JobStatus JobStatus { get; set; }
        public decimal AverageIncome { get; set; }

        public int OpenJobPositions => _jobMarket.OpenJobPositionsCount();
        //private List<IPersonBase> _children;
        //public int UnderageChildrenCount => _children.Count(c => c.AgeStatus == AgeStatus.UnderageChild);

        public int Age { get; set; }

        private AgeStatus _ageStatus;
        
        public float JobReward;
        public float BaseBuyReward;
        public float LuxuryBuyReward;


        public AgeStatus AgeStatus
        {
            get
            {
                _ageStatus = GetAgeStatus();
                return _ageStatus;
            }

            set => _ageStatus = value;
        }
        

        private AgeStatus GetAgeStatus()
        {
            int adultMinAge = _policies.AgeBoundaries.AdultMinAge;
            int workerMaxAge = _policies.AgeBoundaries.WorkerMaxAge;
            if (_ageStatus == AgeStatus.Dead)
                return AgeStatus.Dead;
            if (Age > 0 && Age < adultMinAge)
                return AgeStatus.UnderageChild;
            if (Age >= adultMinAge && Age < workerMaxAge)
                return AgeStatus.WorkerAge;
            return AgeStatus.RetiredAge;
        }

        public PersonObservations(int age, decimal desiredSalary, decimal capital, PoliciesWrapper policies, JobMarketController jobMarket)
        {
            _policies = policies;
            _jobMarket = jobMarket;
            Age = age;
            DesiredSalary = desiredSalary;
            Capital = capital;
        }

        public float GetSatisfactionRate()
        {
            return 0;
        }

    }
}