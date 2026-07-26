using System.Reflection.Metadata.Ecma335;

public enum BookStatus
{
    Available,
    Borrowed,
    Damaged
}

public enum BorrowStatus
{
    Active,
    Returned,
    Overdue
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public List<BookCopy> BorrowedBooks { get; set; } = new List<BookCopy>();

    public User(int id, string name, string email)
    {
        this.Id = id;
        this.Name = name;
        this.Email = email;
    }
}

public class BookCopy
{
    public int Id { get; set; }
    public BookStatus Status { get; private set; }
    public Book ParentBook { get; set; }

    public BookCopy(int id, Book parentBook)
    {
        this.Id = id;
        this.ParentBook = parentBook;
        this.Status = BookStatus.Available;
    }

    public bool IsAvailable() => Status == BookStatus.Available;
    public void MarkBorrowed() => Status = BookStatus.Borrowed;
    public void MarkReturned() => Status = BookStatus.Available;
    public void MarkDamaged() => Status = BookStatus.Damaged;
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public List<BookCopy> Copies { get; set; }
    public Queue<User> Waitlist { get; set; }

    public Book(int id, string title, string author)
    {
        this.Id = id;
        this.Title = title;
        this.Author = author;
        this.Copies = new List<BookCopy>();
        this.Waitlist = new Queue<User>();
    }

    public bool IsAvailable() => Copies.Any(copy => copy.IsAvailable());

    public BookCopy GetAvailableCopy()
        => Copies.FirstOrDefault(copy => copy.IsAvailable());
}
public class BorrowRecord
{
    public int Id { get; set; }
    public User User { get; set; }
    public BookCopy BookCopy { get; set; }
    public BorrowStatus Status { get; private set; }
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    public BorrowRecord(int id, User user, BookCopy copy)
    {
        this.Id = id;
        this.User = user;
        this.BookCopy = copy;
        this.Status = BorrowStatus.Active;
        this.BorrowDate = DateTime.Now;
        this.DueDate = this.BorrowDate.AddDays(15);
        this.ReturnDate = null;
    }

    public void UpdateStatus(BorrowStatus status)
    {
        this.Status = status;
    }

    public bool IsOverdue()
    {
        return DateTime.Now > this.DueDate;
    }

    public void ReturnBook()
    {
        this.ReturnDate = DateTime.Now;
        if (IsOverdue())
            UpdateStatus(BorrowStatus.Overdue);
        else
            UpdateStatus(BorrowStatus.Returned);
    }

    public int CalculateFine()
    {
        if (ReturnDate == null || !IsOverdue())
            return 0;

        int overdueDays = (int)(this.ReturnDate.Value - this.DueDate).TotalDays;
        return overdueDays * 10;
    }
}

public class LibraryService
{
    private List<Book> _books = new List<Book>();
    private List<BorrowRecord> _borrowRecords = new List<BorrowRecord>();
    public void AddBook(Book book)
    {
        _books.Add(book);
    }
    public Book SearchBook(string title)
    {
        Book book = _books.FirstOrDefault(b => b.Title == title);
        if (book == null)
            throw new Exception("Book not found!");
        return book;
    }

    public BorrowRecord BorrowBook(User user, Book book)
    {
        if (user.BorrowedBooks.Count >= 5)
            throw new Exception("Max 5 books limit reached!");

        BookCopy copy = book.GetAvailableCopy();
        if (copy == null)
        {
            AddToWaitlist(user, book);
            throw new Exception("No copy available. Added to waitlist!");
        }

        copy.MarkBorrowed();
        user.BorrowedBooks.Add(copy);

        BorrowRecord br = new BorrowRecord(new Random().Next(), user, copy);
        _borrowRecords.Add(br);
        return br;
    }

    public void ReturnBook(BorrowRecord br)
    {
        br.ReturnBook();
        br.BookCopy.MarkReturned();
        br.User.BorrowedBooks.Remove(br.BookCopy);
        NotifyWaitlist(br.BookCopy.ParentBook);
    }

