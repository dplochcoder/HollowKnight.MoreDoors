using DebugMod;
using MoreDoors.IC;
using System.IO;
using UnityEngine;

namespace MoreDoors.Debug
{
    public static class DebugInterop
    {
        public static void Setup()
        {
            using Stream s = typeof(MoreDoors).Assembly.GetManifestResourceStream("MoreDoors.Resources.Sprites.Keys.Mantis.png");
            using MemoryStream ms = new();
            s.CopyTo(ms);

            Texture2D t2d = new(1, 1);
            t2d.LoadImage(ms.ToArray());

            DebugMod.DebugMod.AddToKeyBindList(typeof(DebugInterop));
        }

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
                ds.DoorForceOpened = false;
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
                ds.DoorForceOpened = false;
            }
        }
    }
}
