using System;

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

        public int ComputeNumDoors(Random r)
        {
            int min, max;
            switch (DoorsLevel)
            {
                case DoorsLevel.Doors:
                    min = 5;
                    max = 7;
                    break;
                case DoorsLevel.MoreDoors:
                    min = 10;
                    max = 14;
                    break;
                case DoorsLevel.AllDoors:
                    min = 24;
                    max = 24;
                    break;
                default:
                    throw new ArgumentException($"Unknown DoorsLevel: {DoorsLevel}");
            }

            return min + r.Next(0, max - min + 1);
        }
    }
}
