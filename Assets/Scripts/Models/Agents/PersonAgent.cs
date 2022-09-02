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
using Settings;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.VisualScripting;
using UnityEngine;

namespace Models.Agents
{




    public class PersonAgent : Agent
    {
        private PersonController _controller;
        private PersonRewardController _rewardController;
        private PersonObservations _observations;


        
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
        public decimal MonthlyIncome => _observations.MonthlyIncome;
        public int UnderageChildrenCount => Children.Count(c => c.AgeStatus == AgeStatus.UnderageChild);
        public bool StaysChildless { get; set; }
        [field:SerializeField]
        public DeathReason Death { get; set; } = DeathReason.HasNotDied;
        public List<PersonAgent> Children { get; } = new();
        public AgeStatus AgeStatus => _observations.AgeStatus;
        public JobModel Job { get; set; }

        private PersonActionsJobPhase _jobActions;
        private PersonActionsBuyBaseProductPhase _baseBuyActions;
        private PersonActionsBuyLuxuryProductPhase _luxuryBuyActions;

        public int Month;
        public int CompletedEps;
        
        public bool maskBaseBuyActions = false;
        public bool maskLuxuryBuyActions = false;
        public bool maskEmployedJobActions = false;
        public bool maskUnemployedJobActions = false;

        const int jobQuitOnly = 0;
        const int jobTryChangeLowDesiredSalary = 1;
        const int jobTryChangeHighDesiredSalary = 2;
        const int jobTryGetNewLowDesiredSalary = 3;
        const int jobTryGetNewAverageDesiredSalary = 4;
        const int jobTryGetNewHighDesiredSalary = 5;
        const int jobNoChange = 6;
        
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
            initage = observations.Age;
            AddParents(parentAId, parentBId);
            _observations = observations;
            _controller = controller;
            _controller.Setup(this);
            _rewardController = new PersonRewardController();
            var actions = _controller.InitActions();
            _jobActions = actions[0] as PersonActionsJobPhase;
            _baseBuyActions = actions[1] as PersonActionsBuyBaseProductPhase;
            _luxuryBuyActions = actions[2] as PersonActionsBuyLuxuryProductPhase;
            StaysChildless = StatisticalDistributionController.CreateRandom(0, 10) == 1;
            isInitDone = true;
            SetupMasking();
        }

        public override void OnEpisodeBegin()
        {
            if (isInitDone == false) return;
            if (Death != DeathReason.HasNotDied) return;
            CompletedEps = CompletedEpisodes;
            initage = Age;
            SetupMasking();
        }

        public void Kill()
        {
            //Destroy(this);
        }

        private void SetupMasking()
        {
            _observations.JobReward = 0;
            _observations.BaseBuyReward = 0;
            _observations.LuxuryBuyReward = 0;
            _observations.CapitalReward = 0;
            _observations.MonthlyExpenses = 0;
            _observations.UnsatisfiedBaseDemand = 0;
            
            if (_observations.AgeStatus == AgeStatus.UnderageChild)
            {
                maskBaseBuyActions = true;
                maskLuxuryBuyActions = true;
                maskEmployedJobActions = true;
                maskUnemployedJobActions = true;
            }
            if (_observations.AgeStatus == AgeStatus.WorkerAge && _observations.JobStatus == JobStatus.Employed)
            {
                maskBaseBuyActions = false;
                maskLuxuryBuyActions = false;
                maskEmployedJobActions = false;
                maskUnemployedJobActions = true;
            }
            if (_observations.AgeStatus == AgeStatus.WorkerAge && _observations.JobStatus == JobStatus.Unemployed)
            {
                maskBaseBuyActions = false;
                maskLuxuryBuyActions = false;
                maskEmployedJobActions = true;
                maskUnemployedJobActions = false;
            }
            if (_observations.AgeStatus == AgeStatus.RetiredAge)
            {
                maskBaseBuyActions = false;
                maskLuxuryBuyActions = false;
                maskEmployedJobActions = true;
                maskUnemployedJobActions = true;
            }
        }
        
        
        public override void CollectObservations(VectorSensor sensor)
        {
            if (Death != DeathReason.HasNotDied) return;

            sensor.AddObservation(_observations.Age);
            sensor.AddObservation(_observations.LuxuryProducts);
            sensor.AddObservation(_observations.OpenJobPositions);
            sensor.AddObservation(_observations.UnsatisfiedBaseDemand);
            sensor.AddObservation((float)_observations.Capital);
            sensor.AddObservation((float)_observations.DesiredSalary);
            sensor.AddObservation((float)_observations.MonthlyIncome);
            sensor.AddObservation((float)_observations.MonthlyExpenses);
            sensor.AddObservation((float)_observations.SatisfactionRate);
            sensor.AddObservation((float)_observations.AverageIncome);
        }

