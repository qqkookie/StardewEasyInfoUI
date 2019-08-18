using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewModdingAPI.Events;

namespace EasyUI
{
    internal class ShowStaminaHealthNumber
    {
        private float TxtWidth;
        private bool OldDanger = false;
        private int Alarm = 0;

        internal ShowStaminaHealthNumber()
        {
            TxtWidth = Game1.dialogueFont.MeasureString("123/456").X;
        }

        internal void ToggleOption(bool showStaminaAndHealth)
        { 
            ModEntry.Events.Display.RenderedHud -= OnRendereHud;

            if (showStaminaAndHealth)
                ModEntry.Events.Display.RenderedHud += OnRendereHud;
        }

        private void OnRendereHud(object sender, RenderedHudEventArgs e)
        // public void Draw()
        {
            if (Game1.eventUp)
                return;

            Rectangle canvas = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
            // Vector2 pos = new Vector2(canvas.Right - (Game1.showingHealth ? 265 : 215), canvas.Bottom - 60);
            Vector2 pos = new Vector2(canvas.Right - TxtWidth - 10, canvas.Bottom - 300);
            Color txtColor = (Game1.player.Stamina * 100 / Game1.player.MaxStamina < 10) ? Color.Wheat : Color.White;

            string txt = $"{Math.Round(Game1.player.Stamina)}/{Game1.player.MaxStamina}";
            Game1.spriteBatch.DrawString(Game1.dialogueFont, String.Format("{0,7}", txt), pos, txtColor) ;

            if (Game1.showingHealth)
            {
                bool danger = Game1.player.health * 100 / Game1.player.maxHealth < 20;
                if (!OldDanger && danger)
                    Alarm = 40;
                OldDanger = danger;
                if (Alarm > 0 && (Alarm-- % 6 == 0))
                    Game1.playSound("drumkit4");

                pos += new Vector2(0, -45);
                txtColor = danger ? Color.Red : Color.DarkOrange;

                txt = $"{Game1.player.health}/{Game1.player.maxHealth}";
                Game1.spriteBatch.DrawString(Game1.dialogueFont, String.Format("{0,7}", txt), pos, txtColor);
            }
        }
    }
}
