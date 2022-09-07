using System;
using System.Collections.Generic;
using Controller.Agents;
using Controller.Data;
using Controller.RepositoryController;
using Enums;
using Factories;
using Models;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Agents
{
    public class PersonAgent : Agent
    {
        [field: SerializeField] public int initage;

        //[FormerlySerializedAs("Month")] public int month;
        [FormerlySerializedAs("CompletedEps")] public int completedEps;

        public bool maskBaseBuyActions;
        public bool maskLuxuryBuyActions;
        public bool maskUnEmployedJobActions;
        public bool maskAllJobActions;
        private PersonBuyAction _buyBuyActions;
        private PersonController _controller;


        private bool _isInitDone;

        private PersonJobAction _job;
        private PersonBuyAction _luxuryBuyPersonBuyActions;
        private PersonObservations _obs;
        private string _parentAId;
        private string _parentBId;
        private PersonRespawnController _respawner;
        private PersonRewardController _rewardController;

        [field: SerializeField] public bool IsDummy { get; protected set; }
        [field: SerializeField] public string Id { get; } = Guid.NewGuid().ToString();
        public JobStatus JobStatus => _obs.JobStatus;
        public int Age => _obs.Age;
        public float Happiness => _obs.Happiness;
        public decimal Capital => _obs.Capital;
        public decimal MonthlyIncome => _obs.Salary;
        public bool StaysChildless { get; set; }
        [field: SerializeField] public DeathReason Death { get; set; } = DeathReason.HasNotDied;
        public List<PersonAgent> Children { get; } = new();
        public AgeStatus AgeStatus => _obs.AgeStatus;
        public JobModel Job { get; set; }

        public void Init(string parentAId, string parentBId, PersonObservations observations,
            PersonController controller, NormalizationController normController)
        {
            AddParents(parentAId, parentBId);
            initage = observations.Age;
            _obs = observations;
            _controller = controller;
            _controller.Setup(this);
            _rewardController = new PersonRewardController(normController, _obs);
            var actions = _controller.InitActions(_rewardController);
            _job = actions[0] as PersonJobAction;
            _buyBuyActions = actions[1] as PersonBuyActionBaseProduct;
            _luxuryBuyPersonBuyActions = actions[2] as PersonBuyActionLuxuryProduct;
            StaysChildless = StatisticalDistributionController.CreateRandom(0, 10) == 1;
            _respawner = new PersonRespawnController(observations);
            _buyBuyActions.Init(_obs, _rewardController, _controller);
            _luxuryBuyPersonBuyActions.Init(_obs, _rewardController, _controller);
            //isInitDone = true;
            SetupMasking();
        }

        public void InitMonth()
        {
            completedEps = CompletedEpisodes;
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
            for (var ci = 0; ci < (int) JobSt.LastItem; ci++)
                sensor.AddObservation((int) _obs.JobStatus == ci ? 1.0f : 0.0f);
            for (var ci = 0; ci < (int) AgeSt.LastItem; ci++)
                sensor.AddObservation((int) _obs.AgeStatus == ci ? 1.0f : 0.0f);
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
            actionMask.SetActionEnabled(PersonAgentDiscreteActions.JobBranch, PersonAgentDiscreteActions.JobQuit,
                false);
        }

        private void MaskAllJobDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(PersonAgentDiscreteActions.JobBranch, PersonAgentDiscreteActions.JobQuit,
                false);
            actionMask.SetActionEnabled(PersonAgentDiscreteActions.JobBranch, PersonAgentDiscreteActions.JobNew, false);
        }

        private void MaskLuxuryBuyDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(PersonAgentDiscreteActions.LuxBuyBranch, PersonAgentDiscreteActions.LuxBuxMax,
                false);
            actionMask.SetActionEnabled(PersonAgentDiscreteActions.LuxBuyBranch,
                PersonAgentDiscreteActions.LuxBuyLimitHigh, false);
            actionMask.SetActionEnabled(PersonAgentDiscreteActions.LuxBuyBranch,
                PersonAgentDiscreteActions.LuxBuyLimitLow, false);
        }

        private void MaskBaseBuyDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(PersonAgentDiscreteActions.BaseBuyBranch, PersonAgentDiscreteActions.BaseBuxMax,
                false);
            actionMask.SetActionEnabled(PersonAgentDiscreteActions.BaseBuyBranch,
                PersonAgentDiscreteActions.BaseBuyLimitHigh, false);
            actionMask.SetActionEnabled(PersonAgentDiscreteActions.BaseBuyBranch,
                PersonAgentDiscreteActions.BaseBuyLimitLow, false);
        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            if (maskUnEmployedJobActions) MaskUnEmployedJobDecisions(actionMask);
            if (maskAllJobActions) MaskAllJobDecisions(actionMask);
            if (maskBaseBuyActions) MaskBaseBuyDecisions(actionMask);
            if (maskLuxuryBuyActions) MaskLuxuryBuyDecisions(actionMask);
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            if (Death != DeathReason.HasNotDied) return;
            int desiredSalaryDecision = actionBuffers.DiscreteActions[0] + 1;
            decimal baseM = _obs.LastMonthExpenses > 500 ? _obs.LastMonthExpenses * desiredSalaryDecision
                : _obs.AverageIncome > 500 ? _obs.AverageIncome : 500;
            decimal desiredSalary = baseM * desiredSalaryDecision;
            int jobDecision = actionBuffers.DiscreteActions[1];
            int baseBuyDecision = actionBuffers.DiscreteActions[2];
            int luxBuyDecision = actionBuffers.DiscreteActions[3];
            int takeLoanDecision = actionBuffers.DiscreteActions[4] * 1000;

            if (_obs.AgeStatus != AgeStatus.UnderageChild)
            {
                if (takeLoanDecision != 0)
                {
                    if (_controller.GetLoan(takeLoanDecision) == false)
                        AddReward(-0.05f);
                    else
                        AddReward(0.01f);
                }

                switch (baseBuyDecision)
                {
                    case PersonAgentDiscreteActions.BaseBuxMax:
                        _buyBuyActions.BuyDemandedProduct(Children.Count);
                        break;
                    case PersonAgentDiscreteActions.BaseBuyLimitLow:
                        _buyBuyActions.BuyDemandedProduct(Children.Count, _obs.Salary);
                        break;
                    case PersonAgentDiscreteActions.BaseBuyLimitHigh:
                        _buyBuyActions.BuyDemandedProduct(Children.Count, _obs.Capital);
                        break;
                    case PersonAgentDiscreteActions.BaseBuyNothing:
                        AddReward(-0.01F);
                        break;
                }

                switch (luxBuyDecision)
                {
                    case PersonAgentDiscreteActions.LuxBuxMax:
                        _luxuryBuyPersonBuyActions.BuyDemandedProduct(Children.Count);
                        break;
                    case PersonAgentDiscreteActions.LuxBuyLimitLow:
                        _luxuryBuyPersonBuyActions.BuyDemandedProduct(Children.Count, _obs.Salary);
                        break;
                    case PersonAgentDiscreteActions.LuxBuyLimitHigh:
                        _luxuryBuyPersonBuyActions.BuyDemandedProduct(Children.Count, _obs.Capital);
                        break;
                    case PersonAgentDiscreteActions.LuxBuyNothing:
                        AddReward(-0.01F);
                        break;
                }
            }

            if (_obs.AgeStatus == AgeStatus.WorkerAge)
            {
                decimal salaryBefore = _obs.Salary;
                switch (jobDecision)
                {
                    case PersonAgentDiscreteActions.JobQuit:
                        _job.QuitJobAndStayUnemployed();
                        break;
                    case PersonAgentDiscreteActions.JobNew:
                        decimal oldSalary = _obs.Salary;
                        _obs.DesiredSalary = desiredSalary;
                        _job.SearchForNewJob(desiredSalary);
                        if (_obs.Salary > oldSalary * 1.1M) AddReward(0.5f);
                        break;
                    case PersonAgentDiscreteActions.JobNoChange:
                        _job.DoNothing();
                        if (_obs.JobStatus == JobStatus.Unemployed && _obs.AgeStatus == AgeStatus.WorkerAge)
                            AddReward(-0.1f);
                        break;
                }
            }
        }

        public void RequestMonthlyDecisions(decimal averageIncome)
        {
            if (Death != DeathReason.HasNotDied) return;
            _obs.AverageIncome = averageIncome;
            _obs.LastMonthExpenses = _obs.ThisMonthExpenses;
            _obs.ThisMonthExpenses = 0;
            SetupMasking();
            RequestDecision();
            Academy.Instance.EnvironmentStep();
        }

        public void YearlyAgentUpdate(decimal avgIncome, TempPopulationUpdateModel tempPop, PopulationFactory factory,
            PopulationPropabilityController probController)
        {
            Academy.Instance.StatsRecorder.Add("POP/DEMANDMISSING", _obs.UnsatisfiedBaseDemand);
            Academy.Instance.StatsRecorder.Add("POP/LUX", _obs.LuxuryProducts);
            Academy.Instance.StatsRecorder.Add("POP/OPENJOBS", _obs.OpenJobPositions);
            Academy.Instance.StatsRecorder.Add("POP/INCOME", (float) _obs.MonthlyIncomeAccumulatedForYear);
            Academy.Instance.StatsRecorder.Add("POP/EXPENSES", (float) _obs.MonthlyExpensesAccumulatedForYear);

            if (_obs.MonthlyExpensesAccumulatedForYear >
                _obs.MonthlyIncomeAccumulatedForYear + _obs.Capital + 100000
                || _obs.UnsatisfiedBaseDemand > _buyBuyActions.GetDemand(Children.Count) * 6)
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
                _obs.Happiness = _rewardController.CombinedReward();
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

        public void Kill(bool isNegReward)
        {
            if (isNegReward)
            {
                SetReward(-1);
                EndEpisode();
            }
            Destroy(gameObject);
        }

        private enum JobSt
        {
            Unemployed,
            Retired,
            Employed,
            None,
            LastItem
        }

        private enum AgeSt
        {
            UnderageChild,
            WorkerAge,
            RetiredAge,
            Dead,
            LastItem
        }
    }
}