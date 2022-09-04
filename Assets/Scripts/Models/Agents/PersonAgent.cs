using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Controller.Actions;
using Controller.Rewards;
using Enums;
using Factories;
using Models.Meta;
using Models.Observations;
using Models.Population;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using UnityEngine.Serialization;

namespace Models.Agents
{




    public class PersonAgent : Agent
    {
        private PersonController _controller;
        private PersonRewardController _rewardController;
        private PersonObservations _observations;
        private PersonRespawnController _respawner;


        
        [field:SerializeField]
        public bool IsDummy { get; protected set; }
        [field:SerializeField]
        public string Id { get; } = Guid.NewGuid().ToString();
        private string _parentAId;
        private string _parentBId;
        public JobStatus JobStatus => _observations.JobStatus;

        [field: SerializeField] 
        public int initage;
        public int Age => _observations.Age;
        public decimal Capital => _observations.Capital;
        public decimal MonthlyIncome => _observations.Salary;
        public int UnderageChildrenCount => Children.Count(c => c.AgeStatus == AgeStatus.UnderageChild);
        public bool StaysChildless { get; set; }
        [field:SerializeField]
        public DeathReason Death { get; set; } = DeathReason.HasNotDied;
        public List<PersonAgent> Children { get; } = new();
        public AgeStatus AgeStatus => _observations.AgeStatus;
        public JobModel Job { get; set; }

        private PersonActionsJobPhaseFree _jobActions;
        private PersonActionsBuyBaseProductPhase _baseBuyActions;
        private PersonActionsBuyLuxuryProductPhase _luxuryBuyActions;

        public int Month;
        public int CompletedEps;
        
        public bool maskBaseBuyActions = false;
        public bool maskLuxuryBuyActions = false;
        public bool maskUnEmployedJobActions = false;
        public bool maskAllJobActions = false;

        
        const int desiredSalaryBranch = 0;
        const int jobBranch = 1;
        const int baseBuyBranch = 2;
        const int luxBuyBranch = 3;
        
        const int jobQuit = 0;
        const int jobNew = 1;
        const int jobNoChange = 2;
        
        const int baseBuxMax = 0;
        const int baseBuyLimitLow = 1;
        const int baseBuyLimitHigh = 2;
        const int baseBuyNothing = 3;
        
        const int luxBuxMax = 0;
        const int luxBuyLimitLow = 1;
        const int luxBuyLimitHigh = 2;
        const int luxBuyNothing = 3;


        private bool isInitDone;
        
        public void Init(string parentAId, string parentBId, PersonObservations observations, PersonController controller)
        {
            AddParents(parentAId, parentBId);
            initage = observations.Age;
            _observations = observations;
            _controller = controller;
            _controller.Setup(this);
            _rewardController = new PersonRewardController();
            var actions = _controller.InitActions();
            _jobActions = actions[0] as PersonActionsJobPhaseFree;
            _baseBuyActions = actions[1] as PersonActionsBuyBaseProductPhase;
            _luxuryBuyActions = actions[2] as PersonActionsBuyLuxuryProductPhase;
            StaysChildless = StatisticalDistributionController.CreateRandom(0, 10) == 1;
            _respawner = new PersonRespawnController(observations);
            //isInitDone = true;
            SetupMasking();
        }

        public void InitMonth()
        {
            CompletedEps = CompletedEpisodes;
            initage = Age;
        }

        public void Kill()
        {
            //Destroy(this);
        }

