using ItemChanger.Internal.Menu;
using Modding;
using MoreDoors.Data;
using MoreDoors.IC;
using MoreDoors.Rando;
using static RandomizerMod.Localization;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MoreDoors
{
    public class MoreDoors : Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
    {
        public static MoreDoors Instance { get; private set; }
        public static GlobalSettings GS { get; private set; } = new();

        public bool ToggleButtonInsideMenu => false;

        public static new void Log(string msg) { ((Loggable)Instance).Log(msg); }

        public MoreDoors() : base("MoreDoors") => Instance = this;

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Preloader.Instance.SavePreloads(preloadedObjects);
            DoorData.Load();

            Vanilla.Setup();
            if (ModHooks.GetMod("Randomizer 4") is Mod)
            {
                RandoInterop.Setup();
            }
        }

        public override List<(string, string)> GetPreloadNames() => new(Preloader.Instance.GetPreloadNames());

        public void OnLoadGlobal(GlobalSettings s) => GS = s ?? new();

        public GlobalSettings OnSaveGlobal() => GS ?? new();

        public override string GetVersion() => Version.Instance;

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            ModMenuScreenBuilder builder = new(Localize("More Doors"), modListMenu);
            builder.AddHorizontalOption(new()
            {
                Name = Localize("Enable in Vanilla"),
                Description = "If yes, MoreDoors will be added to vanilla save files.",
                Values = new string[] { "No", "Yes" },
                Saver = i => GS.EnableInVanilla = i == 1,
                Loader = () => GS.EnableInVanilla ? 1 : 0
            });
            return builder.CreateMenuScreen();
        }
    }
}
