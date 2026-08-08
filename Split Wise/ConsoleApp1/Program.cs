
public enum SplitType
{
    Equal,
    Exact,
    Percentage
}

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
        this.Balance = new Dictionary<User, double>();
    }
}
public class Split
{
    public User User;
    public double Amount;
    public Split(User user, double amount)
    {
        this.User = user;
        this.Amount = amount;
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

    public Expense(int id, string description, double amount, User PaidBy, SplitType type)
    {
        this.Id = id;
        this.Description = description;
        this.Amount = amount;
        this.PaidBy = PaidBy;
        this.SplitType = type;
        this.Splits = new List<Split>();
        this.CreatedAt = DateTime.Now;
    }
}
public class Group
{
    public int Id { get; set; }
    public string GroupName { get; set; }
    public List<User> Users { get; set; }
    public List<Expense> Expenses { get; set; }

    public Group(int id, String name)
    {
        this.Id = id;
        this.GroupName = name;
        this.Users = new List<User>();
        this.Expenses = new List<Expense>();
    }
    public void AddUser(User user)
    {
        Users.Add(user);
    }
    public void AddExpense(Expense expense)
    {
        Expenses.Add(expense);
    }
}

public interface ISplitStrategy
{
    public List<Split> Calculate(double amount, List<User> users);
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
    public ExactSplitStrategy(Dictionary<User, double> exactAmount)
    {
        _exactAmounts = exactAmount;
    }

    public List<Split> Calculate(double amount, List<User> users)
    {
        double total = _exactAmounts.Values.Sum();
        if(total != amount)
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
 