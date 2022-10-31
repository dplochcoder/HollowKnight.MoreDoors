using RandomizerMod.Settings;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace MoreDoors.Rando
{
    public class SettingsProxy : RandoSettingsProxy<RandomizationSettings, RandomizationSettings.Version>
    {
        public static readonly SettingsProxy Instance = new();

        private class Policy : VersioningPolicy<RandomizationSettings.Version>
        {
            public static readonly Policy Instance = new();

            public override RandomizationSettings.Version Version => RandomizationSettings.Version.Instance;

            public override bool Allow(RandomizationSettings.Version version) => version == Version;
        }

        public override string ModKey => nameof(MoreDoors);

        public override VersioningPolicy<RandomizationSettings.Version> VersioningPolicy => Policy.Instance;

        public override bool TryProvideSettings(out RandomizationSettings? settings)
        {
            settings = MoreDoors.GS.RandoSettings;
            return settings.IsEnabled;
        }

        public override void ReceiveSettings(RandomizationSettings? settings) => ConnectionMenu.Instance.ApplySettings(settings ?? new());
    }
}