        private void SetupMasking()
        {
            if (_observations.AgeStatus == AgeStatus.UnderageChild)
            {
                maskBaseBuyActions = true;
                maskLuxuryBuyActions = true;
                maskUnEmployedJobActions = true;
                maskAllJobActions = true;
            }
            if (_observations.AgeStatus == AgeStatus.WorkerAge && _observations.JobStatus == JobStatus.Employed)
            {
                maskBaseBuyActions = false;
                maskLuxuryBuyActions = false;
                maskUnEmployedJobActions = false;
                maskAllJobActions = false;
            }
            if (_observations.AgeStatus == AgeStatus.WorkerAge && _observations.JobStatus == JobStatus.Unemployed)
            {
                maskBaseBuyActions = false;
                maskLuxuryBuyActions = false;
                maskUnEmployedJobActions = true;
                maskAllJobActions = false;
            }
            if (_observations.AgeStatus == AgeStatus.RetiredAge)
            {
                maskBaseBuyActions = false;
                maskLuxuryBuyActions = false;
                maskUnEmployedJobActions = true;
                maskAllJobActions = true;
            }
        }
        
        
        public override void CollectObservations(VectorSensor sensor)
        {
            //if (Death != DeathReason.HasNotDied) return;

            //sensor.AddObservation(_observations.Age);
            sensor.AddObservation(_observations.LuxuryProducts);
            sensor.AddObservation(_observations.OpenJobPositions);
            sensor.AddObservation(_observations.UnsatisfiedBaseDemand);
            sensor.AddObservation((float)_observations.Capital);
            sensor.AddObservation((float)_observations.DesiredSalary);
            sensor.AddObservation((float)_observations.Salary);
            sensor.AddObservation((float)_observations.MonthlyExpensesAccumulatedForYear);
            sensor.AddObservation((float)_observations.MonthlyIncomeAccumulatedForYear);
            //sensor.AddObservation((float)_observations.SatisfactionRate);
            sensor.AddObservation((float)_observations.AverageIncome);
            sensor.AddObservation((int)_observations.JobStatus);
            sensor.AddObservation((int)_observations.AgeStatus);
        }

        private void MaskUnEmployedJobDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(jobBranch, jobQuit, false);
        }
        
