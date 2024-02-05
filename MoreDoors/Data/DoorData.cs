using ItemChanger;
using ItemChanger.Locations;
using MoreDoors.IC;
using Newtonsoft.Json;
using RandoSettingsManager.SettingsManagement.Versioning;
using System;
using System.Collections.Generic;

using JsonUtil = PurenailCore.SystemUtil.JsonUtil<MoreDoors.MoreDoors>;

namespace MoreDoors.Data;

public record DoorData
{
    public static readonly SortedDictionary<string, DoorData> Data = JsonUtil.DeserializeEmbedded<SortedDictionary<string, DoorData>>("MoreDoors.Resources.Data.doors.json");

    public static DoorData GetFromJson(string doorName) => Data[doorName];

    public static DoorData GetFromModule(string doorName) => ItemChangerMod.Modules.Get<MoreDoorsModule>().DoorStates[doorName].Data;

    public static int Count => Data.Count;

    public static void Load()
    {
        foreach (var e in Data)
        {
            var doorName = e.Key;
            var data = e.Value;

            KeyItem key = new(doorName, data);
            key.AddLocationInteropTags(data);

            Finder.DefineCustomItem(key);
            Finder.DefineCustomLocation(data.Key.Location);
        }

        MoreDoors.Log("Loaded Doors");
    }

    public string CamelCaseName;
    public string UpperCaseName;
    public string UIName;

    public record DoorInfo
    {
        public enum SplitMode
        {
            Normal,
            LeftTwin,
            RightTwin,
        }
        private enum Side
        {
            Left,
            Right,
        }

        public record Location
        {
            public record LogicTransition
            {
                public string SceneName;
                public string GateName;

                [JsonIgnore]
                public string Name => $"{SceneName}[{GateName}]";
                [JsonIgnore]
                public string ProxyName => $"{SceneName}_Proxy[{GateName}]";
            }

            public record SecretMask
            {
                public float Width;
                public float Height;
            }

            public LogicTransition? Transition;
            public SecretMask? Mask;
            public bool RequiresLantern;
            public float X;
            public float Y;

            public bool ValidateAndUpdate(bool requireTransition, out string err)
            {
                if (requireTransition && Transition == null)
                {
                    err = "Door is missing Transition";
                    return false;
                }

                err = "";
                return true;
            }
        }

        public ISprite Sprite;
        public string NoKeyDesc;
        public string KeyDesc;
        // Location where the player looks left to the door.
        public Location LeftLocation;
        // Location where the player looks right to the door.
        public Location RightLocation;
        public SplitMode Mode;
        public List<IDeployer>? Deployers;

        private Location SplitLocation(Side side) => Mode switch
        {
            SplitMode.Normal => side == Side.Left ? LeftLocation : RightLocation,
            SplitMode.LeftTwin => LeftLocation,
            SplitMode.RightTwin => RightLocation,
        };

        public string LeftSceneName => SplitLocation(Side.Left).Transition.SceneName;
        public string RightSceneName => SplitLocation(Side.Right).Transition.SceneName;

        public bool ValidateAndUpdate(out string err)
        {
            if (!LeftLocation.ValidateAndUpdate(Mode != SplitMode.RightTwin, out err)) return false;
            if (!RightLocation.ValidateAndUpdate(Mode != SplitMode.LeftTwin, out err)) return false;

            err = "";
            return true;
        }
    }
    public DoorInfo Door;

    public record KeyInfo
    {
        public string ItemName;
        public string UIItemName;
        public string ShopDesc;
        public string InvDesc;
        public string UsedInvDesc;
        public ISprite Sprite;
        public AbstractLocation Location;
        public string Logic;

        public record WorldMapLocation
        {
            public string? SceneName;
            public float X;
            public float Y;

            [JsonIgnore]
            public (string, float, float) AsTuple => (SceneName, X, Y);
        }
        public WorldMapLocation? WorldMapLocationOverride;
        public List<WorldMapLocation>? ExtraWorldMapLocations = null;

        public List<WorldMapLocation> GetWorldMapLocations()
        {
            List<WorldMapLocation> locations = new();

            if (WorldMapLocationOverride != null)
            {
                WorldMapLocation first = WorldMapLocationOverride;
                first.SceneName ??= Location.sceneName;
                locations.Add(first);
            }
            else if (Location is DualLocation dl && dl.trueLocation is CoordinateLocation cl)
            {
                locations.Add(new()
                {
                    SceneName = Location.sceneName,
                    X = cl.x,
                    Y = cl.y
                });
            }
            else
            {
                throw new ArgumentException($"Key {ItemName} is missing world map location");
            }

            ExtraWorldMapLocations?.ForEach(l => locations.Add(l));
            return locations;
        }
    }
    public KeyInfo Key;

    [JsonIgnore]
    public string PDKeyName => $"moreDoors{CamelCaseName}Key";

    [JsonIgnore]
    public string PDDoorOpenedName => $"moreDoors{CamelCaseName}DoorOpened";

    [JsonIgnore]
    public string PDDoorLeftForceOpenedName => $"moreDoors{CamelCaseName}LeftForceOpened";

    [JsonIgnore]
    public string PDDoorRightForceOpenedName => $"moreDoors{CamelCaseName}RightForceOpened";

    [JsonIgnore]
    public string KeyTermName => $"MOREDOORS_{UpperCaseName}_KEY";

    [JsonIgnore]
    public string NoKeyPromptId => $"MOREDOORS_{UpperCaseName}_DOOR_NOKEY";

    [JsonIgnore]
    public string KeyPromptId => $"MOREDOORS_{UpperCaseName}_DOOR_KEY";

    public bool ValidateAndUpdate(out string err)
    {
        if (!Door.ValidateAndUpdate(out err)) return false;

        string s = Key.Location.sceneName;
        if (Key.Location is DualLocation dl)
        {
            if (dl.falseLocation.sceneName != s)
            {
                err = "Bad false location sceneName";
                return false;
            }
            if (dl.trueLocation.sceneName != s)
            {
                err = "Bad true location sceneName";
                return false;
            }
        }

        err = "";
        return true;
    }
}
