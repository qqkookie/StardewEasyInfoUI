using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace EasyInfoUI
{
    internal static class Utils
    {
        //internal String

        internal static bool OK(this String s)
        {
            return !String.IsNullOrWhiteSpace(s);
        }

        internal static bool NoOK(this String s)
        {
            return String.IsNullOrWhiteSpace(s);
        }

        internal static int SafeParseInt(this string s, int defaultValue = 0)
        {
            return (!string.IsNullOrWhiteSpace(s) && int.TryParse(s, out int result))
                    ? result : defaultValue;
        }

        internal static bool SafeParseBool(this string s, bool defaultValue = false)
        {
            return (!string.IsNullOrWhiteSpace(s) && bool.TryParse(s, out bool result))
                    ? result : defaultValue;
        }

        internal static string SafeGetString(this string key, string defaultValue = "")
        {
            return (!string.IsNullOrEmpty(key) && ModEntry.Translation != null)
                ? ModEntry.Translation.Get(key) : defaultValue;
        }

        internal static TValue SafeGet<Tkey, TValue>(this IDictionary<Tkey, TValue> dictionary, Tkey key, TValue defaultValue = default)
        {
            return (dictionary != null && dictionary.TryGetValue(key, out TValue value))
                        ? value : defaultValue;
        }

        #region Memebers
        private static readonly Dictionary<string, int> _npcHeadShotSize = new Dictionary<string, int>()
        {
            { "Piere", 9 },
            { "Sebastian", 7 },
            { "Evelyn", 5 },
            { "Penny", 6 },
            { "Jas", 6 },
            { "Caroline", 5 },
            { "Dwarf", 5 },
            { "Sam", 9 },
            { "Maru", 6 },
            { "Wizard", 9 },
            { "Jodi", 7 },
            { "Krobus", 7 },
            { "Alex", 8 },
            { "Kent", 10 },
            { "Linus", 4 },
            { "Harvey", 9 },
            { "Shane", 8 },
            { "Haley", 6 },
            { "Robin", 7 },
            { "Marlon", 2 },
            { "Emily", 8 },
            { "Marnie", 5 },
            { "Abigail", 7 },
            { "Leah", 6 },
            { "George", 5 },
            { "Elliott", 9 },
            { "Gus", 7 },
            { "Lewis", 8 },
            { "Demetrius", 11 },
            { "Pam", 5 },
            { "Vincent", 6 },
            { "Sandy", 7 },
            { "Clint", 10 },
            { "Willy", 10 }
        };

        #endregion

        internal static Rectangle GetHeadShot(this NPC npc)
        {
            if (!_npcHeadShotSize.TryGetValue(npc.Name, out int size))
                size = 4;

            Rectangle mugShotSourceRect = npc.getMugShotSourceRect();
            mugShotSourceRect.Height -= size / 2;
            mugShotSourceRect.Y -= size / 2;
            return mugShotSourceRect;
        }

        // --------------------------------------------------------------------

        internal static void CreateSafeDelayedDialogue(String dialogue, int timer)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(timer);

                do
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
                while (Game1.activeClickableMenu is GameMenu);
                Game1.setDialogue(dialogue, true);
            });
        }

        internal static int GetWidthInPlayArea()
        {
            int result;

            if (Game1.isOutdoorMapSmallerThanViewport())
            {
                int right = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right;
                int totalWidth = Game1.currentLocation.map.Layers[0].LayerWidth * Game1.tileSize;
                int someOtherWidth = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - totalWidth;

                result = right - someOtherWidth / 2;
            }
            else
            {
                result = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right;
            }

            return result;
        }

        internal static void DrawMouseCursor()
        {
            if (!Game1.options.hardwareCursor)
            {
                int mouseCursorToRender = Game1.options.gamepadControls ? Game1.mouseCursor + 44 : Game1.mouseCursor;
                var what = Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, mouseCursorToRender, 16, 16);

                Game1.spriteBatch.Draw(
                    Game1.mouseCursors,
                    new Vector2(Game1.getMouseX(), Game1.getMouseY()),
                    what,//new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.mouseCursor + 32, 16, 16)),
                    Color.White,
                    0.0f,
                    Vector2.Zero,
                    Game1.pixelZoom + (Game1.dialogueButtonScale / 150.0f),
                    SpriteEffects.None,
                    1f);
            }
        }
    }
}
