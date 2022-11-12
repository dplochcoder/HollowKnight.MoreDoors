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

        private static void HookRandoSettingsManager() => RandoSettingsManagerMod.Instance.RegisterConnection(SettingsProxy.Instance);

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

        private void SetEnabledColor() => entryButton.Text.color = MoreDoors.GS.RandoSettings.IsEnabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;

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

        private ConnectionMenu(MenuPage connectionsPage)
        {
            MenuPage moreDoorsPage = new("MoreDoors Main Page", connectionsPage);
            entryButton = new(connectionsPage, Localize("More Doors"));
            entryButton.AddHideAndShowEvent(moreDoorsPage);

            var settings = MoreDoors.GS.RandoSettings;
            MenuElementFactory<RandomizationSettings> factory = new(moreDoorsPage, settings);
            Localize(factory);

            doorsLevel = ModifyColors(factory, nameof(settings.DoorsLevel), DoorsLevel.NoDoors);
            addKeyLocations = ModifyColors(factory, nameof(settings.AddKeyLocations), AddKeyLocations.None);
            SetEnabledColor();

            SmallButton customizeButton = new(moreDoorsPage, Localize("Customize Doors"));
            OnCustomDoorsChanged += () => customizeButton.Text.color = settings.EnabledDoors.Count == DoorData.Count ? Colors.DEFAULT_COLOR : Colors.TRUE_COLOR;

            transitions = (MenuItem<bool>)factory.ElementLookup[nameof(settings.RandomizeDoorTransitions)];
            LockIf(doorsLevel, DoorsLevel.NoDoors, transitions, customizeButton);

            MenuPage customPage = new("MoreDoors Customize Doors", moreDoorsPage);
            FillCustomDoorsPage(customPage);
            customizeButton.AddHideAndShowEvent(customPage);

            new VerticalItemPanel(moreDoorsPage, SpaceParameters.TOP_CENTER_UNDER_TITLE, SpaceParameters.VSPACE_MEDIUM, true,
                doorsLevel, transitions, customizeButton, addKeyLocations);
            OnCustomDoorsChanged?.Invoke();
        }

        public void ApplySettings(RandomizationSettings settings)
        {
            transitions.Unlock();
            transitions.SetValue(settings.RandomizeDoorTransitions);

            doorsLevel.SetValue(settings.DoorsLevel);
            addKeyLocations.SetValue(settings.AddKeyLocations);
            OnCustomDoorsChanged?.Invoke();
        }

        private SmallButton NewDoorsToggleButton(MenuPage page, string text, bool enabled)
        {
            SmallButton b = new(page, text);
            OnCustomDoorsChanged += () =>
            {
                if (DoorData.DoorNames.All(d => MoreDoors.GS.RandoSettings.IsDoorEnabled(d) == enabled)) b.Lock();
                else b.Unlock();
            };
            b.OnClick += () =>
            {
                DoorData.DoorNames.ForEach(d => MoreDoors.GS.RandoSettings.SetDoorEnabled(d, enabled));
                OnCustomDoorsChanged?.Invoke();
            };
            return b;
        }

        private void FillCustomDoorsPage(MenuPage page)
        {
            List<IMenuElement> doorButtons = new();
            foreach (var doorName in DoorData.DoorNames)
            {
                var data = DoorData.Get(doorName);

                ToggleButton button = new(page, Localize(data.UIName));
                button.ValueChanged += b => MoreDoors.GS.RandoSettings.SetDoorEnabled(doorName, b);
                OnCustomDoorsChanged += () =>
                {
                    if (MoreDoors.GS.RandoSettings.IsDoorEnabled(doorName) != button.Value) button.SetValue(!button.Value);
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