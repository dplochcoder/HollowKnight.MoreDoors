using MoreDoors.IC;
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
            int dividend, range;
            switch (DoorsLevel)
            {
                case DoorsLevel.SomeDoors:
                    dividend = 4;
                    range = 1;
                    break;
                case DoorsLevel.MoreDoors:
                    dividend = 2;
                    range = 2;
                    break;
                case DoorsLevel.AllDoors:
                    dividend = 1;
                    range = 0;
                    break;
                default:
                    throw new ArgumentException($"Unknown DoorsLevel: {DoorsLevel}");
            }

            int min = Math.Max(1, (DoorData.Count / dividend) - range);
            int max = Math.Min(DoorData.Count, (DoorData.Count / dividend) + range);
            return min + r.Next(0, max - min + 1);
        }
    }
}
