using MoreDoors.Data;
using System;

namespace MoreDoors.Rando
{
    public enum DoorsLevel
    {
        NoDoors,
        SomeDoors,
        MoreDoors,
        AllDoors
    }

    public enum AddKeyLocations
    {
        None,
        MatchingDoors,
        AllDoors
    }

    public class MoreDoorsSettings
    {
        public DoorsLevel DoorsLevel = DoorsLevel.NoDoors;
        public AddKeyLocations AddKeyLocations = AddKeyLocations.None;

        public int ComputeNumDoors(Random r)
        {
            int dividend, range;
            switch (DoorsLevel)
            {
                case DoorsLevel.NoDoors:
                    return 0;
                case DoorsLevel.SomeDoors:
                    dividend = 4;
                    range = 1;
                    break;
                case DoorsLevel.MoreDoors:
                    dividend = 2;
                    range = 2;
                    break;
                case DoorsLevel.AllDoors:
                    return DoorData.Count;
                default:
                    throw new ArgumentException($"Unknown DoorsLevel: {DoorsLevel}");
            }

            int min = Math.Max(1, (DoorData.Count / dividend) - range);
            int max = Math.Min(DoorData.Count, (DoorData.Count / dividend) + range);
            return min + r.Next(0, max - min + 1);
        }
    }
}
