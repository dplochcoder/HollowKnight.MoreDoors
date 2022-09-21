using ItemChanger;
using ItemChanger.Extensions;
using Modding;
using MoreDoors.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreDoors.IC
{
    public class MoreDoorsModule : ItemChanger.Modules.Module
    {
        // Fake bool with no value, used only in setters.
        public const string EmptyBoolName = "moreDoorsNothing";

        public class DoorState
        {
            public bool KeyObtained = false;
            public bool DoorOpened = false;
            public bool DoorForceOpened = false;
        }

        // Indexed by door name.
        public SortedDictionary<string, DoorState> DoorStates = new();

        private readonly Dictionary<string, string> DoorNamesByKey = new();
        private readonly Dictionary<string, string> DoorNamesByDoor = new();
        private readonly Dictionary<string, HashSet<string>> DoorNamesByScene = new();
        private readonly Dictionary<string, string> DoorNamesByTransition = new();
        private readonly Dictionary<string, string> PromptStrings = new();

        public override void Initialize()
        {
            ModHooks.GetPlayerBoolHook += OverrideGetBool;
            ModHooks.SetPlayerBoolHook += OverrideSetBool;
            ModHooks.LanguageGetHook += OverrideLanguageGet;

            foreach (var doorName in DoorStates.Keys)
            {
                var data = DoorData.Get(doorName);
                DoorNamesByKey[data.PDKeyName] = doorName;
                DoorNamesByDoor[data.PDDoorOpenedName] = doorName;

                DoorNamesByScene.GetOrAdd(data.LeftDoorLocation.SceneName, new()).Add(doorName);
                DoorNamesByScene.GetOrAdd(data.RightDoorLocation.SceneName, new()).Add(doorName);
                DoorNamesByTransition[data.LeftDoorLocation.TransitionName] = doorName;
                DoorNamesByTransition[data.RightDoorLocation.TransitionName] = doorName;

                PromptStrings[data.NoKeyPromptId] = data.NoKeyDesc;
                PromptStrings[data.KeyPromptId] = data.KeyDesc;
            }

            Events.OnSceneChange += OnSceneChange;
            Events.OnBeginSceneTransition += OnBeginSceneTransition;
        }

        public override void Unload()
        {
            ModHooks.GetPlayerBoolHook -= OverrideGetBool;
            ModHooks.SetPlayerBoolHook -= OverrideSetBool;
            ModHooks.LanguageGetHook -= OverrideLanguageGet;
            Events.OnSceneChange -= OnSceneChange;
        }

        private bool OverrideGetBool(string name, bool orig)
        {
            if (DoorNamesByKey.TryGetValue(name, out string doorName))
            {
                return DoorStates[doorName].KeyObtained;
            }
            else if (DoorNamesByDoor.TryGetValue(name, out doorName))
            {
                return DoorStates[doorName].DoorOpened;
            }
            return orig;
        }

        private bool OverrideSetBool(string name, bool orig)
        {
            if (DoorNamesByKey.TryGetValue(name, out string doorName))
            {
                DoorStates[doorName].KeyObtained = orig;
            }
            else if (DoorNamesByDoor.TryGetValue(name, out doorName))
            {
                DoorStates[doorName].DoorOpened = orig;
            }
            return orig;
        }

        private string OverrideLanguageGet(string key, string sheetTitle, string orig) => PromptStrings.TryGetValue(key, out string value) ? value : orig;

        private static readonly HashSet<string> emptySet = new();

        private void OnSceneChange(Scene scene)
        {
            SceneManager? sm = scene.GetRootGameObjects().FirstOrDefault(g => g.GetComponent<SceneManager>() != null)?.GetComponent<SceneManager>();
            foreach (var doorName in DoorNamesByScene.GetOrDefault(scene.name, emptySet))
            {
                // If the door is already opened, skip, even though it's not strictly necessary.
                var state = DoorStates[doorName];
                if (state.DoorOpened || state.DoorForceOpened) continue;

                var data = DoorData.Get(doorName);
                if (scene.name == data.LeftDoorLocation.SceneName)
                {
                    DoorSpawner.SpawnDoor(sm, doorName, true);
                }
                if (scene.name == data.RightDoorLocation.SceneName)
                {
                    DoorSpawner.SpawnDoor(sm, doorName, false);
                }
            }
        }

        private void OnBeginSceneTransition(Transition t)
        {
            // If we went through a door via RoomRando, force it open.
            var tname = $"{t.SceneName}[{t.GateName}]";
            if (DoorNamesByTransition.TryGetValue(tname, out string doorName) && !DoorStates[doorName].DoorOpened)
            {
                DoorStates[doorName].DoorForceOpened = true;
                foreach (var obj in Object.FindObjectsOfType<DoorNameMarker>())
                {
                    if (obj.DoorName == doorName)
                    {
                        obj.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
