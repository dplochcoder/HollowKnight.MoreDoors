using UnityEngine;

namespace MoreDoors.IC;

public interface IDoorDecorator
{
    void Decorate(GameObject door);
}

internal record SpikeAttacher : IDoorDecorator
{
    public float OffsetX;
    public float OffsetY;
    public float Width;
    public float Height;

    public void Decorate(GameObject door)
    {
        GameObject hazard = new("Spikes");
        hazard.transform.SetParent(door.transform);
        hazard.transform.position = door.transform.position;
        hazard.layer = (int) GlobalEnums.PhysLayers.ENEMIES;

        var damage = hazard.AddComponent<DamageHero>();
        damage.hazardType = 2;

        var box = hazard.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.offset = new(OffsetX, OffsetY);
        box.size = new(Width, Height);
    }
}
