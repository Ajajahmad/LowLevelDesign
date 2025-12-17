using ConsoleApp1.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Models
{
    public class ParkingSlot
    {
        public int id { get; set; }
        public SlotType slotType { get; set; }
        public bool isAvailable { get; set; }
        public Vehicle vehicle { get; set; }    
    }
}
