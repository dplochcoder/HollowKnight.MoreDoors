using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace MoreDoors.Rando
{
    public class SettingsProxy : RandoSettingsProxy<RandomizationSettings, string>
    {
        public static readonly SettingsProxy Instance = new();

        public override string ModKey => nameof(MoreDoors);

        private static readonly VersioningPolicy<string> Policy = new StrictModVersioningPolicy(MoreDoors.Instance);

        public override VersioningPolicy<string> VersioningPolicy => Policy;

        public override bool TryProvideSettings(out RandomizationSettings? settings)
        {
            settings = MoreDoors.GS.RandoSettings;
            return settings.IsEnabled;
        }

        public override void ReceiveSettings(RandomizationSettings? settings) => ConnectionMenu.Instance.ApplySettings(settings ?? new());
    }
}
