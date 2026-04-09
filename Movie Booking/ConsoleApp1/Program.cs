
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
class Program
{
    static void Main()
    {
        Console.WriteLine("AJAJ");
    }

}