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
        public double Wage { get; set; } = 0;
        public int Health { get; set; } = 1000;

        public List<InventoryItem> Inventory { get; set; } = new();
        //public int CompanyId { get; set; }
        private readonly System.Random _rand = new();
        
        //private Company _employedAtCompany = null;
        private readonly List<Company> _typeAConnections = new();
        //public bool HasJob { get; private set; }
        private JobChangeLevel _jobChangeIntensity = JobChangeLevel.Unmployed;

        private JobContract _jobContract;

        public Worker()
        {
            Inventory.Add(new InventoryItem
            {
                AvgPaid = 0.5, 
                Count = 0, 
                Product = ProductType.Food
            });
        }
        
        public void AddContract(JobContract contract)
        {
            _jobContract = contract;
        }

        public double Pay()
        {
            Money += Wage;
            return Wage;
        }
        
        public void AddBuyOffers()
        {
            int demand = 10 + _rand.Next(-5, 6);
            if (Inventory[0].Count - demand < 5)
            {
                // above avg price
            }
            else if (Inventory[0].Count - demand < 15)
            {
                // avg price
            }
            else if (Inventory[0].Count - demand < 25)
            {
                // below avg price
            }
            else
            {
                // skip
            }
        }

        public void FullfillBid(ProductType product, int count, double price)
        {
            Inventory.Where(x => x.Product == product).ToArray()[0].Add(count, price);
            Money -= count * price;
        }

        

        public void SetReservationWage()
        {
            ServiceLocator.Instance.stepsWorker++;
            Academy.Instance.StatsRecorder.Add("Worker/Money", (float)Money);
            //Academy.Instance.StatsRecorder.Add("Worker/Bought", DemandFulfilled);

            if (HasJob)
            {
                if (Wage > _employedAtCompany.RealwageRate)
                {
                    _jobChangeIntensity = JobChangeLevel.Underpaid;
                }
                else
                {
                    _jobChangeIntensity = JobChangeLevel.Satisfied;
                    Wage = _employedAtCompany.OfferedWageRate;
                }
            }
            else
            {
                _jobChangeIntensity = JobChangeLevel.Unmployed;
                Wage *= 0.95;
            }

            Wage = Wage < 5 ? 5 : Wage;
            //DemandFulfilled = 0;
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
                if (potentialCompany.OpenPositions > 0 && potentialCompany.OfferedWageRate >= Wage)
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