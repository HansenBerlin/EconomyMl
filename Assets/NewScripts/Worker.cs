using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

namespace NewScripts
{
    public class Worker
    {
        public double Money { get; set; } = 100;
        public double DailySpending { get; set; } = 0;
        public double ReservationWage { get; set; } = 0;
        public int Health { get; set; } = 1000;
        public int DemandFulfilled { get; set; } = 0;

        public int MonthlyDemand { get; set; } = 10;
        //public int CompanyId { get; set; }
        private readonly System.Random _rand = new();
        
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
            if (randomNewSupplier.ProductPrice * 1.2 < randomKnownSupplier.ProductPrice)
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

        public void Pay(double wage)
        {
            Money += wage;
        }
        
        

        public void Buy()
        {
            //var randomIndices = Utilitis.GenerateRandomArray(0, _typeAConnections.Count);
            

            double amountSpent = 0;
            int countBought = 0;

            foreach (var company in _typeAConnections.Where(x => x.IsBlocked == false))
            {
                if (DailySpending - amountSpent < company.ProductPrice)
                {
                    continue;
                }

                int buyAmount = (int)Math.Floor((DailySpending - amountSpent) / company.ProductPrice);
                buyAmount = MonthlyDemand - countBought < buyAmount ? MonthlyDemand - countBought : buyAmount;
                if (Money - buyAmount * company.ProductPrice < 0)
                {
                    continue;
                    Debug.LogWarning("Buy Below zero");
                }
                
                Receipt receipt = company.BuyFromCompany(buyAmount);
                amountSpent += receipt.AmountPaid;
                countBought += receipt.CountBought;
                
            }
            DemandFulfilled += countBought;
            Money -= amountSpent;
            //if (averagePrice / _typeAConnections.Count > DailySpending)
            //{
            //    var newReservationWage = averagePrice / _typeAConnections.Count * 21;
            //    ReservationWage = newReservationWage > ReservationWage ? newReservationWage : ReservationWage;
            //}
        }

        public void SetDailySpending()
        {
            var paymentsForSixMonths = ReservationWage * 6;
            if (Money > paymentsForSixMonths)
            {
                if (HasJob)
                {
                    _employedAtCompany.InvestInCompany(Money - paymentsForSixMonths);
                }
                else
                {
                    _typeAConnections[_rand.Next(0, _typeAConnections.Count)]
                        .InvestInCompany(Money - paymentsForSixMonths);
                }

                Money = paymentsForSixMonths;
            }
            DailySpending = Money * 0.98 / 20;
            if (DemandFulfilled >= MonthlyDemand)
            {
                MonthlyDemand += new System.Random().Next(1, 3) * 20;
            }
            else
            {
                MonthlyDemand -= new System.Random().Next(1, 3) * 20;
            }

            MonthlyDemand = MonthlyDemand < 100 ? 100 : MonthlyDemand > 300 ? 300 : MonthlyDemand;
        }

        public void SetReservationWage()
        {
            ServiceLocator.Instance.stepsWorker++;
            Academy.Instance.StatsRecorder.Add("Worker/Money", (float)Money);
            //Academy.Instance.StatsRecorder.Add("Worker/Bought", DemandFulfilled);

            if (HasJob)
            {
                if (ReservationWage > _employedAtCompany.RealwageRate)
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
                ReservationWage *= 0.95;
            }

            ReservationWage = ReservationWage < 5 ? 5 : ReservationWage;
            //DemandFulfilled = 0;
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
                if (_employedAtCompany.RealwageRate < 5)
                {
                    _employedAtCompany.QuitJob(this);
                    _employedAtCompany = null;
                    _jobChangeIntensity = JobChangeLevel.Unmployed;
                    HasJob = false;
                    searches = (int) _jobChangeIntensity;
                    SearchForNewJob(searches);
                    return;
                }
                var potentialCompanys = ServiceLocator.Instance.Companys
                    .Where(x => x != _employedAtCompany && x.IsBlocked == false)
                    .ToArray();
                for (int i = 0; i < potentialCompanys.Length; i++)
                {
                    if (searches == 0)
                    {
                        break;
                    }

                    searches--;

                    var potentialCompany = potentialCompanys[i];
                    if (potentialCompany.OpenPositions > 0 && potentialCompany.RealwageRate > _employedAtCompany.RealwageRate * 1.1)
                    {
                        _employedAtCompany.QuitJob(this);
                        potentialCompany.SignJobOffer(this);
                        _employedAtCompany = potentialCompany;
                        _jobChangeIntensity = JobChangeLevel.Satisfied;
                        searches = 0;
                    }
                }
            }
            else
            {
                SearchForNewJob(searches);
            }
        }

        private void SearchForNewJob(int searches)
        {
            var potentialCompanys = ServiceLocator.Instance.Companys
                .Where(x => x.IsBlocked == false)
                .ToArray();
            for (int i = 0; i < potentialCompanys.Length; i++)
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
                    _jobChangeIntensity = JobChangeLevel.Satisfied;
                    HasJob = true;
                    searches = 0;
                }
            }
        }
    }
}