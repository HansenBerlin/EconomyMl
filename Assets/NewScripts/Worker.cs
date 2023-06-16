using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

namespace NewScripts
{
    public class Worker
    {
        public decimal Money { get; set; } = 21;
        public decimal DailySpending { get; set; } = 0;
        public decimal ReservationWage { get; set; } = 0;
        public int Health { get; set; } = 1000;
        public int DemandFulfilled { get; set; } = 0;

        public int Demand { get; set; } = int.MaxValue;
        //public int CompanyId { get; set; }
        private readonly System.Random _rand = new(42);
        
        private Company _employedAtCompany = null;
        private readonly List<Company> _typeAConnections = new();
        public bool HasJob { get; private set; }
        private JobChangeLevel _jobChangeIntensity = JobChangeLevel.Unmployed;

        public void SearchNewSupplier()
        {
            var randomKnownSupplier = _typeAConnections[_rand.Next(0, _typeAConnections.Count - 1)];
            var unknownSuppliers = _typeAConnections
                .Except(ServiceLocator.Instance.Companys)
                .Concat(ServiceLocator.Instance.Companys
                    .Except(_typeAConnections)
                    .Except(new List<Company>{_employedAtCompany})
                .ToList()).ToList();
            var randomNewSupplier = unknownSuppliers[_rand.Next(0, unknownSuppliers.Count - 1)];
            if (randomNewSupplier.ProductPrice * 1.2M < randomKnownSupplier.ProductPrice)
            {
                _typeAConnections.Remove(randomKnownSupplier);
                _typeAConnections.Add(randomNewSupplier);
            }
        }

        public void InitialSuppliersSetup(List<Company> companies)
        {
            _typeAConnections.AddRange(companies);
        }

        public void InitialJobSetup(Company startAtCompany)
        {
            _employedAtCompany = startAtCompany;
            HasJob = true;
            ReservationWage = _employedAtCompany.WageRate;
            _jobChangeIntensity = JobChangeLevel.Satisfied;
        }

        public void Pay(decimal wage)
        {
            Money += wage;
        }

        public void Buy()
        {
            var randomIndices = Utilitis.GenerateRandomArray(0, _typeAConnections.Count);
            List<Company> suppliers = new();
            
            if (HasJob)
            {
                suppliers.Add(_employedAtCompany);
            }
            
            for (int i = 0; i < randomIndices.Length; i++)
            {
                suppliers.Add(_typeAConnections[i]);
            }
            
            suppliers.AddRange(ServiceLocator.Instance.Companys.Where(x => x.IsOnEmergencySaleRounds > 0));
            suppliers = suppliers.OrderBy(x => x.ProductPrice).ToList();

            decimal amountSpent = 0;
            decimal averagePrice = 0;

            foreach (var company in suppliers)
            {
                averagePrice += company.ProductPrice;
                if (company.ProductPrice == 0)
                {
                    Debug.LogError("");
                    continue;
                }
                
                int buyAmount = (int)Math.Floor((DailySpending - amountSpent) / company.ProductPrice);
                if (buyAmount == 0)
                {
                    //ReservationWage *= 1.01M;
                    continue;
                }

                Receipt receipt = company.BuyFromCompany(buyAmount);
                amountSpent += receipt.AmountPaid;
                DemandFulfilled += receipt.CountBought;
            }

            Money -= amountSpent;
            if (averagePrice / suppliers.Count > DailySpending)
            {
                ReservationWage = averagePrice / suppliers.Count;
            }
        }

        public void SetDailySpending()
        {
            DailySpending = Money / 21;
        }

        public void SetReservationWage()
        {
            Academy.Instance.StatsRecorder.Add("Worker/Money", (float)Money);
            Academy.Instance.StatsRecorder.Add("Worker/Bought", DemandFulfilled);

            if (HasJob)
            {
                if (ReservationWage > _employedAtCompany.WageRate)
                {
                    _jobChangeIntensity = JobChangeLevel.Underpaid;
                }
                else
                {
                    _jobChangeIntensity = JobChangeLevel.Satisfied;
                    ReservationWage = _employedAtCompany.WageRate;
                }
            }
            else
            {
                _jobChangeIntensity = JobChangeLevel.Unmployed;
                ReservationWage *= 0.95M;
            }
            DemandFulfilled = 0;
        }

        public void Fire()
        {
            HasJob = false;
            _employedAtCompany = null;
        }

        public void SearchJob()
        {
            int searches = (int) _jobChangeIntensity;
            
            if (HasJob)
            {
                var potentialCompanys = ServiceLocator.Instance.Companys.Where(x => x != _employedAtCompany).ToList();
                var randomIndices = Utilitis.GenerateRandomArray(0, potentialCompanys.Count);
                for (int i = 0; i < randomIndices.Length; i++)
                {
                    if (searches == 0)
                    {
                        break;
                    }

                    searches--;

                    var potentialCompany = potentialCompanys[i];
                    if (potentialCompany.OpenPositions > 0 && potentialCompany.WageRate > _employedAtCompany.WageRate)
                    {
                        _employedAtCompany.QuitJob(this);
                        potentialCompany.SignJobOffer(this);
                        _employedAtCompany = potentialCompany;
                        searches = 0;
                    }
                }
            }
            else
            {
                var randomIndices = Utilitis.GenerateRandomArray(0, ServiceLocator.Instance.Companys.Count);
                for (int i = 0; i < randomIndices.Length; i++)
                {
                    if (searches == 0)
                    {
                        break;
                    }

                    searches--;

                    var potentialCompany = ServiceLocator.Instance.Companys[i];
                    if (potentialCompany.OpenPositions > 0 && potentialCompany.WageRate >= ReservationWage)
                    {
                        potentialCompany.SignJobOffer(this);
                        _employedAtCompany = potentialCompany;
                        HasJob = true;
                        searches = 0;
                    }
                }
            }
        }
    }
}