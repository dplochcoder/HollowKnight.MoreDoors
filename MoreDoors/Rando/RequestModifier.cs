using MoreDoors.IC;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using System;

namespace MoreDoors.Rando
{
    public class RequestModifier
    {
        public static void Setup()
        {
            RequestBuilder.OnUpdate.Subscribe(-500f, SetupRefs);
            RequestBuilder.OnUpdate.Subscribe(40f, ModifyItems);
        }

        private static void SetupRefs(RequestBuilder rb)
        {
            if (!RandoInterop.IsEnabled) return;

            foreach (var doorName in RandoInterop.LS.EnabledDoorNames)
            {
                var data = DoorData.Get(doorName);
                rb.EditItemRequest(data.Key.ItemName, info =>
                {
                    info.getItemDef = () => new()
                    {
                        Name = data.Key.ItemName,
                        Pool = PoolNames.Key,
                        MajorItem = false,
                        PriceCap = 400
                    };
                });

                if (RandoInterop.LS.Settings.AddKeyLocations)
                {
                    rb.EditLocationRequest(data.KeyLocationName, info =>
                    {
                        info.getLocationDef = () => new()
                        {
                            Name = data.KeyLocationName,
                            SceneName = data.Key.VanillaLocation.sceneName
                        };
                    });
                }
            }
        }

        private static void ModifyItems(RequestBuilder rb)
        {
            if (!RandoInterop.IsEnabled) return;

            if (!rb.gs.PoolSettings.Keys && !RandoInterop.LS.Settings.AddKeyLocations)
            {
                throw new ArgumentException($"Nowhere to place MoreDoors Keys; Either randomize Keys or set 'Add Key Locations'");
            }

            foreach (var doorName in RandoInterop.LS.EnabledDoorNames)
            {
                var data = DoorData.Get(doorName);
                if (rb.gs.PoolSettings.Keys)
                {
                    rb.AddItemByName(data.Key.ItemName);
                    if (rb.gs.DuplicateItemSettings.DuplicateUniqueKeys)
                    {
                        rb.AddItemByName($"{PlaceholderItem.Prefix}{data.Key.ItemName}");
                    }

                    if (RandoInterop.LS.Settings.AddKeyLocations)
                    {
                        rb.AddLocationByName(data.KeyLocationName);
                    }
                }
                else if (RandoInterop.LS.Settings.AddKeyLocations)
                {
                    rb.AddToVanilla(new(data.Key.ItemName, data.KeyLocationName));
                }
            }
        }
    }
}
