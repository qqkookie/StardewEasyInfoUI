using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewModdingAPI.Events;

namespace EasyInfoUI
{
    internal class IconNewRecipe : IDisposable
    {
        private readonly Dictionary<String, String> _recipesByDescription = new Dictionary<string, string>();
        private Dictionary<String, String> _recipes = new Dictionary<String, string>();
        private String _todaysRecipe;
        // private NPC _gus;
        private bool _drawQueenOfSauceIcon = false;
        // private bool _drawDishOfDayIcon = false;
        private ClickableTextureComponent _queenOfSauceIcon;
        private static IModEvents Events => ModEntry.Events;
        private string _todaysRecipeDisplay;

        internal IconNewRecipe()
        {
        }

        internal void ToggleOption(bool showQueenOfSauceIcon)
        {
            Events.Display.RenderingHud -= OnRenderingHud;
            Events.Display.RenderedHud -= OnRenderedHud;
            Events.GameLoop.DayStarted -= OnDayStarted;
            Events.GameLoop.UpdateTicked -= OnUpdateTicked;

            if (showQueenOfSauceIcon)
            {
                LoadRecipes();
                CheckForNewRecipe();
                Events.GameLoop.DayStarted += OnDayStarted;
                Events.Display.RenderingHud += OnRenderingHud;
                Events.Display.RenderedHud += OnRenderedHud;
                Events.GameLoop.UpdateTicked += OnUpdateTicked;
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
            // check if learned recipe
            if (e.IsOneSecond && _drawQueenOfSauceIcon && Game1.player.knowsRecipe(_todaysRecipe))
                _drawQueenOfSauceIcon = false;
        }

        private void LoadRecipes()
        {
            if (_recipes.Count == 0)
            {
                _recipes = Game1.content.Load<Dictionary<String, String>>("Data\\TV\\CookingChannel");

                foreach (var next in _recipes)
                {
                    string[] values = next.Value.Split('/');

                    if (values.Length > 1)
                    {
                        _recipesByDescription[values[1]] = values[0];
                    }
                }
            }
        }

        /*
        private void FindGus()
        {
            foreach (var location in Game1.locations)
            {
                foreach (var npc in location.characters)
                {
                    if (npc.Name == "Gus")
                    {
                        _gus = npc;
                        break;
                    }
                }
                if (_gus != null)
                    break;
            }
        }

        private string[] GetTodaysRecipe()
        {
            String[] array1 = new string[2];
            int recipeNum = (int)(Game1.stats.DaysPlayed % 224 / 7);
            //var recipes = Game1.content.Load<Dictionary<String, String>>("Data\\TV\\CookingChannel");

            String recipeValue = _recipes.SafeGet(recipeNum.ToString());
            String[] splitValues = null;
            String key = null;
            bool checkCraftingRecipes = true;

            if (String.IsNullOrEmpty(recipeValue))
            {
                recipeValue = _recipes["1"];
                checkCraftingRecipes = false;
            }
            splitValues = recipeValue.Split('/');
            key = splitValues[0];

            ///Game code sets this to splitValues[1] to display the language specific
            ///recipe name. We are skipping a bunch of their steps to just get the
            ///english name needed to tell if the player knows the recipe or not
            array1[0] = key;
            if (checkCraftingRecipes)
            {
                String craftingRecipesValue = CraftingRecipe.cookingRecipes.SafeGet(key);
                if (!String.IsNullOrEmpty(craftingRecipesValue))
                    splitValues = craftingRecipesValue.Split('/');
            }

            string languageRecipeName = (ModEntry.ModHelper.Content.CurrentLocaleConstant == LocalizedContentManager.LanguageCode.en) ?
                key : splitValues[splitValues.Length - 1];

            array1[1] = languageRecipeName;

            //String str = null;
            //if (!Game1.player.cookingRecipes.ContainsKey(key))
            //{
            //    str = Game1.content.LoadString(@"Strings\StringsFromCSFiles:TV.cs.13153", languageRecipeName);
            //}
            //else
            //{
            //    str = Game1.content.LoadString(@"Strings\StringsFromCSFiles:TV.cs.13151", languageRecipeName);
            //}
            //array1[1] = str;

            return array1;
        }
        */

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            // draw icon
            if (!Game1.eventUp)
            {
                if (_drawQueenOfSauceIcon)
                {
                    Point iconPosition = IconHandler.Handler.GetNewIconPosition();

                    _queenOfSauceIcon = new ClickableTextureComponent(
                        new Rectangle(iconPosition.X, iconPosition.Y, 40, 40),
                        Game1.mouseCursors,
                        new Rectangle(609, 361, 28, 28),
                        1.3f);
                    _queenOfSauceIcon.draw(Game1.spriteBatch);
                }

                /*
                if (_drawDishOfDayIcon)
                {
                    Point iconLocation = IconHandler.Handler.GetNewIconPosition();
                    float scale = 2.9f;

                    Game1.spriteBatch.Draw(
                        Game1.objectSpriteSheet,
                        new Vector2(iconLocation.X, iconLocation.Y),
                        new Rectangle(306, 291, 14, 14),
                        Color.White,
                        0,
                        Vector2.Zero,
                        scale,
                        SpriteEffects.None,
                        1f);

                    ClickableTextureComponent texture =
                        new ClickableTextureComponent(
                            _gus.Name,
                            new Rectangle(
                                iconLocation.X - 7,
                                iconLocation.Y - 2,
                                (int)(16.0 * scale),
                                (int)(16.0 * scale)),
                            null,
                            _gus.Name,
                            _gus.Sprite.Texture,
                            _gus.GetHeadShot(),
                            2f);

                    texture.draw(Game1.spriteBatch);

                    if (texture.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        IClickableMenu.drawHoverText(
                            Game1.spriteBatch,
                            String.Format(ModEntry.Translation.Get(LanguageKeys.GusIsSellingRecipe), Game1.dishOfTheDay.DisplayName),
                            Game1.dialogueFont);
                    }
                }
                */
            }
        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the sprite batch, but before it's rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            // draw hover text
            if (_drawQueenOfSauceIcon &&
                _queenOfSauceIcon.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
            {
                IClickableMenu.drawHoverText(
                    Game1.spriteBatch,
                    ModEntry.Translation.Get(
                        LanguageKeys.TodaysRecipe) + _todaysRecipeDisplay,
                    Game1.dialogueFont);
            }
        }


        /// <summary>Raised after the game begins a new day (including when the player loads a save).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            this.CheckForNewRecipe();
        }

        private void CheckForNewRecipe()
        {
            TV tv = new TV();
            int numRecipesKnown = Game1.player.cookingRecipes.Count();
            String[] recipes = typeof(TV).GetMethod("getWeeklyRecipe", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(tv, null) as String[];
            //String[] recipe = GetTodaysRecipe();
            //_todaysRecipe = recipe[1];

            _todaysRecipe = String.Empty;
            if (_recipesByDescription.TryGetValue(recipes[0], out string value))
                _todaysRecipe = value;

            if (Game1.player.cookingRecipes.Count() > numRecipesKnown)
                Game1.player.cookingRecipes.Remove(_todaysRecipe);

            _drawQueenOfSauceIcon = (Game1.dayOfMonth % 7 == 0 || (Game1.dayOfMonth - 3) % 7 == 0) &&
                Game1.stats.DaysPlayed > 5 &&
                !Game1.player.knowsRecipe(_todaysRecipe);
            //_drawDishOfDayIcon = !Game1.player.knowsRecipe(Game1.dishOfTheDay.Name);

            _todaysRecipeDisplay = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en)
                ? _todaysRecipe : $"{recipes[0].Split('!')[0]} ({_todaysRecipe})"; // localized recipe name
        }
    }
}
