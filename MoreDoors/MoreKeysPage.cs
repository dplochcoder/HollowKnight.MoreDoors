using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using MoreDoors.Data;
using MoreDoors.IC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using EmbeddedSprite = MoreDoors.IC.EmbeddedSprite;

namespace MoreDoors
{
    public class MoreKeysPage
    {
        public static readonly MoreKeysPage Instance = new();

        private class KeySlot
        {
            public GameObject obj;
            public SpriteRenderer spriteRenderer;
        }
        private readonly List<KeySlot> keySlots = new();
        private readonly List<string> inventoryKeys = new();

        private static readonly Color DEFAULT_COLOR = new(1, 1, 1);
        private static readonly Color USED_COLOR = new(0.4f, 0.4f, 0.4f);

        public void Update()
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
                    keySlots[i].spriteRenderer.color = mod.DoorStates[door].DoorOpened ? USED_COLOR : DEFAULT_COLOR;
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

        private int selectedIndex = -1;

        private const float X_LEFT = -10;
        private const float X_SPACE = 2;
        private const float Y_TOP = 4;
        private const float Y_SPACE = 2;

        private static float X(int i) => X_LEFT + X_SPACE * (i % 8);
        private static float Y(int i) => Y_TOP - Y_SPACE * (i / 8);

        public void GeneratePage(GameObject moreKeysPage)
        {
            keyTitle = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Name").gameObject);
            keyTitle.transform.SetParent(moreKeysPage.transform);
            keyTitle.transform.localPosition = new(13f, -7.5f, -2f);
            keyTitle.GetComponent<TextMeshPro>().text = "";

            keyDesc = GameObject.Instantiate(GameObject.Find("_GameCameras").transform.Find("HudCamera/Inventory/Charms/Text Desc").gameObject);
            keyDesc.transform.SetParent(moreKeysPage.transform);
            keyDesc.transform.localPosition = new(13f, -9f, 1f);
            keyDesc.GetComponent<TextMeshPro>().text = "";

            for (int i = 0; i < DoorData.Count; i++)
            {
                GameObject obj = new($"MoreDoors Menu Parent {i}");
                obj.transform.SetParent(moreKeysPage.transform);
                obj.transform.localPosition = new(X(i), Y(i), 0);
                obj.AddComponent<BoxCollider2D>().offset = new(0, 0);

                GameObject img = new($"MoreDoors Key Image {i}");
                img.transform.SetParent(obj.transform);
                img.layer = moreKeysPage.layer;
                var sr = img.AddComponent<SpriteRenderer>();
                sr.sprite = emptySprite.Value;
                sr.sortingLayerID = 629535577;
                sr.sortingLayerName = "HUD";

                keySlots.Add(new() {
                    obj = obj,
                    spriteRenderer = sr
                });
            }

            // Removing the jump from arrow button to arrow button.
            var fsm = moreKeysPage.LocateMyFSM("Empty UI");
            fsm.GetState("L Arrow").RemoveTransitionsTo("R Arrow");
            fsm.GetState("R Arrow").RemoveTransitionsTo("L Arrow");

            FsmState state = fsm.GetState("Init Heart Piece");
            state.Name = "Init More Keys";
            state.RemoveTransitionsTo("L Arrow");
            state.AddLastAction(new Lambda(() =>
            {
                foreach (Transform child in moreKeysPage.transform)
                {
                    child.gameObject.SetActive(true);
                }
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
            for (int i = 0; i < DoorData.Count; i++)
            {
                int index = i;
                fsm.AddState(new FsmState(fsm.Fsm)
                {
                    Name = $"Key {index}",
                    Actions = new FsmStateAction[]
                    {
                        new Lambda(() => fsm.gameObject.LocateMyFSM("Update Cursor").FsmVariables.FindFsmGameObject("Item").Value = keySlots[index].obj),
                        new SetSpriteRendererOrder()
                        {
                            gameObject = new() { GameObject = fsm.FsmVariables.FindFsmGameObject("Cursor Glow")},
                            order = 0,
                            delay = 0
                        },
                        new Lambda(() => fsm.gameObject.LocateMyFSM("Update Cursor").SendEvent("UPDATE CURSOR")),
                        new Lambda(() => SetSelectedKeyIndex(i)),
                    }
                });
            }

            // Allow generic transitions.
            foreach (string dir in new string[] { "Up", "Down", "Left", "Right" })
            {
                var fState = fsm.GetState($"{dir} Press");

                for (int i = 0; i < DoorData.Count; i++)
                {
                    fState.AddTransition($"KEY_{i}", $"Key {i}");
                    var dState = fsm.GetState($"Key {i}");
                    dState.AddTransition($"UI {dir.ToUpper()}", $"{dir} Press");
                }
                fState.AddTransition("OUT LEFT", "L Arrow");
                fState.AddTransition("OUT RIGHT", "R Arrow");
            }

            state.AddTransition("FINISHED", "Key 0");

            moreKeysPage.SetActive(false);
        }

        private void SetSelectedKeyIndex(int index)
        {
            selectedIndex = index;
            if (inventoryKeys.Count == 0)
            {
                keyTitle.GetComponent<TextMeshPro>().text = "Nothing Key?";
                keyDesc.GetComponent<TextMeshPro>().text = "Hallownest remains a sealed vault, for now.";
                return;
            }

            string door = inventoryKeys[index];
            var data = DoorData.Get(door);
            keyTitle.GetComponent<TextMeshPro>().text = data.Key.UIItemName;
            keyDesc.GetComponent<TextMeshPro>().text = "TODO: Inventory Descriptions";
        }

        private void HandleUpPress(PlayMakerFSM fsm)
        {
            if (selectedIndex > 0 && selectedIndex < 8)
            {
                fsm.SendEvent("KEY_0");
            }
            else if (selectedIndex >= 8)
            {
                fsm.SendEvent($"KEY_{selectedIndex - 8}");
            }
            else
            {
                fsm.SendEvent("OUT LEFT");
            }
        }

        private void HandleDownPress(PlayMakerFSM fsm)
        {
            bool onBottom = (selectedIndex - (selectedIndex % 8)) + 8 > inventoryKeys.Count;
            if (onBottom)
            {
                fsm.SendEvent("OUT RIGHT");
            }
            else if (selectedIndex + 8 > inventoryKeys.Count - 1)
            {
                fsm.SendEvent($"KEY_{inventoryKeys.Count - 1}");
            }
            else
            {
                fsm.SendEvent($"KEY_{selectedIndex + 8}");
            }
        }

        private void HandleLeftPress(PlayMakerFSM fsm)
        {
            bool onLeft = selectedIndex % 8 == 0;
            if (onLeft)
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
            bool onRight = selectedIndex % 8 == 7;
            if (onRight)
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
