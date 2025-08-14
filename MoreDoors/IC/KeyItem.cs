using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using MoreDoors.Data;
using System.Linq;

namespace MoreDoors.IC;

public class KeyItem : AbstractItem
{
    public string DoorName = "";

    private static InteropTag AddInterop(TaggableObject o)
    {
        var tag = o.GetOrAddTag<InteropTag>();
        tag.Message = "RandoSupplementalMetadata";
        tag.Properties["ModSource"] = nameof(MoreDoors);
        tag.Properties["PoolGroup"] = "Keys";
        return tag;
    }

    // Json constructor
    KeyItem() { }

    public KeyItem(string doorName, string itemName, UIDef? uiDef)
    {
        this.name = itemName;
        this.DoorName = doorName;
        this.UIDef = uiDef;
        AddInterop(this);
    }

    public KeyItem(string doorName, DoorData data) : this(doorName, data.Key!.ItemName, new MsgUIDef()
    {
        name = new BoxedString(data.Key.UIItemName),
        shopDesc = new BoxedString(data.Key.ShopDesc),
        sprite = data.Key.Sprite!,
    })
    { }

    public void AddLocationInteropTags(DoorData data)
    {
        var interop = AddInterop(data.Key!.Location!);

        interop.Properties["WorldMapLocations"] = data.Key.GetWorldMapLocations().Select(l => l.AsTuple).ToArray();
        interop.Properties["PinSpriteKey"] = "Keys";
    }

    public override AbstractItem Clone() => new KeyItem(DoorName, name, UIDef?.Clone());

    public override void GiveImmediate(GiveInfo info) => PlayerData.instance.SetBool(DoorData.GetDoor(DoorName)!.PDKeyName, true);

    public override bool Redundant() => PlayerData.instance.GetBool(DoorData.GetDoor(DoorName)!.PDKeyName);
}
