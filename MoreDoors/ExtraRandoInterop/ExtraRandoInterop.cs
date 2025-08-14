using ExtraRando.Data;
using ExtraRando.ModInterop.ItemChangerInterop.Modules;
using MoreDoors.IC;
using MoreDoors.Rando;
using RandomizerCore.Logic;
using System;

namespace MoreDoors.ExtraRandoInterop;

internal class ExtraRandoInterop
{
    internal static void Setup() => VictoryModule.RequestConditions += list => list.Add(new MoreKeysVictory());
}

internal class MoreKeysVictory : IVictoryCondition
{
    public int CurrentAmount { get; set; }

    public int RequiredAmount { get; set; }

    public int ClampAvailableRange(int setAmount) => Math.Min(Data.DoorData.All().Count, Math.Max(setAmount, 0));

    public string GetHintText() => this.GenerateHintText("The keys can be found at:", item => item is KeyItem);

    public string GetMenuName() => "More Doors";

    public string PrepareLogic(LogicManagerBuilder logicBuilder) => $"{LogicPatcher.MORE_KEYS_TERM} > {RequiredAmount - 1}";

    public void StartListening() => MoreDoorsModule.OnKeyObtained += UpdateKeysCount;

    public void StopListening() => MoreDoorsModule.OnKeyObtained -= UpdateKeysCount;

    private void UpdateKeysCount(string uiName)
    {
        ++CurrentAmount;
        this.CheckForEnding();
    }
}
