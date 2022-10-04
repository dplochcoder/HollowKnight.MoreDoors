using ItemChanger;
using MoreDoors.Data;
using MoreDoors.IC;
using RandomizerCore.Randomization;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MoreDoors.Rando
{
    public class RequestModifier
    {
        public static void Setup()
        {
            RequestBuilder.OnUpdate.Subscribe(-751f, SetupRefs);
            RequestBuilder.OnUpdate.Subscribe(-750f, ApplyTransitionRando);
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

        private const string TRANSITION_STAGE_NAME = "More Doors Transition Stage";

        private static IEnumerable<string> LeftTransitions() => RandoInterop.LS.EnabledDoorNames.Select(d => DoorData.Get(d).Door.LeftLocation.TransitionName);

        private static IEnumerable<string> RightTransitions() => RandoInterop.LS.EnabledDoorNames.Select(d => DoorData.Get(d).Door.RightLocation.TransitionName);

        private static void ApplyTransitionRando(RequestBuilder rb)
        {
            var ts = rb.gs.TransitionSettings;
            if (!MoreDoors.GS.RandoSettings.RandomizeDoorTransitions || ts.Mode != TransitionSettings.TransitionMode.None)
            {
                return;
            }

            foreach (var door in RandoInterop.LS.EnabledDoorNames)
            {
                var data = DoorData.Get(door);
                VanillaDef left = new(data.Door.LeftLocation.TransitionName, data.Door.RightLocation.TransitionName);
                VanillaDef right = new(data.Door.LeftLocation.TransitionName, data.Door.RightLocation.TransitionName);
                rb.RemoveFromVanilla(left);
                rb.RemoveFromVanilla(right);
            }

            // Insert stage at the start because it's a lot more restricted than the item placements
            StageBuilder sb = rb.InsertStage(0, TRANSITION_STAGE_NAME);

            GroupBuilder builder = null;

            if (ts.TransitionMatching == TransitionSettings.TransitionMatchingSetting.NonmatchingDirections)
            {
                SelfDualTransitionGroupBuilder b = new()
                {
                    label = "More Doors Transition Group",
                    stageLabel = TRANSITION_STAGE_NAME,
                    coupled = ts.Coupled,
                };
                b.Transitions.AddRange(LeftTransitions());
                b.Transitions.AddRange(RightTransitions());
                builder = b;
            }
            else
            {
                SymmetricTransitionGroupBuilder b = new()
                {
                    label = "More Doors Left Transition Group",
                    reverseLabel = "More Doors Right Transition Group",
                    coupled = ts.Coupled,
                    stageLabel = TRANSITION_STAGE_NAME
                };
                b.Group1.AddRange(LeftTransitions());
                b.Group2.AddRange(RightTransitions());
                builder = b;
            }

            builder.strategy = rb.gs.ProgressionDepthSettings.GetTransitionPlacementStrategy();
            sb.Add(builder);

            HashSet<string> doorTransitions = new();
            LeftTransitions().ToList().ForEach(t => doorTransitions.Add(t));
            RightTransitions().ToList().ForEach(t => doorTransitions.Add(t));

            bool MatchedTryResolveGroup(RequestBuilder rb, string item, RequestBuilder.ElementType type, out GroupBuilder gb)
            {
                gb = builder;
                return doorTransitions.Contains(item);
            }
            rb.OnGetGroupFor.Subscribe(-1000f, MatchedTryResolveGroup);
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
