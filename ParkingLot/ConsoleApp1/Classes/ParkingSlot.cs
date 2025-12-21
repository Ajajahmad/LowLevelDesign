using ConsoleApp1.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.ParkingLot
{
    public class ParkingSlot
    {
        public int id { get; set; }
        public SlotType slotType { get; set; }
        public bool isAvailable { get; set; }
        public Vehicle vehicle { get; set; }

        public ParkingSlot(int id, string slotType, bool isAvailable)
        {
            this.id = id;
            if (slotType == "Small") this.slotType = SlotType.Small;
            else if (slotType == "Medium") this.slotType = SlotType.Medium;
            else if (slotType == "Large") this.slotType = SlotType.Large;
            this.isAvailable = isAvailable;
        }
        public bool IsValidVehicle(Vehicle vehicle)
        {
            if (vehicle.VehicleType == VehicleType.MotorCycle && slotType == SlotType.Small) return true;
            else if (vehicle.VehicleType == VehicleType.Car && slotType == SlotType.Medium) return true;
            if (vehicle.VehicleType == VehicleType.Truck && slotType == SlotType.Large) return true;
            else return false;
        }
        public bool ParkVehicle(Vehicle vehicle)
        {
            if (IsValidVehicle(vehicle))
            {
                this.vehicle = vehicle;
                isAvailable = false;
                return true;
            }
            return false;
        }
        public Vehicle UnParkVehicle()
        {
            isAvailable = false;
            Vehicle p = vehicle;
            vehicle = null;
            return p;
        }
    }
}
