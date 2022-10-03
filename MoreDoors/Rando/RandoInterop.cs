using ItemChanger;
using MoreDoors.Data;
using MoreDoors.IC;
using MoreDoors.Imports;
using Newtonsoft.Json;
using RandomizerMod.RC;
using System.IO;
using System.Linq;

namespace MoreDoors.Rando
{
    public static class RandoInterop
    {
        public static LocalSettings LS { get; set; }

        public static bool IsEnabled => MoreDoors.GS.RandoSettings.IsEnabled;

        public static void Setup()
        {
            ConnectionMenu.Setup();
            LogicPatcher.Setup();
            RequestModifier.Setup();
            CondensedSpoilerLogger.AddCategory("MoreDoors Keys", _ => IsEnabled,
                new(DoorData.DoorNames.Select(d => DoorData.Get(d).Key.ItemName)));

            RandoController.OnExportCompleted += OnExportCompleted;
            RandomizerMod.Logging.SettingsLog.AfterLogSettings += LogSettings;
        }

        private static void OnExportCompleted(RandoController rc)
        {
            if (!IsEnabled) return;

            var mod = ItemChangerMod.Modules.GetOrAdd<MoreDoorsModule>();
            foreach (var doorName in LS.EnabledDoorNames)
            {
                mod.DoorStates[doorName] = new();
            }
            mod.AddDeployers();
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
}
