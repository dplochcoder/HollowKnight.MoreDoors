using System.Collections.Generic;
using UnityEngine;

namespace MoreDoors.IC
{
    public class Preloader : ItemChanger.Internal.Preloaders.Preloader
    {   public static Preloader Instance { get; } = new();

        public override IEnumerable<(string, string)> GetPreloadNames()
        {
            // FIXME
            yield break;
        }

        public override void SavePreloads(Dictionary<string, Dictionary<string, GameObject>> objectsByScene)
        {
        }
    }
}
