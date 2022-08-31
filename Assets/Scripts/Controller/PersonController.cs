using EconomyBase.Controller.Actions;
using EconomyBase.Controller.Rewards;
using EconomyBase.Enums;
using EconomyBase.Factories;
using EconomyBase.Models;
using EconomyBase.Models.Observations;
using EconomyBase.Models.Population;
using EconomyBase.Settings;

namespace EconomyBase.Controller
{



    public class PersonController
    {
        private readonly PoliciesWrapper _policies;
        private IPersonBase _person;
        private readonly ActionsFactory _factory;
        private PersonObservations _observations;
        private PersonRewardController _rewardController;


        public PersonController(PoliciesWrapper policies, ActionsFactory factory)
        {
            _policies = policies;
            _factory = factory;
        }

        public void Setup(IPersonBase person, PersonObservations observations, PersonRewardController rewardController)
        {
            _observations = observations;
            _person = person;
            _rewardController = rewardController;
            _observations.JobStatus = GetInitialJobStatus();
            _observations.MonthlyIncome = InitialIncome();
        }



        public List<IPersonAction> InitActions()
        {
            return new List<IPersonAction>
            {
                _factory.Create(PersonActionType.JobDecision, this, _observations, _rewardController),
                _factory.Create(PersonActionType.BaseProductBuy, this, _observations, _rewardController),
                _factory.Create(PersonActionType.LuxuryProductBuy, this, _observations, _rewardController)
            };
        }




        private decimal InitialIncome()
        {
            return _observations.JobStatus switch
            {
                JobStatus.Retired => _observations.DesiredSalary,
                JobStatus.Unemployed => ReducedIncome(_observations.DesiredSalary),
                _ => 0
            };
        }

        private decimal ReducedIncome(decimal calculationBase)
        {
            var policy = _policies.WorkerPolicies;
            decimal reducedIncome = calculationBase * policy.UnemployedSupportRate;
            decimal income = reducedIncome > policy.UnemployedSupportMax ? policy.UnemployedSupportMax : reducedIncome;
            income = income < policy.UnemployedSupportMin ? policy.UnemployedSupportMin : income;
            return income;
        }

        private JobStatus GetInitialJobStatus()
        {
            var age = _observations.Age;
            var agesPolicy = _policies.AgeBoundaries;
            (int schoolAge, int minYearsInSchool, _) = _policies.EducationBoundaries;
            if (age < schoolAge)
                return JobStatus.None;
            if (age >= schoolAge && age < schoolAge + minYearsInSchool)
                return JobStatus.None;
            if (age >= schoolAge + minYearsInSchool && age <= agesPolicy.WorkerMaxAge)
                return JobStatus.Unemployed;
            return JobStatus.Retired;
        }

        private DeathReason UpdateDeathData(PopulationPropabilityController controller)
        {
            int randomDeath = StatisticalDistributionController.CreateRandom(0, 10000);
            if (randomDeath == 0)
            {
                //person.AgeStatus = AgeStatus.Dead;
                //tempRemoved.Add(this);
                return DeathReason.Random;
                //DistributeWealthToChildren();
            }

            bool hasDiedOfAge = controller.IsDead(_observations.Age);

            if (!hasDiedOfAge)
            {
                return DeathReason.HasNotDied;
            }
            else
            {
                //AgeStatus = AgeStatus.Dead;
                return DeathReason.Age;
                //DistributeWealthToChildren();
                //tempRemoved.Add(this);
            }


        }

        private void DistributeWealthToChildren()
        {
            decimal capital = _observations.Capital;
            if (capital <= 0) return;
            foreach (var child in _person.Children)
            {
                child.UpdateCapital(capital / _person.Children.Count);
            }
        }



        public void UpdateIncome(decimal amount)
        {
            _observations.MonthlyIncome += amount;
        }

        public void UpdateExpenses(decimal amount)
        {
            _observations.MonthlyExpenses += amount;
        }

        public void RemoveJobWhenFired()
        {
            _observations.DesiredSalary = _observations.MonthlyIncome * 0.99M;
            _observations.MonthlyIncome = ReducedIncome(_observations.MonthlyIncome);
            _person.Job.QuitJob(_person);
            _observations.JobStatus = JobStatus.Unemployed;
        }

