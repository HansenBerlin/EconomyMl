using System.Collections.Generic;
using Agents;
using Controller.Data;
using Controller.RepositoryController;
using Enums;
using Factories;
using Interfaces;
using Models;
using Policies;

namespace Controller.Agents
{
    public class PersonController
    {
        private readonly ActionsFactory _factory;
        private readonly ICountryEconomy _market;
        private readonly PersonObservations _obs;
        private readonly PoliciesWrapper _policies;
        private BankAccountModel _bankAccount;
        private CreditRating _currentRating = CreditRating.A;
        private PersonAgent _person;

        public PersonController(PersonObservations obs, PoliciesWrapper policies, ActionsFactory factory,
            ICountryEconomy market)
        {
            _obs = obs;
            _policies = policies;
            _factory = factory;
            _market = market;
        }

        public void Setup(PersonAgent person)
        {
            _person = person;
            _obs.JobStatus = GetInitialJobStatus();
            _obs.Salary = InitialIncome();
            _bankAccount = _market.OpenBankAccount(_obs.Capital, true);
        }

        public List<IPersonAction> InitActions(PersonRewardController rewardController)
        {
            var actions =  new List<IPersonAction>
            {
                _factory.Create(PersonActionType.JobDecision),
                _factory.Create(PersonActionType.BaseProductBuy),
                _factory.Create(PersonActionType.LuxuryProductBuy)
            };
            foreach (var a in actions)
            {
                a.Init(_obs, rewardController, this);
            }

            return actions;
        }

        private decimal InitialIncome()
        {
            return _obs.JobStatus switch
            {
                JobStatus.Retired => _obs.DesiredSalary,
                JobStatus.Unemployed => ReducedIncome(_obs.DesiredSalary),
                _ => 0
            };
        }

        private decimal ReducedIncome(decimal calculationBase)
        {
            var policy = _policies.FederalUnemployedPaymentPolicies;
            decimal reducedIncome = calculationBase * (decimal) policy.unemployedSupportRate;
            decimal income = reducedIncome > (decimal) policy.unemployedSupportMax
                ? (decimal) policy.unemployedSupportMax
                : reducedIncome;
            income = income < (decimal) policy.unemployedSupportMin ? (decimal) policy.unemployedSupportMin : income;
            return income;
        }

        private JobStatus GetInitialJobStatus()
        {
            int age = _obs.Age;
            var agesPolicy = _policies.AgeBoundaries;
            int schoolAge = _policies.EducationBoundaries.ageToStartSchool;
            int minYearsInSchool = _policies.EducationBoundaries.minYearsInSchool;
            if (age < schoolAge)
                return JobStatus.None;
            if (age >= schoolAge && age < schoolAge + minYearsInSchool)
                return JobStatus.None;
            if (age >= schoolAge + minYearsInSchool && age <= agesPolicy.workerMaxAge)
                return JobStatus.Unemployed;
            return JobStatus.Retired;
        }

        private DeathReason UpdateDeathData(PopulationPropabilityController controller)
        {
            int randomDeath = StatisticalDistributionController.CreateRandom(0, 10000);
            if (randomDeath == 0) return DeathReason.Random;

            bool hasDiedOfAge = controller.IsDead(_obs.Age);

            if (!hasDiedOfAge) return DeathReason.HasNotDied;
            return DeathReason.Age;
        }

        private void DistributeWealthToChildren(decimal capital)
        {
            if (capital <= 0) return;
            foreach (var child in _person.Children) child.UpdateCapital(capital / _person.Children.Count);
        }

        public void ResetBankAccount(decimal newCapital)
        {
            _bankAccount.CloseAccount();
            _bankAccount = _market.OpenBankAccount(newCapital, true);
            _obs.BankAccount = _bankAccount;
        }

        public bool GetLoan(decimal amount)
        {
            _currentRating = RatingController.Calculate(_obs.Capital,
                (decimal) (_obs.ObsMonthlyExpensesAccumulatedForYear - _obs.ObsMonthlyIncomeAccumulatedForYear),
                _obs.LoansTakenSum, _obs.Salary, _currentRating);
            return _bankAccount.IsLoanAdded(amount, _currentRating);
        }


        public void SetupWorkState(JobMarketController jobMarket)
        {
            var job = jobMarket.FindAvailableJob(0);
            if (job.Status == JobPositionStatus.Taken)
            {
                job.TakeJob(_person, _obs.DesiredSalary);
                _obs.JobStatus = JobStatus.Employed;
                _obs.Salary = _obs.DesiredSalary;
                job.Salary = _obs.DesiredSalary;
                _person.Job = job;
            }
        }

        public void RemoveJobWhenFired()
        {
            _obs.Salary = ReducedIncome(_obs.Salary);
            _person.Job.QuitJob(_person);
            _obs.JobStatus = JobStatus.Unemployed;
        }

