using ItemChanger;
using Modding;
using MoreDoors.Data;
using MoreDoors.IC;
using System.Collections.Generic;
using System.Linq;

namespace MoreDoors.Rando;

public static class Vanilla
{
    public static void Setup()
    {
        On.UIManager.StartNewGame += PlaceVanillaItems;
    }

    private static bool IsRandoSave() => RandomizerMod.RandomizerMod.RS?.GenerationSettings != null;

    private static List<string> GetRandoVanillaKeys()
    {
        if (RandomizerMod.RandomizerMod.RS.GenerationSettings.PoolSettings.Keys || MoreDoors.GS.RandoSettings.AddKeyLocations == AddKeyLocations.None) return new();
        return RandoInterop.LS?.EnabledDoorNames.ToList() ?? new();
    }

    private static void PlaceVanillaItems(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
    {
        List<AbstractPlacement> placements = new();

        bool rando = ModHooks.GetMod("Randomizer 4") is Mod && IsRandoSave();
        bool includeVanilla = MoreDoors.GS.EnableInVanilla;
        List<string> doorNames = rando ? GetRandoVanillaKeys() : (includeVanilla ? new(DoorData.Data.Keys) : new());
        foreach (var door in doorNames)
        {
            var data = DoorData.GetFromJson(door);
            placements.Add(data.Key.Location.Wrap().Add(Finder.GetItem(data.Key.ItemName)));
        }
        ItemChangerMod.AddPlacements(placements);

        if (!rando && doorNames.Count > 0)
        {
            var mod = ItemChangerMod.Modules.Add<MoreDoorsModule>();
            doorNames.ForEach(d => mod.DoorStates[d] = new(DoorData.GetFromJson(d)));
            mod.AddDeployers();
        }
        orig(self, permaDeath, bossRush);
    }
}
