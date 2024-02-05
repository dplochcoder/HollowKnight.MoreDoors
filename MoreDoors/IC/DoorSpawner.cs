using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using MoreDoors.Data;
using UnityEngine;

namespace MoreDoors.IC;

public class DoorNameMarker : MonoBehaviour
{
    public string DoorName;
}

internal class DoorSecretMask : MonoBehaviour
{
    public string DoorName;
    public bool Left;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        MoreDoorsModule.OnDoorOpened += FadeMask;
    }

    private void OnDestroy() => MoreDoorsModule.OnDoorOpened -= FadeMask;

    private static readonly float FADE_DELAY = 1.65f;
    private static readonly float FADE_TIME = 2.2f;

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

        var c = spriteRenderer.color;
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

    private static void SetupConversationControl(PlayMakerFSM fsm, DoorData data, bool left)
    {
        fsm.GetState("Init").GetFirstActionOfType<PlayerDataBoolTest>().boolName = left ? data.PDDoorLeftForceOpenedName : data.PDDoorRightForceOpenedName;
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

    private static EmbeddedSprite SECRET_SPRITE = new("SecretMask");

    private static void MaybeSpawnSecretMask(Vector3 basePos, string doorName, DoorData data, bool left, DoorData.DoorInfo.Location loc)
    {
        bool showMask = data.Door.Mode == DoorData.DoorInfo.SplitMode.Normal;
        if (!showMask)
        {
            bool matchesBias = left == (data.Door.Mode == DoorData.DoorInfo.SplitMode.LeftTwin);

            var mod = ItemChangerMod.Modules.Get<MoreDoorsModule>();
            bool matchesGate = loc.Transition != null && mod.LastSceneName == loc.Transition.SceneName && mod.LastGateName == loc.Transition.GateName;

            showMask = matchesBias ^ matchesGate;
        }

        if (!showMask) return;

        var mask = loc.Mask;

        var obj = new GameObject($"{doorName}_SecretMask");
        obj.SetActive(false);
        obj.transform.position = basePos + new Vector3((mask.Width / 2 + 0.5f) * (left ? -1 : 1), 0, -1);
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

    private static readonly Color darkDoorColor = new(0.2647f, 0.2647f, 0.2647f);

    public static void SpawnDoor(SceneManager sm, string doorName, bool left)
    {
        var data = DoorData.GetFromModule(doorName);
        var gameObj = Object.Instantiate(Preloader.Instance.Door);
        SetupConversationControl(gameObj.LocateMyFSM("Conversation Control"), data, left);

        var loc = left ? data.Door.LeftLocation : data.Door.RightLocation;
        gameObj.transform.position = new(loc.X, loc.Y, gameObj.transform.position.z);

        var renderer = gameObj.GetComponent<SpriteRenderer>();
        renderer.sprite = data.Door.Sprite.Value;
        var open = PlayerData.instance.GetBool(data.PDDoorOpenedName);
        if (!open && loc.Mask != null) MaybeSpawnSecretMask(gameObj.transform.position, doorName, data, left, loc);

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
