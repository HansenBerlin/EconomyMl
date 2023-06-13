using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = System.Random;

namespace NewScripts
{
    public class LaborMarket : MonoBehaviour
    {
        public TextMeshProUGUI WorkerText;
        public TextMeshProUGUI AvgText;
        public TextMeshProUGUI HealthText;
        public List<Worker> Workers { get; } = new();
        private readonly Random _rand = new();
        
        public void InitWorkers()
        {
            for (int i = 0; i < 1000; i++)
            {
                Workers.Add(new Worker());
            }
        }


        public void NewRound()
        {
            try
            {
                var avg = Workers?.Where(x => x.CompanyId == 0)?.Select(x => x.Money)?.Average();
                var workeravg = Workers?.Where(x => x.CompanyId != 0)?.Select(x => x.Money)?.Average();
                AvgText.GetComponent<TextMeshProUGUI>().text = $"{avg:0}/{workeravg:0}";
                var workerCount = Workers.Where(x => x.CompanyId != 0).ToList().Count.ToString();
                WorkerText.GetComponent<TextMeshProUGUI>().text = $"{workerCount}/{Workers.Count}";
                var health = Workers.Where(x => x.CompanyId == 0).Select(x => x.Health).Average();
                var workerhealth = Workers.Where(x => x.CompanyId != 0).Select(x => x.Health).Average();
                HealthText.GetComponent<TextMeshProUGUI>().text = $"{health:0}/{workerhealth:0}";

            }
            catch (Exception)
            {
                Debug.LogWarning("Updating stats failed");
            }
        }
        
        public int CompanyWorkerCount(int companyId)
        {
            int count = 0;
            foreach (var worker in Workers)
            {
                if (worker.CompanyId == companyId)
                {
                    count++;
                }
            }

            return count;
        }

        public void Hire(int count, int companyId)
        {
            foreach (var worker in GenerateRandomLoop(Workers))
            {
                if (count == 0)
                {
                    break;
                }
                if (worker.CompanyId == 0)
                {
                    worker.CompanyId = companyId;
                    count--;
                }
            }
        }

        public void Fire(int count, int companyId)
        {
            foreach (var worker in Workers)
            {
                if (count == 0)
                {
                    break;
                }
                if (worker.CompanyId == companyId && worker.IsCeo == false)
                {
                    worker.CompanyId = 0;
                    count--;
                }
            }
        }

        public void Pay(float amount, int companyId, bool isCeoPayment = false)
        {
            foreach (var worker in Workers)
            {
                if (worker.CompanyId == companyId)
                {
                    if (isCeoPayment && worker.IsCeo)
                    {
                        worker.Money += amount;
                    }
                    else if (isCeoPayment == false && worker.IsCeo == false)
                    {
                        worker.Money += amount;
                    }
                }
            }
        }
        
        public float MakeCeoLiable(int companyId)
        {
            foreach (var worker in Workers)
            {
                if (worker.CompanyId == companyId && worker.IsCeo)
                {
                    float money = worker.Money;
                    worker.Money = 0;
                    return money;
                }
            }

            throw new Exception("NO CEO FOUND");
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
        
        public List<Worker> GenerateRandomLoop(List<Worker> listToShuffle)
        {
            for (int i = listToShuffle.Count - 1; i > 0; i--)
            {
                var k = _rand.Next(i + 1);
                (listToShuffle[k], listToShuffle[i]) = (listToShuffle[i], listToShuffle[k]);
            }
            return listToShuffle;
        }
    }
}