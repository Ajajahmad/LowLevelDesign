// See https://aka.ms/new-console-template for more information
using ConsoleApp1.ParkingLot;

Console.WriteLine("Hello, World!");

ParkingSlot Slot1 = new ParkingSlot(1, "Small", true);
ParkingSlot Slot2 = new ParkingSlot(1, "Medium", true);
ParkingSlot Slot3 = new ParkingSlot(1, "Large", true);

List<ParkingFloor> ParkingFloors= new List<ParkingFloor>();
ParkingFloors.Add(new ParkingFloor
{
    FloorNumber = 100,
    Slots = new List<ParkingSlot> { Slot1 }
});

ParkingFloors.Add(new ParkingFloor
{
    FloorNumber = 200,
    Slots = new List<ParkingSlot> { Slot2, Slot3 }
});

ParkingLot parkingLot = new ParkingLot(1,"AnsariParking", "Main Bazar, Lucknow", ParkingFloors);

Vehicle vehicle1 = new Vehicle("UP44 1232", "MotorCycle");
Vehicle vehicle2 = new Vehicle("UP44 1233", "Car");
Vehicle vehicle3 = new Vehicle("UP44 1234", "Truck");

Ticket ticket1 =  parkingLot.ParkVehicle(vehicle1,1);
Ticket ticket2 =  parkingLot.ParkVehicle(vehicle2,2);
Ticket ticket3 =  parkingLot.ParkVehicle(vehicle3,3);

parkingLot.UnparkVehicle(ticket1.Id);
parkingLot.UnparkVehicle(ticket2.Id);
parkingLot.UnparkVehicle(ticket3.Id);