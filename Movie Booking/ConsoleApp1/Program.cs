
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

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
        Console.WriteLine($"     Time: {booking.Show.Starttime}");
        Console.WriteLine($"     Seats: {string.Join(", ", booking.Seats.Select(s => s.SeatNumber))}");
        Console.WriteLine($"     Amount Paid: ₹{booking.TotalAmount}");
    }

    public void SendCancellation(Booking booking)
    {
        Console.WriteLine($"\n  📧 Cancellation notification sent to {booking.User.Email}");
    }
}
class BookingService
{
    private readonly NotificationService notificationService;
    private int bookingCounter = 1;
    private int paymentCounter = 1;
    public BookingService(NotificationService notificationService)
    {
        this.notificationService = notificationService;
    }
    public Booking BookSeats(User user, Show show, List<Seat> selectedSeats, IPaymentStrategy paymentStrategy)
    {
        Console.WriteLine($"\n🎬 Starting booking for {user.Name}...");
        var lockedSeats = new List<Seat>();
        foreach(var seat in selectedSeats)
        {
            if(!seat.TryLock(user.UserId))
            {
                Console.WriteLine($"  ❌ Seat {seat.SeatNumber} not available. Releasing locked seats...");
                lockedSeats.ForEach(s => s.ReleaseLock());
                return null;
            }
            lockedSeats.Add(seat);
            Console.WriteLine($"  🔒 Seat {seat.SeatNumber} locked for {user.Name}");
        }
        
        var booking = new Booking($"BK{bookingCounter++}", user, show, selectedSeats);
        Console.WriteLine($"  📋 Booking {booking.BookingId} created with status: Pending");
        Console.WriteLine($"  💰 Total Amount: ₹{booking.TotalAmount}");

        var payment = new Payment($"PAY{paymentCounter++}", booking.TotalAmount);
        bool paymentSuccess = paymentStrategy.Pay(booking.TotalAmount);

        if (paymentSuccess)
        {
            // Step 4: Confirm booking
            payment.MarkSuccess();
            booking.Confirm(payment);
            selectedSeats.ForEach(s => s.MarkBooked());
            Console.WriteLine($"  ✅ Payment successful! Booking Confirmed.");
            notificationService.SendConfirmation(booking);
        }
        else
        {
            // Step 5: Fail and release
            payment.MarkFailed();
            booking.Fail();
            selectedSeats.ForEach(s => s.ReleaseLock());
            Console.WriteLine($"  ❌ Payment failed. Seats released.");
        }

        return booking;
    }
    public void CancelBooking(Booking booking)
    {
        if (booking.Status != BookingStatus.Confirmed)
        {
            Console.WriteLine("  ⚠️ Only confirmed bookings can be cancelled.");
            return;
        }
        var hoursBefore = (booking.Show.Starttime - DateTime.UtcNow).TotalHours;
        booking.Cancel();
        booking.Seats.ForEach(s => s.ReleaseLock());
        if (hoursBefore >= 24)
            Console.WriteLine($"  ✅ Booking {booking.BookingId} cancelled. Full refund processed.");
        else
            Console.WriteLine($"  ✅ Booking {booking.BookingId} cancelled. No refund (less than 24hrs).");

        notificationService.SendCancellation(booking);
    }
}

class Program
{
    static void Main()
    {
        var movie = new Movie("M1", "Pushpa 2", "Action", 180);
        var theatre = new Theatre("T1", "PVR Cinemas", "Hyderabad");

        var screen = new Screen("sc1", "Screen 1");
        screen.AddSeat(new Seat("s1", "A1", SeatType.Gold, 250));
        screen.AddSeat(new Seat("S2", "A2", SeatType.Gold, 250));
        screen.AddSeat(new Seat("S3", "A3", SeatType.Platinum, 400));
        screen.AddSeat(new Seat("S4", "A4", SeatType.Silver, 150));
        theatre.AddScreen(screen);

        var show = new Show("SH1", movie, screen, DateTime.UtcNow.AddHours(5), 250);
        screen.AddShow(show);
        var user1 = new User("U1", "Ajaj", "ajaj@email.com", "9999999999");
        var user2 = new User("U2", "Rahul", "rahul@email.com", "8888888888");

        var notificationService = new NotificationService();
        var bookingService = new BookingService(notificationService);

        Console.WriteLine("========================================"); 
        Console.WriteLine("   MOVIE TICKET BOOKING SYSTEM");
        Console.WriteLine("========================================");


        Console.WriteLine($"\n🎭 Available seats for {movie.Title}:");
        show.GetAvailableSeats().ForEach(s =>
            Console.WriteLine($"   Seat {s.SeatNumber} | {s.Type} | ₹{s.Price}"));

        Console.WriteLine("\n--- User 1 (Ajaj) booking A1, A2 ---");
        var seatsForUser1 = new List<Seat> { screen.Seats[0], screen.Seats[1] };
        var booking1 = bookingService.BookSeats(user1, show, seatsForUser1, new UPIPayment());

        Console.WriteLine("\n--- User 2 (Rahul) trying A1 (taken) + A3 ---");
        var seatsForUser2 = new List<Seat> { screen.Seats[0], screen.Seats[2] };
        var booking2 = bookingService.BookSeats(user2, show, seatsForUser2, new CreditCardPayment());


        Console.WriteLine("\n--- User 2 (Rahul) booking only A3 ---");
        var seatsForUser2Retry = new List<Seat> { screen.Seats[2] };
        var booking3 = bookingService.BookSeats(user2, show, seatsForUser2Retry, new CreditCardPayment());

        Console.WriteLine($"\n🎭 Remaining available seats:");
        var remaining = show.GetAvailableSeats();
        if (remaining.Any())
            remaining.ForEach(s => Console.WriteLine($"   Seat {s.SeatNumber} | {s.Type} | ₹{s.Price}"));
        else
            Console.WriteLine("   No seats available.");

        Console.WriteLine("\n--- Ajaj cancels his booking ---");
        bookingService.CancelBooking(booking1);

        Console.WriteLine($"\n🎭 Available seats after cancellation:");
        show.GetAvailableSeats().ForEach(s =>
            Console.WriteLine($"   Seat {s.SeatNumber} | {s.Type} | ₹{s.Price}"));
    }

}