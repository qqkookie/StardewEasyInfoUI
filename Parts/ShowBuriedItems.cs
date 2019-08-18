using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

using StardewValley;
using StardewValley.Locations;
using StardewValley.Tools;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyUI
{
    internal class ShowBuriedItems : IDisposable
    {
        private static IModEvents Events => ModEntry.Events;
        private Dictionary<Vector2, Color> BuriedItems = new Dictionary<Vector2, Color>();
        private Texture2D pixelTexture;
        private bool Changed = true;
        
        internal ShowBuriedItems()
        {
            Color[] array = Enumerable.Range(0, 1).Select(i => Color.White).ToArray();

            this.pixelTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            this.pixelTexture.SetData(array);

            ModEntry.Config.ShowMineLadder = !ModEntry.ModHelper.ModRegistry.IsLoaded("Cookie.EasyMine");
        }

        internal void ToggleOption(bool showBuriedItem)
        {
            Events.Player.Warped -= this.OnBuriedChanged;
            Events.Player.InventoryChanged -= this.OnBuriedChanged;
            Events.World.ObjectListChanged -= this.OnBuriedChanged;
            Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
            Events.Display.RenderedWorld -= OnRenderedWorld;

            if (showBuriedItem)
            {
                Events.Player.Warped += this.OnBuriedChanged;
                Events.Player.InventoryChanged += this.OnBuriedChanged;
                Events.World.ObjectListChanged += this.OnBuriedChanged;
                Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
                Events.Display.RenderedWorld += OnRenderedWorld;
            }   
        }

        public void Dispose()
        {
            ToggleOption(false);
        }

        private void OnBuriedChanged(object sender, EventArgs e)
        {
            this.Changed = true;
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!e.IsMultipleOf(10) || !Context.CanPlayerMove)
                return;

            if (UsingDiggingTool() && this.Changed)
                this.FindBuried();
        }

        private bool UsingDiggingTool()
        {
            return (Game1.player.CurrentTool is Hoe) || (Game1.mine != null && (Game1.player.CurrentTool is Pickaxe));
        }

        private string[] Tresures = { "Arch", "Iridium", "Gold", "Coins", "CaveCarrot", "Coal", "Copper" };

        private void FindBuried()
        {
            GameLocation loc = Game1.currentLocation;
            if ( loc == null || (!loc.IsOutdoors && !(loc is MineShaft)))
                return;

            Dictionary <Vector2, Color> Buried = new Dictionary<Vector2, Color>();
            int stoneLeft = 0;
            bool ladderSpawned = false;
            bool doMineLadder = Game1.mine != null && ModEntry.Config.ShowMineLadder && UsingDiggingTool();

            if (doMineLadder)
            {
                stoneLeft = ModEntry.Reflection.GetField<NetIntDelta>(Game1.mine, "netStonesLeftOnThisLevel").GetValue().Value;
                ladderSpawned = ModEntry.Reflection.GetField<bool>(Game1.mine, "ladderHasSpawned", true).GetValue();
            }

            foreach (var obj in loc.Objects.Pairs)
            {
                if (loc.IsOutdoors)
                {
                    if (obj.Value.Name == "Artifact Spot")
                        Buried.Add(obj.Key, Color.Coral);
                }
                else if (doMineLadder)
                {
                    if (obj.Value.Name == "Stone")
                    {
                        // ladder chance calculation taken from checkStoneForItems function in MineShaft class
                        Random rng = new Random( (int)obj.Key.X * 1000 + (int)obj.Key.Y 
                            + Game1.mine.mineLevel + (int)Game1.uniqueIDForThisGame / 2);
                        rng.NextDouble();
                        double chance = 0.02 + 1.0 / (double)Math.Max(1, stoneLeft)
                            + (double)Game1.player.LuckLevel / 100.0 + Game1.dailyLuck / 5.0;
                        if (Game1.mine.characters.Count == 0)
                            chance += 0.04;

                        if (!ladderSpawned && (stoneLeft == 0 || rng.NextDouble() < chance))
                            Buried.Add(obj.Key, Color.Coral);
                    }
                }
            }

            int layerWidth = loc.map.Layers[0].LayerWidth;
            int layerHeight = loc.map.Layers[0].LayerHeight;

            bool outdoorwinter= loc.IsOutdoors && ((!loc.IsFarm && Game1.currentSeason == "winter")|| ModEntry.Config.ShowBuriedClay);
            for (int iy = 0; iy < layerHeight; iy++)
            {
                for (int jx = 0; jx < layerWidth; jx++)
                {
                    Vector2 key = new Vector2(jx, iy);

                    if ( !loc.terrainFeatures.ContainsKey(key) && !loc.isTileOccupied(key, "")
                        && loc.doesTileHaveProperty(jx, iy, "Diggable", "Back") != null
                        && loc.isTilePassable(new xTile.Dimensions.Location((int)key.X, (int)key.Y), Game1.viewport))
                    {
                        string prop = loc.doesTileHaveProperty(jx, iy, "Treasure", "Back");
                        if (prop != null)
                        {
                            string treasure = prop.Split(' ')[0];
                            Buried.Add(key, Color.Lime);
                        }
                        else if (outdoorwinter)
                        {
                            Random rng = new Random(jx * 2000 + iy * 77 + (int)Game1.uniqueIDForThisGame / 2 
                                + (int)Game1.stats.DaysPlayed + (int)Game1.stats.DirtHoed);

                            if (!loc.IsFarm && Game1.currentSeason.Equals("winter") && rng.NextDouble() < 0.08)
                            {
                                if (rng.NextDouble() < 0.5)
                                    Buried.Add(key, Color.Orange);  // winter root
                                else
                                    Buried.Add(key, Color.LightGray);    // snow yam
                            }
                            else if (ModEntry.Config.ShowBuriedClay && rng.NextDouble() < 0.03)
                                Buried.Add(key, Color.SaddleBrown);     // clay
                        }
                    }
                }
            }

            this.BuriedItems.Clear();
            this.BuriedItems = Buried;
            this.Changed = false;
        }

        private void OnRenderedWorld(object sender, EventArgs e)
        {
            if (!Context.CanPlayerMove  || !UsingDiggingTool() || this.BuriedItems.Count == 0)
                return;

            foreach (var item in this.BuriedItems)
            {
                Rectangle rect = new Rectangle((int)(item.Key.X * Game1.tileSize - Game1.viewport.X), 
                    (int)(item.Key.Y * Game1.tileSize - Game1.viewport.Y), Game1.tileSize, Game1.tileSize);

                this.DrawRectangle(Game1.spriteBatch, rect, item.Value);
            }
        }

        private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color)
        {
            spriteBatch.Draw(this.pixelTexture, new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, 2), color);
            spriteBatch.Draw(this.pixelTexture, new Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, 2), color);
            spriteBatch.Draw(this.pixelTexture, new Rectangle(rectangle.Left, rectangle.Top, 2, rectangle.Height), color);
            spriteBatch.Draw(this.pixelTexture, new Rectangle(rectangle.Right, rectangle.Top, 2, rectangle.Height + 2), color);
        }
    }
}