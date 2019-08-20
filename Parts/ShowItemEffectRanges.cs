using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyUI
{
    internal class ShowItemEffectRanges : IDisposable
    {
        private readonly List<Point> RangeArea = new List<Point>();

        internal ShowItemEffectRanges()
        {
        }

        internal void ToggleOption(bool showItemEffectRanges)
        {
            ModEntry.Events.Display.RenderedWorld -= OnRenderedWorld;
            ModEntry.Events.GameLoop.UpdateTicked -= OnUpdateTicked;

            if (showItemEffectRanges)
            {
                ModEntry.Events.Display.RenderedWorld += OnRenderedWorld;
                ModEntry.Events.GameLoop.UpdateTicked += OnUpdateTicked;
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
            if (!e.IsMultipleOf(10) || !(Game1.currentLocation is BuildableGameLocation map) || !Context.CanPlayerMove)
                return;

            // check draw tile outlines
            RangeArea.Clear();

            // if Left Shift key is pressed, display range of hovered object.
            KeyboardState state = Keyboard.GetState();
            bool shifting = state.IsKeyDown(Keys.LeftShift);

            if (shifting)
            {
                Building building = map.getBuildingAt(Game1.currentCursorTile);
                if (building is JunimoHut)
                    HightlightJunimoHuts(map);
            }

            if (!(Game1.player.CurrentItem is StardewValley.Object obj) || obj == null)
            {
                obj = map.getObjectAtTile((int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y);
                if ( obj == null || !shifting)
                    return;
            }

            string objName = obj.Name.ToLower();

            string baseType;
            if (objName.EndsWith("sprinkler"))
                baseType = "sprinkler";
            else if (objName == "bee house")
                baseType = "bee house";
            else if (objName.EndsWith("scarecrow"))
                baseType = "scarecrow";
            else
                return;

            int tileX = (Game1.getMouseX() + Game1.viewport.X) / Game1.tileSize;
            int tileY = (Game1.getMouseY() + Game1.viewport.Y) / Game1.tileSize;
            HightlightRange(tileX, tileY, objName, baseType);

            foreach (var nextThing in Game1.currentLocation.Objects.Pairs)
            {
                objName = nextThing.Value.Name.ToLower();
                if (objName.EndsWith(baseType))
                    HightlightRange((int)nextThing.Key.X, (int)nextThing.Key.Y, objName, baseType);
            }
        }

        private void HightlightJunimoHuts(BuildableGameLocation map)
        {
            foreach (Building nextBuilding in map.buildings)
            {
                if (!(nextBuilding is JunimoHut nextHut))
                    continue;

                for (int iy = 0; iy < 17; ++iy)
                {
                    for (int jx = 0; jx < 17; ++jx)
                    {
                        if (!(jx == 7 || jx == 8) || !(iy == 7 || iy == 8 || iy == 9))
                            RangeArea.Add(new Point(nextHut.tileX.Value + jx - 8, nextHut.tileY.Value + iy - 8));
                    }
                }
            }
        }

        private void HightlightRange( int tileX, int tileY, string objName, string baseType)
        {
            if (baseType == "sprinkler")
            {
                int quality = 0;
                if (objName.StartsWith("sprinkler"))
                    quality = 0;
                else if (objName.StartsWith("quality"))
                    quality = 1;
                else if (objName.StartsWith("iridium"))
                    quality = 2;
                else if (objName.StartsWith("prismatic"))
                    quality = 3;

                HighlightedArea(tileX, tileY, SprinklerMap, quality);
            }
            else if (baseType == "bee house")
            {
                HighlightedArea(tileX, tileY, BeehouseMap, 0);
            }
            else if (baseType == "scarecrow")
            {
                for (int iy = 0; iy < 17; ++iy)
                {
                    for (int jx = 0; jx < 17; ++jx)
                    {
                        if (Math.Abs(iy - 8) + Math.Abs(jx - 8) <= 12)
                            RangeArea.Add(new Point(tileX + jx - 8, tileY + iy - 8));
                    }
                }
            }
        }

        private byte[][] BeehouseMap = new byte[][]
        {
            new byte[] { 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 },
            new byte[] { 8, 8, 8, 8, 0, 0, 0, 8, 8, 8, 8 },
            new byte[] { 8, 8, 8, 0, 0, 0, 0, 0, 8, 8, 8 },
            new byte[] { 8, 8, 0, 0, 0, 0, 0, 0, 0, 8, 8 },
            new byte[] { 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8 },
            new byte[] { 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0 },
            new byte[] { 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8 },
            new byte[] { 8, 8, 0, 0, 0, 0, 0, 0, 0, 8, 8 },
            new byte[] { 8, 8, 8, 0, 0, 0, 0, 0, 8, 8, 8 },
            new byte[] { 8, 8, 8, 8, 0, 0, 0, 8, 8, 8, 8 },
            new byte[] { 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 }
        };

        private byte[][] SprinklerMap = new byte[][]
        {
            new byte[] { 8, 8, 8, 8, 8, 8, 8, 8, 8, },
            new byte[] { 8, 3, 3, 3, 3, 3, 3, 3, 8, },
            new byte[] { 8, 3, 2, 2, 2, 2, 2, 3, 8, },
            new byte[] { 8, 3, 2, 1, 0, 1, 2, 3, 8, },
            new byte[] { 8, 3, 2, 0, 8, 0, 2, 3, 8, },
            new byte[] { 8, 3, 2, 1, 0, 1, 2, 3, 8, },
            new byte[] { 8, 3, 2, 2, 2, 2, 2, 3, 8, },
            new byte[] { 8, 3, 3, 3, 3, 3, 3, 3, 8, },
            new byte[] { 8, 8, 8, 8, 8, 8, 8, 8, 8, },
        };

        // quality = 0 ~ 3 for normal, quality, iridium and prismatic sprinkler, 
        private void HighlightedArea(int xPos, int yPos, byte[][] rangeMap, int quality)
        {
            byte threshhold = (byte)quality;

            int yOffset = rangeMap.Length / 2;
            for (int iy = 0; iy < rangeMap.Length; ++iy)
            {
                int xOffset = rangeMap[iy].Length / 2;
                for (int jx = 0; jx < rangeMap[iy].Length; ++jx)
                {
                    if (rangeMap[iy][jx] <= threshhold)
                        RangeArea.Add(new Point(xPos + jx - xOffset, yPos + iy - yOffset));
                }
            }
        }

        /// <summary>Raised after the game draws to the sprite patch in a draw tick, just before the final sprite batch is rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!(Game1.currentLocation is BuildableGameLocation map) || !Context.CanPlayerMove)
                return;

            // draw tile outlines
            foreach (Point point in RangeArea)
            Game1.spriteBatch.Draw(
                Game1.mouseCursors,
                Game1.GlobalToLocal(new Vector2(point.X * Game1.tileSize, point.Y * Game1.tileSize)),
                new Rectangle(194, 388, 16, 16),
                Color.White * 0.7f,
                0.0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                0.01f);
        }
    }
}
