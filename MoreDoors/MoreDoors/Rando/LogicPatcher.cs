using MoreDoors.IC;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;

namespace MoreDoors.Rando
{
    public static class LogicPatcher
    {
        public static void Setup() => RCData.RuntimeLogicOverride.Subscribe(55f, ModifyLMB);

        public static void ModifyLMB(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!RandoInterop.IsEnabled) return;

            LocalSettings LS = new();
            Random r = new(gs.Seed + 13);
            int numDoors = LS.Settings.ComputeNumDoors(r);

            List<string> doors = new(DoorData.DoorNames);
            doors.Shuffle(r);
            for (int i = 0; i < numDoors && i < doors.Count; i++)
            {
                var doorName = doors[i];
                var data = DoorData.Get(doorName);
                LS.EnabledDoorNames.Add(doorName);

                // Modify transition logic for this door.
                var keyTerm = lmb.GetOrAddTerm(data.KeyLogicName);
                lmb.AddWaypoint(new(data.DoorForcedOpenLogicName, $"{data.LeftDoorLocation.TransitionName} | {data.RightDoorLocation.TransitionName}"));
                lmb.DoLogicEdit(new(data.LeftDoorLocation.TransitionName, $"ORIG + ({data.KeyLogicName} | {data.DoorForcedOpenLogicName})"));
                lmb.DoLogicEdit(new(data.RightDoorLocation.TransitionName, $"ORIG + ({data.KeyLogicName} | {data.DoorForcedOpenLogicName})"));
                lmb.AddItem(new CappedItem(data.Key.ItemName, new TermValue[]  { new(keyTerm, 1) }, new(keyTerm, 1)));

                // Add vanilla key logic defs.
                if (LS.Settings.AddKeyLocations)
                {
                    lmb.AddLogicDef(new(data.KeyLocName, data.Key.VanillaLogic));
                }
            }

            RandoInterop.LS = LS;
        }
    }
}
