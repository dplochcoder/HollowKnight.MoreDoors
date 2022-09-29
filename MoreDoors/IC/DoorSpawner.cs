using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using MoreDoors.Data;
using UnityEngine;

namespace MoreDoors.IC
{
    public class DoorNameMarker : MonoBehaviour
    {
        public string DoorName;
    }

    public static class DoorSpawner
    {

        private static FsmVar NewStringVar(string text)
        {
            FsmVar ret = new(typeof(string));
            ret.stringValue = text;
            return ret;
        }

        private static void ReparentDoor(GameObject obj, Vector3 src, bool left)
        {
            Vector3 dst = obj.transform.position;

            GameObject parent = new();
            parent.name = $"{obj.name} Animation Parent";
            var delta = dst - src;
            if (!left)
            {
                delta.x = dst.x + src.x;
                delta.z = 0.5129f;
                parent.transform.rotation = new(0, 180, 0, 1);
            }
            parent.transform.position = delta;

            obj.transform.SetParent(parent.transform, false);
            obj.transform.localPosition = src;
            obj.transform.rotation = new(0, left ? 0 : 180, 0, 1);
        }

        private static void SetupConversationControl(PlayMakerFSM fsm, DoorData data, bool left)
        {
            fsm.GetState("Check Key").GetFirstActionOfType<PlayerDataBoolTest>().boolName = data.PDKeyName;
            fsm.GetState("Send Text").GetFirstActionOfType<CallMethodProper>().parameters[0] = NewStringVar(data.KeyPromptId);
            fsm.GetState("No Key").GetFirstActionOfType<CallMethodProper>().parameters[0] = NewStringVar(data.NoKeyPromptId);

            var origPosition = fsm.gameObject.transform.position;
            fsm.GetState("Open").AddFirstAction(new Lambda(() => ReparentDoor(fsm.gameObject, origPosition, left)));

            var setters = fsm.GetState("Yes").GetActionsOfType<SetPlayerDataBool>();
            setters[0].boolName = MoreDoorsModule.EmptyBoolName;
            setters[1].boolName = data.PDDoorOpenedName;
        }

        private static void SetupNpcControlOnRight(PlayMakerFSM fsm)
        {
            fsm.FsmVariables.FindFsmBool("Hero Always Left").Value = true;
            fsm.FsmVariables.FindFsmBool("Hero Always Right").Value = false;
        }

        private static readonly Color darkDoorColor = new(0.2647f, 0.2647f, 0.2647f);

        public static void SpawnDoor(SceneManager sm, string doorName, bool left)
        {
            var data = DoorData.Get(doorName);
            var gameObj = Object.Instantiate(Preloader.Instance.Door);
            var renderer = gameObj.GetComponent<SpriteRenderer>();
            renderer.sprite = new EmbeddedSprite($"Doors.{data.Door.Sprite}").Value;

            SetupConversationControl(gameObj.LocateMyFSM("Conversation Control"), data, left);

            var loc = left ? data.Door.LeftLocation : data.Door.RightLocation;
            gameObj.transform.position = new(loc.X, loc.Y, gameObj.transform.position.z);
            if (!left)
            {
                gameObj.transform.rotation = new(0, 180, 0, 0);
                SetupNpcControlOnRight(gameObj.LocateMyFSM("npc_control"));
            }

            gameObj.name = $"{data.CamelCaseName} Door";
            gameObj.AddComponent<DoorNameMarker>().DoorName = doorName;

            var promptMarker = gameObj.FindChild("Prompt Marker");
            promptMarker.transform.localPosition = new(0.7f, 0.77f, 0.206f);
            promptMarker.AddComponent<DeactivateInDarknessWithoutLantern>().enabled = true;

            // TODO: Establish an ordering between mod initializations on scene load.
            // Implement a constraints mod that supports this without the need for linking.
            if (sm.darknessLevel == 2 && !PlayerData.instance.GetBool(nameof(PlayerData.instance.hasLantern)))
            {
                // Why is this so finicky
                Object.Destroy(promptMarker);
                Object.Destroy(gameObj.LocateMyFSM("npc_control"));
                renderer.color = darkDoorColor;
            }

            gameObj.SetActive(true);
        }

    }
}
