                - PROBLEM STATEMENT -

Design a Parking Lot system which books vehicle based on the availability of the size of the vehicle and Charges for based on the parking time.

1. System Requirement
   a. What features does this system support?
   The Parking Lot should support:
   Parking a vehicle
   Unparking a vehicle
   Generating a parking ticket
   Calculating parking fees
   Payment at exit
   Showing spot availability
   No reservations, no online booking

   b. What vehicles are allowed?
   Motorcycle
   Car
   Truck

   c. How is payment handled?
   Payment is done at exit
   Fee is based on parking duration
   support cash/card/UPI payments(Optional)

   d. More info to make it more clear
   Parking lot can have multiple floors
   Each floor has multiple parking spots
   Spots come in 3 sizes: Small(Motorcycle), Medium(Car), Large(Truck)
   One vehicle occupies one spot
   Exit gate calculates fee and frees the spot
   Don't support valet, reservations, or monthly passes

2. Entities -
   Parking Lot
   Parking Floor
   Parking Slot
   Vehicle
   Ticket
   Payment

3. Relationship -
   Parking Lot HAS-A Parking Floor(Composition)
   Parking Floor HAS-A Parking Slot(Composition)
   Parking Slot HAS-A Vehicle(Aggregation)
   Ticket HAS-A Vehicle(Aggregation)
   Ticket HAS-A Payment(Composition)

4. Attributes
   ParkingLot:

   - Id
   - Name
   - Address(Optional)
   - List<ParkingFloor> Floors

   ParkingFloor:

   - Id
   - FloorNumber
   - List<ParkingSlot> Slots

   ParkingSlot:

   - Id
   - SlotType (Small/ Medium/ Large)
   - IsAvailable
   - Vehicle CurrentVehicle

   Vehicle:

   - VehicleNumber
   - VehicleType(Motorcycle / Car / Truck)

   Ticket:

   - Id
   - EntryTime
   - ExitTime
   - Vehicle Vehicle
   - ParkingSlot Slot

   Payment (Optional):

   - Id
   - Ticket Ticket
   - decimal Amount
   - PaymentMethod (cash/ card/ UPI)
   - PaymentTime
   - Status (Success/ Failed / Pending)
