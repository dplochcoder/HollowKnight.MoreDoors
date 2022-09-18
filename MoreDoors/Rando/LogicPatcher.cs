using MoreDoors.IC;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerCore.StringLogic;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;

namespace MoreDoors.Rando
{
    public static class LogicPatcher
    {
        public static void Setup() => RCData.RuntimeLogicOverride.Subscribe(55f, ModifyLMB);

        private static void HandleTransition(LogicManagerBuilder lmb, DoorData data, DoorData.DoorLocation doorLoc, Dictionary<string, string> replacementMap)
        {
            replacementMap[doorLoc.TransitionName] = doorLoc.TransitionProxyName;
            lmb.LogicLookup[doorLoc.TransitionProxyName] = lmb.LogicLookup[doorLoc.TransitionName];
            lmb.AddLogicDef(new(doorLoc.TransitionName, $"{doorLoc.TransitionName} | {doorLoc.TransitionProxyName} + ({data.KeyLogicName} | {data.DoorForcedOpenLogicName})"));
        }

        private static LogicClause SubstituteSimpleTokens(IDictionary<string, string> replMap, LogicClause lc)
        {
            LogicClauseBuilder? lcb = null;
            for (int i = 0; i < lc.Count; i++)
            {
                var token = lc.Tokens[i];
                if (token is SimpleToken st && replMap.TryGetValue(st.Name, out string repl))
                {
                    if (lcb == null)
                    {
                        lcb = new();
                        for (int j = 0; j < i; j++) lcb.Append(lc.Tokens[j]);
                    }

                    lcb.Append(new SimpleToken(repl));
                }
                else lcb?.Append(token);
            }

            return lcb == null ? lc : new(lcb);
        }

        public static void ModifyLMB(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!RandoInterop.IsEnabled) return;

            LocalSettings LS = new();
            Random r = new(gs.Seed + 13);
            int numDoors = LS.Settings.ComputeNumDoors(r);

            List<string> doors = new(DoorData.DoorNames);
            doors.Shuffle(r);
            Dictionary<string, string> replacementMap = new();
            for (int i = 0; i < numDoors && i < doors.Count; i++)
            {
                var doorName = doors[i];
                var data = DoorData.Get(doorName);
                LS.EnabledDoorNames.Add(doorName);

                // Modify transition logic for this door.
                var keyTerm = lmb.GetOrAddTerm(data.KeyLogicName);
                lmb.AddWaypoint(new(data.DoorForcedOpenLogicName, $"{data.LeftDoorLocation.TransitionName} | {data.RightDoorLocation.TransitionName}"));

                // Replace the transition waypoints with proxies.
                HandleTransition(lmb, data, data.LeftDoorLocation, replacementMap);
                HandleTransition(lmb, data, data.RightDoorLocation, replacementMap);

                lmb.AddItem(new CappedItem(data.Key.ItemName, new TermValue[]  { new(keyTerm, 1) }, new(keyTerm, 1)));

                // Add vanilla key logic defs.
                if (LS.Settings.AddKeyLocations)
                {
                    lmb.AddLogicDef(new(data.KeyLocName, data.Key.VanillaLogic));
                }
            }

            // Substitute proxies.
            //
            // Some logic is lazy and will list a single transition for access even when multiple transitions grant access, because it
            // assumes that the listed transition implies access to the others. This is not true when said transitions are blocked off by
            // MoreDoors placements, so we introduce a separate proxy waypoint to mean 'access-to-the-door' as opposed to
            // 'access-to-the-transition'.
            List<string> names = new(lmb.LogicLookup.Keys);
            foreach (var name in names)
            {
                if (replacementMap.ContainsKey(name)) continue;
                lmb.LogicLookup[name] = SubstituteSimpleTokens(replacementMap, lmb.LogicLookup[name]);
            }

            RandoInterop.LS = LS;
        }
    }
}
