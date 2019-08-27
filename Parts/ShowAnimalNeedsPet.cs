using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

using StardewValley;
using StardewValley.Characters;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyInfoUI
{
    internal class ShowAnimalNeedsPet : IDisposable
    {
        private float _yMovementPerDraw = 0f;
        private float _alpha = 1f;

        internal ShowAnimalNeedsPet()
        { }

        internal void ToggleOption(bool showWhenAnimalNeedsPet)
        {
            ModEntry.Events.Display.RenderedWorld -= OnRenderedWorld;

            if (showWhenAnimalNeedsPet)
            {
                ModEntry.Events.Display.RenderedWorld += OnRenderedWorld;
            }
        }

        public void Dispose()
        {
            ToggleOption(false);
        }

        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            GameLocation map = Game1.currentLocation;

            if (map == null || (!(map is Farm) && !(map is AnimalHouse)) || !Game1.player.IsLocalPlayer
                || !Context.IsPlayerFree || Game1.eventUp || Game1.activeClickableMenu != null )
                return;

            // Bob up and down in a sin wave each draw
            float sine = (float)Math.Sin(Game1.ticks / 20.0);
            _yMovementPerDraw = -6f + 6f * sine;
            _alpha = 0.8f + 0.2f * sine;

            if (map is Farm)
                DrawIconForPets();

            DrawForFarmAnimals();
        }

        private void DrawIconForPets()
        {
            foreach (var character in Game1.currentLocation.characters)
            {
                if (character is Pet &&
                    !ModEntry.Reflection.GetField<bool>(character, "wasPetToday").GetValue() &&
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

        private void DrawForFarmAnimals()
        {
            StardewValley.Network.NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animalsInCurrentLocation;

            if ((Game1.currentLocation is AnimalHouse house))
                animalsInCurrentLocation = house.animals;
            else if (Game1.currentLocation is Farm farm)
                animalsInCurrentLocation = farm.animals;
            else
                return;

            if (animalsInCurrentLocation == null)
                return;

            foreach (var akv in animalsInCurrentLocation.Pairs)
            {
                FarmAnimal animal = akv.Value;

                if (animal.IsEmoting)
                    continue;

                Vector2 above = GetPetPositionAboveAnimal(animal);

                if (!animal.wasPet.Value && animal.friendshipTowardFarmer.Value < 1000)
                {
                    // Draw Need pet icon
                    float offset = 0;
                    String animalType = animal.type.Value.ToLower();

                    if (animalType.Contains("cow") || animalType.Contains("sheep") ||
                        animalType.Contains("goat") || animalType.Contains("pig"))
                    {
                        offset = 50f;
                    }

                    Game1.spriteBatch.Draw(
                        Game1.mouseCursors,
                        new Vector2(above.X + offset, above.Y + offset + _yMovementPerDraw),
                        new Rectangle(32, 0, 16, 16),
                        Color.White * _alpha,
                        0.0f,
                        Vector2.Zero,
                        4f,
                        SpriteEffects.None,
                        1f);
                }

                if (animal.currentProduce.Value != 430 && animal.currentProduce.Value > 0
                    && animal.age.Value >= animal.ageWhenMature.Value)
                {
                    // Show Animal has product
                    double span = Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
                    above.Y += (float) Math.Sin( span / 300.0f + animal.Name.GetHashCode()) * 5.0f;

                    Rectangle srcRect = new Rectangle(3 * (Game1.tileSize / 4) % Game1.emoteSpriteSheet.Width,
                        3 * (Game1.tileSize / 4) / Game1.emoteSpriteSheet.Width * (Game1.tileSize / 4),
                        Game1.tileSize / 4, Game1.tileSize / 4);

                    Game1.spriteBatch.Draw(
                        Game1.emoteSpriteSheet,
                        new Vector2(above.X + 14f, above.Y),
                        srcRect,
                        Color.White * 0.9f,
                        0.0f,
                        Vector2.Zero,
                        4f,
                        SpriteEffects.None,
                        1f);

                    srcRect = GameLocation.getSourceRectForObject(animal.currentProduce.Value);

                    Game1.spriteBatch.Draw(
                        Game1.objectSpriteSheet,
                        new Vector2(above.X + 28f, above.Y + 8f),
                        srcRect,
                        Color.White * 0.9f,
                        0.0f,
                        Vector2.Zero,
                        2.2f,
                        SpriteEffects.None,
                        1f);
                }
            }
        }

        private Vector2 GetPetPositionAboveAnimal(Character animal)
        {
            var vp = Game1.viewport;
            var map = Game1.currentLocation.map;

            float chx = vp.Width <= map.DisplayWidth ? animal.position.X - vp.X + 16 :
                animal.position.X + ((vp.Width - map.DisplayWidth) / 2 + 18);
            float chy = vp.Height <= map.DisplayHeight ? animal.position.Y - vp.Y - 34 :
                animal.position.Y + ((vp.Height - map.DisplayHeight) / 2 - 50);

            return new Vector2(chx, chy);
        }
    }
}
