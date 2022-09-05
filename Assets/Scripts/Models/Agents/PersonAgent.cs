using System;
using System.Collections.Generic;
using System.Linq;
using Controller;
using Controller.Actions;
using Controller.Rewards;
using Enums;
using Factories;
using Models.Market;
using Models.Meta;
using Models.Observations;
using Models.Population;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.Serialization;

namespace Models.Agents
{




    public class PersonAgent : Agent
    {
        private PersonController _controller;
        private PersonRewardController _rewardController;
        private PersonObservations _obs;
        private PersonRespawnController _respawner;


        
        [field:SerializeField]
        public bool IsDummy { get; protected set; }
        [field:SerializeField]
        public string Id { get; } = Guid.NewGuid().ToString();
        private string _parentAId;
        private string _parentBId;
        public JobStatus JobStatus => _obs.JobStatus;

        [field: SerializeField] 
        public int initage;
        public int Age => _obs.Age;
        public int Happiness => _obs.Age;
        public decimal Capital => _obs.Capital;
        public decimal MonthlyIncome => _obs.Salary;
        public int UnderageChildrenCount => Children.Count(c => c.AgeStatus == AgeStatus.UnderageChild);
        public bool StaysChildless { get; set; }
        [field:SerializeField]
        public DeathReason Death { get; set; } = DeathReason.HasNotDied;
        public List<PersonAgent> Children { get; } = new();
        public AgeStatus AgeStatus => _obs.AgeStatus;
        public JobModel Job { get; set; }

        private PersonActionsJobPhaseFree _jobActions;
        private PersonActionsBuyBaseProductPhase _baseBuyActions;
        private PersonActionsBuyLuxuryProductPhase _luxuryBuyActions;

        private enum JobSt { Unemployed, Retired, Employed, None, LastItem }

        private enum AgeSt { UnderageChild, WorkerAge, RetiredAge, Dead, LastItem }

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
            _obs = observations;
            _controller = controller;
            _controller.Setup(this);
            _rewardController = new PersonRewardController();
            var actions = _controller.InitActions();
            _jobActions = actions[0] as PersonActionsJobPhaseFree;
            _baseBuyActions = actions[1] as PersonActionsBuyBaseProductPhase;
            _luxuryBuyActions = actions[2] as PersonActionsBuyLuxuryProductPhase;
            StaysChildless = StatisticalDistributionController.CreateRandom(0, 10) == 1;
            _respawner = new PersonRespawnController(observations);
            _baseBuyActions.Init(_obs, _rewardController, _controller);
            _luxuryBuyActions.Init(_obs, _rewardController, _controller);
            //isInitDone = true;
            SetupMasking();
        }

        public void InitMonth()
        {
            CompletedEps = CompletedEpisodes;
            initage = Age;
        }

