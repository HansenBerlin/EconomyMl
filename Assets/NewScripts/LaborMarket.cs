using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = System.Random;

namespace NewScripts
{
    public class LaborMarket : MonoBehaviour
    {
        
        public List<Worker> Workers { get; } = new();
        public List<JobOffer> JobOffers { get; } = new();
        public List<JobBid> JobBids { get; } = new();
        public List<JobContract> Contracts { get; } = new();

        public int DemandForWorkforce { get; private set; }
        
        public void InitWorkers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Workers.Add(new Worker());
            }
        }

        public double AveragePayment()
        {
            double average = Workers.Select(x => x.Wage).Average();
            return average == 0 ? 100 : average;
        }

        public void AddJobOffer(JobOffer offer)
        {
            JobOffers.Add(offer);
        }
        
        public void AddJobBids(List<JobBid> bids)
        {
            JobBids.AddRange(bids);
        }

        public void ResolveMarket()
        {
            var offers = JobOffers.OrderBy(x => x.Price).ToList();
            var bids = JobBids.OrderByDescending(x => x.Price).ToList();

            while (offers.Count > 0 && bids.Count > 0)
            {
                var offer = offers[0];
                var bid = bids[0];
                if (offer.Price < bid.Price)
                {
                    var contract = new JobContract(offer.Worker, bid.Employer, offer.Price);
                    Contracts.Add(contract);
                    offers.Remove(offer);
                    bids.Remove(bid);
                }
                else
                {
                    break;
                }
            }

            DemandForWorkforce = bids.Count - offers.Count;
            JobOffers.Clear();
            JobBids.Clear();
        }

        public void RemoveSick()
        {
            for (var index = Workers.Count - 1; index >= 0; index--)
            {
                var worker = Workers[index];
                if (worker.Health <= 0)
                {
                    //Workers.Remove(worker);
                }
            }
        }
    }
}