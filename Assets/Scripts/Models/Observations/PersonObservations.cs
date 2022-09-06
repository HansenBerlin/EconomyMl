using Controller.Data;
using Controller.RepositoryController;
using Enums;
using Models.Finance;
using Settings;

namespace Models.Observations
{



    public class PersonObservations
    {
        private readonly PoliciesWrapper _policies;
        private readonly JobMarketController _jobMarket;
        private readonly NormalizationController _normController;
        public BankAccountModel BankAccount { get; set; }
        public long LuxuryProducts { get; set; }
        public int OpenJobPositions => _jobMarket.OpenJobPositionsCount();
        public long UnsatisfiedBaseDemand { get; set; }
        public decimal Capital => BankAccount.Savings;
        public decimal DesiredSalary { get; set; }
        public decimal Salary { get; set; }
        public decimal LastSalaryBeforeRetirement { get; set; }
        public decimal LoansTakenSum => BankAccount.LoansSum;
        public decimal MonthlyExpensesAccumulatedForYear { get; set; }
        public decimal MonthlyIncomeAccumulatedForYear { get; set; }
        public decimal AverageIncome { get; set; }
        public float ObsLoansSum => _normController.Normalize(nameof(LoansTakenSum), (float)LoansTakenSum);
        public float ObsLuxuryProducts => _normController.Normalize(nameof(LuxuryProducts), LuxuryProducts);
        public float ObsOpenJobPositions  => _normController.Normalize(nameof(OpenJobPositions), OpenJobPositions);
        public float ObsUnsatisfiedBaseDemand  => _normController.Normalize(nameof(UnsatisfiedBaseDemand), UnsatisfiedBaseDemand);
        public float ObsCapital  => _normController.Normalize(nameof(Capital), (float)Capital);
        public float ObsDesiredSalary  => _normController.Normalize(nameof(DesiredSalary), (float)DesiredSalary);
        public float ObsSalary  => _normController.Normalize(nameof(Salary), (float)Salary);
        public float ObsMonthlyExpensesAccumulatedForYear => _normController.Normalize(nameof(MonthlyExpensesAccumulatedForYear), (float)MonthlyExpensesAccumulatedForYear);
        public float ObsMonthlyIncomeAccumulatedForYear  => _normController.Normalize(nameof(MonthlyIncomeAccumulatedForYear), (float)MonthlyIncomeAccumulatedForYear);
        public float ObsAverageIncome  => _normController.Normalize(nameof(AverageIncome), (float)AverageIncome);
        public JobStatus JobStatus { get; set; }
        public decimal ThisMonthExpenses { get; set; }
        public decimal LastMonthExpenses { get; set; }
        public int Age { get; set; }
        private AgeStatus _ageStatus;
        public float JobReward;
        public float BaseBuyReward;
        public float LuxuryBuyReward;
        public float Happiness = 0;


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
            int adultMinAge = _policies.AgeBoundaries.adultMinAge;
            int workerMaxAge = _policies.AgeBoundaries.workerMaxAge;
            if (_ageStatus == AgeStatus.Dead)
                return AgeStatus.Dead;
            if (Age > 0 && Age < adultMinAge)
                return AgeStatus.UnderageChild;
            if (Age >= adultMinAge && Age < workerMaxAge)
                return AgeStatus.WorkerAge;
            return AgeStatus.RetiredAge;
        }

        public PersonObservations(int age, decimal desiredSalary, BankAccountModel bankAccount, PoliciesWrapper policies, JobMarketController jobMarket, NormalizationController normController)
        {
            _normController = normController;
            _policies = policies;
            _jobMarket = jobMarket;
            BankAccount = bankAccount;
            Age = age;
            DesiredSalary = desiredSalary;
            _normController.AddNew(nameof(LuxuryProducts), NormRange.One, LuxuryProducts);
            _normController.AddNew(nameof(OpenJobPositions), NormRange.One, OpenJobPositions);
            _normController.AddNew(nameof(UnsatisfiedBaseDemand), NormRange.One, UnsatisfiedBaseDemand);
            _normController.AddNew(nameof(Capital), NormRange.Two, (float)Capital);
            _normController.AddNew(nameof(DesiredSalary), NormRange.One, (float)DesiredSalary);
            _normController.AddNew(nameof(Salary), NormRange.One, (float)Salary);
            _normController.AddNew(nameof(MonthlyIncomeAccumulatedForYear), NormRange.One, (float)MonthlyIncomeAccumulatedForYear);
            _normController.AddNew(nameof(MonthlyExpensesAccumulatedForYear), NormRange.One, (float)MonthlyExpensesAccumulatedForYear);
            _normController.AddNew(nameof(AverageIncome), NormRange.One, (float)AverageIncome);
            _normController.AddNew(nameof(LoansTakenSum), NormRange.One, (float)LoansTakenSum);
        }

        public float GetSatisfactionRate()
        {
            return 0;
        }

    }
}