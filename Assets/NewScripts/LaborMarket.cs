using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace NewScripts
{
    public class LaborMarket : MonoBehaviour
    {
        public TextMeshProUGUI WorkerText;
        public TextMeshProUGUI AvgText;
        public TextMeshProUGUI HealthText;
        public List<Worker> Workers { get; private set; } = new();


        public int WorkersCount => Workers.Count;
        
        
        public void NewRound()
        {
            var avg = Workers.Select(x => x.Money).Average();
            AvgText.GetComponent<TextMeshProUGUI>().text = $"{avg:0.##}";
            WorkerText.GetComponent<TextMeshProUGUI>().text = WorkersCount.ToString();
            var health = Workers.Select(x => x.Health).Average();
            HealthText.GetComponent<TextMeshProUGUI>().text = $"{health:0.##}";
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
            for (int i = 0; i < count; i++)
            {
                var worker = new Worker()
                {
                    CompanyId = companyId
                };
                Workers.Add(worker);
            }
        }

        public void Fire(int count, int companyId)
        {
            for (var index = Workers.Count - 1; index >= 0; index--)
            {
                if (count == 0)
                {
                    break;
                }
                var worker = Workers[index];
                if (worker.CompanyId == companyId)
                {
                    Workers.Remove(worker);
                    count--;
                }
            }
        }

        public void Pay(int amount, int companyId)
        {
            
            foreach (var worker in Workers)
            {
                if (worker.CompanyId == companyId)
                {
                    worker.Money += amount;
                }
            }
        }
    }
}