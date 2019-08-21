using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;
using StardewModdingAPI.Events;

namespace EasyInfoUI
{
    internal class ShowCropAndBarrelTime : IDisposable
    {
        private readonly Dictionary<int, String> _indexOfCropNames = new Dictionary<int, string>();
        private StardewValley.Object _currentTile;
        private TerrainFeature _terrain;
        private Building _currentTileBuilding = null;

        internal ShowCropAndBarrelTime()
        {
        }

        internal void ToggleOption(bool showCropAndBarrelTimes)
        {
            ModEntry.Events.Display.RenderingHud -= OnRenderingHud;
            ModEntry.Events.GameLoop.UpdateTicked -= OnUpdateTicked;

            if (showCropAndBarrelTimes)
            {
                ModEntry.Events.Display.RenderingHud += OnRenderingHud;
                ModEntry.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            }
        }

        public void Dispose()
        {
            ToggleOption(false);
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!e.IsMultipleOf(4))
                return;

            // get tile under cursor
            _currentTileBuilding = Game1.currentLocation is BuildableGameLocation buildableLocation
                ? buildableLocation.getBuildingAt(Game1.currentCursorTile)
                : null;
            if (Game1.currentLocation != null)
            {
                if (Game1.currentLocation.Objects == null ||
                    !Game1.currentLocation.Objects.TryGetValue(Game1.currentCursorTile, out _currentTile))
                {
                    _currentTile = null;
                }

                if (Game1.currentLocation.terrainFeatures == null ||
                    !Game1.currentLocation.terrainFeatures.TryGetValue(Game1.currentCursorTile, out _terrain))
                {
                    if (_currentTile is IndoorPot pot &&
                        pot.hoeDirt.Value != null)
                    {
                        _terrain = pot.hoeDirt.Value;
                    }
                    else
                    {
                        _terrain = null;
                    }
                }
            }
            else
            {
                _currentTile = null;
                _terrain = null;
            }
        }

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            string Trans(string key) => ModEntry.Translation.Get(key);

