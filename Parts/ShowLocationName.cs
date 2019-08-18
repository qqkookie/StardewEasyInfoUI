using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyUI
{
    internal class ShowLocationName
    {
        private static ITranslationHelper Translation => ModEntry.Translation;

        private static string DisplayName;
        private static SpriteFont OSDFont;
        private static int OSDTimer = 0;

        internal ShowLocationName()
        {
        }

        internal void ToggleOption(bool showLocationName)
        {
            if (!ModEntry.Config.ShowLocationPopUp)
            {
                ModEntry.Events.Player.Warped -= OnWarped;

                if (showLocationName)
                    ModEntry.Events.Player.Warped += OnWarped;
            }
        }

        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            if (!Context.IsWorldReady || !e.IsLocalPlayer)
                return;

            GameLocation map = Game1.currentLocation;

            string locationName = map.Name;
            DisplayName = Translation.Get("location." + locationName);

            if (Game1.IsMultiplayer && map is Cabin cabin)
            {
                DisplayName = String.Format(DisplayName, cabin.owner.farmName.Value, cabin.owner.Name);
            }
            else if (map is Farm || map is FarmHouse )
            {
                DisplayName = String.Format(DisplayName, Game1.player.farmName.Value, Game1.player.Name);
            }
            else if (map is MineShaft || locationName.StartsWith("UndergroundMine"))
            {
                int lev = Game1.mine.mineLevel;
                if (lev <= 120)
                    DisplayName = String.Format(Translation.Get("location.UndergroundMine"), lev);
                else
                    DisplayName = String.Format(Translation.Get("location.SkullCaveUnderground"), lev - 120);
            }
            else if (locationName == "Temp" && Game1.isFestival())
            {
                string season = Game1.currentSeason;
                int day = Game1.dayOfMonth;

                if ((season == "spring" && (day == 13 || day == 24))
                    || (season == "summer" && (day == 11 || day == 28))
                    || (season == "fall" && (day == 16 || day == 27))
                    || (season == "winter" && (day == 8 || day == 25)))
                {
                    DisplayName = Game1.content.LoadString($"Data\\Festvals\\FestivalDates:{season}{day}");
                }
                else
                    ModEntry.Logger.Log($"Unknown festival day! {season} {day}", LogLevel.Warn);
            }

            if (String.IsNullOrEmpty(DisplayName) || DisplayName.Contains("no translation"))
            {
                DisplayName = locationName;
                ModEntry.Logger.Log($"No translation: {DisplayName} for {Translation.Locale}", LogLevel.Info);
            }
            else if (!ModEntry.Config.ShowLocationPopUp)
            {
                OSDFont = Game1.smallFont;
                OSDFont.Spacing = -2f;
                OSDTimer = 60*2;
                if (locationName == "Temp" || locationName == "BeachNightMarket")
                    Game1.addHUDMessage(new HUDMessage(DisplayName, 1));
            }
            else
                Game1.showGlobalMessage(DisplayName);
        }

        private static void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            if (ModEntry.Config.ShowLocationPopUp || OSDTimer <= 0)
                return;

            float scale = 2.0f;
            SpriteBatch Sb = e.SpriteBatch;
            Vector2 txtSize = OSDFont.MeasureString(DisplayName)* scale;
            Viewport vp = Game1.graphics.GraphicsDevice.Viewport;
            Vector2 pos = new Vector2((vp.Width - txtSize.X)/2, (vp.Height - txtSize.Y)/10);
            float alpha = (OSDTimer > 30) ? 1.0f : OSDTimer / 30f;

            Sb.DrawString(OSDFont, DisplayName, pos, Color.DarkSlateGray * alpha,
                0, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
            Sb.DrawString(OSDFont, DisplayName, pos + new Vector2(-4, -4), Color.LightYellow * alpha,
                0, Vector2.Zero, scale, SpriteEffects.None, 0.0f);

            OSDTimer--;
        }
    }
}
