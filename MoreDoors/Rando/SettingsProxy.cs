using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace MoreDoors.Rando
{
    public class SettingsProxy : RandoSettingsProxy<RandomizationSettings, string>
    {
        public override string ModKey => nameof(MoreDoors);

        public override VersioningPolicy<string> VersioningPolicy => new StrictModVersioningPolicy(MoreDoors.Instance);

        public override bool TryProvideSettings(out RandomizationSettings? settings)
        {
            settings = MoreDoors.GS.RandoSettings;
            return settings.IsEnabled;
        }

        public override void ReceiveSettings(RandomizationSettings? settings) => ConnectionMenu.Instance.ApplySettings(settings ?? new());
    }
}
