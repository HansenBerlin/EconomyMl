using System.Collections.Generic;
using System.Linq;
using NewScripts.DataModelling;
using NewScripts.Game.Models;
using NewScripts.Game.Services;
using NewScripts.Interfaces;
using Unity.MLAgents;
using UnityEngine;

namespace NewScripts.Game.Entities
{
    public class LaborMarket
    { 
        public List<Worker> Workers { get; } = new();
        private List<JobOffer> JobOffers { get; } = new();
        private List<JobBid> JobBids { get; } = new();
        private List<JobContract> Contracts { get; } = new();
        public int DemandForWorkforce { get; private set; }

        public void InitWorkers(int count, BidCalculatorService bidCalculatorService)
        {
            for (int i = 0; i < count; i++)
            {
                Workers.Add(new Worker(bidCalculatorService));
            }
        }

        public decimal AveragePayment()
        {
            decimal average = Contracts.Count > 0 ? Contracts.Select(x => x.Wage).Average() : 100;
            return average;
        }

        public void AddJobOffer(JobOffer offer)
        {
            JobOffers.Add(offer);
        }
        
        public void AddJobBid(JobBid bid)
        {
            JobBids.Add(bid);
        }

        public void RemoveContract(JobContract contract)
        {
            Contracts.Remove(contract);
        }

        public void ResolveMarket()
        {
            var offers = JobOffers.OrderBy(x => x.Wage).ToList();
            var bids = JobBids.OrderByDescending(x => x.Wage).ToList();

            while (offers.Count > 0 && bids.Count > 0)
            {
                var offer = offers[0];
                var bid = bids[0];
                if (offer.Wage < bid.Wage)
                {
                    var contract = new JobContract(offer.Worker, bid.Employer, offer.Wage);
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
            Academy.Instance.StatsRecorder.Add("Labor/Demand", DemandForWorkforce);

            JobOffers.Clear();
            JobBids.Clear();

        }
    }
}