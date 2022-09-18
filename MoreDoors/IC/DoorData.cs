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
        }

        public string VarName;
        public string LogicName;
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
        public string PDDoorOpenedName => MoreDoorsModule.PlayerDataDoorOpenedName(this);

        [JsonIgnore]
        public string PDKeyName => MoreDoorsModule.PlayerDataKeyName(this);

        [JsonIgnore]
        public string KeyLogicName => MoreDoorsModule.KeyLogicName(this);

        [JsonIgnore]
        public string DoorForcedOpenLogicName => MoreDoorsModule.DoorForcedOpenLogicName(this);

        [JsonIgnore]
        public string KeyLocName => Key.VanillaLocation.name;

        [JsonIgnore]
        public string NoKeyPromptId => MoreDoorsModule.NoKeyPromptId(this);

        [JsonIgnore]
        public string KeyPromptId => MoreDoorsModule.KeyPromptId(this);
    }
}
