using IL.TMPro;
using ItemChanger;
using ItemChanger.Locations;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using MoreDoors.Data;
using System.Collections.Generic;
using System.Linq;

namespace MoreDoors.IC
{
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

            AddInterop(this);

            var loc = data.Key.Location;
            // TODO: This seems like a bug in ItemChanger that we have to do this.
            // DualPlacement concatenates tags from both delegate locations, but it ignores any tags set on the DualLocation itself.
            if (loc is DualLocation dl) loc = dl.falseLocation;

            var interop = AddInterop(loc);

            List<(string, float, float)> positions = new();
            positions.Add(data.Key.GetWorldMapLocation().AsTuple);
            data.Key.ExtraWorldMapLocations?.ForEach(eLoc => positions.Add(eLoc.AsTuple));
            interop.Properties["WorldMapLocations"] = positions.ToArray();
        }

        public override AbstractItem Clone() => new KeyItem(DoorName);

        public override void GiveImmediate(GiveInfo info) => PlayerData.instance.SetBool(DoorData.Get(DoorName).PDKeyName, true);

        public override bool Redundant() => PlayerData.instance.GetBool(DoorData.Get(DoorName).PDKeyName);
    }
}
