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
        if (RandomizerMod.RandomizerMod.RS.GenerationSettings.PoolSettings.Keys || MoreDoors.GS.RandoSettings.AddKeyLocations == AddKeyLocations.None) return [];
        return RandoInterop.LS?.EnabledDoorNames.ToList() ?? [];
    }

    private static void PlaceVanillaItems(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
    {
        List<AbstractPlacement> placements = [];

        bool rando = ModHooks.GetMod("Randomizer 4") is Mod && IsRandoSave();
        bool includeVanilla = MoreDoors.GS.EnableInVanilla;
        List<string> doorNames = rando ? GetRandoVanillaKeys() : (includeVanilla ? new(DoorData.AllDoors().Keys) : new());
        foreach (var door in doorNames)
        {
            var data = DoorData.GetDoor(door)!;
            placements.Add(data.Key!.Location!.Wrap().Add(Finder.GetItem(data.Key!.ItemName)!));
        }

        ItemChangerMod.CreateSettingsProfile(false);
        ItemChangerMod.AddPlacements(placements);

        if (!rando && doorNames.Count > 0)
        {
            var mod = ItemChangerMod.Modules.Add<MoreDoorsModule>();
            doorNames.ForEach(d => mod.DoorStates[d] = new());
        }
        orig(self, permaDeath, bossRush);
    }
}
