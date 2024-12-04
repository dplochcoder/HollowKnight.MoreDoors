using ItemChanger;
using MoreDoors.Data;
using MoreDoors.IC;
using MoreDoors.Imports;
using Newtonsoft.Json;
using RandomizerMod.RC;
using System.IO;
using System.Linq;

namespace MoreDoors.Rando;

public static class RandoInterop
{
    public static LocalSettings LS { get; set; } = new();

    private static void EarlyCreateSettings(RandoController rc)
    {
        if (!IsEnabled) return;

        LS = new();
        System.Random r = new(rc.gs.Seed + 13);
        LS.EnabledDoorNames = LS.Settings.ComputeActiveDoors(rc.gs, r);
    }

    public static bool IsEnabled => MoreDoors.GS.RandoSettings.IsEnabled;

    public static void Setup()
    {
        ConnectionMenu.Setup();
        LogicPatcher.Setup();
        RequestModifier.Setup();
        CondensedSpoilerLogger.AddCategory("MoreDoors Keys", _ => IsEnabled,
            new(DoorData.AllDoors().Select(e => e.Value.Key!.ItemName)));

        RandoController.OnBeginRun += EarlyCreateSettings;
        RandoController.OnExportCompleted += OnExportCompleted;
        RandomizerMod.Logging.SettingsLog.AfterLogSettings += LogSettings;
        RandomizerMod.Logging.LogManager.AddLogger(new MoreDoorsLogger());
    }

    private static void OnExportCompleted(RandoController rc)
    {
        if (!IsEnabled) return;

        var mod = ItemChangerMod.Modules.GetOrAdd<MoreDoorsModule>();
        foreach (var doorName in LS.EnabledDoorNames) mod.DoorStates[doorName] = new();
    }

    private static void LogSettings(RandomizerMod.Logging.LogArguments args, TextWriter tw)
    {
        if (!IsEnabled) return;

        tw.WriteLine("Logging MoreDoors MoreDoorsSettings:");
        using JsonTextWriter jtw = new(tw) { CloseOutput = false };
        RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jtw, LS.Settings);
        tw.WriteLine();
    }

}
