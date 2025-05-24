﻿using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using MoreDoors.Data;
using MoreDoors.IC;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using EmbeddedSprite = MoreDoors.IC.EmbeddedSprite;

namespace MoreDoors;

internal record InventorySpacingParams
{
    public float xLeft;
    public float xSpace;
    public float yTop;
    public float ySpace;
    public int rowSize;
    public float keyScale;

    public static InventorySpacingParams Create(int doors)
    {
        if (doors <= 33)
        {
            return new()
            {
                xLeft = -10.98f,
                xSpace = 2.196f,
                yTop = 0.2f,
                ySpace = 2.196f,
                rowSize = 11,
                keyScale = 1.52f,
            };
        }
        else if (doors <= 52)
        {
            return new()
            {
                xLeft = -9.72f,
                xSpace = 1.62f,
                yTop = 0,
                ySpace = 1.62f,
                rowSize = 13,
                keyScale = 1.09f,
            };
        }
        else if (doors <= 95)
        {
            return new()
            {
                xLeft = -10.98f,
                xSpace = 1.22f,
                yTop = 0,
                ySpace = 1.22f,
                rowSize = 19,
                keyScale = 0.82f,
            };
        }
        else throw new ArgumentException("Too many doors");
    }

    public float X(int i) => xLeft + xSpace * (i % rowSize);
    public float Y(int i) => yTop - ySpace * (i / rowSize);

    public Vector3 KeyObtainedScale() => new(keyScale, keyScale, keyScale);

    public Vector3 KeyUsedScale() => new(keyScale * 0.72f, keyScale * 0.72f, keyScale * 0.72f);
}

public class MoreKeysPage
{
    public static readonly MoreKeysPage Instance = new();

    private class KeySlot
    {
        public readonly GameObject obj;
        public readonly GameObject img;
        public readonly GameObject check;
        public readonly SpriteRenderer spriteRenderer;

        public KeySlot(GameObject obj, GameObject img, GameObject check, SpriteRenderer spriteRenderer)
        {
            this.obj = obj;
            this.img = img;
            this.check = check;
            this.spriteRenderer = spriteRenderer;
        }
    }
    private readonly List<KeySlot> keySlots = [];
    private readonly List<string> inventoryKeys = [];

    private static readonly Color KEY_OBTAINED_COLOR = new(1, 1, 1);
    private static readonly Color KEY_USED_COLOR = new(0.25f, 0.25f, 0.25f);

    public void Update()
    {
        try
        {
            UpdateImpl();
        }
        catch (Exception e)
        {
            MoreDoors.LogError($"Error updating More Keys menu: {e}");
        }
    }

    private bool GetMod(out MoreDoorsModule? mod)
    {
        try
        {
            mod = ItemChangerMod.Modules.Get<MoreDoorsModule>();
            return mod != null;
        }
        catch (Exception)
        {
            mod = default;
            return false;
        }
    }

    private void UpdateImpl()
    {
        if (!GetMod(out var mod)) return;
        inventoryKeys.Clear();

        foreach (var e in mod!.DoorStates)
        {
            var door = e.Key;
            var dState = e.Value;
            if (dState.KeyObtained) inventoryKeys.Add(door);
        }

        var spacing = InventorySpacingParams.Create(DoorData.All().Count);
        for (int i = 0; i < DoorData.All().Count; i++)
        {
            var ks = keySlots[i];
            string? door = i < inventoryKeys.Count ? inventoryKeys[i] : null;
            if (door != null)
            {
                var ds = mod.DoorStates[door];
                ks.spriteRenderer.sprite = DoorData.GetDoor(door)!.Key!.Sprite!.Value;
                ks.spriteRenderer.color = ds.DoorOpened ? KEY_USED_COLOR : KEY_OBTAINED_COLOR;
                ks.img.transform.localScale = ds.DoorOpened ? spacing.KeyUsedScale() : spacing.KeyObtainedScale();
                ks.check.SetActive(ds.KeyObtained && ds.DoorOpened);
            }
            else
            {
                ks.spriteRenderer.sprite = emptySprite.Value;
                ks.check.SetActive(false);
            }
        }
    }

    private readonly ISprite checkSprite = new EmbeddedSprite("Checkmark");
    private readonly ISprite emptySprite = new EmbeddedSprite("UnplacedKey");
    private GameObject? keyTitle;
    private GameObject? keyDesc;

    private const int leftArrowIndex = -2;
    private const int rightArrowIndex = -3;
    private int selectedIndex = -1;

    public void GeneratePage(GameObject moreKeysPage)
    {
        try
        {
            GeneratePageImpl(moreKeysPage);
        }
        catch (Exception e)
        {
            MoreDoors.LogError($"Error setting up More Keys menu: {e}");
        }
    }

