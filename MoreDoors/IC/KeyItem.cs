using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using MoreDoors.Data;
using System.Linq;

namespace MoreDoors.IC;

public class KeyItem : AbstractItem
{
    public string DoorName;

    private static InteropTag AddInterop(TaggableObject o)
    {
        var tag = o.AddTag<InteropTag>();
        tag.Message = "RandoSupplementalMetadata";
        tag.Properties["PoolGroup"] = "Keys";
        tag.Properties["ModSource"] = nameof(MoreDoors);
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

    public KeyItem(string doorName, DoorData data) : this(doorName, data.Key.ItemName, new MsgUIDef()
    {
        name = new BoxedString(data.Key.UIItemName),
        shopDesc = new BoxedString(data.Key.ShopDesc),
        sprite = data.Key.Sprite,
    })
    { }

    public void AddLocationInteropTags(DoorData data)
    {
        if (HasTag<InteropTag>()) return;

        var loc = data.Key.Location;
        // TODO: This seems like a bug in ItemChanger that we have to do this.
        // DualPlacement concatenates tags from both delegate locations, but it ignores any tags set on the DualLocation itself.
        if (loc is DualLocation dl) loc = dl.falseLocation;

        AddInterop(loc).Properties["WorldMapLocations"] = data.Key.GetWorldMapLocations().Select(l => l.AsTuple).ToArray();
    }

    public override AbstractItem Clone() => new KeyItem(DoorName, name, UIDef?.Clone());

    public override void GiveImmediate(GiveInfo info) => PlayerData.instance.SetBool(DoorData.GetFromModule(DoorName).PDKeyName, true);

    public override bool Redundant() => PlayerData.instance.GetBool(DoorData.GetFromModule(DoorName).PDKeyName);
}
