using PurenailCore.ModUtil;
using System.Collections.Generic;
using UnityEngine;

namespace MoreDoors.IC
{
    public class Preloader : PurenailCore.ModUtil.Preloader
    {
        public static readonly Preloader Instance = new();

        public PreloadedObject Door;
        public PreloadedObject Shiny;

        public Preloader()
        {
            Door = new(this, "Ruins2_11_b", "Love Door");
            Shiny = new(this, "Crossroads_ShamanTemple", "Shiny Item");
        }
    }
}
