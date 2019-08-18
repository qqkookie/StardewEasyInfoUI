﻿using System;
using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI.Events;

namespace EasyUI
{
    internal class IconTravelingMerchant : IDisposable
    {
        private bool _travelingMerchantIsHere = false;
        private ClickableTextureComponent _travelingMerchantIcon;
        private static IModEvents Events => ModEntry.Events;

        internal IconTravelingMerchant()
        {
        }

        internal void ToggleOption(bool showTravelingMerchant)
        {
            Events.Display.RenderingHud -= OnRenderingHud;
            Events.Display.RenderedHud -= OnRenderedHud;
            Events.GameLoop.DayStarted -= OnDayStarted;

            if (showTravelingMerchant)
            {
                UpdateTravelingMerchant();
                Events.Display.RenderingHud += OnRenderingHud;
                Events.Display.RenderedHud += OnRenderedHud;
                Events.GameLoop.DayStarted += OnDayStarted;
            }
        }

        public void Dispose()
        {
            ToggleOption(false);
        }

        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, EventArgs e)
        {
            UpdateTravelingMerchant();
        }

        private void UpdateTravelingMerchant()
        {
            int dayOfWeek = Game1.dayOfMonth % 7;
            _travelingMerchantIsHere = dayOfWeek == 0 || dayOfWeek == 5;
        }

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            // draw traveling merchant
            if (!Game1.eventUp && _travelingMerchantIsHere)
            {
                Point iconPosition = IconHandler.Handler.GetNewIconPosition();
                _travelingMerchantIcon = 
                    new ClickableTextureComponent(
                        new Rectangle(iconPosition.X, iconPosition.Y, 40, 40), 
                        Game1.mouseCursors, 
                        new Rectangle(192, 1411, 20, 20), 
                        2f);
                _travelingMerchantIcon.draw(Game1.spriteBatch);
            }
        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the sprite batch, but before it's rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            // draw hover text
            if (_travelingMerchantIsHere && _travelingMerchantIcon.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
            {
                string hoverText = ModEntry.Translation.Get(
                    LanguageKeys.TravelingMerchantIsInTown);
                IClickableMenu.drawHoverText(
                    Game1.spriteBatch,
                    hoverText, Game1.dialogueFont);
            }
        }
    }
}
