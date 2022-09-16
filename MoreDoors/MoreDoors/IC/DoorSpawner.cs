using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;

namespace MoreDoors.IC
{
    public static class DoorSpawner
    {

        private static FsmVar NewStringVar(string text)
        {
            FsmVar ret = new();
            ret.SetValue(text);
            return ret;
        }

        public static void SpawnDoor(string doorName, bool left)
        {
            var data = DoorData.Get(doorName);
            var gameObj = Preloader.Instance.NewDoor();

            var fsm = gameObj.LocateMyFSM("Conversation Control");
            fsm.GetState("Check Key").GetFirstActionOfType<PlayerDataBoolTest>().boolName = data.PDKeyName;
            fsm.GetState("Send Text").GetFirstActionOfType<CallMethodProper>().parameters[0] =
                NewStringVar(MoreDoorsModule.YesKeyConvoId(data.LogicName));
            fsm.GetState("No Key").GetFirstActionOfType<CallMethodProper>().parameters[0] =
                NewStringVar(MoreDoorsModule.NoKeyConvoId(data.LogicName));
            var setters = fsm.GetState("Yes").GetActionsOfType<SetPlayerDataBool>();
            setters[0].boolName = data.PDKeyName;
            setters[1].boolName = data.PDDoorOpenedName;

            var loc = left ? data.LeftDoorLocation : data.RighttDoorLocation;
            gameObj.transform.position = new(loc.X, loc.Y, gameObj.transform.position.z);
            if (!left)
            {
                gameObj.transform.rotation = new(0, 180, 0, 0);
            }
        }

    }
}
