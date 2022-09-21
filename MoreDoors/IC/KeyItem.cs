using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;

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
                sprite = new EmbeddedSprite(data.Key.Sprite)
            };

            var interop = AddTag<InteropTag>();
            interop.Message = "RandoSupplementalMetadata";
            interop.Properties["PoolGroup"] = "Keys";
            interop.Properties["ModSource"] = MoreDoors.Instance.GetName();
        }

        public override AbstractItem Clone() => new KeyItem(DoorName);

        public override void GiveImmediate(GiveInfo info) => PlayerData.instance.SetBool(DoorData.Get(DoorName).PDKeyName, true);

        public override bool Redundant() => PlayerData.instance.GetBool(DoorData.Get(DoorName).PDKeyName);
    }
}
