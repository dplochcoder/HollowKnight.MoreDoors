using ItemChanger;
using MoreDoors.Data;
using PurenailCore.RandoUtil;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerCore.StringLogic;
using RandomizerMod.Menu;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;
using StartDef = RandomizerMod.RandomizerData.StartDef;

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
            RCData.RuntimeLogicOverride.Subscribe(55f, ModifyCoreDefinitions);

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

        private delegate StartDef StartModifier(StartDef startDef);

        private static StartModifier ForbidWithMoreDoors(string unless = "FALSE") => sd => sd with
        {
            RandoLogic = $"({sd.RandoLogic ?? sd.Logic}) + ({MoreDoorsRando}=0 | {unless})"
        };

        private static readonly Dictionary<string, StartModifier> StartModifiers = new()
        {
            { "Abyss", sd => sd with { Transition = "Abyss_06_Core[left3]" } },
            { "East Fog Canyon", ForbidWithMoreDoors("ROOMRANDO") },
            { "Hallownest's Crown", ForbidWithMoreDoors("ROOMRANDO") },
            { "Hive", ForbidWithMoreDoors("ROOMRANDO") },
            { "Lower Greenpath", ForbidWithMoreDoors("ROOMRANDO") },
            { "Mantis Village", ForbidWithMoreDoors() },
            { "Queens's Gardens", ForbidWithMoreDoors("ROOMRANDO") },
            { "Queen's Station", ForbidWithMoreDoors("ROOMRANDO") },
            { "West Blue Lake", ForbidWithMoreDoors() },
            { "West Fog Canyon", ForbidWithMoreDoors("ROOMRANDO") }
        };
        private static void PatchStartLocations(Dictionary<string, StartDef> startDefs)
        {
            List<string> keys = new(startDefs.Keys);
            foreach (var start in keys)
            {
                if (StartModifiers.TryGetValue(start, out StartModifier ms))
                {
                    var sd = startDefs[start];
                    startDefs[start] = ms(sd);
                }
            }
        }

        private static void HandleTransition(LogicManagerBuilder lmb, DoorData data, DoorData.DoorInfo.Location doorLoc, HashSet<string> fixedTerms, Dictionary<string, SimpleToken> replacementMap)
        {
            fixedTerms.Add(data.DoorOpenedWaypoint);
            fixedTerms.Add(doorLoc.TransitionName);
            fixedTerms.Add(doorLoc.TransitionProxyName);
            replacementMap[doorLoc.TransitionName] = new(doorLoc.TransitionProxyName);

            lmb.GetOrAddTerm(doorLoc.TransitionProxyName, TermType.State);
            lmb.AddWaypoint(new(doorLoc.TransitionProxyName, lmb.LogicLookup[doorLoc.TransitionName].ToInfix()));
            lmb.DoLogicEdit(new(doorLoc.TransitionProxyName, $"ORIG | {doorLoc.TransitionName}"));

            string lanternClause = doorLoc.RequiresLantern ? " + LANTERN" : "";
            lmb.AddLogicDef(new(doorLoc.TransitionName, $"{doorLoc.TransitionName} | {doorLoc.TransitionProxyName}{lanternClause} + ({data.KeyTermName} | {data.DoorOpenedWaypoint})"));
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
            LS.EnabledDoorNames = LS.Settings.ComputeActiveDoors(gs, r);

            foreach (var doorName in DoorData.DoorNames)
            {
                var data = DoorData.Get(doorName);

                if (LS.IncludeDoor(doorName))
                {
                    // Modify transition logic for this door.
                    var keyTerm = lmb.GetOrAddTerm(data.KeyTermName);
                    lmb.AddWaypoint(new(data.DoorOpenedWaypoint, $"{data.Door.LeftLocation.TransitionName} | {data.Door.RightLocation.TransitionName}"));
                    HandleTransition(lmb, data, data.Door.LeftLocation, LS.ModifiedLogicNames, LS.LogicSubstitutions);
                    HandleTransition(lmb, data, data.Door.RightLocation, LS.ModifiedLogicNames, LS.LogicSubstitutions);
                    lmb.AddItem(new CappedItem(data.Key.ItemName, new TermValue[] { new(keyTerm, 1) }, new(keyTerm, 1)));

                    // Modify the infection wall.
                    if (doorName == "False")
                    {
                        // The right side of the infection wall is in logic only through defeating false knight.
                        lmb.DoLogicEdit(new("Crossroads_10[left1]", "ORIG + (ROOMRANDO | Defeated_False_Knight)"));
                        // The left side of the infection wall is only reachable if the right side is reachable.
                        lmb.DoLogicEdit(new("Crossroads_06[right1]", "ORIG + Crossroads_10[left1]"));
                    }
                }

                // Add vanilla key logic defs.
                if (LS.IncludeKeyLocation(doorName))
                {
                    lmb.AddLogicDef(new(data.Key.Location.name, data.Key.Logic));
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
            LogicReplacer replacer = new();
            replacer.IgnoredNames = new(RandoInterop.LS.ModifiedLogicNames);
            replacer.SimpleTokenReplacements = new(RandoInterop.LS.LogicSubstitutions);
            replacer.Apply(lmb);

            // We don't need this data any more, get rid of it.
            RandoInterop.LS.ModifiedLogicNames.Clear();
            RandoInterop.LS.LogicSubstitutions.Clear();
        }
    }
}
