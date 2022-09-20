using ItemChanger;
using Modding;
using MoreDoors.IC;
using Newtonsoft.Json;
using RandomizerMod.RC;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MoreDoors.Rando
{
    public static class RandoInterop
    {
        public static LocalSettings LS { get; set; }

        public static bool IsEnabled => MoreDoors.GS.MoreDoorsSettings.DoorsLevel != DoorsLevel.NoDoors || MoreDoors.GS.MoreDoorsSettings.AddKeyLocations != AddKeyLocations.None;

        public static void Setup()
        {
            ConnectionMenu.Setup();
            LogicPatcher.Setup();
            RequestModifier.Setup();

            if (ModHooks.GetMod("CondensedSpoilerLogger") is Mod)
            {
                List<string> keyNames = new(DoorData.DoorNames.Select(d => DoorData.Get(d).Key.ItemName));
                CondensedSpoilerLogger.API.AddCategory("MoreDoors Keys", _ => IsEnabled, keyNames);
            }

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
