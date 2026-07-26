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