using System.Collections.Generic;
using System.Linq;
using NewScripts.Game.Models;
using NewScripts.Game.Services;
using Unity.MLAgents;
using UnityEngine;

namespace NewScripts.Game.Entities
{
    public class LaborMarket : MonoBehaviour
    {
        public List<Worker> Workers { get; } = new();
        private List<JobOffer> JobOffers { get; } = new();
        private List<JobBid> JobBids { get; } = new();
        private List<JobContract> Contracts { get; } = new();

        private int CountAdded { get; set; }
        public int CountRemoved { get; set; }

        public int DemandForWorkforce { get; private set; }
        

        private void Awake()
        {
            
        }

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
            Debug.Log("3A Resolve markets start.");
            CountAdded = 0;
            var offers = JobOffers.OrderBy(x => x.Wage).ToList();
            var bids = JobBids.OrderByDescending(x => x.Wage).ToList();

            while (offers.Count > 0 && bids.Count > 0)
            {
                var offer = offers[0];
                var bid = bids[0];
                if (offer.Wage < bid.Wage)
                {
                    Academy.Instance.StatsRecorder.Add("Contract/WorkAdd", ++CountAdded);

                    //var contract = new JobContract(offer.Worker, bid.Employer, offer.Wage);
                    var wage = (bid.Wage + offer.Wage) / 2;
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
            Debug.Log("3B Resolve markets done.");

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