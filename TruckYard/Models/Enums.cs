namespace TruckYard.Models;

public enum TruckStatus
{
    Available,
    InYard,
    Loading,
    Unloading,
    InTransit,
    Maintenance
}

public enum MovementType
{
    Entry,
    Exit
}