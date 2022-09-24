using MoreDoors.Data;
using Newtonsoft.Json;
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

        [JsonIgnore]
        public bool IsEnabled => DoorsLevel != DoorsLevel.NoDoors || AddKeyLocations == AddKeyLocations.AllDoors;

        public int ComputeNumDoors(Random r)
        {
            int num, denom, range;
            switch (DoorsLevel)
            {
                case DoorsLevel.NoDoors:
                    return 0;
                case DoorsLevel.SomeDoors:
                    num = 1;
                    denom = 3;
                    range = 1;
                    break;
                case DoorsLevel.MoreDoors:
                    num = 2;
                    denom = 3;
                    range = 2;
                    break;
                case DoorsLevel.AllDoors:
                    return DoorData.Count;
                default:
                    throw new ArgumentException($"Unknown DoorsLevel: {DoorsLevel}");
            }

            int mid = DoorData.Count * num / denom;
            int min = Math.Max(1, mid - range);
            int max = Math.Min(DoorData.Count, mid + range);
            return min + r.Next(0, max - min + 1);
        }
    }
}
