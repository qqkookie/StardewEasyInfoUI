using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyInfoUI
{
    internal class IconLuckOfDay : IDisposable
    {
        private String _hoverText = string.Empty;
        private Color _color = new Color(Color.White.ToVector4());
        private ClickableTextureComponent _icon;
        private static IModEvents Events => ModEntry.Events;
        private static ITranslationHelper Translation => ModEntry.Translation;

        private string[] WeatherDisplay = {
            "weather.wedding",  "weather.festival", "weather.sunny", "weather.spring-breeze",
            "weather.rainy", "weather.lightning", "weather.fall-wind", "weather.snow" };

        internal void Toggle(bool showLuckOfDay)
        {
            Events.GameLoop.DayStarted -= LoadedOrNewDay;
            Events.GameLoop.SaveLoaded -= LoadedOrNewDay;
            Events.Player.Warped -= OnWarped;
            Events.Display.RenderingHud -= OnRenderingHud;
            Events.Display.RenderedHud -= OnRenderedHud;

            if (showLuckOfDay)
            {
                AdjustIconXToBlackBorder();
                Events.GameLoop.DayStarted += LoadedOrNewDay;
                Events.GameLoop.SaveLoaded += LoadedOrNewDay;
                Events.Player.Warped += OnWarped;
                Events.Display.RenderingHud += OnRenderingHud;
                Events.Display.RenderedHud += OnRenderedHud;
            }
        }

        internal IconLuckOfDay()
        {
            for (int ii = 0; ii < WeatherDisplay.Length; ii++)
                WeatherDisplay[ii] = Translation.Get(WeatherDisplay[ii]);
        }

        public void Dispose()
        {
            Toggle(false);
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private async void LoadedOrNewDay(object sender, EventArgs e)
        {
            // calculate luck
            _color = new Color(Color.White.ToVector4());

            if (Game1.dailyLuck < -0.07)
            {
                _hoverText = Translation.Get(LanguageKeys.MaybeStayHome);
                _color.B = 155;
                _color.G = 155;
            }
            else if (Game1.dailyLuck < 0)
            {
                _hoverText = Translation.Get(LanguageKeys.NotFeelingLuckyAtAll);
                _color.B = 165;
                _color.G = 165;
                _color.R = 165;
                _color *= 0.8f;
            }
            else if (Game1.dailyLuck < 0.07)
            {
                _hoverText = Translation.Get(LanguageKeys.LuckyButNotTooLucky);
            }
            else
            {
                _hoverText = Translation.Get(LanguageKeys.FeelingLucky);
                _color.B = 155;
                _color.R = 155;
            }

            if (!ModEntry.Config.ShowTodayMessage)
                return;

            await Task.Delay(2000);
            // string forecast = _helper.Reflection.GetMethod(tv, "getWeatherForecast").Invoke<string>();
            Game1.addHUDMessage(new HUDMessage(GetWeatherToday(), HUDMessage.newQuest_type));

            await Task.Delay(2000);
            string luck = ModEntry.Reflection.GetMethod(new StardewValley.Objects.TV(), "getFortuneForecast").Invoke<string>();
            Game1.addHUDMessage(new HUDMessage(luck, HUDMessage.newQuest_type));
        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the sprite batch, but before it's rendered to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            // draw hover text
            if (_icon.containsPoint(Game1.getMouseX(), Game1.getMouseY()))
                IClickableMenu.drawHoverText(Game1.spriteBatch, _hoverText, Game1.dialogueFont);

            Vector2 pos = Game1.dayTimeMoneyBox.position + new Vector2(116f, 68f);
            Rectangle weatherIconbox = new Rectangle((int)pos.X, (int)pos.Y, 12 * Game1.pixelZoom, 8 * Game1.pixelZoom);
            pos = Game1.dayTimeMoneyBox.position + new Vector2(212f, 68f);
            Rectangle seasonIconbox = new Rectangle((int)pos.X, (int)pos.Y, 12 * Game1.pixelZoom, 8 * Game1.pixelZoom);

            if (weatherIconbox.Contains(Game1.getMouseX(), Game1.getMouseY()))
            {
                string weather = "  " + WeatherDisplay[Game1.weatherIcon] + "   ";
                IClickableMenu.drawHoverText(e.SpriteBatch, weather, Game1.smallFont);
            }
            else if (seasonIconbox.Contains(Game1.getMouseX(), Game1.getMouseY()))
            {
                string season = "   " + Utility.getSeasonNameFromNumber(Utility.getSeasonNumber(Game1.currentSeason)) + "    ";
                IClickableMenu.drawHoverText(e.SpriteBatch, season, Game1.smallFont);
            }
        }

        /// <summary>Raised before drawing the HUD (item toolbar, clock, etc) to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            // draw dice icon
            if (!Game1.eventUp)
            {
                Point iconPosition = IconHandler.Handler.GetNewIconPosition();
                _icon.bounds.X = iconPosition.X;
                _icon.bounds.Y = iconPosition.Y;
                _icon.draw(Game1.spriteBatch, _color, 1f);
            }
        }

        /// <summary>Raised after a player warps to a new location.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            // adjust icon X to black border
            if (e.IsLocalPlayer)
            {
                AdjustIconXToBlackBorder();
            }
        }

        private void AdjustIconXToBlackBorder()
        {
            _icon = new ClickableTextureComponent("",
                new Rectangle(Utils.GetWidthInPlayArea() - 134,
                    290,
                    10 * Game1.pixelZoom,
                    10 * Game1.pixelZoom),
                "",
                "",
                Game1.mouseCursors,
                new Rectangle(50, 428, 10, 14),
                Game1.pixelZoom,
                false);
        }

        private string GetWeatherToday()
        {
            string weather = "";

            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
            {
                // festival
                Dictionary<string, string> dictionary;
                try
                {
                    dictionary = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth);

                    string festval = dictionary["name"];
                    string loc = dictionary["conditions"].Split('/')[0];
                    int timebegin = Convert.ToInt32(dictionary["conditions"].Split('/')[1].Split(' ')[0]);
                    int timeend = Convert.ToInt32(dictionary["conditions"].Split('/')[1].Split(' ')[1]);
                    string locname = "";

                    if (loc == "Town")
                        locname = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13170");

                    else if (!(loc == "Beach"))
                        locname = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13172");
                    else if (loc == "Forest")
                        locname = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13174");

                    weather = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13175",
                        festval, locname, Game1.getTimeOfDayString(timebegin), Game1.getTimeOfDayString(timeend));
                }
                catch (Exception ex)
                {
                    weather = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13164");
                }
            }
            else if (Game1.isSnowing)
            {
                if (Game1.random.NextDouble() >= 0.5)
                    weather = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13181");
                else
                    weather = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13180");
            }
            else if (Game1.isLightning)
            {
                weather = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13185");
            }
            else if (Game1.isRaining)
            {
                weather = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13184");
            }
            else if (Game1.isDebrisWeather)
            {
                if (Game1.currentSeason.Equals("spring"))
                    weather = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13187");
                else if (Game1.currentSeason.Equals("winter"))
                    weather = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13190");
                else
                    weather = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13189");
            }
            else
            {
                if (Game1.random.NextDouble() >= 0.5)
                    weather = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13183");
                else
                    weather = Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13182");
            }

            string tomorrow = Translation.Get("weather.tomorrow");
            string today = Translation.Get("weather.today");
            if (!string.IsNullOrEmpty(tomorrow) && !string.IsNullOrEmpty(today) && weather.Contains(tomorrow))
                return weather.Replace(tomorrow, today);
            return weather;
        }
    }
}
