using System;

namespace MoreDoors.Rando
{
    public enum DoorsLevel
    {
        SomeDoors,
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
                case DoorsLevel.SomeDoors:
                    min = 6;
                    max = 8;
                    break;
                case DoorsLevel.MoreDoors:
                    min = 12;
                    max = 16;
                    break;
                case DoorsLevel.AllDoors:
                    min = 28;
                    max = 28;
                    break;
                default:
                    throw new ArgumentException($"Unknown DoorsLevel: {DoorsLevel}");
            }

            return min + r.Next(0, max - min + 1);
        }
    }
}