        private void MaskUnemployedJobDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(0, jobQuitOnly, false);
            actionMask.SetActionEnabled(0, jobTryGetNewLowDesiredSalary, false);
            actionMask.SetActionEnabled(0, jobTryGetNewAverageDesiredSalary, false);
            actionMask.SetActionEnabled(0, jobTryGetNewHighDesiredSalary, false);
        }
        
        private void MaskEmployedJobDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(0, jobQuitOnly, false);
            actionMask.SetActionEnabled(0, jobTryChangeLowDesiredSalary, false);
            actionMask.SetActionEnabled(0, jobTryChangeHighDesiredSalary, false);
        }

        private void MaskLuxuryBuyDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(0, luxBuxMax, false);
            actionMask.SetActionEnabled(0, luxBuyLimitHigh, false);
            actionMask.SetActionEnabled(0, luxBuyLimitLow, false);
        }
        
        private void MaskBaseBuyDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(0, baseBuxMax, false);
            actionMask.SetActionEnabled(0, baseBuyLimitHigh, false);
            actionMask.SetActionEnabled(0, baseBuyLimitLow, false);
        }
        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (maskEmployedJobActions)
            {
                MaskEmployedJobDecisions(actionMask);
            }
            if (maskUnemployedJobActions)
            {
                MaskUnemployedJobDecisions(actionMask);
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

            var jobDecision= actionBuffers.DiscreteActions[0];
            var baseBuyDecision= actionBuffers.DiscreteActions[1];
            var luxBuyDecision= actionBuffers.DiscreteActions[2];
            
            switch (jobDecision)
            {
                case jobQuitOnly:
                    _jobActions.QuitJobAndStayUnemployed(_observations, _rewardController, _controller);
                    break;
                case jobTryChangeLowDesiredSalary:
                    _jobActions.SearchForNewJobFromEmployedWithSlightlyIncreasedDemandedSalary(_observations, _rewardController, _controller);
                    break;
                case jobTryChangeHighDesiredSalary:
                    _jobActions.SearchForNewJobFromEmployedWithHighIncreasedDemandedSalary(_observations, _rewardController, _controller);
                    break;
                case jobTryGetNewLowDesiredSalary:
                    _jobActions.SearchForNewJobFromUnemployedWithMinimumDemandedSalary(_observations, _rewardController, _controller);
                    break;
                case jobTryGetNewAverageDesiredSalary:
                    _jobActions.SearchForNewJobFromUnemployedWithAverageDemandedSalary(_observations, _rewardController, _controller);
                    break;
                case jobTryGetNewHighDesiredSalary:
                    _jobActions.SearchForNewJobFromUnemployedWithAboveAverageDemandedSalary(_observations, _rewardController, _controller);
                    break;
                case jobNoChange:
                    _jobActions.DoNothing(_observations, _rewardController);
                    break;
            }
            
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
            
            switch (luxBuyDecision)
            {
                case luxBuxMax:
                    _luxuryBuyActions.BuyExactAmountOfDemandedLuxuryProduct(_observations, Children.Count, _rewardController);
                    break;
                case luxBuyLimitLow:
                    _luxuryBuyActions.BuyDemandedLuxuryProductWithIncomeSpendingLimit(_observations, Children.Count, _rewardController);
                    break;
                case luxBuyLimitHigh:
                    _luxuryBuyActions.BuyDemandedBaseProductWithCapitalSpendingLimit(_observations, Children.Count, _rewardController);
                    break;
                case luxBuyNothing:
                    AddReward(-0.01F);
                    break;
            }

            AddReward(_observations.BaseBuyReward);
            Debug.Log($"Reward for base action {baseBuyDecision} " + _observations.BaseBuyReward);
            
            AddReward(_observations.LuxuryBuyReward);
            Debug.Log($"Reward for lux action {luxBuyDecision} " + _observations.LuxuryBuyReward);
            
            AddReward(_observations.JobReward);
            Debug.Log($"Reward for job action {jobDecision} " + _observations.JobReward);
        }

        public void RequestMonthlyDecisions(int month, decimal averageIncome)
        {
            if (Death != DeathReason.HasNotDied) return;

            _observations.AverageIncome = averageIncome;
            Month = month;
            RequestDecision();
            EndEpisode();
        }
        

        public void YearlyAgentUpdate(decimal avgIncome, TempPopulationUpdateModel tempPop, PopulationFactory factory, PopulationPropabilityController probController)
        {
            if (Death != DeathReason.HasNotDied) return;

            float reward = _rewardController.CombinedReward(_observations);
            Debug.Log($"Yearly reward in month {Month} " + reward);
            AddReward(reward);
            _controller.UpdateAgent(avgIncome, tempPop, factory, probController);
            EndEpisode();
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
    }
}