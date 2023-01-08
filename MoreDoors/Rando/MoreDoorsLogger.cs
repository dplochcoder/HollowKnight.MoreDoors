using RandomizerMod.Logging;
using System.IO;

using JsonUtil = PurenailCore.SystemUtil.JsonUtil<MoreDoors.MoreDoors>;

namespace MoreDoors.Rando
{
    public class MoreDoorsLogger : RandoLogger
    {
        public override void Log(LogArguments args)
        {
            if (RandoInterop.IsEnabled)
            {
                LogManager.Write(DoLog, "MoreDoorsSpoiler.json");
            }
        }

        public void DoLog(TextWriter tw)
        {
            JsonUtil.Serialize(RandoInterop.LS, tw);
        }
    }
}
