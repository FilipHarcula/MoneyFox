﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using MoneyFox.Application.Common;
using MoneyFox.Application.Common.Interfaces;
using MoneyFox.Application.Common.QueryObjects;
using MoneyFox.Domain;
using MoneyFox.Domain.Entities;
using MoneyFox.Domain.Exceptions;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MoneyFox.Application.Accounts.Queries.GetTotalEndOfMonthBalance
{
    public class GetAccountEndOfMonthBalanceQuery : IRequest<decimal>
    {
        public GetAccountEndOfMonthBalanceQuery(int accountId)
        {
            AccountId = accountId;
        }

        public int AccountId { get; }

        public class Handler : IRequestHandler<GetAccountEndOfMonthBalanceQuery, decimal>
        {
            private readonly Logger logManager = LogManager.GetCurrentClassLogger();

            private readonly IContextAdapter contextAdapter;

            public Handler(IContextAdapter contextAdapter)
            {
                this.contextAdapter = contextAdapter;
            }

            private int accountId = 0;

            public async Task<decimal> Handle(GetAccountEndOfMonthBalanceQuery request, CancellationToken cancellationToken)
            {
                logManager.Info($"Calculate EndOfMonth Balance for account {request.AccountId}.");
                accountId = request.AccountId;

                var account = await contextAdapter.Context.Accounts.WithId(accountId).FirstAsync();
                decimal balance = await GetCurrentAccountBalanceAsync();

                foreach(Payment payment in await GetUnclearedPaymentsForThisMonthAsync())
                {
                    if(payment.ChargedAccount == null)
                    {
                        throw new InvalidOperationException($"Navigation Property not initialized properly: {nameof(payment.ChargedAccount)}");
                    }

                    balance = AddPaymentToBalance(payment, account, balance);
                }

                return balance;
            }

            public static decimal AddPaymentToBalance(Payment payment, Account account, decimal currentBalance)
            {
                switch(payment.Type)
                {
                    case PaymentType.Expense:
                        currentBalance -= payment.Amount;
                        break;

                    case PaymentType.Income:
                        currentBalance += payment.Amount;
                        break;

                    case PaymentType.Transfer:
                        currentBalance = CalculateBalanceForTransfer(account, currentBalance, payment);
                        break;

                    default:
                        throw new InvalidPaymentTypeException();
                }

                return currentBalance;
            }

            private static decimal CalculateBalanceForTransfer(Account account, decimal balance, Payment payment)
            {
                if(Equals(account.Id, payment.ChargedAccount!.Id))
                {
                    //Transfer from excluded account
                    balance -= payment.Amount;
                }

                if(payment.TargetAccount == null)
                {
                    throw new InvalidOperationException($"Navigation Property not initialized properly: {nameof(payment.TargetAccount)}");
                }

                if(Equals(account.Id, payment.TargetAccount.Id))
                {
                    //Transfer to excluded account
                    balance += payment.Amount;
                }

                return balance;
            }

            private async Task<decimal> GetCurrentAccountBalanceAsync()
            {
                return (await contextAdapter.Context
                                          .Accounts
                                          .WithId(accountId)
                                          .Select(x => x.CurrentBalance)
                                          .ToListAsync())
                                          .Sum();
            }

            private async Task<List<Payment>> GetUnclearedPaymentsForThisMonthAsync()
            {
                return await contextAdapter.Context
                                           .Payments
                                           .HasAccountId(accountId)
                                           .Include(x => x.ChargedAccount)
                                           .Include(x => x.TargetAccount)
                                           .AreNotCleared()
                                           .HasDateSmallerEqualsThan(HelperFunctions.GetEndOfMonth())
                                           .ToListAsync();
            }
        }
    }
}
