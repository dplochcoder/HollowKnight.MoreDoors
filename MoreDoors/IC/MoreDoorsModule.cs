using ItemChanger;
using ItemChanger.Extensions;
using Modding;
using MoreDoors.Data;
using Newtonsoft.Json;
using PurenailCore.ICUtil;
using PurenailCore.SystemUtil;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreDoors.IC;

public class MoreDoorsModule : ItemChanger.Modules.Module
{
    // Fake bool with no value, used only in setters.
    public const string EmptyBoolName = "moreDoorsNothing";
    public const string MoreDoorsEnabledName = "moreDoorsEnabled";
    public const string MenuConvKey = "MORE_DOORS_MENU";

    public class DoorState
    {
        public DoorData Data;
        public DoorState(DoorData data)
        {
            this.Data = data;
        }

        public bool KeyObtained = false;
        public bool DoorOpened = false;

        public bool LeftDoorForceOpened = false;
        public bool RightDoorForceOpened = false;
    }

    // Hack for hk7y plando.
    public bool ImportNewJson = false;

    // Indexed by door name.
    public SortedDictionary<string, DoorState> DoorStates = new();

    private readonly Dictionary<string, string> DoorNamesByKey = new();
    private readonly Dictionary<string, string> DoorNamesByDoor = new();
    private readonly Dictionary<string, string> DoorNamesByLeftForce = new();
    private readonly Dictionary<string, string> DoorNamesByRightForce = new();
    private readonly Dictionary<string, HashSet<string>> DoorNamesByScene = new();
    private readonly Dictionary<string, string> DoorNamesByTransition = new();
    private readonly Dictionary<string, string> PromptStrings = new();
    private readonly Dictionary<string, List<IDeployer>> DeployersByScene = new();

    [JsonIgnore]
    public string LastSceneName { get; private set; }
    [JsonIgnore]
    public string LastGateName { get; private set; }

    // After DarknessRandomizer
    private const float BeforeSceneManagerStartPriority = 110f;

    private void IndexDoor(string doorName, DoorData data)
    {
        DoorNamesByKey[data.PDKeyName] = doorName;
        DoorNamesByDoor[data.PDDoorOpenedName] = doorName;

        DoorNamesByScene.GetOrAddNew(data.Door.LeftSceneName).Add(doorName);
        DoorNamesByScene.GetOrAddNew(data.Door.RightSceneName).Add(doorName);
        DoorNamesByTransition[data.Door.LeftLocation.TransitionName] = doorName;
        DoorNamesByTransition[data.Door.RightLocation.TransitionName] = doorName;
        DoorNamesByLeftForce[data.PDDoorLeftForceOpenedName] = doorName;
        DoorNamesByRightForce[data.PDDoorRightForceOpenedName] = doorName;

        PromptStrings[data.NoKeyPromptId] = data.Door.NoKeyDesc;
        PromptStrings[data.KeyPromptId] = data.Door.KeyDesc;

        data.Door.Deployers?.ForEach(d => DeployersByScene.GetOrAddNew(d.SceneName).Add(d));
    }

    public override void Initialize()
    {
        foreach (var e in DoorStates)
        {
            var doorName = e.Key;
            var data = e.Value.Data;
            if (ImportNewJson || data == null)
            {
                data = DoorData.GetFromJson(doorName);
                e.Value.Data = data;
            }

            IndexDoor(doorName, data);
        }
        PromptStrings[MenuConvKey] = "More Keys";

        ModHooks.GetPlayerBoolHook += OverrideGetBool;
        ModHooks.SetPlayerBoolHook += OverrideSetBool;
        ModHooks.LanguageGetHook += OverrideLanguageGet;
        PriorityEvents.BeforeSceneManagerStart.Subscribe(BeforeSceneManagerStartPriority, OnSceneManagerStart);
        Events.OnBeginSceneTransition += OnUseTransition;
        Events.OnTransitionOverride += OnTransitionOverride;
        Events.OnSceneChange += RunDeployers;

        MoreKeysPage.Instance.Update();
    }

