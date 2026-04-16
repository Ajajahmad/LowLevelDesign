
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

enum BookingStatus
{ 
    Pending, 
    Failed,
    Confirmed,
    Cancelled
}
enum SeatStatus
{
    Locked,
    Booked,
    Available
}
enum SeatType
{
    Silver, Gold, Platinum
}
enum NotificationType
{
    Email, SMS
}
class User
{
    public string UserId { get;  }
    public string Name { get;  }
    public string Email { get;  }
    public string Phone { get;  }
    public User(string userId, string name, string email, string phone)
    {
        UserId = userId;
        Name = name;
        Email = email;
        Phone = phone;
    }
}

class Movie
{
    public string MovieId { get;  }
    public string Title { get; }
    public string Genre { get; }
    public int DurationMinutes { get; }
    public Movie(string movieId, string title, string genre, int durationMinutes)
    {
        MovieId = movieId;
        Title = title;
        Genre = genre;
        DurationMinutes = durationMinutes;
    }
}
class Seat
{
    public string SeatId { get;  }
    public string SeatNumber { get;  }
    public SeatType Type { get;  }
    public decimal Price { get; }
    public SeatStatus Status { get; private set; }
    public string LockedBy { get; private set; }
    public DateTime? LockExpiryTime { get; private set; }
    public  readonly object LockObj = new object();
    public Seat(string seatId, string seatNumber, SeatType type, decimal price)
    {
        SeatId = seatId;
        SeatNumber = seatNumber;
        Type = type;
        Price = price;
    }
    public bool IsAvailable()
    {
        if (Status == SeatStatus.Booked) return false;
        if(Status == SeatStatus.Locked && LockExpiryTime < DateTime.UtcNow)
        {
            ReleaseLock();
            return true;
        }
        return Status == SeatStatus.Available;
    }
    public bool TryLock(string userId , int minutes = 10)
    {
        lock(LockObj)
        {
            if (!IsAvailable()) return false;
            Status = SeatStatus.Locked;
            LockedBy = userId;
            LockExpiryTime = DateTime.UtcNow;
            return true;
        }
    }
    public void ReleaseLock()
    {
        Status = SeatStatus.Available;
        LockedBy = null;
        LockExpiryTime = null;
    }
    public void MarkBooked()
    {
        Status = SeatStatus.Booked;
        LockExpiryTime = null;
    }
}
class Show
{
    public string ShowId { get; }
    public Movie Movie { get; }
    public Screen Screen { get; }
    public DateTime Starttime { get; }
    public decimal BasePrice { get; }
    public Show(string showId, Movie movie, Screen screen, DateTime starttime, decimal basePrice)
    {
        ShowId = showId;
        Movie = movie;
        Screen = screen;
        Starttime = starttime;
        BasePrice = basePrice;
    }
    public List<Seat> GetAvailableSeats() => Screen.Seats.Where(s => s.IsAvailable()).ToList();
}
class Screen
{
    public string ScreenId { get; }
    public string ScreenName { get; }
    public List<Seat> Seats { get; } = new List<Seat>();
    public List<Show> Shows { get; } = new List<Show>();
    public Screen(string screenId, string screenName)
    {
        ScreenId = screenId;
        ScreenName = screenName;
    }
    public void AddSeat(Seat seat) => Seats.Add(seat);
    public void AddShow(Show show) => Shows.Add(show);
}
class Theatre
{
    public string TheatreId { get; }
    public string Name { get; }
    public string City { get; }
    public List<Screen> Screens { get; } = new List<Screen>();
    public Theatre(string theatreId, string name, string city)
    {
        TheatreId = theatreId;
        Name = name;
        City = city;
    }
    public void AddScreen(Screen screen) => Screens.Add(screen);
    public List<Show> GetShowsForMovie(string movieId) => Screens.SelectMany(s => s.Shows ).Where(s=> s.Movie.MovieId == movieId).ToList();
}
class Payment
{
    public string PaymentId { get; }
    public decimal Amount { get; }
    public bool IsSuccess { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public Payment(string paymentId, decimal amount)
    {
        PaymentId = paymentId;
        Amount = amount;
    }
    public void MarkSuccess()
    {
        IsSuccess = true;
        PaidAt = DateTime.Now;
    }
    public void MarkFailed() => IsSuccess = false;
}

/// <summary>
/// Interface strategy for payment
/// </summary>

interface IPaymentStrategy
{
    bool Pay(decimal amount);
}
class CreditCardPayment : IPaymentStrategy
{
    public bool Pay(decimal amount)
    {
        Console.WriteLine($"  💳 Processing Credit Card payment of ₹{amount}...");
        return true;
    }
}
class UPIPayment : IPaymentStrategy
{
    public bool Pay(decimal amount)
    {
        Console.WriteLine($"  📱 Processing UPI payment of ₹{amount}...");
        return true;
    }
}
class DebitCardPayment : IPaymentStrategy
{
    public bool Pay(decimal amount)
    {
        Console.WriteLine($"  💳 Processing Debit Card payment of ₹{amount}...");
        return true;
    }
}

class Booking
{
    public string BookingId { get; }
    public User User { get; }
    public Show Show { get; }
    public List<Seat> Seats { get; }
    public Payment Payment { get; private set; }
    public BookingStatus Status { get; private set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; } = DateTime.Now;
    public decimal TotalAmount => Seats.Sum(s => s.Price);
    public Booking(string bookingId, User user, Show show, List<Seat> seats)
    {
        BookingId = bookingId;
        User = user;
        Show = show;
        Seats = seats;
    }
    public void Confirm(Payment payment)
    {
        Payment = payment;
        Status = BookingStatus.Confirmed;
    }
    public void Cancel() { Status = BookingStatus.Cancelled; }
    public void Fail() => Status = BookingStatus.Failed;
}
class NotificationService
{
    public void SendConfirmation(Booking booking)
    {
        Console.WriteLine($"\n  📧 Notification sent to {booking.User.Email}:");
        Console.WriteLine($"     Booking {booking.BookingId} Confirmed!");
        Console.WriteLine($"     Movie: {booking.Show.Movie.Title}");
        Console.WriteLine($"     Time: {booking.Show.StartTime}");
        Console.WriteLine($"     Seats: {string.Join(", ", booking.Seats.Select(s => s.SeatNumber))}");
        Console.WriteLine($"     Amount Paid: ₹{booking.TotalAmount}");
    }

    public void SendCancellation(Booking booking)
    {
        Console.WriteLine($"\n  📧 Cancellation notification sent to {booking.User.Email}");
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("AJAJ");
    }

}