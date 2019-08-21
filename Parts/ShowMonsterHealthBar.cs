using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Monsters;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyInfoUI
{
    internal class ShowMonsterHealthBar
    {
        /// <summary>HP bar border texture</summary>
        private static Texture2D BarBorder;

        /// <summary>HP bar color scheme</summary>
        private static Color[] ColorScheme = 
            { Color.LawnGreen, Color.YellowGreen, Color.Gold, Color.DarkOrange, Color.Crimson };

        internal ShowMonsterHealthBar()
        {

            if (ModEntry.Config.ReverseColorScheme)
                Array.Reverse(ColorScheme);

            ModEntry.Events.Display.RenderedWorld += OnRenderedWorld;

            BarBorder = ModEntry.ModHelper.Content.Load<Texture2D>("Assets/HealthBar.png", ContentSource.ModFolder);
        }

        internal void ToggleOption(bool showhealthbar)
        {
            ModEntry.Events.Display.RenderedWorld -= OnRenderedWorld;

            if (showhealthbar)
                ModEntry.Events.Display.RenderedWorld += OnRenderedWorld;
        }

        private static bool CheckMonster(Monster monster, out Vector2 offset)
        {
            float HeightAdj(Monster mon, float ratio)
            {
                return mon.getLocalPosition(Game1.viewport).Y
                    - mon.Sprite.SpriteHeight * Game1.pixelZoom * ratio;
            }

            offset = Vector2.Zero;

            if (monster is GreenSlime slime)
            {
                if (slime.hasSpecialItem.Value)
                    offset.X = -5;
                else if (slime.cute.Value)
                    offset.X = -2;
                else
                    offset.Y = 5 * Game1.pixelZoom;
            }
            else if (monster is RockCrab || monster is LavaCrab)
            {
                if (monster.Sprite.CurrentFrame % 4 == 0)
                    return false;
            }
            else if (monster is RockGolem)
            {
                if (monster.Health >= monster.MaxHealth)
                    return false;
                offset.Y = HeightAdj(monster, 3.0f / 4.0f);
            }
            else if (monster is Bug bug)
            {
                if (bug.isArmoredBug.Value)
                    return false;
                offset.Y = -15 * Game1.pixelZoom;
            }
            else if (monster is Grub)
            {
                if (monster.Sprite.CurrentFrame == 19)
                    return false;
                offset.Y = HeightAdj(monster, 4.0f / 7.0f);
            }
            else if (monster is Fly)
            {
                offset.Y = HeightAdj(monster, 5.0f / 7.0f);
            }
            else if (monster is DustSpirit)
            {
                offset = new Vector2( 3, 5 * Game1.pixelZoom);
            }
            else if (monster is Bat)
            {
                if (monster.Sprite.CurrentFrame == 4)
                    return false;
                offset = new Vector2(-1, Game1.pixelZoom);
            }
            else if (monster is MetalHead || monster is Mummy)
            {
                offset.Y = -2 * Game1.pixelZoom;
            }
            else if (monster is Skeleton || monster is ShadowBrute || monster is ShadowShaman || monster is SquidKid)
            {
                if (monster.Health == monster.MaxHealth)
                    return false;
                offset.Y =  -7 * Game1.pixelZoom;
            }

            return true;
        }

        private static int KillScoreClass(Monster monster)
        {
            const int LEARNED_KILL_COUNT = 13;
            const int VETRAN_KILL_COUNT = 26;

            if (!ModEntry.Config.RequireKillScore)
                return 3;

            // get number of times that player killed the monster already.
            Game1.stats.specificMonstersKilled.TryGetValue(monster.Name, out int killCount);

            if (killCount + 2 * Game1.player.CombatLevel > VETRAN_KILL_COUNT)
                return 2;
            if (killCount + Game1.player.CombatLevel > LEARNED_KILL_COUNT)
                return 1;
            else
                return 0;
        }

        private static Color HealthColor(float hpRatio)
        {
            if (hpRatio > 0.8f) return ColorScheme[0];
            else if (hpRatio > 0.55f) return ColorScheme[1];
            else if (hpRatio > 0.35f) return ColorScheme[2];
            else if (hpRatio > 0.15f) return ColorScheme[3];
            else return ColorScheme[4];
        }
   
        private static void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (Game1.eventUp || !Game1.showingHealth || Game1.currentLocation == null)
                return;

            const float TXT_OFFSET = -4f;
            const float TXT_SCALE = 0.66f;

            const int SPRITE_HEIGHT = 28;
            const int SPRITE_FRONT = 0;
            const int SPRITE_BACK = 1;
            // const int SPRITE_BACK2 = 2;
            // const int SPRITE_INACTIVE = 3;

            const int BAR_MARGIN = 4;

            /// <summary>HP bar texture</summary>
            Texture2D WhitePixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            WhitePixel.SetData(new Color[] { Color.White });

            //  Display monster health bar
            SpriteBatch Sb = e.SpriteBatch;

            // Iterate through all NPC
            foreach (NPC character in Game1.currentLocation.characters)
            {
                // Visible monsters only
                if (!(character is Monster monster) || monster.IsInvisible || 
                    ! Utility.isOnScreen(monster.position, 3 * Game1.tileSize))
                    continue;

                int KillClass = KillScoreClass(monster);
                if (KillClass == 0)
                    continue;

                // Check if the current monster should not display hp bar
                if (!CheckMonster(monster, out Vector2 offset))
                    continue;

                Vector2 monpos = monster.getLocalPosition(Game1.viewport) + offset;
                Vector2 BarCenter = new Vector2(monpos.X + (float)monster.Sprite.SpriteWidth * Game1.pixelZoom / 2,
                (float)monpos.Y - ((float)monster.Sprite.SpriteHeight + 5) * Game1.pixelZoom / 2);

                //  Display background of the bar
                Rectangle bgBox = new Rectangle(0, SPRITE_HEIGHT * SPRITE_BACK, BarBorder.Width, SPRITE_HEIGHT);

                Sb.Draw( BarBorder, BarCenter, bgBox, Color.White * 1f, 0f, 
                    new Vector2(BarBorder.Width / 2, SPRITE_HEIGHT / 2), 1f, SpriteEffects.None, 0f);

                String healthText = "???";
                SpriteFont txtFont = Game1.smallFont;
                Color txtColor = Color.DarkSlateGray;
                float BarFill = 1f;      // By default, color bar is full

                // Get HP of the monster
                int monhp = monster.Health;
                float hpRatio = (float)monhp / (float)Math.Max(monster.MaxHealth, monhp);
                Color barColor = HealthColor(hpRatio);

                //  for normal hp bar, display the stats
                if (KillClass >= 2)
                {
                    BarFill = hpRatio;
                    if (!ModEntry.Config.RequireKillScore || monhp < 999)
                        healthText = String.Format("{0:000}", monhp);
                    // If it's a very strong monster, we hide the life counter
                    else
                    {
                        healthText = "!!!";
                        txtColor = Color.Red;
                    }
                }

                //Calculate size of the hp box
                Rectangle hpBox = new Rectangle(0, 0, (int)((BarBorder.Width - BAR_MARGIN * 2) * BarFill),
                    SPRITE_HEIGHT - BAR_MARGIN * 2);
                Vector2 hpPos = new Vector2(BarCenter.X - BarBorder.Width / 2 + BAR_MARGIN, BarCenter.Y);

                //Display HP bar
                Sb.Draw(WhitePixel, hpPos, hpBox, barColor, 0f,
                    new Vector2(0, hpBox.Height / 2f), 1f, SpriteEffects.None, 0f);

                // Display HP number with text
                Vector2 textsize = txtFont.MeasureString(healthText);
                Vector2 textPos = new Vector2(textsize.X / 2, textsize.Y / 2 + TXT_OFFSET);

                Sb.DrawString( txtFont, healthText, BarCenter, txtColor, 0f,
                    textPos, TXT_SCALE, SpriteEffects.None, 0f);
  
                //Display foreground of the bar
                Rectangle fgBox = new Rectangle(0, SPRITE_HEIGHT * SPRITE_FRONT, BarBorder.Width, SPRITE_HEIGHT);
                Vector2 fgPos = new Vector2(BarBorder.Width / 2f, SPRITE_HEIGHT / 2f);

                Sb.Draw(BarBorder, BarCenter, fgBox, Color.White * 1.0f, 0f,
                    fgPos, 1f, SpriteEffects.None, 0f); ;

            }
        }
    }
}
