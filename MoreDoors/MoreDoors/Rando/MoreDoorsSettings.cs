namespace MoreDoors.Rando
{
    public enum DoorsLevel
    {
        Doors,
        MoreDoors,
        MostDoors
    }

    public class MoreDoorsSettings
    {
        public bool AddMoreDoors = false;
        public DoorsLevel DoorsLevel = DoorsLevel.MoreDoors;
        public bool AddKeyLocations = false;
    }
}
