using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Agents;
using MathNet.Numerics.LinearAlgebra.Single.Solvers;
using NewScripts.Enums;
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
        int Id { get; }
        string Name { get; }
        decimal Liquidity { get; set; }
        //decimal OfferedWageRate { get; }
        decimal AverageWageRate { get; }
        Decision LastDecision { get; }
        int ProductStockFood { get; }
        int LifetimeMonths { get; }
        void FullfillBid(ProductType product, int count, decimal price);
        void AddContract(JobContract contract);
        void RemoveContract(JobContract contract, WorkerFireReason reason);
        void RequestMonthlyDecision();

        void StartNextPeriod(Decision decision);
        void Produce();
        void EndMonth();
        int WorkerCount { get; }
        PlayerType PlayerType { get; }
        CompanyDecisionStatus DecisionStatus { get; }
        List<CompanyData> Ledger { get; }
    }
}
