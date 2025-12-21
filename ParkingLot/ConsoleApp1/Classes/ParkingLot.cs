using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.ParkingLot
{
    public class ParkingLot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public List<ParkingFloor> Floor { get; set; }
        public Dictionary<int, Ticket> ActiveTickets { get; set; }

        public ParkingLot(int id, string name, string address, List<ParkingFloor> floors)
        {
            Id = id;
            Name = name;
            Address = address;
            Floor = floors;
            ActiveTickets = new Dictionary<int, Ticket>();  
        }
        public void InitializeSpots(List<ParkingFloor> floors)
        {
            foreach (ParkingFloor floor in Floor)
            {
                foreach (ParkingSlot slot in floor.Slots)
                {
                    slot.isAvailable = true;
                }
            }
        }
        public Ticket ParkVehicle(Vehicle vehicle, int ticketId )
        {
            
            foreach (ParkingFloor floor in Floor)
            {
                foreach (ParkingSlot slot in floor.Slots)
                {
                    if (slot.isAvailable)
                    {
                        if (slot.IsValidVehicle(vehicle))
                        {
                            Ticket t = new Ticket();
                            slot.ParkVehicle(vehicle);
                            t.EntryTime = DateTime.UtcNow;
                            t.Vehicle = vehicle;
                            t.Slot = slot;
                            Console.WriteLine("Hello Parking ", ticketId);
                            ActiveTickets.Add(ticketId, t);
                            break;
                        }
                    }
                }
            }
            return new Ticket();
        }
        public Ticket UnparkVehicle(int ticketId)
        {

            Console.WriteLine("Hello UnParking ", ticketId);
            Ticket ticket = new Ticket();   
            if (!this.ActiveTickets.ContainsKey(ticketId))
            {
                return ticket;
            }
            ticket  = this.ActiveTickets[ticketId];
            Payment payment = new Payment();
            payment.Ticket = ticket;
            ticket.CloseTicket();
            ParkingSlot slot = ticket.Slot;
            slot.UnParkVehicle();
            ActiveTickets.Remove(ticketId);
            return ticket;

        }
    }
}
