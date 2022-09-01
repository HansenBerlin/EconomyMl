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
using Unity.MLAgents.Policies;
using Unity.VisualScripting;
using UnityEngine;

namespace Models.Agents
{




    public class PersonAgent : Agent, IPersonBase
    {
        private PersonController _controller;
        private PersonRewardController _rewardController;


        public bool IsDummy { get; protected set; }
        public string Id { get; } = Guid.NewGuid().ToString();
        private string _parentAId;
        private string _parentBId;
        public JobStatus JobStatus => _observations.JobStatus;
        public int Age { get; } = 20;
        public decimal Capital => _observations.Capital;
        public decimal MonthlyIncome => _observations.MonthlyIncome;
        public int UnderageChildrenCount => Children.Count(c => c.AgeStatus == AgeStatus.UnderageChild);
        public bool StaysChildless { get; set; }
        public DeathReason Death { get; set; } = DeathReason.HasNotDied;
        public List<IPersonBase> Children { get; } = new();
        public AgeStatus AgeStatus => _observations.AgeStatus;
        public JobModel Job { get; set; }

        private PersonActionsJobPhase _jobActions;
        private PersonActionsBuyBaseProductPhase _baseBuyActions;
        private PersonActionsBuyLuxuryProductPhase _luxuryBuyActions;
        private PersonObservations _observations;

        public int Month;
        
        public bool maskBaseBuyActions = false;
        public bool maskLuxuryBuyActions = false;
        public bool maskJobActions = false;

        const int baseBuxMax = 0;
        const int baseBuyLimitLow = 1;
        const int baseBuyLimitHigh = 2;
        const int baseBuyNothing = 3;
        
        const int luxBuxMax = 5;
        const int luxBuyLimitLow = 6;
        const int luxBuyLimitHigh = 7;
        const int luxBuyNothing = 8;
        
        const int jobQuitOnly = 9;
        const int jobTryChangeLowDesiredSalary = 10;
        const int jobTryChangeHighDesiredSalary = 11;
        const int jobTryGetNewLowDesiredSalary = 12;
        const int jobTryGetNewAverageDesiredSalary = 13;
        const int jobTryGetNewHighDesiredSalary = 14;
        const int jobNoChange = 15;

        const int doNothing = 16;
        
        public void Init(string parentAId, string parentBId, PersonObservations observations,
            PersonController controller, PersonRewardController rewardController)
        {
            var parameters = gameObject.GetComponent<BehaviorParameters>();
            parameters.BehaviorName = "Economy";
            _observations = observations;
            _controller = controller;
            _rewardController = rewardController;
            _controller.Setup(this, _observations, _rewardController);
            var actions = _controller.InitActions();
            _jobActions = actions[0] as PersonActionsJobPhase;
            _baseBuyActions = actions[1] as PersonActionsBuyBaseProductPhase;
            _luxuryBuyActions = actions[2] as PersonActionsBuyLuxuryProductPhase;
            AddParents(parentAId, parentBId);
            StaysChildless = StatisticalDistributionController.CreateRandom(0, 10) == 1;
        }

        public PersonAgent()
        {
        }
        
