using System;
using Microsoft.Xna.Framework;

using StardewValley;

namespace EasyInfoUI
{
    public class IconHandler
    {
        public static IconHandler Handler { get; private set; }

        static IconHandler()
        {
            if (Handler == null)
                Handler = new IconHandler();
        }

        private int _amountOfVisibleIcons;

        private IconHandler()
        {

        }

        public Point GetNewIconPosition()
        {
            int yPos = Game1.options.zoomButtons ? 290 : 260;
            // int xPosition = (int)Tools.GetWidthInPlayArea() - 134 - 46 * _amountOfVisibleIcons;
            int xPosition = Utils.GetWidthInPlayArea() - 70 - 48 * _amountOfVisibleIcons;
            if (Game1.player.questLog.Any())
                xPosition -= 65;
            
            ++_amountOfVisibleIcons;
            return new Point(xPosition, yPos);
        }

        internal void Reset(object sender, EventArgs e)
        {
            _amountOfVisibleIcons = 0;
        }


    }
}
