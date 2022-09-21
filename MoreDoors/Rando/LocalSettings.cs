using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MoreDoors.Rando
{
    public class LocalSettings
    {
        public MoreDoorsSettings Settings = MoreDoors.GS.MoreDoorsSettings;
        public HashSet<string> EnabledDoorNames = new();

        [JsonIgnore]
        public HashSet<string> ModifiedLogicNames = new();
        [JsonIgnore]
        public Dictionary<string, string> LogicSubstitutions = new();

        public bool IncludeDoor(string doorName) => EnabledDoorNames.Contains(doorName);

        public bool IncludeKeyLocation(string doorName)
        {
            return Settings.AddKeyLocations switch
            {
                AddKeyLocations.None => false,
                AddKeyLocations.MatchingDoors => IncludeDoor(doorName),
                AddKeyLocations.AllDoors => true,
                _ => throw new ArgumentException($"Unknown AddKeyLocations: {Settings.AddKeyLocations}"),
            };
        }
    }
}
