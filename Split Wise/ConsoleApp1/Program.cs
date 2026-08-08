using System;
using System.Collections.Generic;
using System.Linq;

public enum SplitType { Equal, Exact, Percentage }

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Dictionary<User, double> Balance { get; set; }

    public User(int id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
        Balance = new Dictionary<User, double>();
    }
}

public class Split
{
    public User User { get; set; }
    public double Amount { get; set; }

    public Split(User user, double amount)
    {
        User = user;
        Amount = amount;
    }
}

public class Expense
{
    public int Id { get; set; }
    public string Description { get; set; }
    public double Amount { get; set; }
    public User PaidBy { get; set; }
    public SplitType SplitType { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Split> Splits { get; set; }

    public Expense(int id, string description, double amount, User paidBy, SplitType type)
    {
        Id = id;
        Description = description;
        Amount = amount;
        PaidBy = paidBy;
        SplitType = type;
        Splits = new List<Split>();
        CreatedAt = DateTime.Now;
    }
}

public class Group
{
    public int Id { get; set; }
    public string GroupName { get; set; }
    public List<User> Users { get; set; }
    public List<Expense> Expenses { get; set; }

    public Group(int id, string name)
    {
        Id = id;
        GroupName = name;
        Users = new List<User>();
        Expenses = new List<Expense>();
    }

    public void AddUser(User user) => Users.Add(user);
    public void AddExpense(Expense expense) => Expenses.Add(expense);
}

public interface ISplitStrategy
{
    List<Split> Calculate(double amount, List<User> users);
}

public class EqualSplitStrategy : ISplitStrategy
{
    public List<Split> Calculate(double amount, List<User> users)
    {
        double share = amount / users.Count;
        return users.Select(u => new Split(u, share)).ToList();
    }
}

public class ExactSplitStrategy : ISplitStrategy
{
    private Dictionary<User, double> _exactAmounts;

    public ExactSplitStrategy(Dictionary<User, double> exactAmounts)
    {
        _exactAmounts = exactAmounts;
    }

    public List<Split> Calculate(double amount, List<User> users)
    {
        double total = _exactAmounts.Values.Sum();
        if (total != amount)
            throw new Exception("Exact amounts do not add up to total!");
        return users.Select(u => new Split(u, _exactAmounts[u])).ToList();
    }
}

public class PercentageSplitStrategy : ISplitStrategy
{
    private Dictionary<User, double> _percentages;

    public PercentageSplitStrategy(Dictionary<User, double> percentages)
    {
        _percentages = percentages;
    }

    public List<Split> Calculate(double amount, List<User> users)
    {
        double total = _percentages.Values.Sum();
        if (total != 100)
            throw new Exception("Percentages do not add up to 100!");
        return users.Select(u => new Split(u, amount * _percentages[u] / 100)).ToList();
    }
}

public class SplitwiseService
{
    private List<Group> _groups = new List<Group>();

    public void AddGroup(Group group) => _groups.Add(group);

    public void AddExpense(Group group, Expense expense,
                           ISplitStrategy strategy, List<User> users = null)
    {
        List<User> splitUsers = users ?? group.Users;

        List<Split> splits = strategy.Calculate(expense.Amount, splitUsers);
        expense.Splits = splits;

        foreach (Split split in splits)
        {
            if (split.User == expense.PaidBy) continue;

            if (!split.User.Balance.ContainsKey(expense.PaidBy))
                split.User.Balance[expense.PaidBy] = 0;
            split.User.Balance[expense.PaidBy] += split.Amount;

            if (!expense.PaidBy.Balance.ContainsKey(split.User))
                expense.PaidBy.Balance[split.User] = 0;
            expense.PaidBy.Balance[split.User] -= split.Amount;
        }

        group.AddExpense(expense);
        Console.WriteLine($"Expense '{expense.Description}' of Rs.{expense.Amount} added!");
    }

    public void GetBalance(User user)
    {
        Console.WriteLine($"\n--- {user.Name} ka Balance ---");
        if (user.Balance.Count == 0) { Console.WriteLine("Sab settled up!"); return; }

        foreach (var entry in user.Balance)
        {
            if (entry.Value > 0)
                Console.WriteLine($"{user.Name} owes {entry.Key.Name}: Rs.{entry.Value}");
            else if (entry.Value < 0)
                Console.WriteLine($"{entry.Key.Name} owes {user.Name}: Rs.{Math.Abs(entry.Value)}");
            else
                Console.WriteLine($"{user.Name} & {entry.Key.Name} settled!");
        }
    }

    public void SettleUp(User payer, User receiver)
    {
        if (!payer.Balance.ContainsKey(receiver) || payer.Balance[receiver] <= 0)
        {
            Console.WriteLine($"{payer.Name} ko {receiver.Name} ko kuch nahi dena!");
            return;
        }

        double amount = payer.Balance[receiver];
        payer.Balance[receiver] = 0;

        if (receiver.Balance.ContainsKey(payer))
            receiver.Balance[payer] = 0;

        Console.WriteLine($"{payer.Name} ne {receiver.Name} ko Rs.{amount} settle kiya!");
    }
}

public class SplitwiseSystem
{
    public static void Main(string[] args)
    {
        SplitwiseService service = new SplitwiseService();

        User ajaj = new User(1, "Ajaj", "ajaj@email.com");
        User rahul = new User(2, "Rahul", "rahul@email.com");
        User priya = new User(3, "Priya", "priya@email.com");
        User rohan = new User(4, "Rohan", "rohan@email.com");

        Group group = new Group(1, "Goa Trip");
        group.AddUser(ajaj);
        group.AddUser(rahul);
        group.AddUser(priya);
        group.AddUser(rohan);
        service.AddGroup(group);

        Console.WriteLine("=== Equal Split ===");
        Expense expense1 = new Expense(1, "Restaurant", 1200, ajaj, SplitType.Equal);
        service.AddExpense(group, expense1, new EqualSplitStrategy());
        service.GetBalance(ajaj);
        service.GetBalance(rahul);

        Console.WriteLine("\n=== Exact Split ===");
        var exactAmounts = new Dictionary<User, double>
        {
            { ajaj, 500 }, { rahul, 300 }, { priya, 400 }
        };
        var exactUsers = new List<User> { ajaj, rahul, priya };
        Expense expense2 = new Expense(2, "Hotel", 1200, priya, SplitType.Exact);
        service.AddExpense(group, expense2, new ExactSplitStrategy(exactAmounts), exactUsers);
        service.GetBalance(priya);

        Console.WriteLine("\n=== Percentage Split ===");
        var percentages = new Dictionary<User, double>
        {
            { ajaj, 50 }, { rahul, 25 }, { rohan, 25 }
        };
        var percentageUsers = new List<User> { ajaj, rahul, rohan };
        Expense expense3 = new Expense(3, "Cab", 800, rahul, SplitType.Percentage);
        service.AddExpense(group, expense3, new PercentageSplitStrategy(percentages), percentageUsers);
        service.GetBalance(rahul);

        Console.WriteLine("\n=== Settle Up ===");
        service.SettleUp(rahul, ajaj);
        service.GetBalance(rahul);
    }
}