using System.Collections.Generic;

namespace MoreDoors.Rando
{
    public class LocalSettings
    {
        public MoreDoorsSettings Settings = MoreDoors.GS.MoreDoorsSettings;
        public HashSet<string> EnabledDoorNames = new();
    }
}
