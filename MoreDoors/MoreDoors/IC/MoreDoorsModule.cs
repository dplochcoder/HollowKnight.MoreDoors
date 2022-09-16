using System.Collections.Generic;

namespace MoreDoors.IC
{
    public class MoreDoorsModule : ItemChanger.Modules.Module
    {
        public class DoorState
        {
            public bool KeyObtained = false;
            public bool DoorOpened = false;
        }

        // Indexed by door name.
        public SortedDictionary<string, DoorState> DoorStates = new();

        private readonly Dictionary<string, string> DoorNameByKey = new();
        private readonly Dictionary<string, string> DoorNameByDoor = new();

        public override void Initialize()
        {
            Modding.ModHooks.GetPlayerBoolHook += OverrideGetBool;
            Modding.ModHooks.SetPlayerBoolHook += OverrideSetBool;

            foreach (var doorName in DoorStates.Keys)
            {
                var data = DoorData.Get(doorName);
                DoorNameByKey[data.KeyName] = doorName;
                DoorNameByDoor[data.DoorOpenedName] = doorName;
            }
        }

        public override void Unload()
        {
            Modding.ModHooks.GetPlayerBoolHook -= OverrideGetBool;
            Modding.ModHooks.SetPlayerBoolHook -= OverrideSetBool;
        }

        public const string PlayerDataKeyPrefix = "MOREDOORS_";
        public const string PlayerDataDoorPrefix = "MOREDOORS_DOOR_";

        private bool OverrideGetBool(string name, bool orig) => DoorStates.TryGetValue(name, out DoorState doorState) ? doorState.KeyObtained : orig;

        private bool OverrideSetBool(string name, bool orig)
        {
            if (DoorStates.TryGetValue(name, out DoorState doorState))
            {
                doorState.KeyObtained = orig;
            }

            return orig;
        }
    }
}
