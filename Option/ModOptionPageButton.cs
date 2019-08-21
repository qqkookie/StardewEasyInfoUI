using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyInfoUI
{
    internal class ModOptionPageButton : IClickableMenu
    {

        internal Rectangle Bounds { get; }
        //private readonly ModOptionsPageHandler _optionsPageHandler;
        //private bool _hasClicked;

        internal event EventHandler OnLeftClicked;

        internal ModOptionPageButton()
        {
            //_optionsPageHandler = optionsPageHandler;
            width = 64;
            height = 64;
            GameMenu activeClickableMenu = Game1.activeClickableMenu as GameMenu;

            xPositionOnScreen = activeClickableMenu.xPositionOnScreen + activeClickableMenu.width - 200;
            yPositionOnScreen = activeClickableMenu.yPositionOnScreen + 16;
            Bounds = new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height);
            ModEntry.Events.Input.ButtonPressed += OnButtonPressed;
            ModEntry.Events.Display.MenuChanged += OnMenuChanged;
        }

        /// <summary>Raised after a game menu is opened, closed, or replaced.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is GameMenu menu)
            {
                xPositionOnScreen = menu.xPositionOnScreen + menu.width - 200;
            }
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft || e.Button == SButton.ControllerA)
            {
                int x = (int)e.Cursor.ScreenPixels.X;
                int y = (int)e.Cursor.ScreenPixels.Y;
                if (isWithinBounds(x, y))
                {
                    receiveLeftClick(x, y);
                    OnLeftClicked?.Invoke(this, null);
                }
            }

            //if (e.NewState.LeftButton != ButtonState.Pressed || !(Game1.activeClickableMenu is GameMenu))
            //{
            //    _hasClicked = false;
            //}
            //else if ((Game1.activeClickableMenu as GameMenu).currentTab != 3 && 
            //    isWithinBounds(e.NewPosition.X, e.NewPosition.Y) && 
            //    !_hasClicked)
            //{
            //    receiveLeftClick(e.NewPosition.X, e.NewPosition.Y);

            //}
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);

            Game1.spriteBatch.Draw(Game1.mouseCursors, 
                new Vector2(xPositionOnScreen, yPositionOnScreen), 
                new Rectangle(16, 368, 16, 16), 
                Color.White, 
                0.0f, 
                Vector2.Zero, 
                Game1.pixelZoom, 
                SpriteEffects.None,
                0.1f);

            b.Draw(Game1.mouseCursors, 
                new Vector2(xPositionOnScreen + 8, yPositionOnScreen + 14), 
                new Rectangle(32, 672, 16, 16), 
                Color.White, 
                0.0f, 
                Vector2.Zero, 
                3f, 
                SpriteEffects.None,
                0.1f);


            if (isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
            {
                string label = ModEntry.Translation.Get("label.ui-mod-options");
                IClickableMenu.drawHoverText(Game1.spriteBatch, label, Game1.smallFont);
            }
            // Tools.DrawMouseCursor();
            Game1.activeClickableMenu.drawMouse(b);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }
    }
}
