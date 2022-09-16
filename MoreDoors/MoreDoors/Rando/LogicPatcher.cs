using MoreDoors.IC;
using RandomizerCore.Logic;
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
                var term = lmb.GetOrAddTerm(data.Key.LogicTerm);
                lmb.DoLogicEdit(new(data.LeftDoorLocation.TransitionName, $"ORIG + {data.Key.LogicTerm}"));
                lmb.DoLogicEdit(new(data.RighttDoorLocation.TransitionName, $"ORIG + {data.Key.LogicTerm}"));

                // Add vanilla key logic defs.
                if (LS.Settings.AddKeyLocations)
                {
                    lmb.AddLogicDef(new(data.LocName, data.Key.VanillaLogic));
                }
            }

            RandoInterop.LS = LS;
        }
    }
}
