namespace MoreDoors.Rando
{
    public enum DoorsLevel
    {
        Doors,
        MoreDoors,
        AllDoors
    }

    public class MoreDoorsSettings
    {
        public bool AddMoreDoors = false;
        public DoorsLevel DoorsLevel = DoorsLevel.MoreDoors;
        public bool AddKeyLocations = false;
    }
}
