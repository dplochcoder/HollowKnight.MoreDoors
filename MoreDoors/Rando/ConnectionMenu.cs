using MenuChanger;
using RandomizerMod.Menu;
using MenuChanger.MenuElements;
using static RandomizerMod.Localization;
using MenuChanger.Extensions;
using MenuChanger.MenuPanels;

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

        private SmallButton entryButton;

        private ConnectionMenu(MenuPage landingPage)
        {
            MenuPage mainPage = new("MoreDoors Main Page", landingPage);
            entryButton = new(landingPage, Localize("More Doors"));
            entryButton.AddHideAndShowEvent(mainPage);

            MenuElementFactory<MoreDoorsSettings> factory = new(mainPage, MoreDoors.GS.MoreDoorsSettings);
            Localize(factory);

            VerticalItemPanel panel = new(mainPage, SpaceParameters.TOP_CENTER_UNDER_TITLE, SpaceParameters.VSPACE_MEDIUM, true, factory.Elements);
        }
    }
}