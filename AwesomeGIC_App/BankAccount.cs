using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeGIC_App
{
    public class BankAccount
    {
        public string AccountId { get; }
        private List<Transaction> Transactions = new();

        public BankAccount(string accountId)
        {
            AccountId = accountId;
        }

        public bool AddTransaction(DateTime date, string type, decimal amount)
        {
            decimal balance = GetBalanceBefore(date);
            if (type == "W" && (Transactions.Count == 0 || balance < amount)) return false;

            int txnCount = Transactions.Count(t => t.Date == date) + 1;
            string txnId = $"{date:yyyyMMdd}-{txnCount:D2}";

            Transactions.Add(new Transaction
            {
                Date = date,
                TxnId = txnId,
                Type = type,
                Amount = amount
            });

            return true;
        }

        public void PrintTransactions()
        {
            Console.WriteLine($"Account: {AccountId}");
            Console.WriteLine("| Date     | Txn Id      | Type | Amount |");
            foreach (var txn in Transactions.OrderBy(t => t.Date).ThenBy(t => t.TxnId))
            {
                Console.WriteLine($"| {txn.Date:yyyyMMdd} | {txn.TxnId,-11} | {txn.Type,4} | {txn.Amount,6:F2} |");
            }
        }

        public void PrintStatementWithInterest(DateTime monthStart, List<InterestRule> rules)
        {
            var monthEnd = new DateTime(monthStart.Year, monthStart.Month, DateTime.DaysInMonth(monthStart.Year, monthStart.Month));
            var txns = Transactions.Where(t => t.Date >= monthStart && t.Date <= monthEnd).OrderBy(t => t.Date).ToList();
            decimal balance = GetBalanceBefore(monthStart);

            var eodBalances = new Dictionary<DateTime, decimal>();
            for (DateTime date = monthStart; date <= monthEnd; date = date.AddDays(1))
            {
                balance += txns.Where(t => t.Date == date).Sum(t => t.Type == "D" ? t.Amount : -t.Amount);
                eodBalances[date] = balance;
            }

            decimal interest = CalculateInterest(eodBalances, rules, monthStart, monthEnd);
            if (interest > 0)
            {
                txns.Add(new Transaction
                {
                    Date = monthEnd,
                    TxnId = "",
                    Type = "I",
                    Amount = interest
                });
            }

            txns = txns.OrderBy(t => t.Date).ThenBy(t => t.TxnId).ToList();
            balance = GetBalanceBefore(monthStart);
            Console.WriteLine($"Account: {AccountId}");
            Console.WriteLine("| Date     | Txn Id      | Type | Amount | Balance |");

            foreach (var txn in txns)
            {
                balance += txn.Type switch
                {
                    "D" => txn.Amount,
                    "W" => -txn.Amount,
                    "I" => txn.Amount,
                    _ => 0
                };
                Console.WriteLine($"| {txn.Date:yyyyMMdd} | {txn.TxnId,-11} | {txn.Type,4} | {txn.Amount,6:F2} | {balance,7:F2} |");
            }
        }

        private decimal CalculateInterest(Dictionary<DateTime, decimal> eodBalances, List<InterestRule> rules, DateTime start, DateTime end)
        {
            decimal totalInterest = 0;
            var sortedRules = rules.OrderBy(r => r.Date).ToList();

            DateTime periodStart = start;
            for (int i = 0; i <= sortedRules.Count; i++)
            {
                DateTime ruleStart = i < sortedRules.Count ? sortedRules[i].Date : end.AddDays(1);
                DateTime periodEnd = ruleStart.AddDays(-1);

                if (periodEnd < start) continue;
                if (periodStart > end) break;
                if (periodEnd > end) periodEnd = end;

                decimal rate = i == 0 ? 0 : sortedRules[i - 1].Rate;
                for (DateTime d = periodStart; d <= periodEnd; d = d.AddDays(1))
                {
                    totalInterest += eodBalances[d] * (rate / 100);
                }

                periodStart = ruleStart;
            }

            return Math.Round(totalInterest / 365, 2);
        }

        private decimal GetBalanceBefore(DateTime date)
        {
            return Transactions.Where(t => t.Date < date)
                               .Sum(t => t.Type == "D" ? t.Amount :
                                         t.Type == "W" ? -t.Amount :
                                         t.Type == "I" ? t.Amount : 0);
        }
    }
}
