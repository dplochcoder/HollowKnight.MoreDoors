using ItemChanger.Internal;

namespace MoreDoors.IC
{
    public class EmbeddedSprite : ItemChanger.EmbeddedSprite
    {
        private static readonly SpriteManager manager = new(typeof(EmbeddedSprite).Assembly, "MoreDoors.Resources.Sprites.");

        public EmbeddedSprite(string key) => this.key = key;

        public override SpriteManager SpriteManager => manager;
    }
}