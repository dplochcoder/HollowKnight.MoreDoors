using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
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

        private static void SetupConversationControl(PlayMakerFSM fsm, DoorData data, bool left)
        {
            fsm.GetState("Check Key").GetFirstActionOfType<PlayerDataBoolTest>().boolName = data.PDKeyName;
            fsm.GetState("Send Text").GetFirstActionOfType<CallMethodProper>().parameters[0] = NewStringVar(data.KeyPromptId);
            fsm.GetState("No Key").GetFirstActionOfType<CallMethodProper>().parameters[0] = NewStringVar(data.NoKeyPromptId);
            fsm.GetState("Open").AddFirstAction(new Lambda(() => Preloader.Instance.ReparentDoor(fsm.gameObject, left)));

            var setters = fsm.GetState("Yes").GetActionsOfType<SetPlayerDataBool>();
            setters[0].boolName = data.PDKeyName;
            setters[1].boolName = data.PDDoorOpenedName;
        }

        private static void SetupNpcControlOnRight(PlayMakerFSM fsm)
        {
            fsm.FsmVariables.FindFsmBool("Hero Always Left").Value = true;
            fsm.FsmVariables.FindFsmBool("Hero Always Right").Value = false;
        }

        public static void SpawnDoor(string doorName, bool left)
        {
            var data = DoorData.Get(doorName);
            var gameObj = Preloader.Instance.NewDoor();

            SetupConversationControl(gameObj.LocateMyFSM("Conversation Control"), data, left);

            var loc = left ? data.LeftDoorLocation : data.RightDoorLocation;
            gameObj.transform.position = new(loc.X, loc.Y, gameObj.transform.position.z);
            if (!left)
            {
                gameObj.transform.rotation = new(0, 180, 0, 0);
                SetupNpcControlOnRight(gameObj.LocateMyFSM("npc_control"));
            }

            gameObj.name = $"{data.VarName} Door";
            gameObj.AddComponent<DoorNameMarker>().DoorName = doorName;
            gameObj.SetActive(true);
        }

    }
}
