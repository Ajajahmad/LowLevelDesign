using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.ParkingLot
{
    public class Payment
    {
        public int Id { get; set; }
        public Ticket Ticket { get; set; }
        public decimal Amount { get; set; }
        //public PaymentMethod PaymentMethod { get; set; }
        public DateTime PaymentTime { get; set; }

        public bool ProcessPayment()
        {
            TimeSpan timespan = this.Ticket.GetParkingDuration();
            this.Amount = (decimal)(timespan.TotalHours) * 50;
            this.PaymentTime = DateTime.Now;
            if (Amount == 0) return false;
            Console.Write("Total Payment is  -> " , this.PaymentTime);
            return true;
        }
    }
}
