using MenuChanger;
using RandomizerMod.Menu;
using MenuChanger.MenuElements;
using static RandomizerMod.Localization;
using MenuChanger.Extensions;
using MenuChanger.MenuPanels;
using System;
using System.Collections.Generic;

namespace MoreDoors.Rando
{
    internal class ConnectionMenu
    {
        public static ConnectionMenu Instance { get; private set; }

        public static void Setup()
        {
            RandomizerMenuAPI.AddMenuPage(OnRandomizerMenuConstruction, TryGetMenuButton);
            MenuChangerMod.OnExitMainMenu += () => Instance = null;
        }

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

        private void SetEnabledColor() => entryButton.Text.color = MoreDoors.GS.MoreDoorsSettings.IsEnabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;

        private void ModifyColors<T>(MenuElementFactory<MoreDoorsSettings> factory, string fieldName, T none)
        {
            MenuItem<T> item = (MenuItem<T>)factory.ElementLookup[fieldName];
            item.ValueChanged += value =>
            {
                SetColor(item, value, none);
                SetEnabledColor();
            };
            SetColor(item, item.Value, none);
        }

        private SmallButton entryButton;

        private ConnectionMenu(MenuPage landingPage)
        {
            MenuPage mainPage = new("MoreDoors Main Page", landingPage);
            entryButton = new(landingPage, Localize("More Doors"));
            entryButton.AddHideAndShowEvent(mainPage);

            var settings = MoreDoors.GS.MoreDoorsSettings;
            MenuElementFactory<MoreDoorsSettings> factory = new(mainPage, settings);
            Localize(factory);

            ModifyColors(factory, nameof(settings.DoorsLevel), DoorsLevel.NoDoors);
            ModifyColors(factory, nameof(settings.AddKeyLocations), AddKeyLocations.None);
            SetEnabledColor();

            VerticalItemPanel panel = new(mainPage, SpaceParameters.TOP_CENTER_UNDER_TITLE, SpaceParameters.VSPACE_MEDIUM, true, factory.Elements);
        }
    }
}