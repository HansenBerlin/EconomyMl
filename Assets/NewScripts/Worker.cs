using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NewScripts
{
    public class Worker
    {
        public float Money { get; set; } = 100;
        public float DailySpending { get; set; } = 0;
        public float ReservationWage { get; set; } = 0;
        public int Health { get; set; } = 1000;

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
            var randomNewSupplier = ServiceLocator.Instance.Companys[_rand.Next(0, ServiceLocator.Instance.Companys.Count - 1)];
            if (randomNewSupplier.ProductPrice < randomKnownSupplier.ProductPrice * 1.2 && _typeAConnections.Contains(randomNewSupplier) == false)
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

        public void Pay(float wage)
        {
            Money += wage;
        }

        public void Buy()
        {
            int countFullfilled = 0;
            float amountSpent = 0;

            var randomIndices = Utilitis.GenerateRandomArray(0, _typeAConnections.Count);

            for (int i = 0; i < randomIndices.Length; i++)
            {
                var company = _typeAConnections[i];
                if (countFullfilled == Demand || amountSpent + company.ProductPrice > Money)
                {
                    break;
                }
                int maxBySpending = (int)Math.Floor((Money - amountSpent) / company.ProductPrice);
                int maxBySupply = Demand > company.ProductStock ? company.ProductStock : Demand;
                int buyAmount = maxBySpending >= maxBySupply ? maxBySupply : maxBySpending;
                Receipt receipt = company.BuyFromCompany(buyAmount);
                amountSpent += receipt.AmountPaid;
                countFullfilled += receipt.CountBought;
            }
        }

        public void SetDailySpending()
        {
            DailySpending = Money * 0.98F / 21;
        }

        public void SetReservationWage()
        {
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
                ReservationWage += 0.95F;
            }
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