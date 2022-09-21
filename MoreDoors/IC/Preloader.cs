using System.Collections.Generic;
using UnityEngine;

namespace MoreDoors.IC
{
    public class Preloader : ItemChanger.Internal.Preloaders.Preloader
    {   public static Preloader Instance { get; } = new();

        public override IEnumerable<(string, string)> GetPreloadNames()
        {
            yield return ("Ruins2_11_b", "Love Door");
        }

        private GameObject doorTemplate;
        public GameObject NewDoor() => Object.Instantiate(doorTemplate);

        public override void SavePreloads(Dictionary<string, Dictionary<string, GameObject>> objectsByScene)
        {
            doorTemplate = objectsByScene["Ruins2_11_b"]["Love Door"];
        }

        public void ReparentDoor(GameObject obj, bool left)
        {
            Vector3 dst = obj.transform.position;
            Vector3 src = doorTemplate.transform.position;

            GameObject parent = new();
            parent.name = $"{obj.name} Animation Parent";
            var delta = dst - src;
            if (!left)
            {
                delta.x = dst.x + src.x;
                delta.z = 0.5129f;
                parent.transform.rotation = new(0, 180, 0, 1);
            }
            parent.transform.position = delta;

            obj.transform.parent = parent.transform;
            obj.transform.localPosition = src;
        }
    }
}
