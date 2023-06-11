using UnityEngine;

namespace NewScripts
{
    public class LaborMarket : MonoBehaviour
    {
        public int Workers = 900;
        public float WorkerAccumulatedIncome = 0;

        public void Hire(int count)
        {
            Workers += count;
        }

        public int Fire(int count)
        {
            if (Workers - count > 0)
            {
                Workers -= count;
                return count;
            }

            return 0;
        }

        public void Pay(int amount)
        {
            WorkerAccumulatedIncome += amount;
        }
        
        public void Decrease(float amount)
        {
            WorkerAccumulatedIncome -= amount;
        }
    }
}