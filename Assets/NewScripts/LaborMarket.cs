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
        
        public List<Worker> Workers { get; } = new();
        
        public void InitWorkers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Workers.Add(new Worker());
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