        public void UpdateNewJob(JobModel newJob)
        {
            if (_observations.JobStatus == JobStatus.Employed)
            {
                _person.Job.QuitJob(_person);
            }

            newJob.TakeJob(_person, _observations.DesiredSalary);
            _observations.JobStatus = JobStatus.Employed;
            _observations.MonthlyIncome = newJob.Salary;
            _observations.DesiredSalary = newJob.Salary;
            _person.Job = newJob;
        }

        public void QuitJob()
        {
            if (_observations.JobStatus == JobStatus.Employed)
            {
                _person.Job.QuitJob(_person);
                _observations.MonthlyIncome = ReducedIncome(_observations.MonthlyIncome);
                _observations.DesiredSalary = _observations.MonthlyIncome;
                _observations.JobStatus = JobStatus.Unemployed;
            }
        }

        public void EndEpisode()
        {

        }

        public void Update(decimal avgIncome, TempPopulationUpdateModel tempPop, PopulationFactory factory,
            PopulationPropabilityController probController)
        {
            _observations.Age++;
            if (_observations.Age == 18)
            {
                TurnAdult(avgIncome);
            }

            if (_observations.Age is >= 18 and <= 55)
            {
                Reproduce(tempPop, factory);
            }

            if (_observations.Age == 68)
            {
                Retire(tempPop.Retired);
            }

            if (_observations.Capital < -100000000)
            {
                // Die
            }

            var deathState = UpdateDeathData(probController);
            if (deathState != DeathReason.HasNotDied)
            {
                _person.Death = deathState;
                tempPop.Died.Add(_person);
                DistributeWealthToChildren();
            }
        }

        public void AddChild(IPersonBase child)
        {
            if (_observations.Age - 17 < child.Age)
                throw new Exception();
            _person.Children.Add(child);
        }



        public decimal Pay()
        {
            _observations.Capital += _observations.MonthlyIncome;
            return _observations.MonthlyIncome;
        }


        private void Retire(ICollection<IPersonBase> retiredTemp)
        {
            if (_observations.JobStatus == JobStatus.Employed)
            {
                //Job.RemoveWorkerFromCompany();
                _person.Job.QuitJob(_person);
            }

            _observations.JobStatus = JobStatus.Retired;
            _observations.MonthlyIncome *= 0.67M;
            _observations.AgeStatus = AgeStatus.RetiredAge;
            retiredTemp.Add(_person);
        }

        public void ResetExpenses()
        {
            _observations.MonthlyExpenses = 0;
        }



        private void TurnAdult(decimal avgIncome)
        {
            _observations.DesiredSalary = avgIncome * 0.7M;
            _observations.JobStatus = JobStatus.Unemployed;
            _observations.MonthlyIncome = _policies.WorkerPolicies.UnemployedSupportMin;
        }





        private void Reproduce(TempPopulationUpdateModel tempPop, PopulationFactory factory)
        {
            int parentAge = _observations.Age;
            var children = _person.Children;
            if (GetChild(parentAge, children.Count, _person.StaysChildless) == false) return;

            int randomAgeDifference = StatisticalDistributionController.CreateRandom(0, 6) + 10;
            int minAge = parentAge - randomAgeDifference > 18 ? parentAge - randomAgeDifference : 18;
            var secondParent = factory.FindPersonWithinValueRange(minAge, parentAge + randomAgeDifference,
                tempPop.Current, _person.Id);
            if (secondParent == null) throw new Exception();
            var child = factory.CreateChild(0, _person.Id, secondParent.Id);
            if (child.IsDummy == false)
            {
                secondParent.AddChild(child);
                children.Add(child);
                tempPop.Born.Add(child);
            }
        }

        private bool GetChild(int age, int childCount, bool staysChildless)
        {
            // prob insg ca. 1%
            // alter ca 1/3 der range, also 3%
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


            //var test = rn.Next(0, 101);
            decimal prop = StatisticalDistributionController.ReproductionRate();
            bool propMet = prop * propability + 1 > childCount;
            //bool propMet = prop > ChildrenCount;
            int modifier = StatisticalDistributionController.CreateRandom(0, 17);
            if (propMet && modifier == 1 && staysChildless == false)
            {
                return true;
            }

            return false;
        }
    }
}