﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using Enums;
using Models.Agents;

namespace Models.Finance
{
    public class BankAccountModel
    {
        private readonly BankAgent _bank;
        public decimal Savings { get; private set; }
        private readonly List<LoanModel> Loans = new();
        public decimal LoansSum => Loans.Sum(x => x.TotalSumLeft);

        public BankAccountModel(decimal deposit, BankAgent bank)
        {
            _bank = bank;
            Savings = deposit;
        }

        public decimal UpdateSavingsByInterestRate(float positiveRate, float negativeRate)
        {
            if (Savings > 0)
            {
                decimal additionalSavings = Savings * (decimal)(positiveRate / 12);
                Savings += additionalSavings;
                return additionalSavings * -1;
            }
            decimal payments = Savings * (decimal)(negativeRate / 12);
            Savings += payments;
            return payments * -1;
        }

        public bool IsLoanAdded(decimal amount, CreditRating rating)
        {
            var loan = _bank.RequestLoan(amount, rating);
            if (loan.IsDeclined == false)
            {
                Loans.Add(loan);
                Savings += loan.TotalSumLeft;
                return true;
            }

            return false;
        }

        public decimal MonthlyPaymentForLoans()
        {
            decimal totalPaid = 0;
            for (int i = Loans.Count - 1; i >= 0; i--)
            {
                var loan = Loans[i];
                var paid = loan.MakeMonthlyPayment();
                totalPaid += paid;
                Savings -= paid;
                if (loan.MonthLeft == 0)
                {
                    Loans.Remove(loan);
                }
            }

            return totalPaid;
        }

        public void Deposit(decimal sum)
        {
            Savings += sum;
        }

        public decimal Withdraw(decimal sum)
        {
            Savings -= sum;
            return sum;
        }

        public decimal CloseAccount()
        {
            foreach (var l in Loans)
            {
                if (Savings <= 0)
                {
                    l.RemoveFromBank();
                    continue;
                }
                while (l.MonthLeft > 0 && Savings > 0)
                {
                    Savings -= l.MakeMonthlyPayment();
                }
            }
            _bank.RemoveAccount(this);
            Loans.Clear();
            return Savings;
        }
    }
}