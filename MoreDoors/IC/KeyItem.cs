using IL.TMPro;
using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using MoreDoors.Data;

namespace MoreDoors.IC
{
    public class KeyItem : AbstractItem
    {
        public string DoorName;

        public KeyItem(string doorName)
        {
            var data = DoorData.Get(doorName);
            this.name = data.Key.ItemName;

            this.DoorName = doorName;
            this.UIDef = new MsgUIDef()
            {
                name = new BoxedString(data.Key.UIItemName),
                shopDesc = new BoxedString(data.Key.ShopDesc),
                sprite = new EmbeddedSprite($"Keys.{data.Key.Sprite}")
            };

            var interop = AddTag<InteropTag>();
            interop.Message = "RandoSupplementalMetadata";
            interop.Properties["PoolGroup"] = "Keys";
            interop.Properties["ModSource"] = nameof(MoreDoors);

            var loc = data.Key.VanillaLocation;

            // TODO: This seems like a bug in ItemChanger that we have to do this.
            // DualPlacement concatenates tags from both delegate locations, but it ignores any tags set on the DualLocation itself.
            if (loc is DualLocation dl) loc = dl.falseLocation;

            var locInterop = loc.AddTag<InteropTag>();
            locInterop.Message = "RandoSupplementalMetadata";
            locInterop.Properties["PoolGroup"] = "Keys";
            locInterop.Properties["ModSource"] = nameof(MoreDoors);

            // FIXME: This doesn't work, why?
            (float x, float y) = data.Key.Coords;
            locInterop.Properties["WorldMapLocations"] = new (string, float, float)[] { (loc.sceneName, x, y) };
        }

        public override AbstractItem Clone() => new KeyItem(DoorName);

        public override void GiveImmediate(GiveInfo info) => PlayerData.instance.SetBool(DoorData.Get(DoorName).PDKeyName, true);

        public override bool Redundant() => PlayerData.instance.GetBool(DoorData.Get(DoorName).PDKeyName);
    }
}
