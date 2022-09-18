using ItemChanger;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MoreDoors.IC
{
    public record DoorData
    {
        private static readonly SortedDictionary<string, DoorData> data = JsonUtil.Deserialize<SortedDictionary<string, DoorData>>("MoreDoors.Resources.Data.doors.json");

        public static DoorData Get(string doorName) => data[doorName];

        public static IEnumerable<string> DoorNames => data.Keys;

        public static int Count => data.Count;

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
            public float X;
            public float Y;

            [JsonIgnore]
            public string TransitionName => $"{SceneName}[{GateName}]";

            [JsonIgnore]
            public string TransitionProxyName => $"{SceneName}_Proxy[{GateName}]";
        }

        public string CamelCaseName;
        public string UpperCaseName;
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
            public string SpriteKey;
            public AbstractLocation VanillaLocation;
            public string VanillaLogic;
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
