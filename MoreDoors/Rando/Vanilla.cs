using ItemChanger;
using MoreDoors.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreDoors.Rando
{
    public static class Vanilla
    {
        public static void Setup()
        {
            On.UIManager.StartNewGame += PlaceVanillaItems;
        }

        private static void PlaceVanillaItems(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
        {
            if (RandomizerMod.RandomizerMod.RS?.GenerationSettings == null || !RandoInterop.IsEnabled
                || RandomizerMod.RandomizerMod.RS.GenerationSettings.PoolSettings.Keys)
            {
                orig(self, permaDeath, bossRush);
                return;
            }

            List<AbstractPlacement> placements = new();
            foreach (var door in DoorData.DoorNames)
            {
                var data = DoorData.Get(door);
                if (RandoInterop.LS.IncludeDoor(door))
                {
                    placements.Add(data.Key.Location.Wrap().Add(Finder.GetItem(data.Key.ItemName)));
                }
            }
            ItemChangerMod.AddPlacements(placements);

            orig(self, permaDeath, bossRush);
        }
    }
}
