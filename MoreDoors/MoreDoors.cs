using ItemChanger.Internal.Menu;
using Modding;
using MoreDoors.Data;
using MoreDoors.IC;
using MoreDoors.Rando;
using static RandomizerMod.Localization;
using System.Collections.Generic;
using UnityEngine;
using PurenailCore.ModUtil;
using SFCore;

namespace MoreDoors;

public class MoreDoors : Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
{
    public static MoreDoors? Instance { get; private set; }
    public static GlobalSettings GS { get; private set; } = new();

    public bool ToggleButtonInsideMenu => false;

    public static new void LogDebug(string msg) { ((Loggable)Instance!).LogDebug(msg); }

    public static new void Log(string msg) { ((Loggable)Instance!).Log(msg); }

    public static new void LogWarn(string msg) { ((Loggable)Instance!).LogWarn(msg); }

    public static new void LogError(string msg) { ((Loggable)Instance!).LogError(msg); }

    public MoreDoors() : base("MoreDoors")
    {
        Instance = this;
        InventoryHelper.AddInventoryPage(
            InventoryPageType.Empty,
            "More Keys",
            MoreDoorsModule.MENU_KEY,
            "MoreKeys",
            MoreDoorsModule.MORE_DOORS_ENABLED,
            MoreKeysPage.Instance.GeneratePage);
    }

    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        IC.Preloader.Instance.Initialize(preloadedObjects);
        DoorData.Load();

        Vanilla.Setup();
        if (ModHooks.GetMod("DebugMod") is Mod) DebugInterop.DebugInterop.Setup();
        if (ModHooks.GetMod("ExtraRando") is Mod) ExtraRandoInterop.ExtraRandoInterop.Setup();
        if (ModHooks.GetMod("FStatsMod") is Mod) FStatsInterop.FStatsInterop.Setup();
        if (ModHooks.GetMod("Randomizer 4") is Mod) RandoInterop.Setup();
    }

    public override List<(string, string)> GetPreloadNames() => [.. IC.Preloader.Instance.GetPreloadNames()];

    public void OnLoadGlobal(GlobalSettings s)
    {
        GS = s ?? new();
        GS.RandoSettings.MaybeUpdateEnabledDoors();
    }

    public GlobalSettings OnSaveGlobal() => GS ?? new();

    private static readonly string Version = VersionUtil.ComputeVersion<MoreDoors>();

    public override string GetVersion() => Version;

    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        ModMenuScreenBuilder builder = new(Localize("More Doors"), modListMenu);
        builder.AddHorizontalOption(new()
        {
            Name = Localize("Enable in Vanilla"),
            Description = "If yes, MoreDoors will be added to vanilla save files.",
            Values = ["No", "Yes"],
            Saver = i => GS.EnableInVanilla = i == 1,
            Loader = () => GS.EnableInVanilla ? 1 : 0
        });
        builder.AddHorizontalOption(new()
        {
            Name = Localize("Show Key Shinies"),
            Description = "If yes, enemies holding keys will be marked with a shiny as a hint.",
            Values = ["No", "Yes"],
            Saver = i => GS.ShowKeyShinies = i == 1,
            Loader = () => GS.ShowKeyShinies ? 1 : 0
        });
        return builder.CreateMenuScreen();
    }
}
