using DebugMod;
using MoreDoors.IC;
using System.IO;
using System.Linq;
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

            // TODO: Enable this when DebugMod is fixed to avoid overlapping menus.
            // DebugMod.DebugMod.AddTopMenuContent("MoreDoors", new() { new ImageButton(t2d, _ => ToggleMoreKeys()) });
        }

        private static void ToggleMoreKeys()
        {
            var mod = ItemChanger.ItemChangerMod.Modules.Get<MoreDoorsModule>();
            if (mod == null)
            {
                Console.AddLine("MoreDoors mod is not active in this save; doing nothing");
                return;
            }

            bool allKeys = mod.DoorStates.Values.All(ds => ds.KeyObtained);
            if (allKeys)
            {
                Console.AddLine("Removing all MoreDoors Keys and closing all doors");
                foreach (var ds in mod.DoorStates.Values)
                {
                    ds.KeyObtained = false;
                    ds.DoorOpened = false;
                    ds.DoorForceOpened = false;
                }
            }
            else
            {
                Console.AddLine("Giving all MoreDoors Keys");
                foreach (var ds in mod.DoorStates.Values)
                {
                    ds.KeyObtained = true;
                }
            }
            MoreKeysPage.Instance.Update();
        }
    }
}
