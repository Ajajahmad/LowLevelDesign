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