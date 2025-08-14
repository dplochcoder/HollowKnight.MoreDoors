using FStats;
using FStats.StatControllers;
using FStats.Util;
using ItemChanger;
using MoreDoors.IC;
using System.Collections.Generic;
using System.Linq;

namespace MoreDoors.FStatsInterop;

internal class MoreKeysStats : StatController
{
    public record KeyCollection
    {
        public string keyName = "";
        public float time;
    }

    public List<KeyCollection> KeyCollections = [];
    public HashSet<string> Keys = [];

    public override IEnumerable<DisplayInfo> GetDisplayInfos()
    {
        List<string> rows = [.. KeyCollections.OrderBy(kc => kc.time).Select(kc => $"{kc.keyName}: {kc.time.PlaytimeHHMMSS()}")];
        if (rows.Count == 0) yield break;
        
        yield return new()
        {
            Title = $"More Keys Timeline",
            MainStat = $"Keys Collected: {KeyCollections.Count} of {ItemChangerMod.Modules.Get<MoreDoorsModule>()!.DoorStates.Count}",
            StatColumns = Columnize(rows),
            Priority = BuiltinScreenPriorityValues.ExtensionStats
        };
    }

    private const int COL_SIZE = 10;

    private List<string> Columnize(List<string> rows)
    {
        int numCols = (rows.Count + COL_SIZE - 1) / COL_SIZE;
        List<string> list = [];
        for (int i = 0; i < numCols; i++)
        {
            list.Add(string.Join("\n", rows.Slice(i, numCols)));
        }
        return list;
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
            time = FStatsMod.LS.Get<Common>().CountedTime
        });
    }
}

public static class FStatsInterop
{
    public static void Setup() => API.OnGenerateFile += gen => gen(new MoreKeysStats());
}
