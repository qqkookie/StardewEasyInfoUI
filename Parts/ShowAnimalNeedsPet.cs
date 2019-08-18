﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

using StardewValley;
using StardewValley.Characters;
using StardewValley.Network;
using StardewModdingAPI.Events;

namespace EasyUI
{
    internal class ShowAnimalNeedsPet : IDisposable
    {
        private float _yMovementPerDraw = 0f;
        private float _alpha = 1f;
        private static IModEvents Events => ModEntry.Events;

        internal ShowAnimalNeedsPet()
        { }

        internal void ToggleOption(bool showWhenAnimalNeedsPet)
        {
            Events.Player.Warped -= OnWarped;
            Events.Display.RenderingHud -= OnRenderingHud_DrawAnimalHasProduct;

            if (showWhenAnimalNeedsPet)
            {
                Events.Player.Warped += OnWarped;
                Events.Display.RenderingHud += OnRenderingHud_DrawAnimalHasProduct;
            }
        }

        public void Dispose()
        {
            ToggleOption(false);
        }

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderingHud_DrawAnimalHasProduct(object sender, RenderingHudEventArgs e)
        {
            if (!Game1.eventUp &&
                Game1.activeClickableMenu == null &&
                Game1.currentLocation != null)
            {
                var animalsInCurrentLocation = GetAnimalsInCurrentLocation();
                if (animalsInCurrentLocation != null)
                {
                    foreach (var animal in animalsInCurrentLocation.Pairs)
                    {
                        if (!animal.Value.IsEmoting &&
                            animal.Value.currentProduce.Value != 430 &&
                            animal.Value.currentProduce.Value > 0 &&
                            animal.Value.age.Value >= animal.Value.ageWhenMature.Value)
                        {
                            Vector2 positionAboveAnimal = GetPetPositionAboveAnimal(animal.Value);
                            positionAboveAnimal.Y += (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 300.0 + (double)animal.Value.Name.GetHashCode()) * 5.0);
                            Game1.spriteBatch.Draw(
                                Game1.emoteSpriteSheet,
                                new Vector2(positionAboveAnimal.X + 14f, positionAboveAnimal.Y),
                                new Rectangle(3 * (Game1.tileSize / 4) % Game1.emoteSpriteSheet.Width, 3 * (Game1.tileSize / 4) / Game1.emoteSpriteSheet.Width * (Game1.tileSize / 4), Game1.tileSize / 4, Game1.tileSize / 4),
                                Color.White * 0.9f,
                                0.0f,
                                Vector2.Zero,
                                4f,
                                SpriteEffects.None,
                                1f);

                            Rectangle sourceRectangle = GameLocation.getSourceRectForObject(animal.Value.currentProduce.Value);
                            Game1.spriteBatch.Draw(
                                Game1.objectSpriteSheet,
                                new Vector2(positionAboveAnimal.X + 28f, positionAboveAnimal.Y + 8f),
                                sourceRectangle,
                                Color.White * 0.9f,
                                0.0f,
                                Vector2.Zero,
                                2.2f,
                                SpriteEffects.None,
                                1f);
                        }
                    }
                }
            }
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            if (e.IsLocalPlayer)
            {
                if (e.NewLocation is AnimalHouse || e.NewLocation is Farm)
                {
                    StartDrawingPetNeeds();
                }
                else
                {
                    StopDrawingPetNeeds();
                }
            }
        }

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderingHud_DrawNeedsPetTooltip(object sender, RenderingHudEventArgs e)
        {
            if (!Game1.eventUp && Game1.activeClickableMenu == null)
            {
                DrawIconForFarmAnimals();
                DrawIconForPets();
            }
        }

        private void StartDrawingPetNeeds()
        {
            Events.Display.RenderingHud += OnRenderingHud_DrawNeedsPetTooltip;
            Events.GameLoop.UpdateTicked += UpdateTicked;
        }

        private void StopDrawingPetNeeds()
        {
            Events.Display.RenderingHud -= OnRenderingHud_DrawNeedsPetTooltip;
            Events.GameLoop.UpdateTicked -= UpdateTicked;
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // Bob up and down in a sin wave each draw
            float sine = (float)Math.Sin(e.Ticks / 20.0);
            _yMovementPerDraw = -6f + 6f * sine;
            _alpha = 0.8f + 0.2f * sine;
        }

        private void DrawIconForFarmAnimals()
        {
            var animalsInCurrentLocation = GetAnimalsInCurrentLocation();

            if (animalsInCurrentLocation != null)
            {
                foreach (var animal in animalsInCurrentLocation.Pairs)
                {
                    if (!animal.Value.IsEmoting &&
                        !animal.Value.wasPet.Value &&
                        animal.Value.friendshipTowardFarmer.Value < 1000)
                    {
                        Vector2 positionAboveAnimal = GetPetPositionAboveAnimal(animal.Value);
                        String animalType = animal.Value.type.Value.ToLower();

                        if (animalType.Contains("cow") ||
                            animalType.Contains("sheep") ||
                            animalType.Contains("goat") ||
                            animalType.Contains("pig"))
                        {
                            positionAboveAnimal.X += 50f;
                            positionAboveAnimal.Y += 50f;
                        }
                        Game1.spriteBatch.Draw(
                            Game1.mouseCursors,
                            new Vector2(positionAboveAnimal.X, positionAboveAnimal.Y + _yMovementPerDraw),
                            new Rectangle(32, 0, 16, 16),
                            Color.White * _alpha,
                            0.0f,
                            Vector2.Zero,
                            4f,
                            SpriteEffects.None,
                            1f);
                    }
                }
            }
        }

        private void DrawIconForPets()
        {
            foreach (var character in Game1.currentLocation.characters)
            {
                if (character is Pet &&
                    ! ModEntry.Reflection.GetField<bool>(character, "wasPetToday").GetValue() &&
                    ModEntry.Reflection.GetField<int>(character, "friendshipTowardFarmer", true).GetValue() < 1000)
                {
                    Vector2 positionAboveAnimal = GetPetPositionAboveAnimal(character);
                    positionAboveAnimal.X += 50f;
                    positionAboveAnimal.Y -= 20f;
                    Game1.spriteBatch.Draw(
                        Game1.mouseCursors,
                        new Vector2(positionAboveAnimal.X, positionAboveAnimal.Y + _yMovementPerDraw),
                        new Rectangle(32, 0, 16, 16),
                        Color.White * _alpha,
                        0.0f,
                        Vector2.Zero,
                        4f,
                        SpriteEffects.None,
                        1f);
                }
            }
        }

        private Vector2 GetPetPositionAboveAnimal(Character animal)
        {
            return new Vector2(Game1.viewport.Width <= Game1.currentLocation.map.DisplayWidth ? animal.position.X - Game1.viewport.X + 16 : animal.position.X + ((Game1.viewport.Width - Game1.currentLocation.map.DisplayWidth) / 2 + 18),
                Game1.viewport.Height <= Game1.currentLocation.map.DisplayHeight ? animal.position.Y - Game1.viewport.Y - 34 : animal.position.Y + ((Game1.viewport.Height - Game1.currentLocation.map.DisplayHeight) / 2 - 50));
        }

        private NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> GetAnimalsInCurrentLocation()
        {
            NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = null;

            if (Game1.currentLocation is AnimalHouse)
            {
                animals = (Game1.currentLocation as AnimalHouse).animals;
            }
            else if (Game1.currentLocation is Farm)
            {
                animals = (Game1.currentLocation as Farm).animals;
            }

            return animals;
        }
    }
}
