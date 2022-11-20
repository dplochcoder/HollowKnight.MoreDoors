using MenuChanger;
using RandomizerMod.Menu;
using MenuChanger.MenuElements;
using static RandomizerMod.Localization;
using MenuChanger.Extensions;
using MenuChanger.MenuPanels;
using System.Collections.Generic;
using MoreDoors.Data;
using Modding;
using RandoSettingsManager;
using PurenailCore.SystemUtil;
using System.Linq;

namespace MoreDoors.Rando
{
    internal class ConnectionMenu
    {
        public static ConnectionMenu Instance { get; private set; }

        public static void Setup()
        {
            RandomizerMenuAPI.AddMenuPage(OnRandomizerMenuConstruction, TryGetMenuButton);
            MenuChangerMod.OnExitMainMenu += () => Instance = null;

            if (ModHooks.GetMod("RandoSettingsManager") is Mod)
            {
                HookRandoSettingsManager();
            }
        }

        private static void HookRandoSettingsManager() => RandoSettingsManagerMod.Instance.RegisterConnection(new SettingsProxy());

        public static void OnRandomizerMenuConstruction(MenuPage page) => Instance = new(page);

        public static bool TryGetMenuButton(MenuPage page, out SmallButton button)
        {
            button = Instance.entryButton;
            return true;
        }

        private static void SetColor<T>(MenuItem<T> item, T value, T none)
        {
            item.Text.color = EqualityComparer<T>.Default.Equals(value, none) ? Colors.FALSE_COLOR : Colors.DEFAULT_COLOR;
        }

        private void SetEnabledColor() => entryButton.Text.color = Settings.IsEnabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;

        private MenuItem<T> ModifyColors<T>(MenuElementFactory<RandomizationSettings> factory, string fieldName, T none)
        {
            MenuItem<T> item = (MenuItem<T>)factory.ElementLookup[fieldName];
            item.ValueChanged += value =>
            {
                SetColor(item, value, none);
                SetEnabledColor();
            };
            SetColor(item, item.Value, none);
            return item;
        }

        private void LockIf<T>(MenuItem<T> item, T value, params ILockable[] lockables)
        {
            item.ValueChanged += UpdateLocks;
            UpdateLocks(item.Value);

            void UpdateLocks(T t)
            {
                if (EqualityComparer<T>.Default.Equals(t, value))
                {
                    foreach (var lockable in lockables)
                    {
                        lockable.Lock();
                    }
                }
                else
                {
                    foreach (var lockable in lockables)
                    {
                        lockable.Unlock();
                    }
                }
            }

        }

        private SmallButton entryButton;
        private MenuItem<DoorsLevel> doorsLevel;
        private MenuItem<AddKeyLocations> addKeyLocations;
        private MenuItem<bool> transitions;

        // Event for any change in settings
        private delegate void CustomDoorsChanged();
        private event CustomDoorsChanged OnCustomDoorsChanged;

        private RandomizationSettings Settings => MoreDoors.GS.RandoSettings;

        private ConnectionMenu(MenuPage connectionsPage)
        {
            MenuPage moreDoorsPage = new("MoreDoors Main Page", connectionsPage);
            entryButton = new(connectionsPage, Localize("More Doors"));
            entryButton.AddHideAndShowEvent(moreDoorsPage);

            MenuElementFactory<RandomizationSettings> factory = new(moreDoorsPage, Settings);
            Localize(factory);

            doorsLevel = ModifyColors(factory, nameof(Settings.DoorsLevel), DoorsLevel.NoDoors);
            addKeyLocations = ModifyColors(factory, nameof(Settings.AddKeyLocations), AddKeyLocations.None);
            SetEnabledColor();

            SmallButton customizeButton = new(moreDoorsPage, Localize("Customize Doors"));
            OnCustomDoorsChanged += () => customizeButton.Text.color = customizeButton.Locked ? Colors.LOCKED_FALSE_COLOR : (Settings.DisabledDoors.Count == 0 ? Colors.DEFAULT_COLOR : Colors.TRUE_COLOR);

            transitions = (MenuItem<bool>)factory.ElementLookup[nameof(Settings.RandomizeDoorTransitions)];
            LockIf(doorsLevel, DoorsLevel.NoDoors, transitions, customizeButton);
            doorsLevel.ValueChanged += _ => OnCustomDoorsChanged();

            MenuPage customPage = new("MoreDoors Customize Doors", moreDoorsPage);
            FillCustomDoorsPage(customPage);
            customizeButton.AddHideAndShowEvent(customPage);

            new VerticalItemPanel(moreDoorsPage, SpaceParameters.TOP_CENTER_UNDER_TITLE, SpaceParameters.VSPACE_MEDIUM, true,
                doorsLevel, transitions, customizeButton, addKeyLocations);
            OnCustomDoorsChanged();
        }

        public void ApplySettings(RandomizationSettings settings)
        {
            transitions.Unlock();
            transitions.SetValue(settings.RandomizeDoorTransitions);

            doorsLevel.SetValue(settings.DoorsLevel);
            addKeyLocations.SetValue(settings.AddKeyLocations);
            Settings.DisabledDoors.Clear();
            settings.DisabledDoors.ForEach(d => Settings.DisabledDoors.Add(d));
            OnCustomDoorsChanged();
        }

        private SmallButton NewDoorsToggleButton(MenuPage page, string text, bool enabled)
        {
            SmallButton b = new(page, text);
            OnCustomDoorsChanged += () =>
            {
                if (DoorData.DoorNames.All(d => Settings.IsDoorEnabled(d) == enabled)) b.Lock();
                else b.Unlock();
            };
            b.OnClick += () =>
            {
                DoorData.DoorNames.ForEach(d => Settings.SetDoorEnabled(d, enabled));
                OnCustomDoorsChanged();
            };
            return b;
        }

        private void FillCustomDoorsPage(MenuPage page)
        {
            List<IMenuElement> doorButtons = new();
            foreach (var doorName in DoorData.DoorNames)
            {
                var localDoorName = doorName;
                ToggleButton button = new(page, Localize(DoorData.Get(doorName).UIName));
                button.ValueChanged += b =>
                {
                    Settings.SetDoorEnabled(localDoorName, b);
                    OnCustomDoorsChanged();
                };
                OnCustomDoorsChanged += () =>
                {
                    if (Settings.IsDoorEnabled(localDoorName) != button.Value) button.SetValue(!button.Value);
                };

                doorButtons.Add(button);
            }

            SmallButton enableAllButton = NewDoorsToggleButton(page,"Enable All", true);
            SmallButton disableAllButton = NewDoorsToggleButton(page, "Disable All", false);

            GridItemPanel togglePanel = new(page, SpaceParameters.TOP_CENTER, 2, SpaceParameters.VSPACE_SMALL, SpaceParameters.HSPACE_LARGE, false, enableAllButton, disableAllButton);
            GridItemPanel doorsPanel = new(page, SpaceParameters.TOP_CENTER, 4, SpaceParameters.VSPACE_SMALL, SpaceParameters.HSPACE_SMALL, false, doorButtons.ToArray());
            new VerticalItemPanel(page, SpaceParameters.TOP_CENTER, SpaceParameters.VSPACE_MEDIUM, true, togglePanel, doorsPanel);
        }
    }
}