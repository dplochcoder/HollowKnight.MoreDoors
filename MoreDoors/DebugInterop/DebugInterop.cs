using DebugMod;
using MoreDoors.IC;

namespace MoreDoors.DebugInterop;

public static class DebugInterop
{
    public static void Setup() => DebugMod.DebugMod.AddToKeyBindList(typeof(DebugInterop));

    private static bool MoreDoorsEnabled(out MoreDoorsModule? mod)
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
        foreach (var ds in mod!.DoorStates.Values)
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
        foreach (var ds in mod!.DoorStates.Values)
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
        foreach (var ds in mod!.DoorStates.Values)
        {
            ds.DoorOpened = false;
            ds.LeftDoorForceOpened = false;
            ds.RightDoorForceOpened = false;
        }
    }
}
