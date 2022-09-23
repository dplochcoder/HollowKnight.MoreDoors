using ItemChanger;
using MoreDoors.Data;
using RandomizerCore;
using RandomizerCore.Extensions;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerCore.StringLogic;
using RandomizerMod.Menu;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;

namespace MoreDoors.Rando
{
    public static class LogicPatcher
    {
        private const string MoreDoorsRando = "MOREDOORSRANDO";

        public static void Setup()
        {
            RandomizerMenuAPI.OnGenerateStartLocationDict += PatchStartLocations;
            SettingsPM.OnResolveIntTerm += ResolveMoreDoorsRando;

            // This should run before DarknessRandomizer and/or RandoPlus
            RCData.RuntimeLogicOverride.Subscribe(35f, ModifyCoreDefinitions);

            // Updating everything to use proxies should run after.
            RCData.RuntimeLogicOverride.Subscribe(100f, SubstituteProxies);
        }

        private static bool ResolveMoreDoorsRando(string term, out int result)
        {
            if (term == MoreDoorsRando)
            {
                result = RandoInterop.IsEnabled ? 1 : 0;
                return true;
            }

            result = default;
            return false;
        }

        private delegate RandomizerMod.RandomizerData.StartDef ModifyStart(RandomizerMod.RandomizerData.StartDef startDef);

        private static ModifyStart ForbidWithMoreDoors(string unless = "FALSE") => sd => sd with
        {
            RandoLogic = $"({sd.RandoLogic ?? sd.Logic}) + ({MoreDoorsRando}=0 | {unless})"
        };

        private static readonly Dictionary<string, ModifyStart> StartModifiers = new()
        {
            {"Abyss", sd => sd with { Transition = "Abyss_06_Core[left3]" } },
            {"Hallownest's Crown", ForbidWithMoreDoors("ROOMRANDO") },
            {"Hive", ForbidWithMoreDoors("ROOMRANDO") },
            {"Mantis Village", ForbidWithMoreDoors() },
            {"Queens's Gardens", ForbidWithMoreDoors("ROOMRANDO") },
            {"Queen's Station", ForbidWithMoreDoors("ROOMRANDO") },
            {"West Blue Lake", ForbidWithMoreDoors() },
            {"West Fog Canyon", ForbidWithMoreDoors("ROOMRANDO") }
        };

        private static void PatchStartLocations(Dictionary<string, RandomizerMod.RandomizerData.StartDef> startDefs)
        {
            List<string> keys = new(startDefs.Keys);
            foreach (var start in keys)
            {
                if (StartModifiers.TryGetValue(start, out ModifyStart ms))
                {
                    var sd = startDefs[start];
                    startDefs[start] = ms(sd);
                }
            }
        }

        private static void HandleTransition(LogicManagerBuilder lmb, DoorData data, DoorData.DoorLocation doorLoc, HashSet<string> fixedTerms, Dictionary<string, string> replacementMap)
        {
            fixedTerms.Add(doorLoc.TransitionName);
            fixedTerms.Add(doorLoc.TransitionProxyName);
            fixedTerms.Add(data.DoorForcedOpenLogicName);
            replacementMap[doorLoc.TransitionName] = doorLoc.TransitionProxyName;

            lmb.AddWaypoint(new(doorLoc.TransitionProxyName, lmb.LogicLookup[doorLoc.TransitionName].ToInfix()));
            lmb.DoLogicEdit(new(doorLoc.TransitionProxyName, $"ORIG | {doorLoc.TransitionName}"));

            string lanternClause = doorLoc.RequiresLantern ? " + LANTERN" : "";
            lmb.AddLogicDef(new(doorLoc.TransitionName, $"{doorLoc.TransitionName} | {doorLoc.TransitionProxyName}{lanternClause} + ({data.KeyTermName} | {data.DoorForcedOpenLogicName})"));
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

        public static void ModifyCoreDefinitions(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!RandoInterop.IsEnabled) return;

            // TODO: Define this upstream.
            lmb.LP.SetMacro(
                "COMBAT[Shrumal_Ogre]",
                "SIDESLASH | UPSLASH | CYCLONE | GREATSLASH | FULLDASHSLASH | ANYDASHSLASH + DASHMASTER + OBSCURESKIPS | SPICYCOMBATSKIPS");

            LocalSettings LS = new();
            Random r = new(gs.Seed + 13);
            int numDoors = LS.Settings.ComputeNumDoors(r);

            List<string> doors = new(DoorData.DoorNames);
            doors.Shuffle(r);
            foreach (var doorName in doors.Slice(0, numDoors)) LS.EnabledDoorNames.Add(doorName);

            foreach (var doorName in DoorData.DoorNames)
            {
                var data = DoorData.Get(doorName);

                if (LS.IncludeDoor(doorName))
                {
                    // Modify transition logic for this door.
                    var keyTerm = lmb.GetOrAddTerm(data.KeyTermName);
                    lmb.AddWaypoint(new(data.DoorForcedOpenLogicName, $"{data.LeftDoorLocation.TransitionName} | {data.RightDoorLocation.TransitionName}"));

                    // Replace the transition waypoints with proxies.
                    HandleTransition(lmb, data, data.LeftDoorLocation, LS.ModifiedLogicNames, LS.LogicSubstitutions);
                    HandleTransition(lmb, data, data.RightDoorLocation, LS.ModifiedLogicNames, LS.LogicSubstitutions);

                    lmb.AddItem(new CappedItem(data.Key.ItemName, new TermValue[] { new(keyTerm, 1) }, new(keyTerm, 1)));
                }

                // Add vanilla key logic defs.
                if (LS.IncludeKeyLocation(doorName))
                {
                    lmb.AddLogicDef(new(data.KeyLocationName, data.Key.Logic));
                }
            }

            RandoInterop.LS = LS;
        }

        public static void SubstituteProxies(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!RandoInterop.IsEnabled) return;

            // Substitute proxies.
            //
            // Some logic is lazy and will list a single transition for access even when multiple transitions grant access, because it
            // assumes that the listed transition implies access to the others. This is not true when said transitions are blocked off by
            // MoreDoors placements, so we introduce a separate proxy waypoint to mean 'access-to-the-door' as opposed to
            // 'access-to-the-transition'.
            List<string> names = new(lmb.LogicLookup.Keys);
            foreach (var name in names)
            {
                if (RandoInterop.LS.ModifiedLogicNames.Contains(name)) continue;
                lmb.LogicLookup[name] = SubstituteSimpleTokens(RandoInterop.LS.LogicSubstitutions, lmb.LogicLookup[name]);
            }

            // We don't need this data any more, get rid of it.
            RandoInterop.LS.ModifiedLogicNames = null;
            RandoInterop.LS.LogicSubstitutions = null;
        }
    }
}
