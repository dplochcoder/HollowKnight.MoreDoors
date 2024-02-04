using UnityEngine;

namespace MoreDoors.IC;

internal record SpriteDimmer : ItemChanger.Deployer
{
    public string TargetGameObject;
    public float AlphaMultiplier;

    public override GameObject Instantiate() => throw new System.NotImplementedException();

    public override GameObject Deploy()
    {
        var target = GameObject.Find(TargetGameObject);
        if (target == null) return null;

        var spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return null;

        var color = spriteRenderer.color;
        spriteRenderer.color = new(color.r, color.g, color.b, color.a * AlphaMultiplier);
        return null;
    }
}
