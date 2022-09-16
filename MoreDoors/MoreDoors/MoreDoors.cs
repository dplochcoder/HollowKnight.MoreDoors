using Modding;
using MoreDoors.IC;
using MoreDoors.Rando;
using System.Collections.Generic;
using UnityEngine;

namespace MoreDoors
{
    public class MoreDoors : Mod, IGlobalSettings<GlobalSettings>
    {
        public static MoreDoors Instance { get; private set; }
        public static GlobalSettings GS { get; private set; } = new();

        public static new void Log(string msg) { ((Loggable)Instance).Log(msg); }

        public MoreDoors() : base("MoreDoors") => Instance = this;

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            IC.Preloader.Instance.SavePreloads(preloadedObjects);
            DoorData.Load();

            if (ModHooks.GetMod("Randomizer 4") is Mod)
            {
                RandoInterop.Setup();
            }
        }

        public override List<(string, string)> GetPreloadNames() => new(IC.Preloader.Instance.GetPreloadNames());

        public void OnLoadGlobal(GlobalSettings s) => GS = s ?? new();

        public GlobalSettings OnSaveGlobal() => GS ?? new();

        public override string GetVersion() => Version.Instance;
    }
}
