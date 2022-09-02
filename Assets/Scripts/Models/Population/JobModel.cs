using System;
using System.Collections.Generic;
using Enums;
using Models.Agents;

namespace Models.Population
{



    public class JobModel
    {
        public string Id = Guid.NewGuid().ToString();
        public string CompanyId { get; }
        public decimal Salary { get; set; }
        private readonly List<PersonAgent> _employees;

        public JobPositionStatus Status { get; set; }

        //private readonly List<JobModel> _openPositions;
        public ProductType Type { get; }

        public JobModel(decimal salary, List<PersonAgent> employees, string companyId, ProductType type)
        {
            //_openPositions = openJobs;
            Salary = salary;
            _employees = employees;
            Status = JobPositionStatus.Open;
            CompanyId = companyId;
            Type = type;
        }

        public JobModel(JobPositionStatus status)
        {
            Status = status;
        }


        public void TakeJob(PersonAgent worker, decimal desiredSalary)
        {
            if (_employees.Contains(worker))
            {
                throw new Exception();
            }


            if (desiredSalary < Salary)
            {
                Salary = desiredSalary;
            }

            _employees.Add(worker);
            //_openPositions.Remove(this);
            Status = JobPositionStatus.NotAvailable;
        }

        public void QuitJob(PersonAgent worker)
        {
            if (_employees.Contains(worker) == false)
            {
                throw new Exception();
            }

            _employees.Remove(worker);
        }
    }
}