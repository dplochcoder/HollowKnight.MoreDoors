using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreDoors.IC
{
    public class KeyItem : AbstractItem
    {
        public string DoorName;

        public KeyItem(string doorName)
        {
            this.name = DoorData.KeyName(doorName);

            var data = DoorData.Get(doorName);
            this.DoorName = doorName;
            this.UIDef = new MsgUIDef()
            {
                name = new BoxedString(data.Key.Name),
                shopDesc = new BoxedString(data.Key.ShopDesc),
                sprite = new EmbeddedSprite(data.Key.SpriteKey)
            };

            var interop = AddTag<InteropTag>();
            interop.Message = "RandoSupplementalMetadata";
            interop.Properties["PoolGroup"] = "Keys";
            interop.Properties["ModSource"] = MoreDoors.Instance.GetName();
        }

        public override AbstractItem Clone() => new KeyItem(DoorName);

        public override void GiveImmediate(GiveInfo info) => PlayerData.instance.GetKeyForDoor(DoorName);

        public override bool Redundant() => PlayerData.instance.HasKeyForDoor(DoorName);
    }
}
