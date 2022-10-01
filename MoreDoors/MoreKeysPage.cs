using HutongGames.PlayMaker;
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

namespace MoreDoors
{
    public class MoreKeysPage
    {
        public static readonly MoreKeysPage Instance = new();

        private class KeySlot
        {
            public GameObject obj;
            public GameObject img;
            public SpriteRenderer spriteRenderer;
        }
        private readonly List<KeySlot> keySlots = new();
        private readonly List<string> inventoryKeys = new();

        private static readonly Color KEY_OBTAINED_COLOR = new(1, 1, 1);
        private static readonly Color KEY_USED_COLOR = new(0.25f, 0.25f, 0.25f);

        private static readonly Vector3 KEY_OBTAINED_SCALE = new(1.65f, 1.65f, 1.65f);
        private static readonly Vector3 KEY_USED_SCALE = new(1.2f, 1.2f, 1.2f);

        public void Update()
        {
            try
            {
                UpdateImpl();
            }
            catch (Exception e)
            {
                MoreDoors.Log($"Error updating More Keys menu: {e}");
            }
        }

        private void UpdateImpl()
        {
            var mod = ItemChangerMod.Modules.Get<MoreDoorsModule>();

            inventoryKeys.Clear();

            foreach (var e in mod.DoorStates)
            {
                var door = e.Key;
                var dState = e.Value;
                if (dState.KeyObtained) inventoryKeys.Add(door);
            }

            for (int i = 0; i < DoorData.Count; i++)
            {
                string? door = i < inventoryKeys.Count ? inventoryKeys[i] : null;
                if (door != null)
                {
                    var kSprite = new EmbeddedSprite($"Keys.{DoorData.Get(door).Key.Sprite}");
                    keySlots[i].spriteRenderer.sprite = kSprite.Value;
                    keySlots[i].spriteRenderer.color = mod.DoorStates[door].DoorOpened ? KEY_USED_COLOR : KEY_OBTAINED_COLOR;
                    keySlots[i].img.transform.localScale = mod.DoorStates[door].DoorOpened ? KEY_USED_SCALE : KEY_OBTAINED_SCALE;
                }
                else
                {
                    keySlots[i].spriteRenderer.sprite = emptySprite.Value;
                }
            }
        }

        private ISprite emptySprite = new EmbeddedSprite("Menu.UnplacedKey");
        private GameObject keyTitle;
        private GameObject keyDesc;

        private const int leftArrowIndex = -2;
        private const int rightArrowIndex = -3;
        private int selectedIndex = -1;

        private const float X_LEFT = -11;
        private const float X_SPACE = 2.44f;
        private const float Y_TOP = 0;
        private const float Y_SPACE = 2.44f;

        private const int ROW_SIZE = 10;

        private static float X(int i) => X_LEFT + X_SPACE * (i % ROW_SIZE);
        private static float Y(int i) => Y_TOP - Y_SPACE * (i / ROW_SIZE);

