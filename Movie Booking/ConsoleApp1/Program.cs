
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

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
class Program
{
    static void Main()
    {
        Console.WriteLine("AJAJ");
    }

}