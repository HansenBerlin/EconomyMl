using EconomyBase.Enums;
using EconomyBase.Models.Business;
using EconomyBase.Models.Population;

namespace EconomyBase.Factories
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