        public void GeneratePage(GameObject moreKeysPage)
        {
            try
            {
                GeneratePageImpl(moreKeysPage);
            }
            catch (Exception e)
            {
                MoreDoors.Log($"Error setting up More Keys menu: {e}");
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

            for (int i = 0; i < DoorData.Count; i++)
            {
                GameObject obj = new($"MoreDoors Menu Parent {i}");
                obj.transform.SetParent(moreKeysPage.transform);
                obj.layer = moreKeysPage.layer;
                obj.transform.position = new(X(i), Y(i), -3f);
                var bc2d = obj.AddComponent<BoxCollider2D>();
                bc2d.offset = new(0, 0);
                bc2d.size = new(1.5f, 1.5f);

                GameObject img = new($"MoreDoors Key Image {i}");
                img.transform.SetParent(obj.transform);
                img.transform.localPosition = new(0, 0, 0);
                img.transform.localScale = new(1.65f, 1.65f, 1.65f);
                img.layer = moreKeysPage.layer;
                var sr = img.AddComponent<SpriteRenderer>();
                sr.sprite = emptySprite.Value;
                sr.sortingLayerID = 629535577;
                sr.sortingLayerName = "HUD";

                keySlots.Add(new() {
                    obj = obj,
                    img = img,
                    spriteRenderer = sr
                });
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
                Actions = new FsmStateAction[] { new Lambda(() => HandleUpPress(fsm)) }
            });
            fsm.AddState(new FsmState(fsm.Fsm)
            {
                Name = "Down Press",
                Actions = new FsmStateAction[] { new Lambda(() => HandleDownPress(fsm)) }
            });
            fsm.AddState(new FsmState(fsm.Fsm)
            {
                Name = "Left Press",
                Actions = new FsmStateAction[] { new Lambda(() => HandleLeftPress(fsm)) }
            });
            fsm.AddState(new FsmState(fsm.Fsm)
            {
                Name = "Right Press",
                Actions = new FsmStateAction[] { new Lambda(() => HandleRightPress(fsm)) }
            });

            // Add states for each slot on the board.
            var rArrow = fsm.GetState("R Arrow");
            var uCursor = fsm.gameObject.LocateMyFSM("Update Cursor");
            for (int i = 0; i < DoorData.Count; i++)
            {
                int index = i;
                fsm.AddState(new FsmState(fsm.Fsm)
                {
                    Name = $"Key {index}",
                    Actions = new FsmStateAction[]
                    {
                        new Lambda(() => uCursor.FsmVariables.FindFsmGameObject("Item").Value = keySlots[index].obj),
                        new SetSpriteRendererOrder()
                        {
                            gameObject = new() { GameObject = fsm.FsmVariables.FindFsmGameObject("Cursor Glow")},
                            order = 0,
                            delay = 0
                        },
                        new Lambda(() => uCursor.SendEvent("UPDATE CURSOR")),
                        new Lambda(() => SetSelectedKeyIndex(index)),
                    }
                });
                rArrow.AddTransition($"KEY_{index}", $"Key {index}");
            }
            initState.AddTransition("FINISHED", "Key 0");

            // Allow generic transitions.
            foreach (string dir in new string[] { "Up", "Down", "Left", "Right" })
            {
                var dirState = fsm.GetState($"{dir} Press");

                for (int i = 0; i < DoorData.Count; i++)
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
            Update();
        }

        private void SetSelectedKeyIndex(int index)
        {
            selectedIndex = index;
            if (index == rightArrowIndex || index == leftArrowIndex)
            {
                keyTitle.GetComponent<TextMeshPro>().text = "";
                keyDesc.GetComponent<TextMeshPro>().text = "";
                return;
            }
            else if (inventoryKeys.Count == 0)
            {
                keyTitle.GetComponent<TextMeshPro>().text = "???";
                keyDesc.GetComponent<TextMeshPro>().text = "Hallownest remains a sealed vault, for now.";
                return;
            }
            else
            {
                string door = inventoryKeys[index];
                var data = DoorData.Get(door);
                var dState = ItemChangerMod.Modules.Get<MoreDoorsModule>().DoorStates[door];

                keyTitle.GetComponent<TextMeshPro>().text = data.Key.UIItemName;
                keyDesc.GetComponent<TextMeshPro>().text = dState.DoorOpened ? data.Key.UsedInvDesc : data.Key.InvDesc;
            }
        }

        private void HandleUpPress(PlayMakerFSM fsm)
        {
            if (selectedIndex > 0 && selectedIndex < ROW_SIZE)
            {
                fsm.SendEvent("KEY_0");
            }
            else if (selectedIndex >= ROW_SIZE)
            {
                fsm.SendEvent($"KEY_{selectedIndex - ROW_SIZE}");
            }
            else
            {
                fsm.SendEvent("OUT LEFT");
            }
        }

        private void HandleDownPress(PlayMakerFSM fsm)
        {
            bool onBottom = (selectedIndex - (selectedIndex % ROW_SIZE)) + ROW_SIZE > inventoryKeys.Count;
            if (onBottom)
            {
                fsm.SendEvent("OUT RIGHT");
            }
            else if (selectedIndex + ROW_SIZE > inventoryKeys.Count - 1)
            {
                fsm.SendEvent($"KEY_{inventoryKeys.Count - 1}");
            }
            else
            {
                fsm.SendEvent($"KEY_{selectedIndex + ROW_SIZE}");
            }
        }

        private void HandleLeftPress(PlayMakerFSM fsm)
        {
            if (selectedIndex == rightArrowIndex)
            {
                if (inventoryKeys.Count > ROW_SIZE - 1)
                {
                    fsm.SendEvent($"KEY_{ROW_SIZE - 1}");
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

            if (selectedIndex % ROW_SIZE == 0)
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
            if (selectedIndex % ROW_SIZE == ROW_SIZE - 1)
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
}
