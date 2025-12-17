using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Models
{
    public class ParkingFloor
    {
        public int Id { get; set; }
        public int FloorNumber { get; set; }
        public List<ParkingSlot> Slots { get; set; }
    }
}
