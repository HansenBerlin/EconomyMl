using System.Collections.Generic;
using Enums;
using Models.Population;

namespace Factories
{



    public static class JobPositionFactory
    {
        public static List<JobModel> Create(int count, decimal salary, string companyId, List<IPersonBase> workers,
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