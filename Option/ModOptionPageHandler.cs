using System;
using System.Collections.Generic;
using System.Reflection;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyUI
{
    internal class ModOptionPageHandler : IDisposable
    {
        private List<ModOptionPart> _optionParts = new List<ModOptionPart>();
        private readonly List<IDisposable> _partsToDispose;
        private readonly IDictionary<string, String> _options;
        private ModOptionPageButton _modOptionPageButton;
        private ModOptionPage _modOptionPage;
        // private readonly IModHelper _helper;

        private IModEvents Events => ModEntry.Events;
        private IReflectionHelper Reflection => ModEntry.Reflection;

        private int _modOptionsTabPageNumber;

        private readonly IconLuckOfDay _iconLuckOfDay;
        private readonly IconBirthday _iconBirthday;
        private readonly IconNewRecipe _iconNewRecipe;
        private readonly IconToolUpgrade _iconToolUpgrade;
        private readonly IconTravelingMerchant _iconTravelingMerchant;
        internal readonly ShowNPCOnMap _showNPCOnMap;
        private readonly ShowCalendarAndBillboard _showCalendarAndBillboard;
        private readonly ShowAccurateHearts _showAccurateHearts;
        // private readonly ShowBundleItemTooltip _showBundleItemTooltip;

        private readonly ShowMonsterHealthBar _showMonsterHealthBar;
        private readonly ShowItemEffectRanges _showScarecrowAndSprinklerRange;
        private readonly ShowLocationName _showLocationName;
        private readonly ShowBuriedItems _showBuriedItems;
        private readonly ShowStaminaHealthNumber _showStaminaHealthNumber;
        private readonly ShowAnimalNeedsPet _showWAnimalNeedsPet;

        private readonly ShowItemHoverInformation _showItemHoverInformation;
        private readonly ShowCropAndBarrelTime _showCropAndBarrelTime;
        private readonly ShopHarvestPrices _shopHarvestPrices;

        private readonly ExperienceBar _experienceBar;

        internal ModOptionPageHandler(IDictionary<String, String> options)
        {
            _options = options;
           // helper.Events.Display.MenuChanged += ToggleModOptions;
           // _helper = helper;
           // ModConfig modConfig = _helper.ReadConfig<ModConfig>();

            _iconLuckOfDay = new IconLuckOfDay();
            _iconBirthday = new IconBirthday();
            _iconNewRecipe = new IconNewRecipe();
            _iconToolUpgrade = new IconToolUpgrade();
            _iconTravelingMerchant = new IconTravelingMerchant();

            _showNPCOnMap = new ShowNPCOnMap(_options);
            _showCalendarAndBillboard = new ShowCalendarAndBillboard();
            _showAccurateHearts = new ShowAccurateHearts();

            _showMonsterHealthBar = new ShowMonsterHealthBar();
            _showScarecrowAndSprinklerRange = new ShowItemEffectRanges();
            _showLocationName = new ShowLocationName();
            _showBuriedItems = new ShowBuriedItems();
            _showStaminaHealthNumber = new ShowStaminaHealthNumber();
            _showWAnimalNeedsPet = new ShowAnimalNeedsPet();

            _showItemHoverInformation = new ShowItemHoverInformation();
            _showCropAndBarrelTime = new ShowCropAndBarrelTime();
            _shopHarvestPrices = new ShopHarvestPrices();

            _experienceBar = new ExperienceBar();

            _partsToDispose = new List<IDisposable>()
            {
                _iconLuckOfDay,
                _iconBirthday,
                _iconNewRecipe,
                _iconToolUpgrade,
                _iconTravelingMerchant,

                _showNPCOnMap,
                _showCalendarAndBillboard,
                _showAccurateHearts,

                // _showMonsterHealthBar,
                _showScarecrowAndSprinklerRange,
                // _showLocationName,
                _showBuriedItems,
                // _showStaminaHealthNumber,
                _showWAnimalNeedsPet,
                _showItemHoverInformation,
                _showCropAndBarrelTime,
                _shopHarvestPrices,
                _experienceBar,
            };


            string Trans(string key) => ModEntry.Translation.Get(key);

            int whichOption = 1001;
            Version thisVersion = Assembly.GetAssembly(this.GetType()).GetName().Version;
            _optionParts.Add(new ModOptionPart("UI Info Suite v" +
                thisVersion.Major + "." + thisVersion.Minor + "." + thisVersion.Build));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.IconLuckOfDay), whichOption++, _iconLuckOfDay.Toggle, _options, OptionKeys.IconLuckOfDay));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.IconBirthday), whichOption++, _iconBirthday.ToggleOption, _options, OptionKeys.IconBirthday));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.IconNewRecipe), whichOption++, _iconNewRecipe.ToggleOption, _options, OptionKeys.IconNewRecipe));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.IconToolUpgrade), whichOption++, _iconToolUpgrade.ToggleOption, _options, OptionKeys.IconToolUpgrade));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.IconTravelingMerchant), whichOption++, _iconTravelingMerchant.ToggleOption, _options, OptionKeys.IconTravelingMerchant));

            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowNPCOnMap), whichOption++, _showNPCOnMap.ToggleShowNPCLocationsOnMap, _options, OptionKeys.ShowNPCOnMap));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowCalendarAndBillboard), whichOption++, _showCalendarAndBillboard.ToggleOption, _options, OptionKeys.ShowCalendarAndBillboard));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowHeartFills), whichOption++, _showAccurateHearts.ToggleOption, _options, OptionKeys.ShowHeartFills));

            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowMonsterHealthBar), whichOption++, _showMonsterHealthBar.ToggleOption, _options, OptionKeys.ShowMonsterHealthBar));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowItemEffectRanges), whichOption++, _showScarecrowAndSprinklerRange.ToggleOption, _options, OptionKeys.ShowItemEffectRanges));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowLocationName), whichOption++, _showLocationName.ToggleOption, _options, OptionKeys.ShowLocationName));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowBuriedItems), whichOption++, _showBuriedItems.ToggleOption, _options, OptionKeys.ShowBuriedItems));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowStaminaHealthNumber), whichOption++, _showStaminaHealthNumber.ToggleOption, _options, OptionKeys.ShowStaminaHealthNumber));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowAnimalsNeedPets), whichOption++, _showWAnimalNeedsPet.ToggleOption, _options, OptionKeys.ShowAnimalsNeedPets));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowExtraItemInformation), whichOption++, _showItemHoverInformation.ToggleOption, _options, OptionKeys.ShowExtraItemInformation));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowCropAndBarrelTooltip), whichOption++, _showCropAndBarrelTime.ToggleOption, _options, OptionKeys.ShowCropAndBarrelTooltip));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowHarvestPricesInShop), whichOption++, _shopHarvestPrices.ToggleOption, _options, OptionKeys.ShowHarvestPricesInShop));

            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowExperienceBar), whichOption++, _experienceBar.ToggleShowExperienceBar, _options, OptionKeys.ShowExperienceBar));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.AllowExperienceBarToFadeOut), whichOption++, _experienceBar.ToggleExperienceBarFade, _options, OptionKeys.AllowExperienceBarToFadeOut));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowExperienceGain), whichOption++, _experienceBar.ToggleShowExperienceGain, _options, OptionKeys.ShowExperienceGain));
            _optionParts.Add(new ModOptionCheckbox(Trans(OptionKeys.ShowLevelUpAnimation), whichOption++, _experienceBar.ToggleLevelUpAnimation, _options, OptionKeys.ShowLevelUpAnimation));
        }

        public void Dispose()
        {
            foreach (var item in _partsToDispose)
                item.Dispose();
        }

        private void OnButtonLeftClicked(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is GameMenu menu
                && menu.currentTab != GameMenu.optionsTab)
            {
                SetActiveClickableMenuToModOptionsPage();
                Game1.playSound("smallSelect");
            }
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void ToggleModOptions(object sender, MenuChangedEventArgs e)
        {
            // remove from old menu
            if (e.OldMenu != null)
            {
                Events.Display.RenderedActiveMenu -= DrawButton;
                if (_modOptionPageButton != null)
                    _modOptionPageButton.OnLeftClicked -= OnButtonLeftClicked;

                if (e.OldMenu is GameMenu)
                {
                    List<IClickableMenu> tabPages = Reflection.GetField<List<IClickableMenu>>(e.OldMenu, "pages").GetValue();
                    tabPages.Remove(_modOptionPage);
                }
            }

            // add to new menu
            if (e.NewMenu is GameMenu newMenu)
            {
                if (_modOptionPageButton == null)
                {
                    _modOptionPage = new ModOptionPage(_optionParts);
                    _modOptionPageButton = new ModOptionPageButton();
                }

                Events.Display.RenderedActiveMenu += DrawButton;
                _modOptionPageButton.OnLeftClicked += OnButtonLeftClicked;
                List<IClickableMenu> tabPages = Reflection.GetField<List<IClickableMenu>>(newMenu, "pages").GetValue();

                _modOptionsTabPageNumber = tabPages.Count;
                tabPages.Add(_modOptionPage);
            }
        }

        private void SetActiveClickableMenuToModOptionsPage()
        {
            if (Game1.activeClickableMenu is GameMenu menu)
                menu.currentTab = _modOptionsTabPageNumber;
        }

        private void DrawButton(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu is GameMenu &&
                (Game1.activeClickableMenu as GameMenu).currentTab != 3) //don't render when the map is showing
            {
                if ((Game1.activeClickableMenu as GameMenu).currentTab == _modOptionsTabPageNumber)
                {
                    _modOptionPageButton.yPositionOnScreen = Game1.activeClickableMenu.yPositionOnScreen + 24;
                }
                else
                {
                    _modOptionPageButton.yPositionOnScreen = Game1.activeClickableMenu.yPositionOnScreen + 16;
                }
                _modOptionPageButton.draw(Game1.spriteBatch);

                //Might need to render hover text here
            }
        }
    }
}