    private void GeneratePageImpl(GameObject moreKeysPage)
    {
        keySlots.Clear();
        inventoryKeys.Clear();
        selectedIndex = -1;

        keyTitle = UnityEngine.Object.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Name").gameObject);
        keyTitle.name = "More Keys Title";
        keyTitle.transform.SetParent(moreKeysPage.transform);
        keyTitle.transform.position = new(0, -1, 0.3f);
        keyTitle.transform.localScale = new(1.2f, 1.2f, 1.2f);
        keyTitle.GetComponent<TextMeshPro>().text = "";

        keyDesc = UnityEngine.Object.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Desc").gameObject);
        keyDesc.name = "More Keys Desc";
        keyDesc.transform.SetParent(moreKeysPage.transform);
        keyDesc.transform.position = new(0, -1.5f, 3.3f);
        var mesh = keyDesc.GetComponent<TextMeshPro>();
        mesh.text = "";
        mesh.alignment = TextAlignmentOptions.Top;
        keyDesc.GetComponent<TextContainer>().rect = new(0, 0, 22, 10);

        var spacing = InventorySpacingParams.Create(DoorData.All().Count);
        for (int i = 0; i < DoorData.All().Count; i++)
        {
            GameObject obj = new($"MoreDoors Menu Parent {i}");
            obj.transform.SetParent(moreKeysPage.transform);
            obj.layer = moreKeysPage.layer;
            obj.transform.position = new(spacing.X(i), spacing.Y(i), -3f);
            var bc2d = obj.AddComponent<BoxCollider2D>();
            bc2d.offset = new(0, 0);
            bc2d.size = new(1.5f, 1.5f);

            GameObject img = new($"MoreDoors Key Image {i}");
            img.transform.SetParent(obj.transform);
            img.transform.localPosition = new(0, 0, 0);
            img.transform.localScale = spacing.KeyObtainedScale();
            img.layer = moreKeysPage.layer;
            var sr = img.AddComponent<SpriteRenderer>();
            sr.sprite = emptySprite.Value;
            sr.sortingLayerID = 629535577;
            sr.sortingLayerName = "HUD";

            GameObject check = new($"MoreDoors Key Used Image {i}");
            check.transform.SetParent(img.transform);
            check.layer = moreKeysPage.layer;
            check.transform.localPosition = new(0.5f, 0.5f, 0);
            check.transform.localScale = new(0.5f, 0.5f, 0.5f);
            var sr2 = check.AddComponent<SpriteRenderer>();
            sr2.sprite = checkSprite.Value;
            sr2.sortingLayerID = 629535577;
            sr2.sortingLayerName = "HUD";
            check.SetActive(false);

            keySlots.Add(new(obj, img, check, sr));
        }

        // Removing the jump from arrow button to arrow button.
        var fsm = moreKeysPage.LocateMyFSM("Empty UI");
        fsm.GetState("L Arrow").RemoveTransitionsTo("R Arrow");
        fsm.GetState("R Arrow").RemoveTransitionsTo("L Arrow");

        FsmState initState = fsm.GetState("Init Heart Piece");
        initState.Name = "Init More Keys";
        initState.RemoveTransitionsTo("L Arrow");
        initState.AddLastAction(new Lambda(() =>
        {
            keySlots.ForEach(ks => ks.obj.SetActive(true));
            fsm.SendEvent("FINISHED");
        }));

        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Up Press",
            Actions = [new Lambda(() => HandleUpPress(fsm))]
        });
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Down Press",
            Actions = [new Lambda(() => HandleDownPress(fsm))]
        });
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Left Press",
            Actions = [new Lambda(() => HandleLeftPress(fsm))]
        });
        fsm.AddState(new FsmState(fsm.Fsm)
        {
            Name = "Right Press",
            Actions = [new Lambda(() => HandleRightPress(fsm))]
        });

        // Add states for each slot on the board.
        var rArrow = fsm.GetState("R Arrow");
        var uCursor = fsm.gameObject.LocateMyFSM("Update Cursor");
        for (int i = 0; i < DoorData.All().Count; i++)
        {
            int index = i;
            fsm.AddState(new FsmState(fsm.Fsm)
            {
                Name = $"Key {index}",
                Actions =
                [
                    new Lambda(() => uCursor.FsmVariables.FindFsmGameObject("Item").Value = keySlots[index].obj),
                    new SetSpriteRendererOrder()
                    {
                        gameObject = new() { GameObject = fsm.FsmVariables.FindFsmGameObject("Cursor Glow")},
                        order = 0,
                        delay = 0
                    },
                    new Lambda(() => uCursor.SendEvent("UPDATE CURSOR")),
                    new Lambda(() => SetSelectedKeyIndex(index)),
                ]
            });
            rArrow.AddTransition($"KEY_{index}", $"Key {index}");
        }
        initState.AddTransition("FINISHED", "Key 0");

        // Allow generic transitions.
        foreach (string dir in new string[] { "Up", "Down", "Left", "Right" })
        {
            var dirState = fsm.GetState($"{dir} Press");

            for (int i = 0; i < DoorData.All().Count; i++)
            {
                dirState.AddTransition($"KEY_{i}", $"Key {i}");
                var keyState = fsm.GetState($"Key {i}");
                keyState.AddTransition($"UI {dir.ToUpper()}", $"{dir} Press");
            }
            dirState.AddTransition("OUT LEFT", "L Arrow");
            dirState.AddTransition("OUT RIGHT", "R Arrow");
        }

        var lArrow = fsm.GetState("L Arrow");
        lArrow.AddLastAction(new Lambda(() => SetSelectedKeyIndex(leftArrowIndex)));
        lArrow.AddTransition("UI RIGHT", "Key 0");
        rArrow.AddLastAction(new Lambda(() => SetSelectedKeyIndex(rightArrowIndex)));
        rArrow.AddTransition("UI LEFT", "Left Press");

        moreKeysPage.SetActive(false);
        UpdateImpl();
    }

    private void SetSelectedKeyIndex(int index)
    {
        selectedIndex = index;
        if (index == rightArrowIndex || index == leftArrowIndex)
        {
            keyTitle!.GetComponent<TextMeshPro>().text = "";
            keyDesc!.GetComponent<TextMeshPro>().text = "";
        }
        else if (inventoryKeys.Count == 0)
        {
            keyTitle!.GetComponent<TextMeshPro>().text = "???";
            keyDesc!.GetComponent<TextMeshPro>().text = "Hallownest remains a sealed vault, for now.";
        }
        else
        {
            string door = inventoryKeys[index];
            var dState = ItemChangerMod.Modules.Get<MoreDoorsModule>()!.DoorStates[door];
            var data = DoorData.GetDoor(door)!;

            keyTitle!.GetComponent<TextMeshPro>().text = data.Key!.UIItemName;
            keyDesc!.GetComponent<TextMeshPro>().text = dState.DoorOpened ? data.Key.UsedInvDesc : data.Key.InvDesc;
        }
    }

    private void HandleUpPress(PlayMakerFSM fsm)
    {
        var spacing = InventorySpacingParams.Create(DoorData.All().Count);
        if (selectedIndex > 0 && selectedIndex < spacing.rowSize)
        {
            fsm.SendEvent("KEY_0");
        }
        else if (selectedIndex >= spacing.rowSize)
        {
            fsm.SendEvent($"KEY_{selectedIndex - spacing.rowSize}");
        }
        else
        {
            fsm.SendEvent("OUT LEFT");
        }
    }

    private void HandleDownPress(PlayMakerFSM fsm)
    {
        var spacing = InventorySpacingParams.Create(DoorData.All().Count);
        bool onBottom = (selectedIndex - (selectedIndex % spacing.rowSize)) + spacing.rowSize > inventoryKeys.Count;
        if (onBottom)
        {
            fsm.SendEvent("OUT RIGHT");
        }
        else if (selectedIndex + spacing.rowSize > inventoryKeys.Count - 1)
        {
            fsm.SendEvent($"KEY_{inventoryKeys.Count - 1}");
        }
        else
        {
            fsm.SendEvent($"KEY_{selectedIndex + spacing.rowSize}");
        }
    }

    private void HandleLeftPress(PlayMakerFSM fsm)
    {
        var spacing = InventorySpacingParams.Create(DoorData.All().Count);
        if (selectedIndex == rightArrowIndex)
        {
            if (inventoryKeys.Count > spacing.rowSize - 1)
            {
                fsm.SendEvent($"KEY_{spacing.rowSize - 1}");
            }
            else if (inventoryKeys.Count > 0)
            {
                fsm.SendEvent($"KEY_{inventoryKeys.Count - 1}");
            }
            else
            {
                fsm.SendEvent("KEY_0");
            }
            return;
        }

        if (selectedIndex % spacing.rowSize == 0)
        {
            fsm.SendEvent("OUT LEFT");
        }
        else
        {
            fsm.SendEvent($"KEY_{selectedIndex - 1}");
        }
    }

    private void HandleRightPress(PlayMakerFSM fsm)
    {
        var spacing = InventorySpacingParams.Create(DoorData.All().Count);
        if (selectedIndex % spacing.rowSize == spacing.rowSize - 1)
        {
            fsm.SendEvent("OUT RIGHT");
        }
        else if (selectedIndex + 1 > inventoryKeys.Count - 1)
        {
            fsm.SendEvent("OUT RIGHT");
        }
        else
        {
            fsm.SendEvent($"KEY_{selectedIndex + 1}");
        }
    }
}
