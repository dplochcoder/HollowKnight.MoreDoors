namespace MoreDoors.Rando
{
    public enum DoorsLevel
    {
        Doors,
        Doorser,
        Doorsest
    }

    public class MoreDoorsSettings
    {
        public bool AddMoreDoors = false;
        public DoorsLevel DoorsLevel = DoorsLevel.Doorser;
    }
}
