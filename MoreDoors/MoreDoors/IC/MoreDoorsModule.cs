using ItemChanger;
using ItemChanger.Extensions;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
        private readonly Dictionary<string, HashSet<string>> DoorNamesByScene = new();

        public override void Initialize()
        {
            Modding.ModHooks.GetPlayerBoolHook += OverrideGetBool;
            Modding.ModHooks.SetPlayerBoolHook += OverrideSetBool;

            foreach (var doorName in DoorStates.Keys)
            {
                var data = DoorData.Get(doorName);
                DoorNameByKey[data.PDKeyName] = doorName;
                DoorNameByDoor[data.PDDoorOpenedName] = doorName;

                DoorNamesByScene.GetOrAdd(data.LeftDoorLocation.SceneName, new()).Add(doorName);
                DoorNamesByScene.GetOrAdd(data.RighttDoorLocation.SceneName, new()).Add(doorName);
            }

            Events.OnSceneChange += OnSceneChange;
        }

        public override void Unload()
        {
            Modding.ModHooks.GetPlayerBoolHook -= OverrideGetBool;
            Modding.ModHooks.SetPlayerBoolHook -= OverrideSetBool;
            Events.OnSceneChange -= OnSceneChange;
        }

        public static string PlayerDataKeyName(string doorNameVar) => $"moreDoors{doorNameVar}Key";
        public static string PlayerDataDoorOpenedName(string doorNameVar) => $"moreDoors{doorNameVar}DoorOpened";
        public static string LogicKeyName(string doorNameLogic) => $"MOREDOORS_{doorNameLogic}";

        public static string NoKeyConvoId(string doorNameLogic) => $"MOREDOORS_DOOR_{doorNameLogic}_NOKEY";
        public static string YesKeyConvoId(string doorNameLogic) => $"MOREDOORS_DOOR_{doorNameLogic}_YESKEY";

        private bool OverrideGetBool(string name, bool orig)
        {
            if (DoorNameByKey.TryGetValue(name, out string doorName))
            {
                return DoorStates[doorName].KeyObtained;
            }
            else if (DoorNameByDoor.TryGetValue(name, out doorName))
            {
                return DoorStates[doorName].DoorOpened;
            }
            return orig;
        }

        private bool OverrideSetBool(string name, bool orig)
        {
            if (DoorNameByKey.TryGetValue(name, out string doorName))
            {
                DoorStates[doorName].KeyObtained = orig;
            }
            else if (DoorNameByDoor.TryGetValue(name, out doorName))
            {
                DoorStates[doorName].DoorOpened = orig;
            }
            return orig;
        }

        private static readonly HashSet<string> emptySet = new();

        private void OnSceneChange(Scene scene)
        {
            foreach (var doorName in DoorNamesByScene.GetOrDefault(scene.name, emptySet))
            {
                // If the door is already opened, skip.
                if (DoorStates[doorName].DoorOpened) continue;

                var data = DoorData.Get(doorName);
                if (scene.name == data.LeftDoorLocation.SceneName)
                {
                    DoorSpawner.SpawnDoor(doorName, true);
                }
                if (scene.name == data.RighttDoorLocation.SceneName)
                {
                    DoorSpawner.SpawnDoor(doorName, false);
                }
            }
        }
    }
}
