using System.Collections.Generic;
using UnityEngine;

namespace MoreDoors.IC
{
    public class Preloader : ItemChanger.Internal.Preloaders.Preloader
    {   public static Preloader Instance { get; } = new();

        public override IEnumerable<(string, string)> GetPreloadNames()
        {
            yield return ("Ruins_2_11_b", "Love Door");
        }

        private GameObject doorTemplate;
        public GameObject NewDoor() => Object.Instantiate(doorTemplate);

        public override void SavePreloads(Dictionary<string, Dictionary<string, GameObject>> objectsByScene)
        {
            doorTemplate = objectsByScene["Ruins_2_11_b"]["Love Door"];
        }
    }
}
