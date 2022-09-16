using System.Collections.Generic;

namespace MoreDoors.IC
{
    public class MoreDoorsModule : ItemChanger.Modules.Module
    {
        public SortedDictionary<string, bool> ObtainedKeys = new();

        public override void Initialize()
        {
            Modding.ModHooks.GetPlayerBoolHook += OverrideGetBool;
            Modding.ModHooks.SetPlayerBoolHook += OverrideSetBool;
        }

        public override void Unload()
        {
            Modding.ModHooks.GetPlayerBoolHook -= OverrideGetBool;
            Modding.ModHooks.SetPlayerBoolHook -= OverrideSetBool;
        }

        public const string PlayerDataKeyPrefix = "MOREDOORS_";

        private static bool GetKeyName(string name, out string keyName)
        {
            if (name.StartsWith(PlayerDataKeyPrefix))
            {
                keyName = name.Substring(PlayerDataKeyPrefix.Length);
                return true;
            }

            keyName = "";
            return false;
        }

        private bool OverrideGetBool(string name, bool orig) => GetKeyName(name, out string keyName) ?
                ObtainedKeys[keyName] : orig;

        private bool OverrideSetBool(string name, bool orig)
        {
            if (GetKeyName(name, out string keyName))
            {
                ObtainedKeys[keyName] = orig;
            }

            return orig;
        }
    }
}
