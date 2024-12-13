using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using MoreDoors.Data;
using SFCore.Utils;
using UnityEngine;

namespace MoreDoors.IC;

public class DoorNameMarker : MonoBehaviour
{
    public string DoorName = "";
}

internal class SplitDoorSyncer : MonoBehaviour
{
    private string DoorName = "";

    private void Awake()
    {
        DoorName = GetComponent<DoorNameMarker>().DoorName;
        MoreDoorsModule.OnDoorOpened += SyncDoor;
    }

    private void OnDestroy() => MoreDoorsModule.OnDoorOpened -= SyncDoor;

    private void SyncDoor(string doorName, bool left)
    {
        if (doorName != DoorName) return;

        var fsm = gameObject.LocateMyFSM("Conversation Control");
        if (fsm.ActiveStateName == "Idle") fsm.SetState("Yes");
    }
}

internal class DoorSecretMask : MonoBehaviour
{
    public string DoorName = "";
    public bool Left;

    private SpriteRenderer? spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        MoreDoorsModule.OnDoorOpened += FadeMask;
    }

    private void OnDestroy() => MoreDoorsModule.OnDoorOpened -= FadeMask;

    private static readonly float FADE_DELAY = 1.8f;
    private static readonly float FADE_TIME = 3.2f;

    private bool fading = false;
    private float delay = 0;
    private float fade = 0;

    private void Update()
    {
        if (!fading) return;

        delay += Time.deltaTime;
        if (delay < FADE_DELAY) return;

        float excess = delay - FADE_DELAY;
        delay = FADE_DELAY;

        fade += excess;
        if (fade > FADE_TIME)
        {
            Destroy(gameObject);
            return;
        }

        var c = spriteRenderer!.color;
        spriteRenderer.color = new(c.r, c.g, c.b, 1.0f - (fade / FADE_TIME));
    }

    private void FadeMask(string doorName, bool left)
    {
        if (doorName != DoorName || left != Left) return;
        fading = true;
    }
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

    private static FsmState GetState(PlayMakerFSM fsm, string name) => FsmUtil.GetState(fsm, name);

    private static void SetupConversationControl(PlayMakerFSM fsm, DoorData data, bool left)
    {
        GetState(fsm, "Init").GetFirstActionOfType<PlayerDataBoolTest>().boolName = left ? data.PDDoorLeftForceOpenedName : data.PDDoorRightForceOpenedName;
        GetState(fsm, "Check Key").GetFirstActionOfType<PlayerDataBoolTest>().boolName = data.PDKeyName;
        GetState(fsm, "Send Text").GetFirstActionOfType<CallMethodProper>().parameters[0] = NewStringVar(left ? data.LeftKeyPromptId : data.RightKeyPromptId);
        GetState(fsm, "No Key").GetFirstActionOfType<CallMethodProper>().parameters[0] = NewStringVar(left ? data.LeftNoKeyPromptId : data.RightNoKeyPromptId);

        var origPosition = fsm.gameObject.transform.position;
        GetState(fsm, "Open").AddFirstAction(new Lambda(() => ReparentDoor(fsm.gameObject, origPosition, left)));

        var setters = GetState(fsm, "Yes").GetActionsOfType<SetPlayerDataBool>();
        setters[0].boolName = MoreDoorsModule.EMPTY_BOOL;
        setters[1].boolName = data.PDDoorOpenedName;
    }

    private static void SetupNpcControl(PlayMakerFSM fsm, bool left)
    {
        var playerTag = fsm.AddFsmStringVariable("PlayerTag");
        playerTag.RawValue = "Player";
        GetState(fsm, "Idle").GetFirstActionOfType<Trigger2dEvent>().collideTag = playerTag;

        fsm.FsmVariables.FindFsmBool("Hero Always Left").Value = !left;
        fsm.FsmVariables.FindFsmBool("Hero Always Right").Value = left;
    }

    private static readonly EmbeddedSprite SECRET_SPRITE = new("SecretMask");

    private static void MaybeSpawnSecretMasks(Vector3 basePos, string doorName, DoorData data, bool left, DoorData.DoorInfo.Location loc)
    {
        bool showMasks = data.Door!.Mode == DoorData.DoorInfo.SplitMode.Normal;
        if (!showMasks)
        {
            bool nextToGate = left != (data.Door.Mode == DoorData.DoorInfo.SplitMode.LeftTwin);

            var mod = ItemChangerMod.Modules.Get<MoreDoorsModule>()!;
            bool cameFromGate = mod.LastSceneName == loc.SceneName && mod.LastGateName == loc.GateName;

            showMasks = nextToGate == cameFromGate;
        }

        if (!showMasks) return;

        foreach (var mask in loc.Masks ?? [])
        {
            var obj = new GameObject($"{doorName}_SecretMask");
            obj.SetActive(false);
            obj.transform.position = basePos + new Vector3((mask.Width / 2 + mask.OffsetX) * (left ? -1 : 1), mask.OffsetY, -1);
            obj.transform.localScale = new(mask.Width / 2 * (left ? -1 : 1), mask.Height, 1);

            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = SECRET_SPRITE.Value;
            renderer.sortingLayerName = "Far FG";
            renderer.sortingOrder = 10;

            var secretMask = obj.AddComponent<DoorSecretMask>();
            secretMask.DoorName = doorName;
            secretMask.Left = left;
            obj.SetActive(true);
        }
    }

    private static readonly Color darkDoorColor = new(0.2647f, 0.2647f, 0.2647f);

    public static void SpawnDoor(MoreDoorsModule mod, SceneManager sm, string doorName, bool left)
    {
        var data = DoorData.GetDoor(doorName)!;
        var gameObj = Object.Instantiate(Preloader.Instance.Door);
        var convCtrl = gameObj.LocateMyFSM("Conversation Control");
        SetupConversationControl(convCtrl, data, left);

        var loc = left ? data.Door!.LeftLocation! : data.Door!.RightLocation!;
        gameObj.transform.position = new(loc.X, loc.Y, gameObj.transform.position.z);

        var renderer = gameObj.GetComponent<SpriteRenderer>();
        renderer.sprite = data.Door.Sprite!.Value;
        var open = mod.IsDoorOpened(doorName, left);
        if (!open && loc.Masks != null) MaybeSpawnSecretMasks(gameObj.transform.position, doorName, data, left, loc);
        
        if (!left) gameObj.transform.rotation = new(0, 180, 0, 0);

        gameObj.name = $"{data.CamelCaseName} Door";
        gameObj.AddComponent<DoorNameMarker>().DoorName = doorName;
        if (!open && data.Door.Mode != DoorData.DoorInfo.SplitMode.Normal) gameObj.AddComponent<SplitDoorSyncer>();

        var promptMarker = gameObj.FindChild("Prompt Marker")!;
        promptMarker.transform.localPosition = new(0.7f, 0.77f, 0.206f);
        promptMarker.AddComponent<DeactivateInDarknessWithoutLantern>().enabled = true;

        // Fix the phys box.
        var physBox = gameObj.FindChild("Phys Box")!.GetComponent<BoxCollider2D>();
        physBox.offset += new Vector2(0.5f * (left ? -1 : 1), -0.35f);
        physBox.size += new Vector2(1f, -0.7f);

        if (sm.darknessLevel == 2 && !PlayerData.instance.GetBool(nameof(PlayerData.instance.hasLantern)))
        {
            // Why is this so finicky
            Object.Destroy(promptMarker);
            Object.Destroy(gameObj.LocateMyFSM("npc_control"));
            renderer.color = darkDoorColor;
        }

        loc.Decorators?.ForEach(dec => dec.Decorate(gameObj));
        gameObj.SetActive(true);

        // Something resets the variables on awake so we have to do this last.
        SetupNpcControl(gameObj.LocateMyFSM("npc_control"), left);
    }

}
