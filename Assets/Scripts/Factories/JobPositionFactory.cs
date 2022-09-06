using System.Collections.Generic;
using Assets.Scripts.Enums;
using Assets.Scripts.Models.Agents;
using Assets.Scripts.Models.Population;

namespace Assets.Scripts.Factories
{



    public static class JobPositionFactory
    {
        public static List<JobModel> Create(int count, decimal salary, string companyId, List<PersonAgent> workers,
            ProductType type)
        {
            List<JobModel> jobs = new();

            for (int i = 0; i < count; i++)
            {
                jobs.Add(new JobModel(salary, workers, companyId, type));
            }

            return jobs;
        }

    }
}