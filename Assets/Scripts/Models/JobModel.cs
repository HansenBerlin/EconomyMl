using System;
using System.Collections.Generic;
using Agents;
using Enums;

namespace Models
{
    public class JobModel
    {
        private readonly List<PersonAgent> _employees;
        public string Id = Guid.NewGuid().ToString();

        public JobModel(decimal salary, List<PersonAgent> employees, string companyId, ProductType type)
        {
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

        public string CompanyId { get; }
        public decimal Salary { get; set; }

        public JobPositionStatus Status { get; set; }

        private ProductType Type { get; }


        public void TakeJob(PersonAgent worker, decimal desiredSalary)
        {
            if (_employees.Contains(worker)) throw new Exception();


            if (desiredSalary < Salary) Salary = desiredSalary;

            _employees.Add(worker);
            Status = JobPositionStatus.NotAvailable;
        }

        public void QuitJob(PersonAgent worker)
        {
            if (_employees.Contains(worker))
                _employees.Remove(worker);
        }
    }
}