using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

public enum ElevatorState
{
    Idle,
    MovingUp,
    MovingDown,
    Stopped
}

public enum Direction
{
    Up,
    Down
}

public enum DoorState
{
    Open,
    Close
}

public class ExternalRequest
{
    public int FloorNumber { get; }
    public Direction Direction { get; }

    public ExternalRequest(int number, Direction direction)
    {
        this.FloorNumber = number;
        this.Direction = direction;
    }
}

public class InternalRequest
{
    public int FloorNumber { get; }

    public InternalRequest(int number)
    {
        this.FloorNumber = number;
    }
}

public class Elevator
{
    public int ElevatorNumber { get; set; }
    private SortedSet<int> _upQueue;
    private SortedSet<int> _downQueue;
    public Direction Direction { get; private set; }
    public ElevatorState ElevatorState { get; private set; }
    public DoorState DoorState { get; private set; }
    public int CurrentFloorNumber { get; private set; }
    public int Capacity { get; }
    public int CurrentPeopleCount { get; private set; }

    public Elevator(int capacity, int elevatorNumber)
    {
        this._upQueue = new SortedSet<int>();
        this._downQueue = new SortedSet<int>(
            Comparer<int>.Create((a, b) => b.CompareTo(a))
        );
        this.Capacity = capacity;
        this.CurrentFloorNumber = 0;
        this.ElevatorState = ElevatorState.Idle;
        this.DoorState = DoorState.Close;
        this.Direction = Direction.Up;
        this.ElevatorNumber = elevatorNumber;
        this.CurrentPeopleCount = 0;
    }

    public ElevatorState GetCurrentState()
    {
        return this.ElevatorState;
    }

    public int GetCurrentFloor()
    {
        return this.CurrentFloorNumber;
    }

    public bool TryBoard(int numberOfPeople)
    {
        if (CurrentPeopleCount + numberOfPeople > Capacity)
        {
            Console.WriteLine($"Elevator {ElevatorNumber} is full!");
            return false;
        }
        CurrentPeopleCount += numberOfPeople;
        return true;
    }

    public void Exit(int numberOfPeople)
    {
        CurrentPeopleCount = Math.Max(0, CurrentPeopleCount - numberOfPeople);
    }

    public void OpenDoor()
    {
        this.DoorState = DoorState.Open;
    }

    public void CloseDoor()
    {
        this.DoorState = DoorState.Close;
    }

    public void AddRequest(int selectFloorNumber)
    {
        if (selectFloorNumber > CurrentFloorNumber)
            this._upQueue.Add(selectFloorNumber);
        else if (selectFloorNumber < CurrentFloorNumber)
            this._downQueue.Add(selectFloorNumber);
        else
            throw new Exception("Already on the requested floor");
    }

    public void ProcessQueue()
    {
        while (_upQueue.Count > 0)
        {
            int nextFloor = _upQueue.Min;
            _upQueue.Remove(nextFloor);
            MoveTo(nextFloor);
        }

        while (_downQueue.Count > 0)
        {
            int nextFloor = _downQueue.Max;
            _downQueue.Remove(nextFloor);
            MoveTo(nextFloor);
        }

        this.ElevatorState = ElevatorState.Idle;
        Console.WriteLine($"Elevator {ElevatorNumber}: All requests processed. Elevator is Idle now.");
    }

    public void MoveTo(int floor)
    {
        CloseDoor();

        if (floor > this.CurrentFloorNumber)
        {
            this.ElevatorState = ElevatorState.MovingUp;
            this.Direction = Direction.Up;
        }
        else
        {
            this.ElevatorState = ElevatorState.MovingDown;
            this.Direction = Direction.Down;
        }

        Console.WriteLine($"Elevator {ElevatorNumber}: Moving from {CurrentFloorNumber} to {floor}");

        this.CurrentFloorNumber = floor;
        this.ElevatorState = ElevatorState.Stopped;

        OpenDoor();
        Console.WriteLine($"Elevator {ElevatorNumber}: Arrived at floor {CurrentFloorNumber}. Door Open.");
    }
}

public class ElevatorController
{
    private List<Elevator> _elevators;

    public ElevatorController(List<Elevator> elevators)
    {
        this._elevators = elevators;
    }

    public void RequestElevator(int floor, Direction direction)
    {
        Elevator nearestElevator = FindNearestElevator(floor, direction);
        if (nearestElevator == null)
        {
            Console.WriteLine("No elevator available!");
            return;
        }
        AssignElevator(nearestElevator, floor);
    }

    private Elevator FindNearestElevator(int floor, Direction direction)
    {
        int minDist = int.MaxValue;
        Elevator nearestElevator = null;

        foreach (Elevator elevator in _elevators)
        {
            int distance = Math.Abs(floor - elevator.CurrentFloorNumber);

            if (elevator.ElevatorState == ElevatorState.MovingUp
                && direction == Direction.Up
                && floor > elevator.CurrentFloorNumber
                && distance < minDist)
            {
                minDist = distance;
                nearestElevator = elevator;
            }
            else if (elevator.ElevatorState == ElevatorState.MovingDown
                && direction == Direction.Down
                && floor < elevator.CurrentFloorNumber
                && distance < minDist)
            {
                minDist = distance;
                nearestElevator = elevator;
            }
            else if (elevator.ElevatorState == ElevatorState.Idle
                && distance < minDist)
            {
                minDist = distance;
                nearestElevator = elevator;
            }
        }

        return nearestElevator;
    }

    private void AssignElevator(Elevator elevator, int floor)
    {
        Console.WriteLine($"Elevator {elevator.ElevatorNumber} assigned to floor {floor}");
        elevator.AddRequest(floor);
        elevator.ProcessQueue();
    }
}

public class ElevatorSystem
{
    public static void Main(string[] args)
    {
        Elevator elevator1 = new Elevator(10, 1);
        Elevator elevator2 = new Elevator(10, 2);
        Elevator elevator3 = new Elevator(10, 3);

        List<Elevator> elevators = new List<Elevator> { elevator1, elevator2, elevator3 };

        ElevatorController controller = new ElevatorController(elevators);

        ExternalRequest er1 = new ExternalRequest(3, Direction.Up);
        ExternalRequest er2 = new ExternalRequest(5, Direction.Up);
        ExternalRequest er3 = new ExternalRequest(2, Direction.Down);

        InternalRequest ir1 = new InternalRequest(4);
        InternalRequest ir2 = new InternalRequest(6);
        InternalRequest ir3 = new InternalRequest(1);

        Console.WriteLine("--- External Requests ---");
        controller.RequestElevator(er1.FloorNumber, er1.Direction);
        controller.RequestElevator(er2.FloorNumber, er2.Direction);
        controller.RequestElevator(er3.FloorNumber, er3.Direction);

        Console.WriteLine("\n--- Internal Requests (inside elevator1) ---");
        elevator1.AddRequest(ir1.FloorNumber);
        elevator1.AddRequest(ir2.FloorNumber);
        elevator1.AddRequest(ir3.FloorNumber);
        elevator1.ProcessQueue();
    }
}