using ConsoleApp1.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.ParkingLot
{
    public class Vehicle
    {
        public string VehicleNumber { get; set; }
        public VehicleType VehicleType { get; set; }
        public Vehicle(string VehicleNumber,  string vehicleType)
        {
            this.VehicleNumber = VehicleNumber;
            if (vehicleType == "MotorCycle") this.VehicleType = VehicleType.MotorCycle;
            if (vehicleType == "Car") this.VehicleType = VehicleType.Car;
            if (vehicleType == "Truck") this.VehicleType = VehicleType.Truck;
        }
    }
}
