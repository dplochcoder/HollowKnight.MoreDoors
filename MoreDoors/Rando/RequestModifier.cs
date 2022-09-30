using ItemChanger.Internal.Menu;
using MoreDoors.Data;
using RandomizerCore.Randomization;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;

namespace MoreDoors.Rando
{
    public class RequestModifier
    {
        public static void Setup()
        {
            RequestBuilder.OnUpdate.Subscribe(-500f, SetupRefs);
            RequestBuilder.OnUpdate.Subscribe(40f, ModifyItems);
            RequestBuilder.OnUpdate.Subscribe(102f, DerangeKeys);
        }

        private static void SetupRefs(RequestBuilder rb)
        {
            if (!RandoInterop.IsEnabled) return;

            foreach (var doorName in DoorData.DoorNames)
            {
                var data = DoorData.Get(doorName);
                if (RandoInterop.LS.IncludeDoor(doorName))
                {
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
                }

                if (RandoInterop.LS.IncludeKeyLocation(doorName))
                {
                    rb.EditLocationRequest(data.Key.Location.name, info =>
                    {
                        info.getLocationDef = () => new()
                        {
                            Name = data.Key.Location.name,
                            SceneName = data.Key.Location.sceneName
                        };
                    });
                }
            }
        }

        private static void ModifyItems(RequestBuilder rb)
        {
            if (!RandoInterop.IsEnabled) return;

            if (!rb.gs.PoolSettings.Keys && RandoInterop.LS.Settings.AddKeyLocations == AddKeyLocations.None)
            {
                throw new ArgumentException($"Nowhere to place MoreDoors Keys; Either randomize Keys or enable 'Add Key Locations'");
            }

            foreach (var doorName in DoorData.DoorNames)
            {
                var data = DoorData.Get(doorName);
                if (RandoInterop.LS.IncludeDoor(doorName))
                {
                    if (rb.gs.PoolSettings.Keys)
                    {
                        rb.AddItemByName(data.Key.ItemName);
                        if (rb.gs.DuplicateItemSettings.DuplicateUniqueKeys)
                        {
                            rb.AddItemByName($"{PlaceholderItem.Prefix}{data.Key.ItemName}");
                        }

                        if (RandoInterop.LS.IncludeKeyLocation(doorName))
                        {
                            rb.AddLocationByName(data.Key.Location.name);
                        }
                    }
                    else if (RandoInterop.LS.IncludeKeyLocation(doorName))
                    {
                        rb.AddToVanilla(new(data.Key.ItemName, data.Key.Location.name));
                    }
                }
                else if (RandoInterop.LS.IncludeKeyLocation(doorName))
                {
                    rb.AddLocationByName(data.Key.Location.name);
                }
            }
        }

        private static void DerangeKeys(RequestBuilder rb)
        {
            if (!RandoInterop.IsEnabled || !rb.gs.PoolSettings.Keys || !rb.gs.CursedSettings.Deranged) return;

            Dictionary<string, string> keyLoc = new();
            foreach (var door in DoorData.DoorNames)
            {
                var data = DoorData.Get(door);
                keyLoc[data.Key.ItemName] = data.Key.Location.name;
            }

            foreach (var gb in rb.EnumerateItemGroups())
            {
                if (gb.strategy is DefaultGroupPlacementStrategy dgps)
                {
                    dgps.Constraints += (item, loc) =>
                    {
                        string name = item.Name;
                        if (item.Name.StartsWith(PlaceholderItem.Prefix)) name = item.Name.Substring(PlaceholderItem.Prefix.Length);
                        return !keyLoc.TryGetValue(item.Name, out string vLoc) || loc.Name != vLoc;
                    };
                }
            }
        }
    }
}
