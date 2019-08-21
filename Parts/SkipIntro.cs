using System;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyInfoUI
{
    internal class SkipIntro
    {
        private IModEvents _events => ModEntry.Events;
        //private bool _skipIntro = false;

        internal SkipIntro()
        {
            //GameEvents.QuarterSecondTick += CheckForSkip;
            _events.Input.ButtonPressed += OnButtonPressed;
            _events.GameLoop.SaveLoaded += OnSaveLoaded;
            //MenuEvents.MenuChanged += SkipToTitleButtons;
        }

        /// <summary>Raised after the player loads a save slot and the world is initialised.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, EventArgs e)
        {
            // stop checking for skip key
            _events.Input.ButtonPressed -= OnButtonPressed;
            _events.GameLoop.SaveLoaded -= OnSaveLoaded;
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu is TitleMenu menu && e.Button == SButton.Escape)
            {
                menu.skipToTitleButtons();
                _events.Input.ButtonPressed -= OnButtonPressed;
            }
        }

        //private void CheckForSkip(object sender, EventArgs e)
        //{
        //    if (Game1.activeClickableMenu is TitleMenu &&
        //        _skipIntro)
        //    {
        //        _skipIntro = false;
        //        (Game1.activeClickableMenu as TitleMenu)?.skipToTitleButtons();
        //    }
        //}

        //private void SkipToTitleButtons(object sender, EventArgsClickableMenuChanged e)
        //{
        //    TitleMenu menu = e.NewMenu as TitleMenu;
        //    menu?.skipToTitleButtons();
        //    //MenuEvents.MenuChanged -= SkipToTitleButtons;
        //}
    }
}
