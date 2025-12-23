using Newtonsoft.Json;
using RandomizerCore.StringParsing;
using System;
using System.Collections.Generic;

namespace MoreDoors.Rando;

public class LocalSettings
{
    public RandomizationSettings Settings = MoreDoors.GS.RandoSettings;
    public HashSet<string> EnabledDoorNames = [];

    [JsonIgnore]
    public HashSet<string> ModifiedLogicNames = [];
    [JsonIgnore]
    public Dictionary<string, Token> LogicSubstitutions = [];

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
