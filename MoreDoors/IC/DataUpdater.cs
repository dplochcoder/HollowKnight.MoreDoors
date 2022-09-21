using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreDoors.IC
{

    public static class DataUpdater
    {
        private static string InferGitRoot(string path)
        {
            var info = Directory.GetParent(path);
            while (info != null)
            {
                if (Directory.Exists(Path.Combine(info.FullName, ".git")))
                {
                    return info.FullName;
                }
                info = Directory.GetParent(info.FullName);
            }

            return path;
        }

        public static void Run()
        {
            string root = InferGitRoot(Directory.GetCurrentDirectory());
            string path = $"{root}/MoreDoors/Resources/Data/doors.json";

            File.Delete(path);
            JsonUtil.Serialize(DoorData.Data, path);
        }
    }
}
