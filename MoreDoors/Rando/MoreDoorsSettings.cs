using MoreDoors.Data;
using Newtonsoft.Json;
using RandomizerCore.Extensions;
using System;
using System.Collections.Generic;

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

    public class RandomizationSettings
    {
        public DoorsLevel DoorsLevel = DoorsLevel.NoDoors;
        public AddKeyLocations AddKeyLocations = AddKeyLocations.None;

        [JsonIgnore]
        public bool IsEnabled => DoorsLevel != DoorsLevel.NoDoors || AddKeyLocations == AddKeyLocations.AllDoors;

        public HashSet<string> ComputeActiveDoors(Random r)
        {
            List<string> potentialDoors = new(DoorData.DoorNames);
            HashSet<string> doors = new();
            int modifier;
            switch (DoorsLevel)
            {
                case DoorsLevel.NoDoors:
                    return doors;
                case DoorsLevel.SomeDoors:
                    modifier = 1;
                    break;
                case DoorsLevel.MoreDoors:
                    modifier = 2;
                    break;
                case DoorsLevel.AllDoors:
                    potentialDoors.ForEach(d => doors.Add(d));
                    return doors;
                default:
                    throw new ArgumentException($"Unknown DoorsLevel: {DoorsLevel}");
            }

            int mid = potentialDoors.Count * modifier / 3;
            int numDoors = mid - modifier + r.Next(0, modifier * 2 + 1);
            potentialDoors.Shuffle(r);
            for (int i = 0; i < numDoors; i++) doors.Add(potentialDoors[i]);

            return doors;
        }
    }
}
