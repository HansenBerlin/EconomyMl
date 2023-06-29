using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Agents;
using MathNet.Numerics.LinearAlgebra.Single.Solvers;
using NewScripts.Http;
using NewScripts.Http;
using TMPro;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace NewScripts
{
    public interface ICompany
    {
        public int Id { get; }
        public decimal Liquidity { get; set; }
        public decimal OfferedWageRate { get; }
        public decimal ProductPrice { get; }
        public int ProductStock { get; }
        public int LifetimeMonths { get; }
        void FullfillBid(ProductType product, int count, decimal price);
        void AddContract(JobContract contract);
        void RemoveContract(JobContract contract, bool isQuitByEmployer);
        void RequestMonthlyDecision();
        public void StartNextPeriod(decimal price, int workerChange, decimal wage);
        void Produce();
        void EndMonth();
        PlayerDecisionEvent DecisionRequestEventProp { get; }

    }
}