        public void UpdateNewJob(JobModel newJob)
        {
            if (_obs.JobStatus == JobStatus.Employed) _person.Job.QuitJob(_person);

            newJob.TakeJob(_person, _obs.DesiredSalary);
            _obs.JobStatus = JobStatus.Employed;
            _obs.Salary = newJob.Salary;
            _person.Job = newJob;
        }

        public void QuitJob()
        {
            if (_obs.JobStatus == JobStatus.Employed)
            {
                _person.Job.QuitJob(_person);
                _obs.Salary = ReducedIncome(_obs.Salary);
                _obs.DesiredSalary = _obs.Salary;
                _obs.JobStatus = JobStatus.Unemployed;
            }
        }

        public void AddCapital(decimal sum)
        {
            _bankAccount.Deposit(sum);
        }

        public void UpdateAgent(decimal avgIncome, TempPopulationUpdateModel tempPop, PopulationFactory factory,
            PopulationPropabilityController probController)
        {
            _obs.Age++;
            if (_obs.Age == 18) TurnAdult(avgIncome);

            if (_obs.Age is >= 18 and <= 55) Reproduce(tempPop, factory);

            if (_obs.AgeStatus == AgeStatus.RetiredAge)
                _obs.Salary = _obs.LastSalaryBeforeRetirement *
                              (decimal) _policies.FederalUnemployedPaymentPolicies.retirementSupportRate;

            if (_obs.Age == 68) Retire(tempPop.Retired);

            if (_obs.Capital < -100000)
            {
                _person.Death = DeathReason.Starved;
                tempPop.Died.Add(_person);
                _bankAccount.CloseAccount();
                _person.Kill(true);
            }

            var deathState = UpdateDeathData(probController);
            if (deathState != DeathReason.HasNotDied)
            {
                _person.Death = deathState;
                tempPop.Died.Add(_person);
                decimal leftMoney = _bankAccount.CloseAccount();
                if (leftMoney > 0) DistributeWealthToChildren(leftMoney);
                _person.Kill(false);
            }
        }

        public decimal ReceiveMoney()
        {
            _bankAccount.Deposit(_obs.Salary);
            _obs.MonthlyIncomeAccumulatedForYear += _obs.Salary;
            return _obs.Salary;
        }

        public void PayBill(decimal amount)
        {
            _bankAccount.Withdraw(amount);
        }

        private void Retire(ICollection<PersonAgent> retiredTemp)
        {
            _obs.JobStatus = JobStatus.Retired;
            _obs.LastSalaryBeforeRetirement = _obs.Salary;
            _obs.Salary *= (decimal) _policies.FederalUnemployedPaymentPolicies.retirementSupportRate;
            _obs.AgeStatus = AgeStatus.RetiredAge;

            if (_obs.JobStatus == JobStatus.Employed) _person.Job.QuitJob(_person);

            retiredTemp.Add(_person);
        }

        private void TurnAdult(decimal avgIncome)
        {
            _obs.DesiredSalary = avgIncome * 0.7M;
            _obs.JobStatus = JobStatus.Unemployed;
            _obs.Salary = (decimal) _policies.FederalUnemployedPaymentPolicies.unemployedSupportMin;
        }

        private void Reproduce(TempPopulationUpdateModel tempPop, PopulationFactory factory)
        {
            int parentAge = _obs.Age;
            var children = _person.Children;
            if (GetChild(parentAge, children.Count, _person.StaysChildless) == false) return;

            int randomAgeDifference = StatisticalDistributionController.CreateRandom(0, 6) + 10;
            int minAge = parentAge - randomAgeDifference > 18 ? parentAge - randomAgeDifference : 18;
            var secondParent = factory.FindPersonWithinValueRange(minAge, parentAge + randomAgeDifference,
                tempPop.Current, _person.Id);
            var secondParentId = "dead";
            if (secondParent != null) secondParentId = secondParent.Id;
            var child = factory.CreateChild(0, _person.Id, secondParentId);
            if (child.IsDummy == false)
            {
                secondParent?.AddChild(child);
                children.Add(child);
                tempPop.Born.Add(child);
            }
        }

        private bool GetChild(int age, int childCount, bool staysChildless)
        {
            decimal propability = age switch
            {
                >= 18 and < 25 => 0.8M,
                >= 25 and < 35 => 1.2M,
                >= 35 and < 45 => 0.8M,
                >= 45 and < 55 => 0.1M,
                _ => 0
            };

            switch (childCount)
            {
                case 0:
                    propability *= 1.1M;
                    break;
                case 1:
                    propability *= 1.5M;
                    break;
                case 2:
                    propability *= 0.2M;
                    break;
                case > 2:
                    propability *= 0.1M;
                    break;
            }

            decimal prop = StatisticalDistributionController.ReproductionRate();
            bool propMet = prop * propability + 1 > childCount;
            int modifier = StatisticalDistributionController.CreateRandom(0, 17);
            if (propMet && modifier == 1 && staysChildless == false) return true;

            return false;
        }
    }
}