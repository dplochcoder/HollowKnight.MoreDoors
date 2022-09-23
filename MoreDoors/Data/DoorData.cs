﻿using ItemChanger;
using ItemChanger.Locations;
using MoreDoors.IC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MoreDoors.Data
{
    public record DoorData
    {
        public static readonly SortedDictionary<string, DoorData> Data = JsonUtil.Deserialize<SortedDictionary<string, DoorData>>("MoreDoors.Resources.Data.doors.json");

        public static DoorData Get(string doorName) => Data[doorName];

        public static IEnumerable<string> DoorNames => Data.Keys;

        public static int Count => Data.Count;

        public static void Load()
        {
            foreach (var doorName in DoorNames)
            {
                Finder.DefineCustomItem(new KeyItem(doorName));
                Finder.DefineCustomLocation(Get(doorName).Key.VanillaLocation);
            }

            MoreDoors.Log("Loaded Doors");
        }

        public record DoorLocation
        {
            public string SceneName;
            public string GateName;
            public bool RequiresLantern;
            public float X;
            public float Y;

            [JsonIgnore]
            public string TransitionName => $"{SceneName}[{GateName}]";

            [JsonIgnore]
            public string TransitionProxyName => $"{SceneName}_Proxy[{GateName}]";
        }

        public string CamelCaseName;
        public string UpperCaseName;
        public string Sprite;
        public string NoKeyDesc;
        public string KeyDesc;
        public DoorLocation LeftDoorLocation;
        public DoorLocation RightDoorLocation;
        // TODO: Custom door sprites and colors

        public record KeyInfo
        {

            public string ItemName;
            public string UIItemName;
            public string ShopDesc;
            public string Sprite;
            public AbstractLocation VanillaLocation;
            public string VanillaLogic;

            public record MapCoords
            {
                public float X;
                public float Y;
            }
            public MapCoords? VanillaMapCoordsOverride;

            [JsonIgnore]
            public (float x, float y) Coords
            {
                get
                {
                    if (VanillaMapCoordsOverride != null) return (VanillaMapCoordsOverride.X, VanillaMapCoordsOverride.Y);
                    if (VanillaLocation is DualLocation dl && dl.trueLocation is CoordinateLocation cl) return (cl.x, cl.y);
                    throw new ArgumentException($"Key {ItemName} is missing map coords");
                }
            }
        }
        public KeyInfo Key;

        [JsonIgnore]
        public string PDDoorOpenedName => $"moreDoors{CamelCaseName}Key";

        [JsonIgnore]
        public string PDKeyName => $"moreDoors{CamelCaseName}DoorOpened";

        [JsonIgnore]
        public string KeyTermName => $"MOREDOORS_{UpperCaseName}_KEY";

        [JsonIgnore]
        public string DoorForcedOpenLogicName => $"{CamelCaseName}Door_ForcedOpen";

        [JsonIgnore]
        public string KeyLocationName => Key.VanillaLocation.name;

        [JsonIgnore]
        public string NoKeyPromptId => $"MOREDOORS_{UpperCaseName}_DOOR_NOKEY";

        [JsonIgnore]
        public string KeyPromptId => $"MOREDOORS_{UpperCaseName}_DOOR_KEY";
    }
}