        private void SetupMasking()
        {
            if (_obs.AgeStatus == AgeStatus.UnderageChild)
            {
                maskBaseBuyActions = true;
                maskLuxuryBuyActions = true;
                maskUnEmployedJobActions = true;
                maskAllJobActions = true;
            }
            if (_obs.AgeStatus == AgeStatus.WorkerAge && _obs.JobStatus == JobStatus.Employed)
            {
                maskBaseBuyActions = false;
                maskLuxuryBuyActions = false;
                maskUnEmployedJobActions = false;
                maskAllJobActions = false;
            }
            if (_obs.AgeStatus == AgeStatus.WorkerAge && _obs.JobStatus == JobStatus.Unemployed)
            {
                maskBaseBuyActions = false;
                maskLuxuryBuyActions = false;
                maskUnEmployedJobActions = true;
                maskAllJobActions = false;
            }
            if (_obs.AgeStatus == AgeStatus.RetiredAge)
            {
                maskBaseBuyActions = false;
                maskLuxuryBuyActions = false;
                maskUnEmployedJobActions = true;
                maskAllJobActions = true;
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            for (int ci = 0; ci < (int)JobSt.LastItem; ci++)
            {
                sensor.AddObservation((int)_obs.JobStatus == ci ? 1.0f : 0.0f);
            }
            for (int ci = 0; ci < (int)AgeSt.LastItem; ci++)
            {
                sensor.AddObservation((int)_obs.AgeStatus == ci ? 1.0f : 0.0f);
            }
            sensor.AddObservation(_obs.ObsLuxuryProducts);
            sensor.AddObservation(_obs.ObsOpenJobPositions);
            sensor.AddObservation(_obs.ObsUnsatisfiedBaseDemand);
            sensor.AddObservation(_obs.ObsCapital);
            sensor.AddObservation(_obs.ObsDesiredSalary);
            sensor.AddObservation(_obs.ObsSalary);
            sensor.AddObservation(_obs.ObsMonthlyExpensesAccumulatedForYear);
            sensor.AddObservation(_obs.ObsMonthlyIncomeAccumulatedForYear);
            sensor.AddObservation(_obs.ObsAverageIncome);
            sensor.AddObservation(_obs.ObsLoansSum);
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
            decimal baseM = _obs.LastMonthExpenses > 500 ? _obs.LastMonthExpenses * desiredSalaryDecision : _obs.AverageIncome > 500 ? _obs.AverageIncome : 500;
            decimal desiredSalary = baseM * desiredSalaryDecision;
            var jobDecision= actionBuffers.DiscreteActions[1];
            var baseBuyDecision= actionBuffers.DiscreteActions[2];
            var luxBuyDecision= actionBuffers.DiscreteActions[3];
            var takeLoanDecision = actionBuffers.DiscreteActions[4] * 1000;

            if (_obs.AgeStatus != AgeStatus.UnderageChild)
            {
                if (takeLoanDecision != 0)
                {
                    if (_controller.GetLoan(takeLoanDecision) == false)
                    {
                        AddReward(-0.05f);
                    }
                    else
                    {
                        AddReward(0.01f);
                    }
                }
                switch (baseBuyDecision)
                {
                    case baseBuxMax:
                        _baseBuyActions.BuyExactAmountOfDemandedBaseResources(Children.Count);
                        break;
                    case baseBuyLimitLow:
                        _baseBuyActions.BuyDemandedBaseResourcesWithIncomeSpendingLimit(Children.Count);
                        break;
                    case baseBuyLimitHigh:
                        _baseBuyActions.BuyDemandedBaseResourcesWithCapitalSpendingLimit(Children.Count);
                        break;
                    case baseBuyNothing:
                        AddReward(-0.01F);
                        break;
                }
                if (float.IsNaN(_obs.BaseBuyReward) || float.IsInfinity(_obs.BaseBuyReward))
                {
                    throw new Exception($"{Month} invalid value on base reward");
                }
                
                switch (luxBuyDecision)
                {
                    case luxBuxMax:
                        _luxuryBuyActions.BuyExactAmountOfDemandedLuxuryProduct(Children.Count);
                        break;
                    case luxBuyLimitLow:
                        _luxuryBuyActions.BuyDemandedLuxuryProductWithIncomeSpendingLimit(Children.Count);
                        break;
                    case luxBuyLimitHigh:
                        _luxuryBuyActions.BuyDemandedLuxuryProductWithCapitalSpendingLimit(Children.Count);
                        break;
                    case luxBuyNothing:
                        AddReward(-0.01F);
                        break;
                }
                if (float.IsNaN(_obs.LuxuryBuyReward) || float.IsInfinity(_obs.LuxuryBuyReward))
                {
                    throw new Exception($"{Month} invalid value on lux reward");
                }
            }
            else
            {
                AddReward(0.01F);
            }


            if (_obs.AgeStatus == AgeStatus.WorkerAge)
            {
                var salaryBefore = _obs.Salary;
                switch (jobDecision)
                {
                    case jobQuit:
                        _jobActions.QuitJobAndStayUnemployed(_obs, _rewardController, _controller);
                        
                        break;
                    case jobNew:
                        var oldSalary = _obs.Salary;
                        _obs.DesiredSalary = desiredSalary;
                        _jobActions.SearchForNewJob(_obs, _rewardController, _controller, desiredSalary);
                        if (_obs.Salary > oldSalary * 1.1M)
                        {
                            AddReward(0.5f);
                        }
                        break;
                    case jobNoChange:
                        _jobActions.DoNothing(_obs, _rewardController);
                        if (_obs.JobStatus == JobStatus.Unemployed && _obs.AgeStatus == AgeStatus.WorkerAge)
                        {
                            AddReward(-0.1f);
                        }
                        break;
                }

                var salaryAfter = _obs.Salary;
                if (salaryAfter > salaryBefore)
                {
                    AddReward(0.05f);
                }
                if (salaryAfter > _obs.AverageIncome)
                {
                    AddReward(0.05f);
                }

            }
        }

        public void RequestMonthlyDecisions(int month, decimal averageIncome)
        {
            Month = month;
            if (Death != DeathReason.HasNotDied) return;

            _obs.AverageIncome = averageIncome;
            _obs.LastMonthExpenses = _obs.ThisMonthExpenses;
            _obs.ThisMonthExpenses = 0;
            SetupMasking();
            RequestDecision();
            Academy.Instance.EnvironmentStep();
        }

        public void YearlyAgentUpdate(decimal avgIncome, TempPopulationUpdateModel tempPop, PopulationFactory factory, PopulationPropabilityController probController)
        {
            Academy.Instance.StatsRecorder.Add("POP/DEMANDMISSING", _obs.UnsatisfiedBaseDemand);
            Academy.Instance.StatsRecorder.Add("POP/LUX", _obs.LuxuryProducts);
            Academy.Instance.StatsRecorder.Add("POP/OPENJOBS", _obs.OpenJobPositions);
            Academy.Instance.StatsRecorder.Add("POP/INCOME", (float)_obs.MonthlyIncomeAccumulatedForYear);
            Academy.Instance.StatsRecorder.Add("POP/EXPENSES", (float)_obs.MonthlyExpensesAccumulatedForYear);
            
            if (_obs.MonthlyExpensesAccumulatedForYear >
                _obs.MonthlyIncomeAccumulatedForYear + _obs.Capital + 100000
                || _obs.UnsatisfiedBaseDemand > _baseBuyActions.GetDemand(Children.Count) * 6)
            {
                _controller.QuitJob();
                _controller.ResetBankAccount(_respawner.Capital);
                _respawner.Reset(_obs);
                SetReward(-1);
                _obs.Happiness = -1;
                EndEpisode();
            }
            else
            {
                _obs.Happiness = _rewardController.CombinedReward(_obs);
                AddReward(_obs.Happiness);
            }
            _controller.UpdateAgent(avgIncome, tempPop, factory, probController);
            _obs.UnsatisfiedBaseDemand = 0;
            _obs.JobReward = 0;
            _obs.BaseBuyReward = 0;
            _obs.LuxuryBuyReward = 0;
            _obs.MonthlyExpensesAccumulatedForYear = 0;
            _obs.MonthlyIncomeAccumulatedForYear = 0;
            _obs.LuxuryProducts = 0;

        }

        public void UpdateCapital(decimal amount)
        {
            _controller.AddCapital(amount);
            //_obs.Capital += amount;
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
            return _controller.ReceiveMoney();
        }
        
        public void SetupWorkState(JobMarketController jobMarket)
        {
            _controller.SetupWorkState(jobMarket);
        }
    }
}