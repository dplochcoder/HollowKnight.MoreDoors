using ItemChanger.Locations;
using System;

using JsonUtil = PurenailCore.SystemUtil.JsonUtil<MoreDoors.MoreDoors>;

namespace MoreDoors.Data
{

    public static class DataUpdater
    {
        public static void Run()
        {
            string root = JsonUtil.InferGitRoot();
            string path = $"{root}/MoreDoors/Resources/Data/doors.json";

            bool anyErr = false;
            foreach (var door in DoorData.DoorNames)
            {
                if (!Validate(DoorData.Get(door), out string err))
                {
                    Console.WriteLine($"Error on door {door}: {err}");
                    anyErr = true;
                }
            }
            if (anyErr) throw new ArgumentException("Errors encountered");


            JsonUtil.RewriteJsonFile(DoorData.Data, path);
        }

        private static bool Validate(DoorData d, out string err)
        {
            string s = d.Key.Location.sceneName;
            if (d.Key.Location is DualLocation dl)
            {
                if (dl.falseLocation.sceneName != s)
                {
                    err = "Bad false location sceneName";
                    return false;
                }
                if (dl.trueLocation.sceneName != s)
                {
                    err = "Bad true location sceneName";
                    return false;
                }
            }

            err = "";
            return true;
        }
    }
}