        public override void OnEpisodeBegin()
        {
            
        }
        
        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(_observations.Age);
            sensor.AddObservation(_observations.LuxuryProducts);
            sensor.AddObservation(_observations.OpenJobPositions);
            sensor.AddObservation(_observations.UnsatisfiedBaseDemand);
            sensor.AddObservation((float)_observations.Capital);
            sensor.AddObservation((float)_observations.DesiredSalary);
            sensor.AddObservation((float)_observations.MonthlyIncome);
            sensor.AddObservation((float)_observations.MonthlyExpenses);
            sensor.AddObservation((float)_observations.SatisfactionRate);
            
        }
        
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (_observations.JobStatus is JobStatus.None or JobStatus.Retired || maskJobActions)
            {
                actionMask.SetActionEnabled(0, jobTryChangeHighDesiredSalary, false);
                actionMask.SetActionEnabled(0, jobTryChangeLowDesiredSalary, false);
                actionMask.SetActionEnabled(0, jobTryGetNewHighDesiredSalary, false);
                actionMask.SetActionEnabled(0, jobTryGetNewLowDesiredSalary, false);
                actionMask.SetActionEnabled(0, jobQuitOnly, false);
                actionMask.SetActionEnabled(0, jobNoChange, false);
            }
            else if (_observations.JobStatus != JobStatus.Unemployed)
            {
                actionMask.SetActionEnabled(0, jobTryChangeHighDesiredSalary, false);
                actionMask.SetActionEnabled(0, jobTryChangeLowDesiredSalary, false);
                actionMask.SetActionEnabled(0, jobQuitOnly, false);
            }
            else if (_observations.JobStatus != JobStatus.Employed)
            {
                actionMask.SetActionEnabled(0, jobTryGetNewHighDesiredSalary, false);
                actionMask.SetActionEnabled(0, jobTryGetNewLowDesiredSalary, false);
            }
            if (_observations.AgeStatus is not AgeStatus.RetiredAge or AgeStatus.WorkerAge || maskBaseBuyActions)
            {
                actionMask.SetActionEnabled(0, baseBuxMax, false);
                actionMask.SetActionEnabled(0, baseBuyLimitHigh, false);
                actionMask.SetActionEnabled(0, baseBuyLimitLow, false);
                actionMask.SetActionEnabled(0, baseBuyNothing, false);
            }
            if (_observations.AgeStatus is not AgeStatus.RetiredAge or AgeStatus.WorkerAge || maskLuxuryBuyActions)
            {
                actionMask.SetActionEnabled(0, luxBuxMax, false);
                actionMask.SetActionEnabled(0, luxBuyLimitHigh, false);
                actionMask.SetActionEnabled(0, luxBuyLimitLow, false);
                actionMask.SetActionEnabled(0, luxBuyNothing, false);
            }
        }
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var decision= actionBuffers.DiscreteActions[0];
            Debug.Log("Action: " + decision);

            switch (decision)
            {
                case baseBuxMax:
                    _baseBuyActions.BuyExactAmountOfDemandedBaseResources();
                    break;
                case baseBuyLimitLow:
                    _baseBuyActions.BuyDemandedBaseResourcesWithIncomeSpendingLimit();
                    break;
                case baseBuyLimitHigh:
                    _baseBuyActions.BuyDemandedBaseResourcesWithCapitalSpendingLimit();
                    break;
                case baseBuyNothing:
                    AddReward(-0.01F);
                    break;
                case luxBuxMax:
                    _luxuryBuyActions.BuyExactAmountOfDemandedLuxuryProduct();
                    break;
                case luxBuyLimitLow:
                    _luxuryBuyActions.BuyDemandedLuxuryProductWithIncomeSpendingLimit();
                    break;
                case luxBuyLimitHigh:
                    _luxuryBuyActions.BuyDemandedBaseProductWithCapitalSpendingLimit();
                    break;
                case luxBuyNothing:
                    AddReward(-0.01F);
                    break;
                case jobNoChange:
                {
                    if (_observations.JobStatus == JobStatus.Unemployed)
                    {
                        AddReward(-0.1F);
                    }

                    break;
                }
                case jobQuitOnly:
                    _jobActions.QuitJobAndStayUnemployed();
                    break;
                case jobTryChangeHighDesiredSalary:
                    _jobActions.SearchForNewJobFromEmployedWithHighIncreasedDemandedSalary();
                    break;
                case jobTryChangeLowDesiredSalary:
                    _jobActions.SearchForNewJobFromEmployedWithSlightlyIncreasedDemandedSalary();
                    break;
                case jobTryGetNewHighDesiredSalary:
                case jobTryGetNewAverageDesiredSalary:
                    _jobActions.SearchForNewJobFromUnemployedWithAboveAverageDemandedSalary();
                    break;
                case jobTryGetNewLowDesiredSalary:
                    _jobActions.SearchForNewJobFromUnemployedWithMinimumDemandedSalary();
                    break;
                case doNothing:
                {
                    if ((maskJobActions && maskBaseBuyActions && maskLuxuryBuyActions) == false)
                    {
                        AddReward(-0.1F);
                    }

                    break;
                }
            }
            UpdateReward(decision);
            UpdateMasking(decision);
        }

        private void UpdateMasking(int decision)
        {
            switch (decision)
            {
                case <= baseBuyNothing:
                    maskBaseBuyActions = true;
                    break;
                case <= luxBuyNothing:
                    maskLuxuryBuyActions = true;
                    break;
                case <= jobNoChange:
                    maskJobActions = true;
                    break;
            }
        }
        
        private void UpdateReward(int decision)
        {
            switch (decision)
            {
                case <= baseBuyNothing:
                    AddReward(_observations.BaseBuyReward);
                    break;
                case <= luxBuyNothing:
                    AddReward(_observations.LuxuryBuyReward);
                    break;
                case <= jobNoChange:
                    AddReward(_observations.JobReward);
                    break;
            }
        }

        public void ResetMasking(int month)
        {
            Month = month;
            AddReward(_controller.GetCombinedReward());
            _observations.MonthlyExpenses = 0;
            if (maskJobActions && maskBaseBuyActions && maskLuxuryBuyActions)
            {
                maskJobActions = false;
                maskBaseBuyActions = false;
                maskLuxuryBuyActions = false;
            }
        }

        public void YearlyAgentUpdate(decimal avgIncome, TempPopulationUpdateModel tempPop, PopulationFactory factory,
            PopulationPropabilityController probController)
        {
            _controller.Update(avgIncome, tempPop, factory, probController);
        }

        public void UpdateCapital(decimal amount)
        {
            _observations.Capital += amount;
        }

        public void AddChild(IPersonBase child)
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