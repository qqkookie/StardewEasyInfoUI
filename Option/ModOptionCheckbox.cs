using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StardewValley;
using StardewValley.Menus;

namespace EasyInfoUI
{
    internal class ModOptionCheckbox : ModOptionPart
    {
        private const int PixelSize = 9;

        private readonly Action<bool> _toggleOptionsDelegate;
        private bool _isChecked;
        private readonly IDictionary<String, String> _options;
        private readonly String _optionKey;

        internal ModOptionCheckbox(
            String label, 
            int whichOption, 
            Action<bool> toggleOptionDelegate, 
            IDictionary<String, String> options, 
            String optionKey, 
            bool defaultValue = true, 
            int x = -1, 
            int y = -1)
            : base(label, x, y, PixelSize * Game1.pixelZoom, PixelSize * Game1.pixelZoom, whichOption)
        {
            _toggleOptionsDelegate = toggleOptionDelegate;
            _options = options;
            _optionKey = optionKey;

            if (!_options.ContainsKey(_optionKey))
                _options[_optionKey] = defaultValue.ToString();

            _isChecked = _options[_optionKey].SafeParseBool();
            _toggleOptionsDelegate(_isChecked);
        }

        internal override void ReceiveLeftClick(int x, int y)
        {
            if (_canClick)
            {
                Game1.playSound("drumkit6");
                base.ReceiveLeftClick(x, y);
                _isChecked = !_isChecked;
                _options[_optionKey] = _isChecked.ToString();
                _toggleOptionsDelegate(_isChecked);
            }
        }

        internal override void Draw(SpriteBatch batch, int slotX, int slotY)
        {
            batch.Draw(Game1.mouseCursors, new Vector2(slotX + Bounds.X, slotY + Bounds.Y), new Rectangle?(_isChecked ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked), Color.White * (_canClick ? 1f : 0.33f), 0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 0.4f);
            base.Draw(batch, slotX, slotY);
        }
    }
}
