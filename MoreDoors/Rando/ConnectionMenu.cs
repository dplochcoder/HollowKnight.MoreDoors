using MenuChanger;
using RandomizerMod.Menu;
using MenuChanger.MenuElements;
using static RandomizerMod.Localization;
using MenuChanger.Extensions;
using MenuChanger.MenuPanels;
using System;
using System.Collections.Generic;
using ConnectionSettingsCode;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine;
using MoreDoors.Data;

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

        private SmallButton entryButton;

        private ConnectionMenu(MenuPage connectionsPage)
        {
            MenuPage moreDoorsPage = new("MoreDoors Main Page", connectionsPage);
            entryButton = new(connectionsPage, Localize("More Doors"));
            entryButton.AddHideAndShowEvent(moreDoorsPage);

            var settings = MoreDoors.GS.RandoSettings;
            MenuElementFactory<RandomizationSettings> factory = new(moreDoorsPage, settings);
            Localize(factory);

            var doorsLevel = ModifyColors(factory, nameof(settings.DoorsLevel), DoorsLevel.NoDoors);
            var addKeyLocations = ModifyColors(factory, nameof(settings.AddKeyLocations), AddKeyLocations.None);
            SetEnabledColor();

            DoorsMaskElement dme = new();
            MenuPage customPage = new("MoreDoors Customize Doors", moreDoorsPage);
            FillCustomDoorsPage(customPage, dme);

            SmallButton customizeButton = new(moreDoorsPage, Localize("Customize Doors"));
            customizeButton.AddHideAndShowEvent(customPage);

            new VerticalItemPanel(moreDoorsPage, SpaceParameters.TOP_CENTER_UNDER_TITLE, SpaceParameters.VSPACE_MEDIUM, true,
                doorsLevel, customizeButton, addKeyLocations);
            new SettingsCode(moreDoorsPage, MoreDoors.Instance, doorsLevel, dme, addKeyLocations);
            new SettingsCode(customPage, MoreDoors.Instance, doorsLevel, dme, addKeyLocations);
        }

        private void FillCustomDoorsPage(MenuPage page, IValueElement<int> mask)
        {
            List<IMenuElement> doorButtons = new();
            for (int i = 0; i < DoorData.Count; i++)
            {
                var doorName = DoorData.DoorNames[i];
                var data = DoorData.Get(doorName);

                int index = i;
                ToggleButton button = new(page, Localize(data.UIName));
                button.ValueChanged += b => mask.SetValue((mask.Value & ~(1 << index)) | (b ? (1 << index) : 0));
                mask.ValueChanged += _ =>
                {
                    bool newValue = MoreDoors.GS.RandoSettings.IsDoorAllowed(index);
                    if (button.Value != newValue) button.SetValue(newValue);
                };

                button.SetValue(MoreDoors.GS.RandoSettings.IsDoorAllowed(index));
                doorButtons.Add(button);
            }

            GridItemPanel panel = new(page, SpaceParameters.TOP_CENTER, 4, SpaceParameters.VSPACE_SMALL, SpaceParameters.HSPACE_SMALL, true, doorButtons.ToArray());
        }
    }

    internal class DoorsMaskElement : IValueElement<int>
    {
        public int Value => MoreDoors.GS.RandoSettings.DoorsMask;

        public Type ValueType => typeof(int);

        public MenuPage Parent => null;

        public bool Hidden => true;

        object IValueElement.Value => Value;

        public event Action<int> ValueChanged;
        public event Action<IValueElement> SelfChanged;

        public void SetValue(int t)
        {
            int old = MoreDoors.GS.RandoSettings.DoorsMask;
            if (t == old) return;

            MoreDoors.GS.RandoSettings.DoorsMask = t;
            ValueChanged?.Invoke(t);
            SelfChanged?.Invoke(this);
        }

        public void SetValue(object o)
        {
            if (o is int i) SetValue(i);
        }

        public void Show() { }
        public void Translate(Vector2 delta) { }
        public void Bind(object o, MemberInfo mi) => throw new NotImplementedException();
        public void Destroy() { }
        public ISelectable GetISelectable(Neighbor neighbor) => throw new NotImplementedException();
        public Selectable GetSelectable(Neighbor neighbor) => throw new NotImplementedException();
        public void Hide() { }
        public void MoveTo(Vector2 pos) { }
        public void SetNeighbor(Neighbor neighbor, ISelectable selectable) => throw new NotImplementedException();
    }
}