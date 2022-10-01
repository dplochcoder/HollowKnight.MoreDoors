using PurenailCore.ModUtil;
using UnityEngine;

namespace MoreDoors.IC
{
    public class Preloader : PurenailCore.ModUtil.Preloader
    {
        public static readonly Preloader Instance = new();

        [Preload("Ruins2_11_b", "Love Door")]
        public GameObject Door { get; private set; }

        [Preload("Crossroads_ShamanTemple", "Shiny Item")]
        public GameObject Shiny { get; private set; }
    }
}
