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
                var avg = Workers?.Where(x => x.HasJob)?.Select(x => x.Money)?.Average();
                var workeravg = Workers?.Where(x => x.HasJob == false)?.Select(x => x.Money)?.Average();
                AvgText.GetComponent<TextMeshProUGUI>().text = $"{avg:0}/{workeravg:0}";
                var workerCount = Workers.Where(x => x.HasJob).ToList().Count.ToString();
                WorkerText.GetComponent<TextMeshProUGUI>().text = $"{workerCount}/{Workers.Count}";
            }
            catch (Exception)
            {
                Debug.LogWarning("Updating stats failed");
            }
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