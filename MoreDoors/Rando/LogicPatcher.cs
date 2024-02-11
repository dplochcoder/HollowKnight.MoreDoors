using MoreDoors.Data;
using PurenailCore.RandoUtil;
using PurenailCore.SystemUtil;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerCore.StringLogic;
using RandomizerMod.Menu;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System.Collections.Generic;
using System.Linq;
using StartDef = RandomizerMod.RandomizerData.StartDef;

namespace MoreDoors.Rando;

public static class LogicPatcher
{
    public static void Setup()
    {
        RandomizerMenuAPI.OnGenerateStartLocationDict += PatchStartLocations;
        SettingsPM.OnResolveIntTerm += ResolveMoreDoorsRando;

        // This should run before DarknessRandomizer and/or RandoPlus
        RCData.RuntimeLogicOverride.Subscribe(55f, ModifyCoreDefinitions);

        // Updating everything to use proxies should run after.
        RCData.RuntimeLogicOverride.Subscribe(100f, SubstituteProxies);
    }

    private const string TERM_PREFIX = "MOREDOORSRANDO[";

    private static bool ParseMoreDoorsTerm(string term, out string doorName)
    {
        if (term.StartsWith(TERM_PREFIX) && term.EndsWith("]"))
        {
            doorName = term.Substring(TERM_PREFIX.Length, term.Length - TERM_PREFIX.Length - 1);
            return true;
        }

        doorName = "";
        return false;
    }

    private static bool ResolveMoreDoorsRando(string term, out int result)
    {
        if (ParseMoreDoorsTerm(term, out var doorName))
        {
            result = (RandoInterop.IsEnabled && RandoInterop.LS != null && RandoInterop.LS.EnabledDoorNames.Contains(doorName)) ? 1 : 0;
            return true;
        }

        result = default;
        return false;
    }

    private delegate StartDef StartModifier(StartDef startDef);

    private static StartModifier ForbidUnless(string exception, params string[] doorNames)
    {
        var clauses = doorNames.Select(n => $"MOREDOORSRANDO[{n}]=0").ToArray();
        var doorsClause = string.Join(" + ", clauses);
        return sd => sd with
        {
            RandoLogic = $"({sd.RandoLogic ?? sd.Logic}) + (({doorsClause}) | {exception})"
        };
    }

    private static StartModifier Forbid(params string[] doorNames) => ForbidUnless("FALSE", doorNames);

    private static StartModifier ForbidUnlessRoomRando(params string[] doorNames) => ForbidUnless("ROOMRANDO", doorNames);

    private static readonly Dictionary<string, StartModifier> StartModifiers = new()
    {
        { "Distant Village", Forbid(DoorNames.VILLAGE) },
        { "East Fog Canyon", ForbidUnlessRoomRando(DoorNames.ARCHIVE) },
        { "Hallownest's Crown", ForbidUnlessRoomRando(DoorNames.CROWN) },
        { "Kingdom's Edge", ForbidUnlessRoomRando(DoorNames.BARDOON) },
        { "Lower Greenpath", ForbidUnlessRoomRando(DoorNames.MOSS) },
    };
    private static void PatchStartLocations(Dictionary<string, StartDef> startDefs)
    {
        // Forbid starting in a room with a door in it, to be safe.
        Dictionary<string, List<string>> sceneToDoors = new();
        foreach (var data in DoorData.Data)
        {
            var door = data.Value.Door;
            sceneToDoors.GetOrAddNew(door.LeftSceneName).Add(data.Key);
            sceneToDoors.GetOrAddNew(door.RightSceneName).Add(data.Key);
        }

        List<string> keys = new(startDefs.Keys);
        foreach (var startName in keys)
        {
            var start = startDefs[startName];

            if (sceneToDoors.TryGetValue(start.SceneName, out var doors)) startDefs[startName] = Forbid(doors.ToArray())(startDefs[startName]);
            if (StartModifiers.TryGetValue(startName, out var modifier)) startDefs[startName] = modifier(startDefs[startName]);
        }
    }
    private static void HandleDoorLogic(LogicManagerBuilder lmb, DoorData data, HashSet<string> fixedTerms, Dictionary<string, SimpleToken> replacementMap)
    {
        switch (data.Door.Mode)
        {
            case DoorData.DoorInfo.SplitMode.Normal:
                {
                    HandleTransition(lmb, data, data.Door.LeftLocation, fixedTerms, replacementMap);
                    HandleTransition(lmb, data, data.Door.RightLocation, fixedTerms, replacementMap);
                    break;
                }
            case DoorData.DoorInfo.SplitMode.LeftTwin:
                {
                    HandleTransition(lmb, data, data.Door.LeftLocation, fixedTerms, replacementMap);
                    break;
                }
            case DoorData.DoorInfo.SplitMode.RightTwin:
                {
                    HandleTransition(lmb, data, data.Door.RightLocation, fixedTerms, replacementMap);
                    break;
                }
        }
    }

