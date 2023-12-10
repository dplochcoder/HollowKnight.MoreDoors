﻿using ItemChanger.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JsonUtil = PurenailCore.SystemUtil.JsonUtil<MoreDoors.MoreDoors>;

namespace MoreDoors.Data;

public static class DataUpdater
{
    public static void Run()
    {
        string root = JsonUtil.InferGitRoot();
        string jsonPath = $"{root}/MoreDoors/Resources/Data/doors.json";
        string namesPath = $"{root}/MoreDoors/Data/DoorNames.cs";

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

        JsonUtil.RewriteJsonFile(DoorData.Data, jsonPath);
        RewriteDoorNamesFile(namesPath);
    }

    private static void RewriteDoorNamesFile(string path)
    {
        List<string> content = new();
        content.Add("namespace MoreDoors.Data;");
        content.Add("");
        content.Add("internal clas DoorNames");
        content.Add("{");
        foreach (var door in DoorData.DoorNames)
        {
            content.Add($"    public const string {ConstName(door)} = \"{door}\"");
        }
        content.Add("}");
        content.Add("");

        File.Delete(path);
        File.WriteAllText(path, string.Join("\n", content.ToArray()));
    }

    private static string ConstName(string name) => name.ToUpper().Replace(" ", "_").Replace("'", "");

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
