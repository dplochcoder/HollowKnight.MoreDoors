using FStats;
using FStats.StatControllers;
using FStats.Util;
using ItemChanger;
using MoreDoors.IC;
using System.Collections.Generic;
using System.Linq;

namespace MoreDoors.FStats
{
    internal class MoreKeysStats : StatController
    {
        public record KeyCollection
        {
            public string keyName;
            public float time;
        }

        public List<KeyCollection> KeyCollections = new();
        public HashSet<string> Keys = new();

        public override IEnumerable<DisplayInfo> GetDisplayInfos()
        {
            List<string> rows = KeyCollections.OrderBy(kc => kc.time).Select(kc => $"{kc.keyName}: {kc.time.PlaytimeHHMMSS()}").ToList();
            if (rows.Count == 0) return new List<DisplayInfo>();

            return Paginate(rows, "");
        }

        private const int COL_LENGTH = 8;

        private IEnumerable<DisplayInfo> Paginate(List<string> rows, string suffix)
        {
            if (rows.Count <= COL_LENGTH * 2)
            {
                List<string> cols = new();
                if (rows.Count <= COL_LENGTH)
                {
                    cols.Add(string.Join("\n", rows));
                }
                else
                {
                    cols.Add(string.Join("\n", rows.Slice(0, 2)));
                    cols.Add(string.Join("\n", rows.Slice(1, 2)));
                }

                yield return new()
                {
                    Title = $"More Keys Timeline{suffix}",
                    MainStat = $"Keys Collected: {KeyCollections.Count} of {ItemChangerMod.Modules.Get<MoreDoorsModule>().DoorStates.Count}",
                    StatColumns = cols,
                    Priority = BuiltinScreenPriorityValues.ExtensionStats
                };
                yield break;
            }

            int pageSize = COL_LENGTH * 2;
            for (int i = 0; i < rows.Count; i += pageSize)
            {
                foreach (var di in Paginate(rows.GetRange(i, pageSize), $" ({(i + pageSize) / pageSize} of {(rows.Count + pageSize - 1) / pageSize})"))
                {
                    yield return di;
                }
            }
        }

        public override void Initialize() => MoreDoorsModule.OnKeyObtained += OnKeyObtained;

        public override void Unload() => MoreDoorsModule.OnKeyObtained -= OnKeyObtained;

        private void OnKeyObtained(string keyName)
        {
            if (Keys.Contains(keyName)) return;

            Keys.Add(keyName);
            KeyCollections.Add(new()
            {
                keyName = keyName,
                time = Common.Instance.CountedTime
            });
        }
    }

    public static class FStatsInterop
    {
        public static void Setup() => API.OnGenerateFile += gen => gen(new MoreKeysStats());
    }
}
