using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.ParkingLot
{
    public class Ticket
    {
        public int Id { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime ExitTime { get; set; }
        public Vehicle Vehicle { get; set; }
        public ParkingSlot Slot { get; set; }

        public void CloseTicket()
        {
            Payment payment = new Payment();
            payment.ProcessPayment();
            this.ExitTime = DateTime.Now;
        }
        public TimeSpan GetParkingDuration()
        {
            return this.ExitTime - this.ExitTime;
        }

    }
}