        private void MaskAllJobDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(jobBranch, jobQuit, false);
            actionMask.SetActionEnabled(jobBranch, jobNew, false);
        }

        private void MaskLuxuryBuyDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(luxBuyBranch, luxBuxMax, false);
            actionMask.SetActionEnabled(luxBuyBranch, luxBuyLimitHigh, false);
            actionMask.SetActionEnabled(luxBuyBranch, luxBuyLimitLow, false);
        }
        
        private void MaskBaseBuyDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(baseBuyBranch, baseBuxMax, false);
            actionMask.SetActionEnabled(baseBuyBranch, baseBuyLimitHigh, false);
            actionMask.SetActionEnabled(baseBuyBranch, baseBuyLimitLow, false);
        }
        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (maskUnEmployedJobActions)
            {
                MaskUnEmployedJobDecisions(actionMask);
            }
            if (maskAllJobActions)
            {
                MaskAllJobDecisions(actionMask);
            }
            if (maskBaseBuyActions)
            {
                MaskBaseBuyDecisions(actionMask);
            }
            if (maskLuxuryBuyActions)
            {
                MaskLuxuryBuyDecisions(actionMask);
            }
        }
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (Death != DeathReason.HasNotDied) return;
            var desiredSalaryDecision = actionBuffers.DiscreteActions[0] + 1;
            decimal baseM = _observations.LastMonthExpenses > 500 ? _observations.LastMonthExpenses * desiredSalaryDecision : _observations.AverageIncome > 500 ? _observations.AverageIncome : 500;
            decimal desiredSalary = baseM * desiredSalaryDecision;
            var jobDecision= actionBuffers.DiscreteActions[1];
            var baseBuyDecision= actionBuffers.DiscreteActions[2];
            var luxBuyDecision= actionBuffers.DiscreteActions[3];

            if (_observations.AgeStatus != AgeStatus.UnderageChild)
            {
                switch (baseBuyDecision)
                {
                    case baseBuxMax:
                        _baseBuyActions.BuyExactAmountOfDemandedBaseResources(_observations, Children.Count, _rewardController);
                        break;
                    case baseBuyLimitLow:
                        _baseBuyActions.BuyDemandedBaseResourcesWithIncomeSpendingLimit(_observations, Children.Count, _rewardController);
                        break;
                    case baseBuyLimitHigh:
                        _baseBuyActions.BuyDemandedBaseResourcesWithCapitalSpendingLimit(_observations, Children.Count, _rewardController);
                        break;
                    case baseBuyNothing:
                        AddReward(-0.01F);
                        break;
                }
                if (float.IsNaN(_observations.BaseBuyReward) || float.IsInfinity(_observations.BaseBuyReward))
                {
                    throw new Exception($"{Month} invalid value on base reward");
                }
                
                switch (luxBuyDecision)
                {
                    case luxBuxMax:
                        _luxuryBuyActions.BuyExactAmountOfDemandedLuxuryProduct(_observations, Children.Count, _rewardController);
                        break;
                    case luxBuyLimitLow:
                        _luxuryBuyActions.BuyDemandedLuxuryProductWithIncomeSpendingLimit(_observations, Children.Count, _rewardController);
                        break;
                    case luxBuyLimitHigh:
                        _luxuryBuyActions.BuyDemandedLuxuryProductWithCapitalSpendingLimit(_observations, Children.Count, _rewardController);
                        break;
                    case luxBuyNothing:
                        AddReward(-0.01F);
                        break;
                }
                if (float.IsNaN(_observations.LuxuryBuyReward) || float.IsInfinity(_observations.LuxuryBuyReward))
                {
                    throw new Exception($"{Month} invalid value on lux reward");
                }
            }
            else
            {
                AddReward(0.01F);
            }


            if (_observations.AgeStatus == AgeStatus.WorkerAge)
            {
                switch (jobDecision)
                {
                    case jobQuit:
                        _jobActions.QuitJobAndStayUnemployed(_observations, _rewardController, _controller);
                        
                        break;
                    case jobNew:
                        var oldSalary = _observations.Salary;
                        _observations.DesiredSalary = desiredSalary;
                        _jobActions.SearchForNewJob(_observations, _rewardController, _controller, desiredSalary);
                        if (_observations.Salary > oldSalary * 1.1M)
                        {
                            AddReward(0.5f);
                        }
                        break;
                    case jobNoChange:
                        _jobActions.DoNothing(_observations, _rewardController);
                        if (_observations.JobStatus == JobStatus.Unemployed && _observations.AgeStatus == AgeStatus.WorkerAge)
                        {
                            AddReward(-0.1f);
                        }
                        break;
                }
                if (float.IsNaN(_observations.JobReward) || float.IsInfinity(_observations.JobReward))
                {
                    throw new Exception($"{Month} invalid value on job reward");
                }
            }
            else
            {
                AddReward(0.01F);
            }

            
        }

        public void RequestMonthlyDecisions(int month, decimal averageIncome)
        {
            Month = month;
            if (Death != DeathReason.HasNotDied) return;

            _observations.AverageIncome = averageIncome;
            _observations.LastMonthExpenses = _observations.ThisMonthExpenses;
            _observations.ThisMonthExpenses = 0;
            SetupMasking();
            RequestDecision();
            Academy.Instance.EnvironmentStep();
        }

        public void YearlyAgentUpdate(decimal avgIncome, TempPopulationUpdateModel tempPop, PopulationFactory factory, PopulationPropabilityController probController)
        {
            if (Death != DeathReason.HasNotDied)
            {
                AddReward(2);
                return;
            }

            //Debug.Log($"Yearly reward in month {Month} " + reward);
            if ((_observations.MonthlyExpensesAccumulatedForYear >
                _observations.MonthlyIncomeAccumulatedForYear + _observations.Capital + 100000)
                || _observations.UnsatisfiedBaseDemand > _baseBuyActions.GetDemand(_observations, Children.Count) * 6)
            {
                _controller.QuitJob();
                _respawner.Reset(_observations);
                SetReward(-1);
                EndEpisode();
            }
            else
            {
                float reward = _rewardController.CombinedReward(_observations);
                AddReward(reward);
            }
            _controller.UpdateAgent(avgIncome, tempPop, factory, probController);
            _observations.UnsatisfiedBaseDemand = 0;
            _observations.JobReward = 0;
            _observations.BaseBuyReward = 0;
            _observations.LuxuryBuyReward = 0;
            _observations.MonthlyExpensesAccumulatedForYear = 0;
            _observations.MonthlyIncomeAccumulatedForYear = 0;
            _observations.LuxuryProducts = 0;

        }

        public void UpdateCapital(decimal amount)
        {
            _observations.Capital += amount;
        }

        public void AddChild(PersonAgent child)
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
        
        public void SetupWorkState(JobMarketController jobMarket)
        {
            _controller.SetupWorkState(jobMarket);
        }
    }
}