            // draw hover tooltip
            if (_currentTileBuilding != null)
            {
                if (_currentTileBuilding is Mill millBuilding)
                {
                    if (millBuilding.input.Value != null)
                    {
                        if (!millBuilding.input.Value.isEmpty())
                        {
                            int wheatCount = 0;
                            int beetCount = 0;

                            foreach (var item in millBuilding.input.Value.items)
                            {
                                if (item != null &&
                                    !String.IsNullOrEmpty(item.Name))
                                {
                                    switch (item.Name)
                                    {
                                        case "Wheat": wheatCount = item.Stack; break;
                                        case "Beet": beetCount = item.Stack; break;
                                    }
                                }
                            }

                            StringBuilder builder = new StringBuilder();

                            if (wheatCount > 0)
                                builder.Append(wheatCount + " wheat");

                            if (beetCount > 0)
                            {
                                if (wheatCount > 0)
                                    builder.Append(Environment.NewLine);
                                builder.Append(beetCount + " beets");
                            }

                            if (builder.Length > 0)
                            {
                                IClickableMenu.drawHoverText(
                                   Game1.spriteBatch,
                                   builder.ToString(),
                                   Game1.smallFont);
                            }
                        }
                    }
                }
            }
            else if (_currentTile != null &&
                (!_currentTile.bigCraftable.Value ||
                _currentTile.MinutesUntilReady > 0))
            {
                if (_currentTile.bigCraftable.Value &&
                    _currentTile.MinutesUntilReady > 0 &&
                    _currentTile.heldObject.Value != null &&
                    _currentTile.Name != "Heater")
                {
                    StringBuilder hoverText = new StringBuilder();
                    hoverText.AppendLine(_currentTile.heldObject.Value.DisplayName);
                    
                    if (_currentTile is Cask)
                    {
                        Cask currentCask = _currentTile as Cask;
                        // hoverText.Append((int)(currentCask.daysToMature.Value / currentCask.agingRate.Value))
                        //     .Append(" " + _helper.SafeGetString( LanguageKeys.DaysToMature));
                        hoverText.Append( String.Format(Trans(LanguageKeys.DaysToMature), 
                            (int)(currentCask.daysToMature.Value / currentCask.agingRate.Value)));
                    }
                    else
                    {
                        int hours = _currentTile.MinutesUntilReady / 60;
                        int days = hours / 24;
                        int minutes = _currentTile.MinutesUntilReady % 60;
                        if (days > 0)
                            hoverText.Append(days).Append(" ")
                                .Append(Trans(LanguageKeys.Days))
                                .Append(", ");
                        if (hours > 0)
                            hoverText.Append(hours).Append(" ")
                                .Append(Trans(
                                    LanguageKeys.Hours))
                                .Append(", ");
                        hoverText.Append(minutes).Append(" ")
                            .Append(Trans(
                                LanguageKeys.Minutes));
                    }
                    IClickableMenu.drawHoverText(
                        Game1.spriteBatch,
                        hoverText.ToString(),
                        Game1.smallFont);
                }
            }
            else if (_terrain != null)
            {
                if (_terrain is HoeDirt)
                {
                    HoeDirt hoeDirt = _terrain as HoeDirt;
                    if (hoeDirt.crop != null &&
                        !hoeDirt.crop.dead.Value)
                    {
                        int num = 0;

                        if (hoeDirt.crop.fullyGrown.Value &&
                            hoeDirt.crop.dayOfCurrentPhase.Value > 0)
                        {
                            num = hoeDirt.crop.dayOfCurrentPhase.Value;
                        }
                        else
                        {
                            for (int i = 0; i < hoeDirt.crop.phaseDays.Count - 1; ++i)
                            {
                                if (hoeDirt.crop.currentPhase.Value == i)
                                    num -= hoeDirt.crop.dayOfCurrentPhase.Value;

                                if (hoeDirt.crop.currentPhase.Value <= i)
                                    num += hoeDirt.crop.phaseDays[i];
                            }
                        }

                        if (hoeDirt.crop.indexOfHarvest.Value > 0)
                        {
                            String hoverText = _indexOfCropNames.SafeGet(hoeDirt.crop.indexOfHarvest.Value);
                            if (String.IsNullOrEmpty(hoverText))
                            {
                                hoverText = new StardewValley.Object(new Debris(hoeDirt.crop.indexOfHarvest.Value, Vector2.Zero, Vector2.Zero).chunkType.Value, 1).DisplayName;
                                _indexOfCropNames.Add(hoeDirt.crop.indexOfHarvest.Value, hoverText);
                            }

                            StringBuilder finalHoverText = new StringBuilder();
                            finalHoverText.Append(hoverText).Append(": ");
                            if (num > 0)
                            {
                                finalHoverText.Append(num).Append(" ")
                                    .Append(Trans(
                                        LanguageKeys.Days));
                            }
                            else
                            {
                                finalHoverText.Append(Trans(
                                    LanguageKeys.ReadyToHarvest));
                            }
                            IClickableMenu.drawHoverText(
                                Game1.spriteBatch,
                                finalHoverText.ToString(),
                                Game1.smallFont);
                        }
                    }
                }
                else if (_terrain is FruitTree)
                {
                    FruitTree tree = _terrain as FruitTree;
                    var text = new StardewValley.Object(new Debris(tree.indexOfFruit.Value, Vector2.Zero, Vector2.Zero).chunkType.Value, 1).DisplayName;
                    if (tree.daysUntilMature.Value > 0)
                    {
                        // text += Environment.NewLine + tree.daysUntilMature.Value + " " + _helper.SafeGetString(LanguageKeys.DaysToMature);
                        text += Environment.NewLine + String.Format(
                            Trans(LanguageKeys.DaysToMature), tree.daysUntilMature.Value);
                    }
                    IClickableMenu.drawHoverText(
                            Game1.spriteBatch,
                            text,
                            Game1.smallFont);
                }
            }
        }
    }
}
