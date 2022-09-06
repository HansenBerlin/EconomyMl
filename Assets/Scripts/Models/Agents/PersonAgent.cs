using System;
using System.Collections.Generic;
using System.Linq;
using Controller.Agents;
using Controller.Data;
using Controller.RepositoryController;
using Enums;
using Factories;
using Models.Meta;
using Models.Observations;
using Models.Population;
using Templates;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
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
        public float Happiness => _obs.Happiness;
        public decimal Capital => _obs.Capital;
        public decimal MonthlyIncome => _obs.Salary;
        public bool StaysChildless { get; set; }
        [field:SerializeField]
        public DeathReason Death { get; set; } = DeathReason.HasNotDied;
        public List<PersonAgent> Children { get; } = new();
        public AgeStatus AgeStatus => _obs.AgeStatus;
        public JobModel Job { get; set; }

        private PersonJobAction _job;
        private PersonBuyAction _buyBuyActions;
        private PersonBuyAction _luxuryBuyPersonBuyActions;

        private enum JobSt { Unemployed, Retired, Employed, None, LastItem }

        private enum AgeSt { UnderageChild, WorkerAge, RetiredAge, Dead, LastItem }

        //[FormerlySerializedAs("Month")] public int month;
        [FormerlySerializedAs("CompletedEps")] public int completedEps;
        
        public bool maskBaseBuyActions = false;
        public bool maskLuxuryBuyActions = false;
        public bool maskUnEmployedJobActions = false;
        public bool maskAllJobActions = false;



        private bool _isInitDone;
        
        public void Init(string parentAId, string parentBId, PersonObservations observations, PersonController controller, NormalizationController normController)
        {
            AddParents(parentAId, parentBId);
            initage = observations.Age;
            _obs = observations;
            _controller = controller;
            _controller.Setup(this);
            _rewardController = new PersonRewardController(normController, _obs);
            var actions = _controller.InitActions();
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
            actionMask.SetActionEnabled(PersonAgentDiscreteAcions.JobBranch, PersonAgentDiscreteAcions.JobQuit, false);
        }
        
        private void MaskAllJobDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(PersonAgentDiscreteAcions.JobBranch, PersonAgentDiscreteAcions.JobQuit, false);
            actionMask.SetActionEnabled(PersonAgentDiscreteAcions.JobBranch, PersonAgentDiscreteAcions.JobNew, false);
        }

        private void MaskLuxuryBuyDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(PersonAgentDiscreteAcions.LuxBuyBranch, PersonAgentDiscreteAcions.LuxBuxMax, false);
            actionMask.SetActionEnabled(PersonAgentDiscreteAcions.LuxBuyBranch, PersonAgentDiscreteAcions.LuxBuyLimitHigh, false);
            actionMask.SetActionEnabled(PersonAgentDiscreteAcions.LuxBuyBranch, PersonAgentDiscreteAcions.LuxBuyLimitLow, false);
        }
        
        private void MaskBaseBuyDecisions(IDiscreteActionMask actionMask)
        {
            actionMask.SetActionEnabled(PersonAgentDiscreteAcions.BaseBuyBranch, PersonAgentDiscreteAcions.BaseBuxMax, false);
            actionMask.SetActionEnabled(PersonAgentDiscreteAcions.BaseBuyBranch, PersonAgentDiscreteAcions.BaseBuyLimitHigh, false);
            actionMask.SetActionEnabled(PersonAgentDiscreteAcions.BaseBuyBranch, PersonAgentDiscreteAcions.BaseBuyLimitLow, false);
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
            decimal baseM = _obs.LastMonthExpenses > 500 ? _obs.LastMonthExpenses * desiredSalaryDecision 
                : _obs.AverageIncome > 500 ? _obs.AverageIncome : 500;
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
                    case PersonAgentDiscreteAcions.BaseBuxMax:
                        _buyBuyActions.BuyDemandedProduct(Children.Count);
                        break;
                    case PersonAgentDiscreteAcions.BaseBuyLimitLow:
                        _buyBuyActions.BuyDemandedProduct(Children.Count, _obs.Salary);
                        break;
                    case PersonAgentDiscreteAcions.BaseBuyLimitHigh:
                        _buyBuyActions.BuyDemandedProduct(Children.Count, _obs.Capital);
                        break;
                    case PersonAgentDiscreteAcions.BaseBuyNothing:
                        AddReward(-0.01F);
                        break;
                }
                switch (luxBuyDecision)
                {
                    case PersonAgentDiscreteAcions.LuxBuxMax:
                        _luxuryBuyPersonBuyActions.BuyDemandedProduct(Children.Count);
                        break;
                    case PersonAgentDiscreteAcions.LuxBuyLimitLow:
                        _luxuryBuyPersonBuyActions.BuyDemandedProduct(Children.Count, _obs.Salary);
                        break;
                    case PersonAgentDiscreteAcions.LuxBuyLimitHigh:
                        _luxuryBuyPersonBuyActions.BuyDemandedProduct(Children.Count, _obs.Capital);
                        break;
                    case PersonAgentDiscreteAcions.LuxBuyNothing:
                        AddReward(-0.01F);
                        break;
                }
            }

            if (_obs.AgeStatus == AgeStatus.WorkerAge)
            {
                var salaryBefore = _obs.Salary;
                switch (jobDecision)
                {
                    case PersonAgentDiscreteAcions.JobQuit:
                        _job.QuitJobAndStayUnemployed();
                        break;
                    case PersonAgentDiscreteAcions.JobNew:
                        var oldSalary = _obs.Salary;
                        _obs.DesiredSalary = desiredSalary;
                        _job.SearchForNewJob(desiredSalary);
                        if (_obs.Salary > oldSalary * 1.1M)
                        {
                            AddReward(0.5f);
                        }
                        break;
                    case PersonAgentDiscreteAcions.JobNoChange:
                        _job.DoNothing();
                        if (_obs.JobStatus == JobStatus.Unemployed && _obs.AgeStatus == AgeStatus.WorkerAge)
                        {
                            AddReward(-0.1f);
                        }
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

        public void YearlyAgentUpdate(decimal avgIncome, TempPopulationUpdateModel tempPop, PopulationFactory factory, PopulationPropabilityController probController)
        {
            Academy.Instance.StatsRecorder.Add("POP/DEMANDMISSING", _obs.UnsatisfiedBaseDemand);
            Academy.Instance.StatsRecorder.Add("POP/LUX", _obs.LuxuryProducts);
            Academy.Instance.StatsRecorder.Add("POP/OPENJOBS", _obs.OpenJobPositions);
            Academy.Instance.StatsRecorder.Add("POP/INCOME", (float)_obs.MonthlyIncomeAccumulatedForYear);
            Academy.Instance.StatsRecorder.Add("POP/EXPENSES", (float)_obs.MonthlyExpensesAccumulatedForYear);
            
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
    }
}