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
            parent.transform.position = dst - src;
            parent.transform.rotation = new(0, left ? 0 : 180, 0, 0);
            obj.transform.parent = parent.transform;
            obj.transform.localPosition = src;
        }
    }
}
