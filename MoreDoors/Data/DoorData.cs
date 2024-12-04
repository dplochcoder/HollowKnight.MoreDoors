using ItemChanger;
using ItemChanger.Locations;
using MoreDoors.IC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

using JsonUtil = PurenailCore.SystemUtil.JsonUtil<MoreDoors.MoreDoors>;

namespace MoreDoors.Data;

public record DoorData
{
    private static readonly SortedDictionary<string, DoorData> EmbeddedData = JsonUtil.DeserializeEmbedded<SortedDictionary<string, DoorData>>("MoreDoors.Resources.Data.doors.json");
    private static readonly SortedDictionary<string, DoorData> ExtensionData = [];
    private static readonly SortedDictionary<string, DoorData> AllData = [];

    internal static IReadOnlyDictionary<string, DoorData> EmbeddedDoors() => EmbeddedData;

    internal static IReadOnlyDictionary<string, DoorData> AllDoors() => AllData;

    private static void AddDoor(string name, DoorData data)
    {
        if (AllData.ContainsKey(name)) return;
        AllData.Add(name, data);

        KeyItem key = new(name, data);
        key.AddLocationInteropTags(data);

        Finder.DefineCustomItem(key);
        Finder.DefineCustomLocation(data.Key!.Location!);
    }

    public static DoorData? GetDoor(string name)
    {
        if (EmbeddedData.TryGetValue(name, out var data)) return data;
        else if (ExtensionData.TryGetValue(name, out data)) return data;
        else return null;
    }

    public static void Load()
    {
        foreach (var e in EmbeddedData) AddDoor(e.Key, e.Value);
        MoreDoors.Log("Loaded Doors");
    }

    // Extensions can call this to add their own doors for custom plandos.
    public static void AddExtensionDoor(string name, DoorData data)
    {
        ExtensionData.Add(name, data);
        AddDoor(name, data);
    }

    public string CamelCaseName = "";
    public string UpperCaseName = "";
    public string UIName = "";

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
            public record SecretMask
            {
                public float Width;
                public float Height;
                public float OffsetX;
                public float OffsetY;
            }

            public string SceneName = "";
            public string GateName = "";
            public string NoKeyDesc = "";
            public string KeyDesc = "";
            public List<SecretMask>? Masks;
            public bool RequiresLantern;
            public float X;
            public float Y;
            public List<IDoorDecorator>? Decorators;

            [JsonIgnore]
            public string TransitionName => $"{SceneName}[{GateName}]";

            [JsonIgnore]
            public string TransitionProxyName => $"{SceneName}_Proxy[{GateName}]";

            public bool ValidateAndUpdate(out string err)
            {
                err = "";
                return true;
            }
        }

        public ISprite? Sprite;
        // Location where the player looks left to the door.
        public Location? LeftLocation;
        // Location where the player looks right to the door.
        public Location? RightLocation;
        public SplitMode Mode;
        public List<IDeployer>? Deployers;

        private Location SplitLocation(Side side) => Mode switch
        {
            SplitMode.Normal => side == Side.Left ? LeftLocation! : RightLocation!,
            SplitMode.LeftTwin => LeftLocation!,
            SplitMode.RightTwin => RightLocation!,
            _ => throw new System.ArgumentException($"Unknown Side: {side}")
        };

        [JsonIgnore]
        public string LeftSceneName => SplitLocation(Side.Left).SceneName;
        [JsonIgnore]
        public string RightSceneName => SplitLocation(Side.Right).SceneName;

        public bool ValidateAndUpdate(out string err)
        {
            if (!LeftLocation!.ValidateAndUpdate(out err)) return false;
            if (!RightLocation!.ValidateAndUpdate(out err)) return false;

            bool split = Mode == SplitMode.Normal;
            bool matching = LeftLocation.TransitionName == RightLocation.TransitionName;
            if (split == matching)
            {
                err = "Split mode does not match transitions";
                return false;
            }

            err = "";
            return true;
        }
    }
    public DoorInfo? Door;

    public record KeyInfo
    {
        public string ItemName = "";
        public string UIItemName = "";
        public string ShopDesc = "";
        public string InvDesc = "";
        public string UsedInvDesc = "";
        public ISprite? Sprite;
        public AbstractLocation? Location;
        public string Logic = "FALSE";

        public record WorldMapLocation
        {
            public string SceneName = "";
            public float X;
            public float Y;

            [JsonIgnore]
            public (string, float, float) AsTuple => (SceneName, X, Y);
        }
        public WorldMapLocation? WorldMapLocationOverride;
        public List<WorldMapLocation>? ExtraWorldMapLocations = null;

        public List<WorldMapLocation> GetWorldMapLocations()
        {
            List<WorldMapLocation> locations = [];

            if (WorldMapLocationOverride != null)
            {
                WorldMapLocation first = WorldMapLocationOverride;
                first.SceneName ??= Location!.sceneName ?? "";
                locations.Add(first);
            }
            else if (Location is DualLocation dl && dl.trueLocation is CoordinateLocation cl)
            {
                locations.Add(new()
                {
                    SceneName = Location!.sceneName ?? "",
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
    public KeyInfo? Key;

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
    public string LeftNoKeyPromptId => $"MOREDOORS_{UpperCaseName}_LEFT_DOOR_NOKEY";
    [JsonIgnore]
    public string LeftKeyPromptId => $"MOREDOORS_{UpperCaseName}_LEFT_DOOR_KEY";
    [JsonIgnore]
    public string RightNoKeyPromptId => $"MOREDOORS_{UpperCaseName}_RIGHT_DOOR_NOKEY";
    [JsonIgnore]
    public string RightKeyPromptId => $"MOREDOORS_{UpperCaseName}_RIGHT_DOOR_KEY";

    public bool ValidateAndUpdate(out string err)
    {
        if (!Door!.ValidateAndUpdate(out err)) return false;

        string s = Key!.Location!.sceneName ?? "";
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
