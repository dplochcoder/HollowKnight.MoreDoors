using ItemChanger;
using ItemChanger.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreDoors.IC;

internal record SpriteDimmer : IDeployer
{
    public string SceneName;
    public string TargetGameObject;
    public float AlphaMultiplier;

    string IDeployer.SceneName => SceneName;

    public void OnSceneChange(Scene to)
    {
        var target = to.FindGameObject(TargetGameObject);
        if (target == null) return;

        var spriteRenderer = target.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        var color = spriteRenderer.color;
        spriteRenderer.color = new(color.r, color.g, color.b, color.a * AlphaMultiplier);
        return;
    }
}