    internal void DebugResetData(IDictionary<string, DoorData> data)
    {
        DoorNamesByKey.Clear();
        DoorNamesByDoor.Clear();
        DoorNamesByScene.Clear();
        DoorNamesByTransition.Clear();
        DoorNamesByLeftForce.Clear();
        DoorNamesByRightForce.Clear();
        PromptStrings.Clear();
        DeployersByScene.Clear();

        foreach (var e in data)
        {
            var doorName = e.Key;
            var doorData = e.Value;

            DoorStates[doorName].Data = doorData;
            IndexDoor(doorName, doorData);
        }
        PromptStrings[MenuConvKey] = "More Keys";
    }

    public override void Unload()
    {
        ModHooks.GetPlayerBoolHook -= OverrideGetBool;
        ModHooks.SetPlayerBoolHook -= OverrideSetBool;
        ModHooks.LanguageGetHook -= OverrideLanguageGet;
        PriorityEvents.BeforeSceneManagerStart.Unsubscribe(BeforeSceneManagerStartPriority, OnSceneManagerStart);
        Events.OnBeginSceneTransition -= OnUseTransition;
        Events.OnTransitionOverride -= OnTransitionOverride;
        Events.OnSceneChange -= RunDeployers;
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
        else if (DoorNamesByLeftForce.TryGetValue(name, out doorName))
        {
            return DoorStates[doorName].LeftDoorForceOpened;
        }
        else if (DoorNamesByRightForce.TryGetValue(name, out doorName))
        {
            return DoorStates[doorName].RightDoorForceOpened;
        }
        else if (name == MoreDoorsEnabledName)
        {
            return DoorStates.Count > 0;
        }
        return orig;
    }

    public bool IsDoorOpened(string doorName, bool left)
    {
        var state = DoorStates[doorName];
        return state.DoorOpened || (left && state.LeftDoorForceOpened) || (!left && state.RightDoorForceOpened);
    }

    public delegate void DoorOpened(string doorName, bool left);

    public static event DoorOpened OnDoorOpened;

    public delegate void KeyObtained(string uiName);

    public static event KeyObtained OnKeyObtained;

    private bool OverrideSetBool(string name, bool newValue)
    {
        if (DoorNamesByKey.TryGetValue(name, out string doorName))
        {
            var state = DoorStates[doorName];
            state.KeyObtained = newValue;

            if (newValue) OnKeyObtained?.Invoke(state.Data.Key.UIItemName);
            MoreKeysPage.Instance.Update();
        }
        else if (DoorNamesByDoor.TryGetValue(name, out doorName))
        {
            var state = DoorStates[doorName];
            state.DoorOpened = newValue;
            if (newValue)
            {
                state.LeftDoorForceOpened = newValue;
                state.RightDoorForceOpened = newValue;

                OnDoorOpened?.Invoke(doorName, true);
                OnDoorOpened?.Invoke(doorName, false);
            }
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
            var data = DoorStates[doorName].Data;
            if (sceneName == data.Door.LeftSceneName) DoorSpawner.SpawnDoor(sm, doorName, true);
            if (sceneName == data.Door.RightSceneName) DoorSpawner.SpawnDoor(sm, doorName, false);
        }
    }

    private void OnUseTransition(Transition t) => OnUseITransition(t);

    private void OnTransitionOverride(Transition src, Transition origDst, ITransition newDst) => OnUseITransition(newDst);

    private void RunDeployers(Scene to)
    {
        if (DeployersByScene.TryGetValue(to.name, out var list)) list.ForEach(d => d.OnSceneChange(to));
    }

    private void OnUseITransition(ITransition t) {
        LastSceneName = t.SceneName;
        LastGateName = t.GateName;

        // If we went through a door via RoomRando, force it open.
        var tname = $"{t.SceneName}[{t.GateName}]";
        if (DoorNamesByTransition.TryGetValue(tname, out string doorName) && !DoorStates[doorName].DoorOpened)
        {
            var door = DoorStates[doorName].Data.Door;
            if (door.Mode != DoorData.DoorInfo.SplitMode.Normal) return;

            if (door.LeftLocation.TransitionName == tname)
            {
                DoorStates[doorName].LeftDoorForceOpened = true;
                OnDoorOpened?.Invoke(doorName, true);
            }
            else if (door.RightLocation.TransitionName == tname)
            {
                DoorStates[doorName].RightDoorForceOpened = true;
                OnDoorOpened?.Invoke(doorName, false);
            }

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
