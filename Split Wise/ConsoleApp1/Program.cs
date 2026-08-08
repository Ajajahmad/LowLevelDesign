
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
    }
}


public class SplitwiseService
{
    private List<Group> _groups = new List<Group>();

    public void AddGroup(Group group)
    {
        _groups.Add(group);
    }

    public void AddExpense(Group group, Expense expense, ISplitStrategy strategy)
    {
        // Step 1 — Strategy se splits calculate karo
        List<Split> splits = strategy.Calculate(expense.Amount,
                                                 group.Users);

        // Step 2 — Splits expense mein store karo
        expense.Splits = splits;

        // Step 3 — Balance update karo
        foreach (Split split in splits)
        {
            // Payer khud ko pay nahi karega
            if (split.User == expense.PaidBy)
                continue;

            // split.User → PaidBy ko dena hai
            if (!split.User.Balance.ContainsKey(expense.PaidBy))
                split.User.Balance[expense.PaidBy] = 0;

            split.User.Balance[expense.PaidBy] += split.Amount;

            // PaidBy → split.User se lena hai
            if (!expense.PaidBy.Balance.ContainsKey(split.User))
                expense.PaidBy.Balance[split.User] = 0;

            expense.PaidBy.Balance[split.User] -= split.Amount;
        }

        // Step 4 — Group mein expense add karo
        group.AddExpense(expense);

        Console.WriteLine($"Expense '{expense.Description}' of Rs.{expense.Amount} added!");
    }

    public void GetBalance(User user)
    {
        Console.WriteLine($"\n--- {user.Name} ka Balance ---");

        if (user.Balance.Count == 0)
        {
            Console.WriteLine("Sab settled up hai!");
            return;
        }

        foreach (var entry in user.Balance)
        {
            if (entry.Value > 0)
                Console.WriteLine($"{user.Name} owes {entry.Key.Name}: Rs.{entry.Value}");
            else if (entry.Value < 0)
                Console.WriteLine($"{entry.Key.Name} owes {user.Name}: Rs.{Math.Abs(entry.Value)}");
            else
                Console.WriteLine($"{user.Name} aur {entry.Key.Name} settled up hain!");
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

        // Balance zero karo dono ka
        payer.Balance[receiver] = 0;
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

        // Test 1 — Equal Split
        Console.WriteLine("=== Equal Split ===");
        Expense expense1 = new Expense(1, "Restaurant", 1200, ajaj, SplitType.Equal);
        service.AddExpense(group, expense1, new EqualSplitStrategy());

        service.GetBalance(ajaj);
        service.GetBalance(rahul);

        // Test 2 — Exact Split
        Console.WriteLine("\n=== Exact Split ===");
        var exactAmounts = new Dictionary<User, double>
        {
            { ajaj,  500 },
            { rahul, 300 },
            { priya, 400 }
        };
        Expense expense2 = new Expense(2, "Hotel", 1200, priya, SplitType.Exact);
        service.AddExpense(group, expense2, new ExactSplitStrategy(exactAmounts));

        service.GetBalance(priya);

        // Test 3 — Percentage Split
        Console.WriteLine("\n=== Percentage Split ===");
        var percentages = new Dictionary<User, double>
        {
            { ajaj,  50 },
            { rahul, 25 },
            { rohan, 25 }
        };
        Expense expense3 = new Expense(3, "Cab", 800, rahul, SplitType.Percentage);
        service.AddExpense(group, expense3, new PercentageSplitStrategy(percentages));

        service.GetBalance(rahul);

        // Test 4 — Settle Up
        Console.WriteLine("\n=== Settle Up ===");
        service.SettleUp(rahul, ajaj);
        service.GetBalance(rahul);
    }
}