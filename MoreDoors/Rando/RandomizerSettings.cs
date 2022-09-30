using MenuChanger.Attributes;
using MoreDoors.Data;
using Newtonsoft.Json;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static readonly int FullDoorsMask = FullMask(DoorData.Count);
        [MenuIgnore]
        public int DoorsMask = FullDoorsMask;

        [JsonIgnore]
        public bool IsEnabled => DoorsMask != 0 && (DoorsLevel != DoorsLevel.NoDoors || AddKeyLocations == AddKeyLocations.AllDoors);

        public bool IsDoorAllowed(int index) => (DoorsMask & (1 << index)) != 0;

        public void SetDoorAllowed(int index, bool value) => DoorsMask = (DoorsMask & ~(1 << index)) | (value ? (1 << index) : 0);

        private static int FullMask(int n)
        {
            if (n > 31) throw new ArgumentException("Too many doors =<");

            int o = 0;
            for (int i = 0; i < n; i++) o |= (1 << i);
            return o;
        }

        public HashSet<string> ComputeActiveDoors(GenerationSettings gs, Random r)
        {
            List<string> potentialDoors = Enumerable.Range(0, DoorData.Count)
                .Where(i => IsDoorAllowed(i))
                .Select(i => DoorData.DoorNames[i]).ToList();
            if (gs.LongLocationSettings.WhitePalaceRando != LongLocationSettings.WPSetting.Allowed) potentialDoors.Remove("Pain");

            HashSet<string> doors = new();
            if (potentialDoors.Count == 0) return doors;

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

            // Clamp to at least one door.
            if (numDoors > potentialDoors.Count - 1) numDoors = potentialDoors.Count - 1;
            if (numDoors < 1) numDoors = 1;

            potentialDoors.Shuffle(r);
            for (int i = 0; i < numDoors; i++) doors.Add(potentialDoors[i]);

            return doors;
        }
    }
}