    private static void HandleTransition(LogicManagerBuilder lmb, DoorData data, DoorData.DoorInfo.Location doorLoc, HashSet<string> fixedTerms, Dictionary<string, SimpleToken> replacementMap)
    {
        fixedTerms.Add(doorLoc.TransitionName);
        fixedTerms.Add(doorLoc.TransitionProxyName);
        replacementMap[doorLoc.TransitionName] = new(doorLoc.TransitionProxyName);
        replacementMap[$"{doorLoc.TransitionName}/"] = new($"{doorLoc.TransitionProxyName}/");

        lmb.GetOrAddTerm(doorLoc.TransitionName, TermType.State);
        lmb.AddWaypoint(new(doorLoc.TransitionProxyName, lmb.LogicLookup[doorLoc.TransitionName].ToInfix()));

        bool split = data.Door.Mode == DoorData.DoorInfo.SplitMode.Normal;
        string lanternClause = doorLoc.RequiresLantern ? " + LANTERN" : "";
        if (split) lmb.DoLogicEdit(new(doorLoc.TransitionProxyName, $"ORIG | {doorLoc.TransitionName}"));
        else lmb.DoLogicEdit(new(doorLoc.TransitionProxyName, $"ORIG + {data.KeyTermName} | {doorLoc.TransitionName}{lanternClause} + {data.KeyTermName}"));

        lmb.AddLogicDef(new(doorLoc.TransitionName, $"{doorLoc.TransitionName} | {doorLoc.TransitionProxyName}{lanternClause} + {data.KeyTermName}"));
    }

    public static void ModifyCoreDefinitions(GenerationSettings gs, LogicManagerBuilder lmb)
    {
        if (!RandoInterop.IsEnabled) return;

        // TODO: Define this upstream.
        lmb.LP.SetMacro(
            "COMBAT[Shrumal_Ogre]",
            "SIDESLASH | UPSLASH | CYCLONE | GREATSLASH | FULLDASHSLASH | ANYDASHSLASH + DASHMASTER + OBSCURESKIPS | SPICYCOMBATSKIPS");

        var ls = RandoInterop.LS;
        foreach (var e in DoorData.Data)
        {
            var doorName = e.Key;
            var data = e.Value;

            if (ls.IncludeDoor(doorName))
            {
                // Modify transition logic for this door.
                var keyTerm = lmb.GetOrAddTerm(data.KeyTermName);
                HandleDoorLogic(lmb, data, ls.ModifiedLogicNames, ls.LogicSubstitutions);
                lmb.AddItem(new CappedItem(data.Key.ItemName, new TermValue[] { new(keyTerm, 1) }, new(keyTerm, 1)));

                // Modify the infection wall.
                if (doorName == DoorNames.FALSE)
                {
                    // The right side of the infection wall is in logic only through defeating false knight.
                    lmb.DoLogicEdit(new("Crossroads_10[left1]", "ORIG + (ROOMRANDO | Defeated_False_Knight)"));
                    // The left side of the infection wall is only reachable if the right side is reachable.
                    lmb.DoLogicEdit(new("Crossroads_06[right1]", "ORIG + Crossroads_10[left1]"));
                }
            }

            // Add vanilla key logic defs.
            if (ls.IncludeKeyLocation(doorName)) lmb.AddLogicDef(new(data.Key.Location.name, data.Key.Logic));
        }
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
