﻿using MoreDoors.Data;
using Newtonsoft.Json;
using PurenailCore.SystemUtil;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoreDoors.Rando;

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
    public bool RandomizeDoorTransitions = false;
    public AddKeyLocations AddKeyLocations = AddKeyLocations.None;
    public SortedSet<string> DisabledDoors = [];

    [JsonIgnore]
    public bool IsEnabled => DisabledDoors.Count < DoorData.All().Count && (DoorsLevel != DoorsLevel.NoDoors || AddKeyLocations == AddKeyLocations.AllDoors);

    public bool IsDoorEnabled(string door) => !DisabledDoors.Contains(door);

    public void SetDoorEnabled(string door, bool value)
    {
        if (value) DisabledDoors.Remove(door);
        else DisabledDoors.Add(door);
    }

    public void MaybeUpdateEnabledDoors() => DisabledDoors.RemoveWhere(d => !DoorData.All().ContainsKey(d));
}

public static class RandomizationSettingsExtensions
{
    public static HashSet<string> ComputeActiveDoors(this RandomizationSettings settings, GenerationSettings gs, Random r)
    {
        List<string> potentialDoors = [.. DoorData.All().Keys.Where(d => !settings.DisabledDoors.Contains(d))];
        if (gs.LongLocationSettings.WhitePalaceRando != LongLocationSettings.WPSetting.Allowed) potentialDoors.Remove("Pain");

        HashSet<string> doors = [];
        if (potentialDoors.Count == 0) return doors;

        int modifier;
        switch (settings.DoorsLevel)
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
                throw new ArgumentException($"Unknown DoorsLevel: {settings.DoorsLevel}");
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
