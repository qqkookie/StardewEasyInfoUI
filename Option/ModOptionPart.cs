﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using StardewValley;
using StardewValley.BellsAndWhistles;

namespace EasyUI
{
    internal class ModOptionPart
    {
        private const int DefaultX = 8;
        private const int DefaultY = 4;
        private const int DefaultPixelSize = 9;
        private Rectangle _bounds;
        private String _label;
        private int _whichOption;
        protected bool _canClick = true;

        internal Rectangle Bounds { get { return _bounds; } }

        internal ModOptionPart(String label)
            : this(label, -1, -1, DefaultPixelSize * Game1.pixelZoom, DefaultPixelSize * Game1.pixelZoom)
        {

        }

        internal ModOptionPart(String label, int x, int y, int width, int height, int whichOption = -1)
        {
            if (x < 0)
                x = DefaultX * Game1.pixelZoom;

            if (y < 0)
                y = DefaultY * Game1.pixelZoom;

            _bounds = new Rectangle(x, y, width, height);
            _label = label;
            _whichOption = whichOption;
        }

        internal virtual void ReceiveLeftClick(int x, int y)
        {

        }

        internal virtual void LeftClickHeld(int x, int y)
        {

        }

        internal virtual void LeftClickReleased(int x, int y)
        {

        }

        internal virtual void ReceiveKeyPress(Keys key)
        {

        }

        internal virtual void Draw(SpriteBatch batch, int slotX, int slotY)
        {
            if (_whichOption < 0)
            {
                SpriteText.drawString(batch, _label, slotX + _bounds.X, slotY + _bounds.Y + Game1.pixelZoom * 3, 999, -1, 999, 1, 0.1f);
            }
            else
            {
                Utility.drawTextWithShadow(batch, 
                    _label, 
                    Game1.dialogueFont, 
                    new Vector2(slotX + _bounds.X + _bounds.Width + Game1.pixelZoom * 2, slotY + _bounds.Y), 
                    _canClick ? Game1.textColor : Game1.textColor * 0.33f, 
                    1f, 
                    0.1f);
            }
        }
    }
}
