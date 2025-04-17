using System.Transactions;

namespace AwesomeGIC_App.Tests
{
    [TestClass]
    public class BankAccountTests
    {
        [TestMethod]
        public void AddDepositTransaction_ShouldIncreaseBalance()
        {
            var account = new BankAccount("AC001");

            bool success = account.AddTransaction(new DateTime(2023, 6, 1), "D", 100m);

            Assert.IsTrue(success);
            decimal expectedBalance = 100m;
            var actualBalance = GetBalance(account, new DateTime(2023, 6, 2));

            Assert.AreEqual(expectedBalance, actualBalance);
        }

        [TestMethod]
        public void AddWithdrawalTransaction_ShouldDecreaseBalance()
        {
            var account = new BankAccount("AC001");
            account.AddTransaction(new DateTime(2023, 6, 1), "D", 200m);
            bool success = account.AddTransaction(new DateTime(2023, 6, 2), "W", 50m);

            Assert.IsTrue(success);
            Assert.AreEqual(150m, GetBalance(account, new DateTime(2023, 6, 3)));
        }

        [TestMethod]
        public void Withdrawal_WithoutSufficientFunds_ShouldFail()
        {
            var account = new BankAccount("AC001");
            bool success = account.AddTransaction(new DateTime(2023, 6, 1), "W", 50m);

            Assert.IsFalse(success);
        }

        [TestMethod]
        public void InterestCalculation_ShouldBeCorrect()
        {
            var account = new BankAccount("AC001");
            account.AddTransaction(new DateTime(2023, 6, 1), "D", 250m);
            account.AddTransaction(new DateTime(2023, 6, 26), "W", 120m);

            var rules = new List<InterestRule>
            {
                new InterestRule(new DateTime(2023, 1, 1), "RULE01", 1.95m),
                new InterestRule(new DateTime(2023, 5, 20), "RULE02", 1.90m),
                new InterestRule(new DateTime(2023, 6, 15), "RULE03", 2.20m)
            };

            var start = new DateTime(2023, 6, 1);
            var end = new DateTime(2023, 6, 30);

            var eod = GetEodBalances(account, start, end);
            var interest = InvokeInterestCalculation(account, eod, rules, start, end);

            Assert.AreEqual(0.39m, interest);
        }

        // Helpers

        private decimal GetBalance(BankAccount account, DateTime beforeDate)
        {
            var method = typeof(BankAccount).GetMethod("GetBalanceBefore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (decimal)method.Invoke(account, new object[] { beforeDate });
        }

        private Dictionary<DateTime, decimal> GetEodBalances(BankAccount account, DateTime start, DateTime end)
        {
            var txnsField = typeof(BankAccount).GetField("Transactions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var txns = (List<Transaction>)txnsField.GetValue(account);

            decimal balance = txns
                .Where(t => t.Date < start)
                .Sum(t => t.Type == "D" ? t.Amount : t.Type == "W" ? -t.Amount : t.Amount);

            var eod = new Dictionary<DateTime, decimal>();
            for (DateTime d = start; d <= end; d = d.AddDays(1))
            {
                balance += txns.Where(t => t.Date == d)
                               .Sum(t => t.Type == "D" ? t.Amount : t.Type == "W" ? -t.Amount : t.Amount);
                eod[d] = balance;
            }

            return eod;
        }

        private decimal InvokeInterestCalculation(BankAccount account, Dictionary<DateTime, decimal> eodBalances, List<InterestRule> rules, DateTime start, DateTime end)
        {
            var method = typeof(BankAccount).GetMethod("CalculateInterest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (decimal)method.Invoke(account, new object[] { eodBalances, rules, start, end });
        }
    }
}