    public int PayFine(User user, BorrowRecord br)
    {
        if (br.User != user)
            throw new Exception("Incorrect user!");
        return br.CalculateFine();
    }

    public void AddToWaitlist(User user, Book book)
    {
        book.Waitlist.Enqueue(user);
    }

    private void NotifyWaitlist(Book book)
    {
        if (book.Waitlist.Count > 0)
        {
            User nextUser = book.Waitlist.Dequeue();
            Console.WriteLine($"Notifying {nextUser.Name} — '{book.Title}' is now available!");
        }
    }
}


public class LibrarySystem
{
    public static void Main(string[] args)
    {
        LibraryService library = new LibraryService();

        Book book1 = new Book(1, "Clean Code", "Robert Martin");
        Book book2 = new Book(2, "Harry Potter", "J.K. Rowling");

        BookCopy copy1 = new BookCopy(1, book1);
        BookCopy copy2 = new BookCopy(2, book1);
        BookCopy copy3 = new BookCopy(3, book2);

        book1.Copies.Add(copy1);
        book1.Copies.Add(copy2);
        book2.Copies.Add(copy3);

        library.AddBook(book1);
        library.AddBook(book2);

        User user1 = new User(1, "Ajaj", "ajaj@email.com");
        User user2 = new User(2, "Rahul", "rahul@email.com");

        Console.WriteLine("--- Search Book ---");
        Book foundBook = library.SearchBook("Clean Code");
        Console.WriteLine($"Found: {foundBook.Title} by {foundBook.Author}");
        Console.WriteLine($"Available: {foundBook.IsAvailable()}");

        Console.WriteLine("\n--- Borrow Book ---");
        BorrowRecord br1 = library.BorrowBook(user1, foundBook);
        Console.WriteLine($"{user1.Name} ne '{foundBook.Title}' li. Due Date: {br1.DueDate.ToShortDateString()}");

        BorrowRecord br2 = library.BorrowBook(user2, foundBook);
        Console.WriteLine($"{user2.Name} ne '{foundBook.Title}' li. Due Date: {br2.DueDate.ToShortDateString()}");

        Console.WriteLine("\n--- No Copy Available — Waitlist Test ---");
        User user3 = new User(3, "Priya", "priya@email.com");
        try
        {
            library.BorrowBook(user3, foundBook);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }

        Console.WriteLine("\n--- Return Book ---");
        library.ReturnBook(br1);
        Console.WriteLine($"{user1.Name} ne '{foundBook.Title}' return ki. Status: {br1.Status}");

        Console.WriteLine("\n--- Fine Calculation ---");
        int fine = library.PayFine(user2, br2);
        Console.WriteLine($"{user2.Name} ka fine: Rs.{fine}");

        Console.WriteLine("\n--- Max 5 Books Limit Test ---");
        User user4 = new User(4, "Rohan", "rohan@email.com");
        Book b1 = new Book(3, "Book A", "Author A");
        Book b2 = new Book(4, "Book B", "Author B");
        Book b3 = new Book(5, "Book C", "Author C");
        Book b4 = new Book(6, "Book D", "Author D");
        Book b5 = new Book(7, "Book E", "Author E");
        Book b6 = new Book(8, "Book F", "Author F");

        b1.Copies.Add(new BookCopy(10, b1));
        b2.Copies.Add(new BookCopy(11, b2));
        b3.Copies.Add(new BookCopy(12, b3));
        b4.Copies.Add(new BookCopy(13, b4));
        b5.Copies.Add(new BookCopy(14, b5));
        b6.Copies.Add(new BookCopy(15, b6));

        library.AddBook(b1); library.AddBook(b2); library.AddBook(b3);
        library.AddBook(b4); library.AddBook(b5); library.AddBook(b6);

        library.BorrowBook(user4, b1);
        library.BorrowBook(user4, b2);
        library.BorrowBook(user4, b3);
        library.BorrowBook(user4, b4);
        library.BorrowBook(user4, b5);

        try
        {
            library.BorrowBook(user4, b6);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }
}