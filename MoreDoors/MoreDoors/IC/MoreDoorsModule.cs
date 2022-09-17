﻿using ItemChanger;
using ItemChanger.Extensions;
using Modding;
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
        private readonly Dictionary<string, string> PromptStrings = new();

        public override void Initialize()
        {
            ModHooks.GetPlayerBoolHook += OverrideGetBool;
            ModHooks.SetPlayerBoolHook += OverrideSetBool;
            ModHooks.LanguageGetHook += OverrideLanguageGet;

            foreach (var doorName in DoorStates.Keys)
            {
                var data = DoorData.Get(doorName);
                DoorNameByKey[data.PDKeyName] = doorName;
                DoorNameByDoor[data.PDDoorOpenedName] = doorName;

                DoorNamesByScene.GetOrAdd(data.LeftDoorLocation.SceneName, new()).Add(doorName);
                DoorNamesByScene.GetOrAdd(data.RightDoorLocation.SceneName, new()).Add(doorName);
                PromptStrings[data.NoKeyPromptId] = data.NoKeyDesc;
                PromptStrings[data.KeyPromptId] = $"{data.KeyDesc}<br>Insert the {data.Key.UIItemName}?";
            }

            Events.OnSceneChange += OnSceneChange;
        }

        public override void Unload()
        {
            ModHooks.GetPlayerBoolHook -= OverrideGetBool;
            ModHooks.SetPlayerBoolHook -= OverrideSetBool;
            ModHooks.LanguageGetHook -= OverrideLanguageGet;
            Events.OnSceneChange -= OnSceneChange;
        }

        public static string PlayerDataKeyName(string doorNameVar) => $"moreDoors{doorNameVar}Key";
        public static string PlayerDataDoorOpenedName(string doorNameVar) => $"moreDoors{doorNameVar}DoorOpened";
        public static string LogicKeyName(string doorNameLogic) => $"MOREDOORS_{doorNameLogic}_KEY";

        public static string NoKeyPromptId(string doorNameLogic) => $"MOREDOORS_DOOR_{doorNameLogic}_NOKEY";
        public static string KeyPromptId(string doorNameLogic) => $"MOREDOORS_DOOR_{doorNameLogic}_KEY";

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

        private string OverrideLanguageGet(string key, string sheetTitle, string orig) => PromptStrings.TryGetValue(key, out string value) ? value : orig;

        private static readonly HashSet<string> emptySet = new();

        private void OnSceneChange(Scene scene)
        {
            foreach (var doorName in DoorNamesByScene.GetOrDefault(scene.name, emptySet))
            {
                // If the door is already opened, skip, even though it's not strictly necessary.
                if (DoorStates[doorName].DoorOpened) continue;

                var data = DoorData.Get(doorName);
                if (scene.name == data.LeftDoorLocation.SceneName)
                {
                    DoorSpawner.SpawnDoor(doorName, true);
                }
                if (scene.name == data.RightDoorLocation.SceneName)
                {
                    DoorSpawner.SpawnDoor(doorName, false);
                }
            }
        }
    }
}
