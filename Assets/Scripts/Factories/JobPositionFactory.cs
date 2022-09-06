using System.Collections.Generic;
using Agents;
using Enums;
using Models;

namespace Factories
{
    public static class JobPositionFactory
    {
        public static List<JobModel> Create(int count, decimal salary, string companyId, List<PersonAgent> workers,
            ProductType type)
        {
            List<JobModel> jobs = new();

            for (var i = 0; i < count; i++) jobs.Add(new JobModel(salary, workers, companyId, type));

            return jobs;
        }
    }
}