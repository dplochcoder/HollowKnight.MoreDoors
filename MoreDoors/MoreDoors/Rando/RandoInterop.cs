using ItemChanger;
using MoreDoors.IC;
using Newtonsoft.Json;
using RandomizerMod.RC;
using System.IO;

namespace MoreDoors.Rando
{
    public static class RandoInterop
    {
        public static LocalSettings LS { get; set; }

        public static bool IsEnabled => MoreDoors.GS.MoreDoorsSettings.AddMoreDoors;

        public static void Setup()
        {
            ConnectionMenu.Setup();
            LogicPatcher.Setup();
            RequestModifier.Setup();

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
