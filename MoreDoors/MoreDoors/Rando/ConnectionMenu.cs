using System.Collections.Generic;
using System;
using MenuChanger;
using RandomizerMod.Menu;
using MenuChanger.MenuElements;
using static RandomizerMod.Localization;
using MenuChanger.Extensions;

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

        public SmallButton entryButton;
        public MenuPage mainPage;
        public MenuElementFactory<MoreDoorsSettings> factory;

        private static T Lookup<T>(MenuElementFactory<MoreDoorsSettings> factory, string name) where T : MenuItem => factory.ElementLookup[name] as T ?? throw new ArgumentException("Menu error");

        private static void LockIfFalse(MenuItem<bool> src, List<ILockable> dest)
        {
            void onChange(bool value)
            {
                foreach (var lockable in dest)
                {
                    if (value) lockable.Unlock();
                    else lockable.Lock();
                }
            }

            src.ValueChanged += onChange;
            onChange(src.Value);
        }

        private ConnectionMenu(MenuPage landingPage)
        {
            mainPage = new("MoreDoors Main Page", landingPage);
            entryButton = new(landingPage, Localize("More Doors"));
            entryButton.AddHideAndShowEvent(mainPage);

            var settings = MoreDoors.GS.MoreDoorsSettings;
            factory = new(mainPage, settings);
            var addMoreDoors = Lookup<MenuItem<bool>>(factory, nameof(settings.AddMoreDoors));
            var doorsLevel = Lookup<MenuItem>(factory, nameof(settings.DoorsLevel));
            var addKeyLocations = Lookup<MenuItem>(factory, nameof(settings.AddKeyLocations));

            LockIfFalse(addMoreDoors, new() { doorsLevel, addKeyLocations });
        }
    }
}