using AwesomeGIC_App;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class Program
{
    static Dictionary<string, BankAccount> Accounts = new();
    static List<InterestRule> InterestRules = new();


    public static void Main()
    {
        string input;
        do
        {
            Console.WriteLine("Welcome to AwesomeGIC Bank! What would you like to do?");
            Console.WriteLine("[T] Input transactions");
            Console.WriteLine("[I] Define interest rules");
            Console.WriteLine("[P] Print statement");
            Console.WriteLine("[Q] Quit");
            Console.Write("> ");

            input = Console.ReadLine()?.Trim().ToUpper();

            switch (input)
            {
                case "T":
                    InputTransactions();
                    break;
                case "I":
                    DefineInterestRules();
                    break;
                case "P":
                    PrintStatement();
                    break;
                case "Q":
                    Console.WriteLine("Thank you for banking with AwesomeGIC Bank.\nHave a nice day!");
                    break;
                default:
                    Console.WriteLine("Invalid input. Please try again.");
                    break;
            }

        } while (input != "Q");
    }

    public static void InputTransactions()
    {
        Console.WriteLine("Please enter transaction details in <Date> <Account> <Type> <Amount> format");
        Console.WriteLine("(or enter blank to go back to main menu):");

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) break;

            var parts = line.Split(' ');
            if (parts.Length != 4 ||
                !DateTime.TryParseExact(parts[0], "yyyyMMdd", null, DateTimeStyles.None, out var date) ||
                !decimal.TryParse(parts[3], out var amount) ||
                amount <= 0 ||
                !"DWdw".Contains(parts[2]))
            {
                Console.WriteLine("Invalid input. Please try again.");
                continue;
            }

            var accountId = parts[1];
            var type = parts[2].ToUpper();

            if (!Accounts.ContainsKey(accountId))
                Accounts[accountId] = new BankAccount(accountId);

            var account = Accounts[accountId];
            if (!account.AddTransaction(date, type, amount))
            {
                Console.WriteLine("Transaction failed due to insufficient balance or invalid first transaction.");
            }

            account.PrintTransactions();
        }
    }

    public static void DefineInterestRules()
    {
        Console.WriteLine("Please enter interest rules details in <Date> <RuleId> <Rate in %> format");
        Console.WriteLine("(or enter blank to go back to main menu):");

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) break;

            var parts = line.Split(' ');
            if (parts.Length != 3 ||
                !DateTime.TryParseExact(parts[0], "yyyyMMdd", null, DateTimeStyles.None, out var date) ||
                !decimal.TryParse(parts[2], out var rate) || rate <= 0 || rate >= 100)
            {
                Console.WriteLine("Invalid input. Please try again.");
                continue;
            }

            var ruleId = parts[1];
            InterestRules.RemoveAll(r => r.Date == date);
            InterestRules.Add(new InterestRule(date, ruleId, rate));
            InterestRules = InterestRules.OrderBy(r => r.Date).ToList();

            Console.WriteLine("Interest rules:");
            Console.WriteLine("| Date     | RuleId | Rate (%) |");
            foreach (var rule in InterestRules)
            {
                Console.WriteLine($"| {rule.Date:yyyyMMdd} | {rule.RuleId,-6} | {rule.Rate,8:F2} |");
            }
        }
    }

    public static void PrintStatement()
    {
        Console.WriteLine("Please enter account and month to generate the statement <Account> <Year><Month>");
        Console.WriteLine("(or enter blank to go back to main menu):");

        Console.Write("> ");
        var line = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(line)) return;

        var parts = line.Split(' ');
        if (parts.Length != 2 ||
            !DateTime.TryParseExact(parts[1] + "01", "yyyyMMdd", null, DateTimeStyles.None, out var monthStart))
        {
            Console.WriteLine("Invalid input. Please try again.");
            return;
        }

        var accountId = parts[0];
        if (!Accounts.TryGetValue(accountId, out var account))
        {
            Console.WriteLine("Account not found.");
            return;
        }

        account.PrintStatementWithInterest(monthStart, InterestRules);
    }
}

