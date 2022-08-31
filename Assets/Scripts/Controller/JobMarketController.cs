using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Models.Population;

namespace Controller
{



    public class JobMarketController
    {
        private readonly List<JobModel> _openJobPositions = new();

        public void AddOpenJobPositions(IEnumerable<JobModel> openJobPositions)
        {
            _openJobPositions.AddRange(openJobPositions);
        }

        public int OpenJobPositionsCount()
        {
            return _openJobPositions.Count;
        }

        public void AdaptSalaryForLeftopenPositions(decimal newSalary, string companyId)
        {
            foreach (var j in _openJobPositions.Where(x => x.CompanyId == companyId))
            {
                j.Salary = newSalary;
            }
        }

        public void AddOpenJobPositions(List<JobModel> openJobPositions, int maxTotalPositions, string companyId)
        {
            if (maxTotalPositions < openJobPositions.Count)
            {
                throw new Exception();
            }

            int countCurrent = _openJobPositions.Count(x => x.CompanyId == companyId);
            if (maxTotalPositions > countCurrent)
            {
                AddOpenJobPositions(openJobPositions.GetRange(0, maxTotalPositions - countCurrent));
            }
            else if (maxTotalPositions < countCurrent)
            {
                RemoveOpenJobPositions(countCurrent - maxTotalPositions, companyId);
            }
        }

        private void RemoveOpenJobPositions(int count, string forCompanyId)
        {
            for (int i = count - 1; i >= 0; i--)
            {
                _openJobPositions.Where(x => x.CompanyId == forCompanyId).ToList().RemoveAt(0);
            }
        }


        public JobModel FindAvailableJob(decimal minimalSalary)
        {
            var availablePositions = _openJobPositions.Where(
                    x => x.Status == JobPositionStatus.Open && x.Salary >= minimalSalary)
                .OrderByDescending(x => x.Salary).ToList();
            if (availablePositions.Count > 0)
            {

                var position = availablePositions[0];
                if (position.Type == ProductType.FossileEnergy)
                {
                    Console.WriteLine();
                }

                position.Status = JobPositionStatus.Taken;
                _openJobPositions.Remove(position);
                return position;
            }

            return new JobModel(JobPositionStatus.NotAvailable);
        }
    }
}