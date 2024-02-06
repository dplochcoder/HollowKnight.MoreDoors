using DebugMod;
using MoreDoors.Data;
using MoreDoors.IC;
using System.Collections.Generic;
using JsonUtil = PurenailCore.SystemUtil.JsonUtil<MoreDoors.MoreDoors>;

namespace MoreDoors.Debug;

public static class DebugInterop
{
    public static void Setup() => DebugMod.DebugMod.AddToKeyBindList(typeof(DebugInterop));

    private static bool MoreDoorsEnabled(out MoreDoorsModule mod)
    {
        mod = ItemChanger.ItemChangerMod.Modules.Get<MoreDoorsModule>();
        if (mod == null)
        {
            Console.AddLine("MoreDoors not enabled in this save; doing nothing");
            return false;
        }

        return true;
    }

    [BindableMethod(name = "Give More Keys", category = "MoreDoors")]
    public static void GiveMoreKeys()
    {
        if (!MoreDoorsEnabled(out var mod)) return;

        Console.AddLine("Giving all MoreDoors Keys");
        foreach (var ds in mod.DoorStates.Values)
        {
            ds.KeyObtained = true;
        }
        MoreKeysPage.Instance.Update();
    }

    [BindableMethod(name = "Take More Keys", category = "MoreDoors")]
    public static void TakeMoreKeys()
    {
        if (!MoreDoorsEnabled(out var mod)) return;

        Console.AddLine("Removing all MoreDoors Keys and closing all doors");
        foreach (var ds in mod.DoorStates.Values)
        {
            ds.KeyObtained = false;
            ds.DoorOpened = false;
            ds.LeftDoorForceOpened = false;
            ds.RightDoorForceOpened = false;
        }
        MoreKeysPage.Instance.Update();
    }

    [BindableMethod(name = "Close Doors", category = "MoreDoors")]
    public static void CloseDoors()
    {
        if (!MoreDoorsEnabled(out var mod)) return;

        Console.AddLine("Closing all MoreDoors doors");
        foreach (var ds in mod.DoorStates.Values)
        {
            ds.DoorOpened = false;
            ds.LeftDoorForceOpened = false;
            ds.RightDoorForceOpened = false;
        }
    }

    [BindableMethod(name = "Debug Reload Json", category = "MoreDoors")]
    public static void ReloadJson()
    {
        if (!MoreDoorsEnabled(out var mod)) return;

        try
        {
            var data = JsonUtil.DeserializeEmbedded<DebugData>("MoreDoors.Resources.Data.debug.json");
            var newDoorData = JsonUtil.DeserializeFromPath<SortedDictionary<string, DoorData>>(data.DoorsJsonPath);
            mod.DebugResetData(newDoorData);

            Console.AddLine("Debug data reset from json on disk");
        }
        catch (System.Exception)
        {
            Console.AddLine("Debug data not available in this build");
            return;
        }
    }
}
