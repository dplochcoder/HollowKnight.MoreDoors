using ItemChanger;
using ItemChanger.Extensions;
using Modding;
using MoreDoors.Data;
using Newtonsoft.Json;
using PurenailCore.ICUtil;
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
        public const string MoreDoorsEnabledName = "moreDoorsEnabled";

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

        // After DarknessRandomizer
        private const float BeforeSceneManagerStartPriority = 110f;

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

                DoorNamesByScene.GetOrAdd(data.Door.LeftLocation.SceneName, new()).Add(doorName);
                DoorNamesByScene.GetOrAdd(data.Door.RightLocation.SceneName, new()).Add(doorName);
                DoorNamesByTransition[data.Door.LeftLocation.TransitionName] = doorName;
                DoorNamesByTransition[data.Door.RightLocation.TransitionName] = doorName;

                PromptStrings[data.NoKeyPromptId] = data.Door.NoKeyDesc;
                PromptStrings[data.KeyPromptId] = data.Door.KeyDesc;
            }
            PromptStrings["MORE_KEYS_MENU"] = "More Keys";

            PriorityEvents.BeforeSceneManagerStart.Subscribe(BeforeSceneManagerStartPriority, OnSceneManagerStart);
            Events.OnTransitionOverride += OnTransitionOverride;
        }

        public override void Unload()
        {
            ModHooks.GetPlayerBoolHook -= OverrideGetBool;
            ModHooks.SetPlayerBoolHook -= OverrideSetBool;
            ModHooks.LanguageGetHook -= OverrideLanguageGet;
            PriorityEvents.BeforeSceneManagerStart.Unsubscribe(BeforeSceneManagerStartPriority, OnSceneManagerStart);
            Events.OnTransitionOverride -= OnTransitionOverride;
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
            else if (name == MoreDoorsEnabledName)
            {
                return DoorStates.Count > 0;
            }
            return orig;
        }

        private bool OverrideSetBool(string name, bool newValue)
        {
            if (DoorNamesByKey.TryGetValue(name, out string doorName))
            {
                var state = DoorStates[doorName];
                state.KeyObtained = newValue;
                MoreKeysPage.Instance.Update();
            }
            else if (DoorNamesByDoor.TryGetValue(name, out doorName))
            {
                var state = DoorStates[doorName];
                DoorStates[doorName].DoorOpened = newValue;
                MoreKeysPage.Instance.Update();
            }
            return newValue;
        }

        private string OverrideLanguageGet(string key, string sheetTitle, string orig) => PromptStrings.TryGetValue(key, out string value) ? value : orig;

        private static readonly HashSet<string> emptySet = new();

        private void OnSceneManagerStart(SceneManager sm)
        {
            var sceneName = sm.gameObject.scene.name;
            foreach (var doorName in DoorNamesByScene.GetOrDefault(sceneName, emptySet))
            {
                // If the door is already opened, skip, even though it's not strictly necessary.
                var state = DoorStates[doorName];
                if (state.DoorOpened || state.DoorForceOpened) continue;

                var data = DoorData.Get(doorName);
                if (sceneName == data.Door.LeftLocation.SceneName)
                {
                    DoorSpawner.SpawnDoor(sm, doorName, true);
                }
                if (sceneName == data.Door.RightLocation.SceneName)
                {
                    DoorSpawner.SpawnDoor(sm, doorName, false);
                }
            }
        }

        private void OnTransitionOverride(Transition src, Transition origDst, ITransition newDst)
        {
            // If we went through a door via RoomRando, force it open.
            var tname = $"{newDst.SceneName}[{newDst.GateName}]";
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
