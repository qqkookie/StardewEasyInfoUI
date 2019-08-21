using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyInfoUI
{
    // This source code is based on [Community Bundle Item Tooltip mod](https://www.nexusmods.com/stardewvalley/mods/1329/
    // Source: https://github.com/musbah/StardewValleyMods/blob/master/BundleTooltips/ModEntry.cs
    // Also https://github.com/CJBok/SDV-Mods/blob/master/CJBShowItemSellPrice/StardewCJB.cs
    internal class ShowItemHoverInformation : IDisposable
    {
        private IModEvents Events => ModEntry.Events;
        private IReflectionHelper Reflection => ModEntry.Reflection;
        //private ITranslationHelper _Translation;

        private List<int> BundleItemList;
        private Dictionary<int, int[][]> BundleDB;
        private Dictionary<int, string[]> BundleNames;

        /// <summary>Item that currently mouse curosr is pointing to.</summary>
        private Item HoveredItem;

        /*********
        ** Public methods
        *********/

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        internal ShowItemHoverInformation()
        {
            //This will be filled with the itemIDs of every item in every bundle (for a fast search without details)
            LoadBundleData();
        }

        /*********
        ** Private methods
        *********/

        public void Dispose()
        {
            ToggleOption(false);
        }

        internal void ToggleOption(bool showBundle)
        {
            Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            Events.Display.RenderedHud -= OnRenderedHudEvent;
            Events.Display.RenderedActiveMenu -= OnRenderedActiveMenu;

            if (showBundle)
            {
                Events.GameLoop.UpdateTicked += OnUpdateTicked;
                Events.Display.RenderedHud += OnRenderedHudEvent;
                Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
            }
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!e.IsMultipleOf(10) || !Context.IsPlayerFree)
                return;

            /// <summary>The cached toolbar instance.</summary>
            Toolbar toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();

            // toolbar.hoverItem is set to null after rendered HUD
            HoveredItem = (toolbar != null) ? Reflection.GetField<Item>(toolbar, "hoverItem").GetValue() : null;

        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the sprite batch, 
        /// but before it's rendered to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open).</summary>
        private void OnRenderedHudEvent(object sender, RenderedHudEventArgs e)
        {
            if (HoveredItem != null && Context.IsPlayerFree && Game1.activeClickableMenu == null)
                DrawItemHoverInfo(e.SpriteBatch, HoveredItem);
        }

        /*
        /// <summary>Get the hovered item from the on-screen toolbar.</summary>
        private Item GetItemFromToolbar()
        {
            if (!Context.IsPlayerFree || this.Toolbar == null || this.ToolbarSlots == null)
                return null;

            // find hovered slot
            int x = Game1.getMouseX();
            int y = Game1.getMouseY();
            ClickableComponent hoveredSlot = this.ToolbarSlots.FirstOrDefault(slot => slot.containsPoint(x, y));
            if (hoveredSlot == null)
                return null;

            // get inventory index
            int index = this.ToolbarSlots.IndexOf(hoveredSlot);
            if (index < 0 || index > Game1.player.Items.Count - 1)
                return null;

            // get hovered item
            return Game1.player.Items[index];
        }
        
        private Item GetHoveredItemFromToolbar()
        {
            foreach (IClickableMenu menu in Game1.onScreenMenus)
            {
                if (menu is Toolbar toolbar)
                {
                    return _Reflection.GetField<Item>(menu, "hoverItem").GetValue();
                }
            }

            return null;
        }
        */

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            Item item = this.GetHoveredItemFromMenu(Game1.activeClickableMenu);

            if (item != null && Game1.activeClickableMenu != null)
                DrawItemHoverInfo(e.SpriteBatch, item);

            if ((Game1.activeClickableMenu is GameMenu gamemenu) && gamemenu.currentTab == GameMenu.craftingTab)
            {
                // Display crafted count of the item
                CraftingPage craftingPage = (CraftingPage)Reflection.GetField<List<IClickableMenu>>
                    (gamemenu, "pages", true).GetValue()[GameMenu.craftingTab];

                CraftingRecipe craftingRecipe = Reflection.GetField<CraftingRecipe>
                    (craftingPage, "hoverRecipe", true).GetValue();

                if (craftingPage != null && craftingRecipe != null)
                {
                    Point pos = Game1.getMousePosition();
                    string desc = ModEntry.Translation.Get("label.crafted-count")       //  "Number crafted: ",
                         + Game1.player.craftingRecipes[craftingRecipe.name].ToString();
                    DrawExtraTextBox(Game1.smallFont, desc, pos.X + Game1.tileSize / 2, pos.Y - Game1.tileSize / 2);
                }
            }
        }

        /// <summary>Get the hovered item from an arbitrary menu.</summary>
        /// <param name="menu">The menu whose hovered item to find.</param>
        private Item GetHoveredItemFromMenu(IClickableMenu menu)
        {
            // game menu
            if (menu is GameMenu gameMenu)
            {
                IClickableMenu page = Reflection.GetField<List<IClickableMenu>>(gameMenu, "pages").GetValue()[gameMenu.currentTab];
                if (page is InventoryPage)
                    return Reflection.GetField<Item>(page, "hoveredItem").GetValue();
                else if (page is CraftingPage)
                    return Reflection.GetField<Item>(page, "hoverItem").GetValue();
            }

            // from inventory UI (so things like shops and so on)
            else if (menu is MenuWithInventory inventoryMenu)
                return inventoryMenu.hoveredItem;

            // CJB mods
            else if (menu.GetType().FullName == "CJBItemSpawner.Framework.ItemMenu")
                return Reflection.GetField<Item>(menu, "HoveredItem").GetValue();

            return null;
        }

        /// <summary>Bundle name translation key.</summary>
        private readonly Dictionary<string, string> BundleNameKey = new Dictionary<string, string> {
                { "Boiler Room", "CommunityCenter_AreaName_BoilerRoom" },
                { "Bulletin Board", "CommunityCenter_AreaName_BulletinBoard" },
                { "Crafts Room", "CommunityCenter_AreaName_CraftsRoom" },
                { "Fish Tank", "CommunityCenter_AreaName_FishTank" },
                { "Pantry", "CommunityCenter_AreaName_Pantry" },
                { "Vault", "CommunityCenter_AreaName_Vault" },
            };

        /// <summary>Load all bundles database.</summary>
        private void LoadBundleData()
        {
            BundleItemList = new List<int>();
            BundleDB = new Dictionary<int, int[][]>();
            BundleNames = new Dictionary<int, string[]>();

            Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Bundles");

            foreach (KeyValuePair<string, string> keyValuePair in dictionary)
            {
                // format of the values are itemID itemAmount itemQuality

                // if bundleIndex is between 23 and 26, then they're vault bundles so don't add to dictionary

                string[] split = keyValuePair.Key.Split('/');
                string bundleName = split[0];

                if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en)
                    bundleName = Game1.content.LoadString("Strings\\Locations:" + BundleNameKey[bundleName]);

                string bundleSubName;

                string[] subsplit = keyValuePair.Value.Split('/');
                bundleSubName = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en
                    ? subsplit[0] : subsplit[subsplit.Length - 1];

                int bundleIndex = Convert.ToInt32(split[1]);
                if (!(bundleIndex >= 23 && bundleIndex <= 26))
                {
                    //creating an array for the bundle names
                    string[] bundleNames = new string[] { bundleName, bundleSubName };

                    //creating an array of items[i][j] , i is the item index, j=0 itemId, j=1 itemAmount, j=2 itemQuality, j=3 order of the item for it's own bundle
                    string[] allItems = keyValuePair.Value.Split('/')[2].Split(' ');
                    int allItemsLength = allItems.Length / 3;

                    int[][] items = new int[allItemsLength][];

                    int j = 0;
                    int i = 0;
                    while (j < allItemsLength)
                    {
                        items[j] = new int[4];
                        items[j][0] = Convert.ToInt32(allItems[0 + i]);
                        items[j][1] = Convert.ToInt32(allItems[1 + i]);
                        items[j][2] = Convert.ToInt32(allItems[2 + i]);
                        items[j][3] = i / 3;

                        BundleItemList.Add(items[j][0]);
                        i = i + 3;
                        j++;
                    }

                    BundleDB.Add(bundleIndex, items);
                    BundleNames.Add(bundleIndex, bundleNames);
                }
            }

            //remove duplicates
            BundleItemList = new HashSet<int>(BundleItemList).ToList();
        }

        private string GetBundleInfo(Item item)
        {
            if (item == null || BundleItemList == null || BundleItemList.Count == 0
                || Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
                return String.Empty;

            string nameDisplay = Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName");
            string stackDisplay = ModEntry.Translation.Get("label.bundle-stack");

            StardewValley.Locations.CommunityCenter communityCenter = Game1.getLocationFromName("CommunityCenter") as StardewValley.Locations.CommunityCenter;

            List<int[]> itemInfo = new List<int[]>();
            Dictionary<string, List<string>> descriptions = new Dictionary<string, List<string>>();

            foreach (int itemInBundles in BundleItemList)
            {
                if (item.ParentSheetIndex == itemInBundles)
                {
                    foreach (KeyValuePair<int, int[][]> bundle in BundleDB)
                    {
                        for (int i = 0; i < bundle.Value.Length; i++)
                        {
                            //Getting the item name because the bundle itself doesn't actually make sure that the correct item is being placed
                            //(parentSheetIndex of object can overlap with another item from another sheet)
                            string itemName = "";
                            if (Game1.objectInformation.ContainsKey(bundle.Value[i][0]))
                            {
                                itemName = Game1.objectInformation[bundle.Value[i][0]].Split('/')[0];
                            }

                            var isItemInBundleSlot = communityCenter.bundles[bundle.Key][bundle.Value[i][3]];
                            if ((item is StardewValley.Object) && item.Stack != 0 && bundle.Value[i] != null && bundle.Value[i][0] == item.ParentSheetIndex && itemName == item.Name && bundle.Value[i][2] <= ((StardewValley.Object)item).Quality)
                            {
                                if (!isItemInBundleSlot)
                                {
                                    //Saving i to check if the items are the same or not later on
                                    itemInfo.Add(new int[] { bundle.Key, bundle.Value[i][1], i });
                                    descriptions[BundleNames[bundle.Key][0]] = new List<string>();
                                }
                            }
                        }
                    }
                }
            }

            foreach (int[] info in itemInfo)
            {
                string bundleName = BundleNames[info[0]][0];
                string bundleSubName = BundleNames[info[0]][1];
                int quantity = info[1];

                string display = string.Format(nameDisplay, bundleSubName);
                if (quantity > 1)
                    display = String.Format(stackDisplay, display, quantity, item.Stack);

                descriptions[bundleName].Add(display);
            }

            string tooltipText = String.Empty;

            if (descriptions.Count > 0)
            {
                int count = 0;
                foreach (KeyValuePair<string, List<string>> keyValuePair in descriptions)
                {
                    if (count > 0)
                        tooltipText += "\n";

                    tooltipText += keyValuePair.Key;
                    foreach (string value in keyValuePair.Value)
                    {
                        tooltipText += "\n    " + value;
                    }
                    count++;
                }
                // this.DrawHoverTextBox(Game1.smallFont, tooltipText, isItFromToolbar, item.Stack);
            }
            return tooltipText;
        }

        /// <summary>Draw tooltip box with bundle info and price info for an item.</summary>
        /// <param name="sb">The sprite batch to update.</param>
        /// <param name="item">The item whose info to display.</param>
        private void DrawItemHoverInfo(SpriteBatch sb, Item item)
        {
            spriteBatch = sb;
            font = Game1.smallFont;

            DrawItemHoverInfo(item);
        }

        /// <summary>The spritesheet source rectangle for the tooltip box.</summary>
        private readonly Rectangle boxSourceRect = new Rectangle(0, 256, 60, 60);

        /// <summary>The spritesheet source rectangle for the coin icon.</summary>
        private readonly Rectangle coinSourceRect = new Rectangle(5, 69, 6, 6);

        /// <summary>The padding between elements in the tooltip box.</summary>
        private const float padding = 6.0f;

        SpriteBatch spriteBatch = Game1.spriteBatch;
        SpriteFont font = Game1.smallFont;

        /// <summary>Draw tooltip box with bundle info and price info for an item.</summary>
        private void DrawItemHoverInfo(Item item)
        {
            /// <summary>Relative offset from cursor position to tooltip box.</summary>
            Point offsetFromCursor = new Point(-Game1.tileSize / 2, Game1.tileSize / 2);

            /// <summary>tooltip box's border margin surrounding text inside the box.</summary>
            const float margin = 20.0f;

            string bundleTxt = GetBundleInfo(item);
            bool showBundle = !String.IsNullOrEmpty(bundleTxt);
            Vector2 bundleSize = Vector2.Zero;
            Vector2 innerSize = Vector2.Zero;

            if (showBundle)
            {
                bundleSize = font.MeasureString(bundleTxt);
                innerSize = bundleSize;
            }

            int itemPrice = item is StardewValley.Object obj ? obj.sellToStorePrice() : item.salePrice() / 2;
            bool showStack = item.Stack > 1;

            string unitLabel = String.Empty;
            string unitPrice = String.Empty;
            string stackLabel = String.Empty;
            string stackPrice = String.Empty;
            Vector2 lineSize = Vector2.Zero;

            if (itemPrice > 0)
            {
                if (showBundle)
                    innerSize.Y += padding;

                // prepare text
                unitLabel = ModEntry.Translation.Get("label.unit-price");
                unitPrice = itemPrice.ToString();

                // get dimensions
                float coin = coinSourceRect.Width * Game1.pixelZoom + padding + 12;
                lineSize = font.MeasureString(unitLabel + unitPrice);
                innerSize.X = Math.Max(innerSize.X, lineSize.X + coin);
                innerSize.Y += lineSize.Y;

                if (showStack)
                {
                    stackLabel = ModEntry.Translation.Get("label.stack-price");
                    stackPrice = (itemPrice * item.Stack).ToString();
                    lineSize = font.MeasureString(stackLabel + stackPrice);
                    innerSize.X = Math.Max(innerSize.X, lineSize.X + coin);
                    innerSize.Y += lineSize.Y;
                }
            }
            else if (!showBundle)
                return;

            // box margins
            Vector2 outerSize = innerSize + new Vector2(margin * 2.2f, margin * 1.7f);

            // get tooltip position
            int outx = (int)(Mouse.GetState().X / Game1.options.zoomLevel) + offsetFromCursor.X - (int)outerSize.X;
            int outy = (int)(Mouse.GetState().Y / Game1.options.zoomLevel) + offsetFromCursor.Y;

            // adjust position to fit on screen
            Rectangle area = new Rectangle(outx, outy, (int)outerSize.X, (int)outerSize.Y);
            if (area.Right > Game1.viewport.Width)
                outx = Game1.viewport.Width - area.Width;
            if (area.Bottom > Game1.viewport.Height)
                outy = Game1.viewport.Height - area.Height;

            // draw tooltip box
            IClickableMenu.drawTextureBox(spriteBatch, Game1.menuTexture, this.boxSourceRect,
                outx, outy, (int)outerSize.X, (int)outerSize.Y, Color.White);

            Vector2 txtPos = new Vector2(outx + margin, outy + margin);

            // draw bundle tooltip
            if (showBundle)
            {
                Utility.drawTextWithShadow(spriteBatch, bundleTxt, font, txtPos, Game1.textColor);
                txtPos.Y += bundleSize.Y + padding;
            }

            // draw price tooltip
            if (itemPrice > 0)
            {
                DrawPriceLine(txtPos, innerSize, unitLabel, unitPrice);
                txtPos.Y += lineSize.Y;

                if (showStack)
                {
                    DrawPriceLine(txtPos, innerSize, stackLabel, stackPrice);
                    txtPos.Y += lineSize.Y;
                }
            }
        }

        // Draw one price line
        private void DrawPriceLine(Vector2 linePos, Vector2 innerSize, string label, string price)
        {
            // label
            Utility.drawTextWithShadow(spriteBatch, label, font, new Vector2(linePos.X, linePos.Y), Game1.textColor);
            // draw coins
            linePos.X += innerSize.X - coinSourceRect.Width * Game1.pixelZoom;
            spriteBatch.Draw(Game1.debrisSpriteSheet, linePos, coinSourceRect, Color.White,
                0.0f, Vector2.Zero, Game1.pixelZoom, SpriteEffects.None, 1f);
            // price
            linePos.X -= font.MeasureString(price).X + padding;
            Utility.drawTextWithShadow(spriteBatch, price, font, linePos, Game1.textColor);
        }

        // Draw small text box
        private void DrawExtraTextBox(SpriteFont font, string description, int x, int y)
        {
            Vector2 txtSize = font.MeasureString(description);
            int width = (int)txtSize.X + Game1.tileSize / 2 + 40;
            int height = (int)txtSize.Y + Game1.tileSize / 3 + 5;
            if (x < 0)
                x = 0;

            int vpHeight = Game1.graphics.GraphicsDevice.Viewport.Height;

            if (y + height > vpHeight)
                y = vpHeight - height;

            IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.menuTexture,
                new Rectangle(0, 256, 60, 60), x, y, width, height, Color.White, 1f, true);

            Utility.drawTextWithShadow(Game1.spriteBatch, description, font,
                new Vector2((x + Game1.tileSize / 4), (y + Game1.tileSize / 4)),
                Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
        }
    }
}

// ============================================================================
// Old obsolute code. (Version 1.7.23, 2019, Jan.)

#if false
    class ShowItemHoverInformation : IDisposable
    {
        private readonly Dictionary<String, List<int>> _prunedRequiredBundles = new Dictionary<string, List<int>>();
        private readonly ClickableTextureComponent _bundleIcon =
            new ClickableTextureComponent(
                "",
                new Rectangle(0, 0, Game1.tileSize, Game1.tileSize),
                "",
                Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover", new object[0]),
                Game1.mouseCursors,
                new Rectangle(331, 374, 15, 14),
                Game1.pixelZoom);

        private Item _hoverItem;
        private CommunityCenter _communityCenter;
        private Dictionary<String, String> _bundleData;
        private readonly IModEvents _events;

        public ShowItemHoverInformation(IModEvents events)
        {
            _events = events;
        }

        public void ToggleOption(bool showItemHoverInformation)
        {
            _events.Player.InventoryChanged -= OnInventoryChanged;
            _events.Display.Rendered -= OnRendered;
            _events.Display.RenderedHud -= OnRenderedHud;
            _events.Display.Rendering -= OnRendering;

            if (showItemHoverInformation)
            {
                _communityCenter = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
                _bundleData = Game1.content.Load<Dictionary<String, String>>("Data\\Bundles");
                PopulateRequiredBundles();

                _events.Player.InventoryChanged += OnInventoryChanged;
                _events.Display.Rendered += OnRendered;
                _events.Display.RenderedHud += OnRenderedHud;
                _events.Display.Rendering += OnRendering;
            }
        }

        public void Dispose()
        {
            ToggleOption(false);
        }

        /// <summary>Raised before the game draws anything to the screen in a draw tick, as soon as the sprite batch is opened.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRendering(object sender, EventArgs e)
        {
            _hoverItem = Tools.GetHoveredItem();
        }

        /// <summary>Raised after drawing the HUD (item toolbar, clock, etc) to the sprite batch, but before it's rendered to the screen. The vanilla HUD may be hidden at this point (e.g. because a menu is open). Content drawn to the sprite batch at this point will appear over the HUD.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRenderedHud(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu == null)
            {
                DrawAdvancedTooltip();
            }
        }

        /// <summary>Raised after the game draws to the sprite patch in a draw tick, just before the final sprite batch is rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRendered(object sender, EventArgs e)
        {
            if (Game1.activeClickableMenu != null)
            {
                DrawAdvancedTooltip();
            }
        }

        /// <summary>Raised after items are added or removed to a player's inventory. NOTE: this event is currently only raised for the current player.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (e.IsLocalPlayer)
                this.PopulateRequiredBundles();
        }

        private void PopulateRequiredBundles()
        {
            _prunedRequiredBundles.Clear();
            foreach (var bundle in _bundleData)
            {
                String[] bundleRoomInfo = bundle.Key.Split('/');
                String bundleRoom = bundleRoomInfo[0];
                int roomNum;

                switch(bundleRoom)
                {
                    case "Pantry": roomNum = 0; break;
                    case "Crafts Room": roomNum = 1; break;
                    case "Fish Tank": roomNum = 2; break;
                    case "Boiler Room": roomNum = 3; break;
                    case "Vault": roomNum = 4; break;
                    case "Bulletin Board": roomNum = 5; break;
                    default: continue;
                }

                if (_communityCenter.shouldNoteAppearInArea(roomNum))
                {
                    int bundleNumber = bundleRoomInfo[1].SafeParseInt32();
                    string[] bundleInfo = bundle.Value.Split('/');
                    string bundleName = bundleInfo[0];
                    string[] bundleValues = bundleInfo[2].Split(' ');
                    List<int> source = new List<int>();

                    for (int i = 0; i < bundleValues.Length; i += 3)
                    {
                        int bundleValue = bundleValues[i].SafeParseInt32();
                        if (bundleValue != -1 &&
                            !_communityCenter.bundles[bundleNumber][i / 3])
                        {
                            source.Add(bundleValue);
                        }
                    }

                    if (source.Count > 0)
                        _prunedRequiredBundles.Add(bundleName, source);
                }
            }
        }

        private void DrawAdvancedTooltip()
        {
            if (_hoverItem != null &&
                _hoverItem.Name != "Scythe" &&
                !(_hoverItem is StardewValley.Tools.FishingRod))
            {
                //String text = string.Empty;
                //String extra = string.Empty;
                int truePrice = Tools.GetTruePrice(_hoverItem);
                int itemPrice = 0;
                int stackPrice = 0;

                if (truePrice > 0)
                {
                    itemPrice = truePrice / 2;
                    //int width = (int)Game1.smallFont.MeasureString(" ").Length();
                    //int numberOfSpaces = 46 / ((int)Game1.smallFont.MeasureString(" ").Length()) + 1;
                    //StringBuilder spaces = new StringBuilder();
                    //for (int i = 0; i < numberOfSpaces; ++i)
                    //{
                    //    spaces.Append(" ");
                    //}
                    //text = "\n" + spaces.ToString() + (truePrice / 2);
                    if (_hoverItem.getStack() > 1)
                    {
                        stackPrice = (itemPrice * _hoverItem.getStack());
                        //text += " (" + (truePrice / 2 * _hoverItem.getStack()) + ")";
                    }
                }
                int cropPrice = 0;

                //bool flag = false;
                if (_hoverItem is StardewValley.Object &&
                    (_hoverItem as StardewValley.Object).Type == "Seeds" &&
                    itemPrice > 0 &&
                    (_hoverItem.Name != "Mixed Seeds" ||
                    _hoverItem.Name != "Winter Seeds"))
                {
                    StardewValley.Object itemObject = new StardewValley.Object(new Debris(new Crop(_hoverItem.ParentSheetIndex, 0, 0).indexOfHarvest.Value, Game1.player.position, Game1.player.position).chunkType.Value, 1);
                    //extra += "    " + itemObject.Price;
                    cropPrice = itemObject.Price;
                    //flag = true;
                }

                //String hoverTile = _hoverItem.DisplayName + text + extra;
                //String description = _hoverItem.getDescription();
                //Vector2 vector2 = DrawTooltip(Game1.spriteBatch, _hoverItem.getDescription(), hoverTile, _hoverItem);
                //vector2.X += 30;
                //vector2.Y -= 10;

                String requiredBundleName = null;

                foreach (var requiredBundle in _prunedRequiredBundles)
                {
                    if (requiredBundle.Value.Contains(_hoverItem.ParentSheetIndex) &&
                        !_hoverItem.Name.Contains("arecrow") &&
                        _hoverItem.Name != "Chest" &&
                        _hoverItem.Name != "Recycling Machine" &&
                        _hoverItem.Name != "Solid Gold Lewis")
                    {
                        requiredBundleName = requiredBundle.Key;
                        break;
                    }
                }

                int largestTextWidth = 0;
                int stackTextWidth = (int)(Game1.smallFont.MeasureString(stackPrice.ToString()).Length());
                int itemTextWidth = (int)(Game1.smallFont.MeasureString(itemPrice.ToString()).Length());
                largestTextWidth = (stackTextWidth > itemTextWidth) ? stackTextWidth : itemTextWidth;
                int windowWidth = Math.Max(largestTextWidth + 90, String.IsNullOrEmpty(requiredBundleName) ? 100 : 300);

                int windowHeight = 75;

                if (stackPrice > 0)
                    windowHeight += 40;

                if (cropPrice > 0)
                    windowHeight += 40;

                int windowY = Game1.getMouseY() + 20;

                windowY = Game1.viewport.Height - windowHeight - windowY < 0 ? Game1.viewport.Height - windowHeight : windowY;

                int windowX = Game1.getMouseX() - windowWidth - 25;

                if (Game1.getMouseX() > Game1.viewport.Width - 300)
                {
                    windowX = Game1.viewport.Width - windowWidth - 350;
                }
                else if (windowX < 0)
                {
                    windowX = Game1.getMouseX() + 350;
                }

                Vector2 windowPos = new Vector2(windowX, windowY);
                Vector2 currentDrawPos = new Vector2(windowPos.X + 30, windowPos.Y + 40);


                if (itemPrice > 0)
                {


                    IClickableMenu.drawTextureBox(
                        Game1.spriteBatch,
                        Game1.menuTexture,
                        new Rectangle(0, 256, 60, 60),
                        (int)windowPos.X,
                        (int)windowPos.Y,
                        windowWidth,
                        windowHeight,
                        Color.White);

                    Game1.spriteBatch.Draw(
                        Game1.debrisSpriteSheet,
                        new Vector2(currentDrawPos.X, currentDrawPos.Y + 4),
                        Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16),
                        Color.White,
                        0,
                        new Vector2(8, 8),
                        Game1.pixelZoom,
                        SpriteEffects.None,
                        0.95f);

                    Game1.spriteBatch.DrawString(
                        Game1.smallFont,
                        itemPrice.ToString(),
                        new Vector2(currentDrawPos.X + 22, currentDrawPos.Y - 8),
                        Game1.textShadowColor);

                    Game1.spriteBatch.DrawString(
                        Game1.smallFont,
                        itemPrice.ToString(),
                        new Vector2(currentDrawPos.X + 20, currentDrawPos.Y - 10),
                        Game1.textColor);

                    currentDrawPos.Y += 40;

                    if (stackPrice > 0)
                    {
                        Game1.spriteBatch.Draw(
                            Game1.debrisSpriteSheet,
                            new Vector2(currentDrawPos.X, currentDrawPos.Y),
                            Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16),
                            Color.White,
                            0,
                            new Vector2(8, 8),
                            Game1.pixelZoom,
                            SpriteEffects.None,
                            0.95f);

                        Game1.spriteBatch.Draw(
                            Game1.debrisSpriteSheet,
                            new Vector2(currentDrawPos.X, currentDrawPos.Y + 10),
                            Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16),
                            Color.White,
                            0,
                            new Vector2(8, 8),
                            Game1.pixelZoom,
                            SpriteEffects.None,
                            0.95f);

                        Game1.spriteBatch.DrawString(
                            Game1.smallFont,
                            stackPrice.ToString(),
                            new Vector2(currentDrawPos.X + 22, currentDrawPos.Y - 8),
                            Game1.textShadowColor);

                        Game1.spriteBatch.DrawString(
                            Game1.smallFont,
                            stackPrice.ToString(),
                            new Vector2(currentDrawPos.X + 20, currentDrawPos.Y - 10),
                            Game1.textColor);

                        currentDrawPos.Y += 40;
                    }

                    //Game1.spriteBatch.Draw(
                    //    Game1.debrisSpriteSheet,
                    //    new Vector2(vector2.X, vector2.Y),
                    //    Game1.getSourceRectForStandardTileSheet(Game1.debrisSpriteSheet, 8, 16, 16),
                    //    Color.White,
                    //    0,
                    //    new Vector2(8, 8),
                    //    Game1.pixelZoom,
                    //    SpriteEffects.None,
                    //    0.95f);

                    if (cropPrice > 0)
                    {
                        //Game1.spriteBatch.Draw(
                        //    Game1.mouseCursors, new Vector2(vector2.X + Game1.dialogueFont.MeasureString(text).X - 10.0f, vector2.Y - 20f),
                        //    new Rectangle(60, 428, 10, 10),
                        //    Color.White,
                        //    0.0f,
                        //    Vector2.Zero,
                        //    Game1.pixelZoom,
                        //    SpriteEffects.None,
                        //    0.85f);

                        Game1.spriteBatch.Draw(
                            Game1.mouseCursors,
                            new Vector2(currentDrawPos.X - 15, currentDrawPos.Y - 10),
                            new Rectangle(60, 428, 10, 10),
                            Color.White,
                            0.0f,
                            Vector2.Zero,
                            Game1.pixelZoom * 0.75f,
                            SpriteEffects.None,
                            0.85f);

                        Game1.spriteBatch.DrawString(
                            Game1.smallFont,
                            cropPrice.ToString(),
                            new Vector2(currentDrawPos.X + 22, currentDrawPos.Y - 8),
                            Game1.textShadowColor);

                        Game1.spriteBatch.DrawString(
                            Game1.smallFont,
                            cropPrice.ToString(),
                            new Vector2(currentDrawPos.X + 20, currentDrawPos.Y - 10),
                            Game1.textColor);
                    }
                }

                if (!String.IsNullOrEmpty(requiredBundleName))
                {
                    int num1 = (int)windowPos.X - 30;
                    int num2 = (int)windowPos.Y - 10;
                    int num3 = num1 + 52;
                    int y3 = num2 - 2;
                    int num4 = 288;
                    int height = 36;
                    int num5 = 36;
                    int width = num4 / num5;
                    int num6 = 6;

                    for (int i = 0; i < 36; ++i)
                    {
                        float num7 = (float)(i >= num6 ? 0.92 - (i - num6) * (1.0 / (num5 - num6)) : 0.92f);
                        Game1.spriteBatch.Draw(
                            Game1.staminaRect,
                            new Rectangle(num3 + width * i, y3, width, height),
                            Color.Crimson * num7);
                    }

                    Game1.spriteBatch.DrawString(
                        Game1.dialogueFont,
                        requiredBundleName,
                        new Vector2(num1 + 72, num2),
                        Color.White);

                    _bundleIcon.bounds.X = num1 + 16;
                    _bundleIcon.bounds.Y = num2;
                    _bundleIcon.scale = 3;
                    _bundleIcon.draw(Game1.spriteBatch);
                }
                //RestoreMenuState();
            }
        }

        private void RestoreMenuState()
        {
            if (Game1.activeClickableMenu is ItemGrabMenu)
            {
                (Game1.activeClickableMenu as MenuWithInventory).hoveredItem = _hoverItem;
            }
        }


        private static Vector2 DrawTooltip(SpriteBatch batch, String hoverText, String hoverTitle, Item hoveredItem)
        {
            bool flag = hoveredItem != null &&
                hoveredItem is StardewValley.Object &&
                (hoveredItem as StardewValley.Object).Edibility != -300;

            int healAmmountToDisplay = flag ? (hoveredItem as StardewValley.Object).Edibility : -1;
            string[] buffIconsToDisplay = null;
            if (flag)
            {
                String objectInfo = Game1.objectInformation[(hoveredItem as StardewValley.Object).ParentSheetIndex];
                if (Game1.objectInformation[(hoveredItem as StardewValley.Object).ParentSheetIndex].Split('/').Length >= 7)
                {
                    buffIconsToDisplay = Game1.objectInformation[(hoveredItem as StardewValley.Object).ParentSheetIndex].Split('/')[6].Split('^');
                }
            }

            return DrawHoverText(batch, hoverText, Game1.smallFont, -1, -1, -1, hoverTitle, -1, buffIconsToDisplay, hoveredItem);
        }

        private static Vector2 DrawHoverText(SpriteBatch batch, String text, SpriteFont font, int xOffset = 0, int yOffset = 0, int moneyAmountToDisplayAtBottom = -1, String boldTitleText = null, int healAmountToDisplay = -1, string[] buffIconsToDisplay = null, Item hoveredItem = null)
        {
            Vector2 result = Vector2.Zero;

            if (String.IsNullOrEmpty(text))
            {
                result = Vector2.Zero;
            }
            else
            {
                if (String.IsNullOrEmpty(boldTitleText))
                    boldTitleText = null;

                int num1 = 20;
                int infoWindowWidth = (int)Math.Max(healAmountToDisplay != -1 ? font.MeasureString(healAmountToDisplay.ToString() + "+ Energy" + (Game1.tileSize / 2)).X : 0, Math.Max(font.MeasureString(text).X, boldTitleText != null ? Game1.dialogueFont.MeasureString(boldTitleText).X : 0)) + Game1.tileSize / 2;
                int extraInfoBackgroundHeight = (int)Math.Max(
                    num1 * 3,
                    font.MeasureString(text).Y + Game1.tileSize / 2 + (moneyAmountToDisplayAtBottom > -1 ? (font.MeasureString(string.Concat(moneyAmountToDisplayAtBottom)).Y + 4.0) : 0) + (boldTitleText != null ? Game1.dialogueFont.MeasureString(boldTitleText).Y + (Game1.tileSize / 4) : 0) + (healAmountToDisplay != -1 ? 38 : 0));
                if (buffIconsToDisplay != null)
                {
                    for (int i = 0; i < buffIconsToDisplay.Length; ++i)
                    {
                        if (!buffIconsToDisplay[i].Equals("0"))
                            extraInfoBackgroundHeight += 34;
                    }
                    extraInfoBackgroundHeight += 4;
                }

                String categoryName = null;
                if (hoveredItem != null)
                {
                    extraInfoBackgroundHeight += (Game1.tileSize + 4) * hoveredItem.attachmentSlots();
                    categoryName = hoveredItem.getCategoryName();
                    if (categoryName.Length > 0)
                        extraInfoBackgroundHeight += (int)font.MeasureString("T").Y;

                    if (hoveredItem is MeleeWeapon)
                    {
                        extraInfoBackgroundHeight = (int)(Math.Max(
                            num1 * 3,
                            (boldTitleText != null ?
                                Game1.dialogueFont.MeasureString(boldTitleText).Y + (Game1.tileSize / 4)
                                : 0) +
                            Game1.tileSize / 2) +
                            font.MeasureString("T").Y +
                            (moneyAmountToDisplayAtBottom > -1 ?
                                font.MeasureString(string.Concat(moneyAmountToDisplayAtBottom)).Y + 4.0
                                : 0) +
                            (hoveredItem as MeleeWeapon).getNumberOfDescriptionCategories() *
                            Game1.pixelZoom * 12 +
                            font.MeasureString(Game1.parseText((hoveredItem as MeleeWeapon).Description,
                            Game1.smallFont,
                            Game1.tileSize * 4 +
                            Game1.tileSize / 4)).Y);

                        infoWindowWidth = (int)Math.Max(infoWindowWidth, font.MeasureString("99-99 Damage").X + (15 * Game1.pixelZoom) + (Game1.tileSize / 2));
                    }
                    else if (hoveredItem is Boots)
                    {
                        Boots hoveredBoots = hoveredItem as Boots;
                        extraInfoBackgroundHeight = extraInfoBackgroundHeight - (int)font.MeasureString(text).Y + (int)(hoveredBoots.getNumberOfDescriptionCategories() * Game1.pixelZoom * 12 + font.MeasureString(Game1.parseText(hoveredBoots.description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4)).Y);
                        infoWindowWidth = (int)Math.Max(infoWindowWidth, font.MeasureString("99-99 Damage").X + (15 * Game1.pixelZoom) + (Game1.tileSize / 2));
                    }
                    else if (hoveredItem is StardewValley.Object &&
                        (hoveredItem as StardewValley.Object).Edibility != -300)
                    {
                        StardewValley.Object hoveredObject = hoveredItem as StardewValley.Object;
                        healAmountToDisplay = (int)Math.Ceiling(hoveredObject.Edibility * 2.5) + hoveredObject.Quality * hoveredObject.Edibility;
                        extraInfoBackgroundHeight += (Game1.tileSize / 2 + Game1.pixelZoom * 2) * (healAmountToDisplay > 0 ? 2 : 1);
                    }
                }

                //Crafting ingredients were never used

                int xPos = Game1.getOldMouseX() + Game1.tileSize / 2 + xOffset;
                int yPos = Game1.getOldMouseY() + Game1.tileSize / 2 + yOffset;

                if (xPos + infoWindowWidth > Game1.viewport.Width)
                {
                    xPos = Game1.viewport.Width - infoWindowWidth;
                    yPos += Game1.tileSize / 4;
                }

                if (yPos + extraInfoBackgroundHeight > Game1.viewport.Height)
                {
                    xPos += Game1.tileSize / 4;
                    yPos = Game1.viewport.Height - extraInfoBackgroundHeight;
                }
                int hoveredItemHeight = (int)(hoveredItem == null || categoryName.Length <= 0 ? 0 : font.MeasureString("asd").Y);

                IClickableMenu.drawTextureBox(
                    batch,
                    Game1.menuTexture,
                    new Rectangle(0, 256, 60, 60),
                    xPos,
                    yPos,
                    infoWindowWidth,
                    extraInfoBackgroundHeight,
                    Color.White);

                if (boldTitleText != null)
                {
                    IClickableMenu.drawTextureBox(
                        batch,
                        Game1.menuTexture,
                        new Rectangle(0, 256, 60, 60),
                        xPos,
                        yPos,
                        infoWindowWidth,
                        (int)(Game1.dialogueFont.MeasureString(boldTitleText).Y + Game1.tileSize / 2 + hoveredItemHeight - Game1.pixelZoom),
                        Color.White,
                        1,
                        false);

                    batch.Draw(
                        Game1.menuTexture,
                        new Rectangle(xPos + Game1.pixelZoom * 3, yPos + (int)Game1.dialogueFont.MeasureString(boldTitleText).Y + Game1.tileSize / 2 + hoveredItemHeight - Game1.pixelZoom, infoWindowWidth - Game1.pixelZoom * 6, Game1.pixelZoom),
                        new Rectangle(44, 300, 4, 4),
                        Color.White);

                    batch.DrawString(
                        Game1.dialogueFont,
                        boldTitleText,
                        new Vector2(xPos + Game1.tileSize / 4, yPos + Game1.tileSize / 4 + 4) + new Vector2(2, 2),
                        Game1.textShadowColor);

                    batch.DrawString(
                        Game1.dialogueFont,
                        boldTitleText,
                        new Vector2(xPos + Game1.tileSize / 4, yPos + Game1.tileSize / 4 + 4) + new Vector2(0, 2),
                        Game1.textShadowColor);

                    batch.DrawString(
                        Game1.dialogueFont,
                        boldTitleText,
                        new Vector2(xPos + Game1.tileSize / 4, yPos + Game1.tileSize / 4 + 4),
                        Game1.textColor);

                    yPos += (int)Game1.dialogueFont.MeasureString(boldTitleText).Y;
                }

                int yPositionToReturn = yPos;
                if (hoveredItem != null && categoryName.Length > 0)
                {
                    yPos -= 4;
                    Utility.drawTextWithShadow(
                        batch,
                        categoryName,
                        font,
                        new Vector2(xPos + Game1.tileSize / 4, yPos + Game1.tileSize / 4 + 4),
                        hoveredItem.getCategoryColor(),
                        1,
                        -1,
                        2,
                        2);
                    yPos += (int)(font.MeasureString("T").Y + (boldTitleText != null ? Game1.tileSize / 4 : 0) + Game1.pixelZoom);
                }
                else
                {
                    yPos += (boldTitleText != null ? Game1.tileSize / 4 : 0);
                }

                if (hoveredItem is Boots)
                {
                    Boots boots = hoveredItem as Boots;
                    Utility.drawTextWithShadow(
                        batch,
                        Game1.parseText(
                            boots.description,
                            Game1.smallFont,
                            Game1.tileSize * 4 + Game1.tileSize / 4),
                        font,
                        new Vector2(xPos + Game1.tileSize / 4, yPos + Game1.tileSize / 4 + 4),
                        Game1.textColor);

                    yPos += (int)font.MeasureString(
                        Game1.parseText(
                            boots.description,
                            Game1.smallFont,
                            Game1.tileSize * 4 + Game1.tileSize / 4)).Y;

                    if (boots.defenseBonus.Value > 0)
                    {
                        Utility.drawWithShadow(
                            batch,
                            Game1.mouseCursors,
                            new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom, yPos + Game1.tileSize / 4 + 4),
                            new Rectangle(110, 428, 10, 10),
                            Color.White,
                            0,
                            Vector2.Zero,
                            Game1.pixelZoom);

                        Utility.drawTextWithShadow(
                            batch,
                            Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", new object[] { boots.defenseBonus.Value }),
                            font,
                            new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom * 13, yPos + Game1.tileSize / 4 + Game1.pixelZoom * 3),
                            Game1.textColor * 0.9f);
                        yPos += (int)Math.Max(font.MeasureString("TT").Y, 12 * Game1.pixelZoom);
                    }

                    if (boots.immunityBonus.Value > 0)
                    {
                        Utility.drawWithShadow(
                            batch,
                            Game1.mouseCursors,
                            new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom, yPos + Game1.tileSize / 4 + 4),
                            new Rectangle(150, 428, 10, 10),
                            Color.White,
                            0,
                            Vector2.Zero,
                            Game1.pixelZoom);
                        Utility.drawTextWithShadow(
                            batch,
                            Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", new object[] { boots.immunityBonus.Value }),
                            font,
                            new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom * 13, yPos + Game1.tileSize / 4 + Game1.pixelZoom * 3),
                            Game1.textColor * 0.9f);

                        yPos += (int)Math.Max(font.MeasureString("TT").Y, 12 * Game1.pixelZoom);
                    }
                }
                else if (hoveredItem is MeleeWeapon)
                {
                    MeleeWeapon meleeWeapon = hoveredItem as MeleeWeapon;
                    Utility.drawTextWithShadow(
                        batch,
                        Game1.parseText(meleeWeapon.Description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4),
                        font,
                        new Vector2(xPos + Game1.tileSize / 4, yPos + Game1.tileSize / 4 + 4),
                        Game1.textColor);
                    yPos += (int)font.MeasureString(Game1.parseText(meleeWeapon.Description, Game1.smallFont, Game1.tileSize * 4 + Game1.tileSize / 4)).Y;

                    if ((meleeWeapon as Tool).IndexOfMenuItemView != 47)
                    {
                        Utility.drawWithShadow(
                            batch,
                            Game1.mouseCursors,
                            new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom, yPos + Game1.tileSize / 4 + 4),
                            new Rectangle(120, 428, 10, 10),
                            Color.White,
                            0,
                            Vector2.Zero,
                            Game1.pixelZoom);

                        Utility.drawTextWithShadow(
                            batch,
                            Game1.content.LoadString("Strings\\UI:ItemHover_Damage", new object[] { meleeWeapon.minDamage.Value, meleeWeapon.maxDamage.Value }),
                            font,
                            new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom * 13, yPos + Game1.tileSize / 4 + Game1.pixelZoom * 3),
                            Game1.textColor * 0.9f);
                        yPos += (int)Math.Max(font.MeasureString("TT").Y, 12 * Game1.pixelZoom);

                        if (meleeWeapon.speed.Value != (meleeWeapon.type.Value == 2 ? -8 : 0))
                        {
                            Utility.drawWithShadow(
                                batch,
                                Game1.mouseCursors,
                                new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom, yPos + Game1.tileSize / 4 + 4),
                                new Rectangle(130, 428, 10, 10),
                                Color.White,
                                0,
                                Vector2.Zero,
                                Game1.pixelZoom,
                                false,
                                1);
                            bool flag = meleeWeapon.type.Value == 2 ? meleeWeapon.speed.Value < -8 : meleeWeapon.speed.Value < 0;
                            String speedText = ((meleeWeapon.type.Value == 2 ? meleeWeapon.speed.Value + 8 : meleeWeapon.speed.Value) / 2).ToString();
                            Utility.drawTextWithShadow(
                                batch,
                                Game1.content.LoadString("Strings\\UI:ItemHover_Speed", new object[] { (meleeWeapon.speed.Value > 0 ? "+" : "") + speedText }),
                                font,
                                new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom * 13, yPos + Game1.tileSize / 4 + Game1.pixelZoom * 3),
                                flag ? Color.DarkRed : Game1.textColor * 0.9f);
                            yPos += (int)Math.Max(font.MeasureString("TT").Y, 12 * Game1.pixelZoom);
                        }

                        if (meleeWeapon.addedDefense.Value > 0)
                        {
                            Utility.drawWithShadow(
                                batch,
                                Game1.mouseCursors,
                                new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom, yPos + Game1.tileSize / 4 + 4),
                                new Rectangle(110, 428, 10, 10),
                                Color.White,
                                0.0f,
                                Vector2.Zero,
                                Game1.pixelZoom,
                                false,
                                1f);
                            Utility.drawTextWithShadow(
                                batch,
                                Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", new object[] { meleeWeapon.addedDefense.Value }),
                                font,
                                new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom * 13, yPos + Game1.tileSize / 4 + Game1.pixelZoom * 3),
                                Game1.textColor * 0.9f);
                            yPos += (int)Math.Max(font.MeasureString("TT").Y, 12 * Game1.pixelZoom);
                        }

                        if (meleeWeapon.critChance.Value / 0.02 >= 2.0)
                        {
                            Utility.drawWithShadow(
                                batch,
                                Game1.mouseCursors,
                                new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom, yPos + Game1.tileSize / 4 + 4),
                                new Rectangle(40, 428, 10, 10),
                                Color.White,
                                0.0f,
                                Vector2.Zero,
                                Game1.pixelZoom,
                                false,
                                1f);
                            Utility.drawTextWithShadow(
                                batch, Game1.content.LoadString("Strings\\UI:ItemHover_CritChanceBonus", new object[] { meleeWeapon.critChance.Value / 0.02 }),
                                font,
                                new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom * 13, yPos + Game1.tileSize / 4 + Game1.pixelZoom * 3),
                                Game1.textColor * 0.9f);
                            yPos += (int)Math.Max(font.MeasureString("TT").Y, 12 * Game1.pixelZoom);
                        }

                        if (((double)meleeWeapon.critMultiplier.Value - 3.0) / 0.02 >= 1.0)
                        {
                            Utility.drawWithShadow(
                                batch,
                                Game1.mouseCursors,
                                new Vector2(xPos + Game1.tileSize / 4, yPos + Game1.tileSize / 4 + 4),
                                new Rectangle(160, 428, 10, 10),
                                Color.White,
                                0.0f,
                                Vector2.Zero,
                                Game1.pixelZoom,
                                false,
                                1f);

                            Utility.drawTextWithShadow(
                                batch, Game1.content.LoadString("Strings\\UI:ItemHover_CritPowerBonus", new object[] { (int)((meleeWeapon.critMultiplier.Value - 3.0) / 0.02) }),
                                font,
                                new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom * 11, yPos + Game1.tileSize / 4 + Game1.pixelZoom * 3),
                                Game1.textColor * 0.9f);
                            yPos += (int)Math.Max(font.MeasureString("TT").Y, 12 * Game1.pixelZoom);
                        }

                        if (meleeWeapon.knockback.Value != meleeWeapon.defaultKnockBackForThisType(meleeWeapon.type.Value))
                        {
                            Utility.drawWithShadow(
                                batch,
                                Game1.mouseCursors,
                                new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom, yPos + Game1.tileSize / 4 + 4),
                                new Rectangle(70, 428, 10, 10),
                                Color.White,
                                0.0f,
                                Vector2.Zero, Game1.pixelZoom,
                                false,
                                1f);
                            Utility.drawTextWithShadow(
                                batch,
                                Game1.content.LoadString(
                                    "Strings\\UI:ItemHover_Weight",
                                    new object[] { meleeWeapon.knockback.Value > meleeWeapon.defaultKnockBackForThisType(meleeWeapon.type.Value) ? "+" : "" + Math.Ceiling(Math.Abs(meleeWeapon.knockback.Value - meleeWeapon.defaultKnockBackForThisType(meleeWeapon.type.Value) * 10.0)) }),
                                font,
                                new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom * 13, yPos + Game1.tileSize / 4 + Game1.pixelZoom * 3),
                                Game1.textColor * 0.9f);
                            yPos += (int)Math.Max(font.MeasureString("TT").Y, 12 * Game1.pixelZoom);
                        }
                    }

                }
                else if (text.Length > 1)
                {
                    int textXPos = xPos + Game1.tileSize / 4;
                    int textYPos = yPos + Game1.tileSize / 4 + 4;
                    batch.DrawString(
                        font,
                        text,
                        new Vector2(textXPos, textYPos) + new Vector2(2, 2),
                        Game1.textShadowColor);

                    batch.DrawString(
                        font,
                        text,
                        new Vector2(textXPos, textYPos) + new Vector2(0, 2),
                        Game1.textShadowColor);

                    batch.DrawString(
                        font,
                        text,
                        new Vector2(textXPos, textYPos) + new Vector2(2, 0),
                        Game1.textShadowColor);

                    batch.DrawString(
                        font,
                        text,
                        new Vector2(textXPos, textYPos),
                        Game1.textColor * 0.9f);

                    yPos += (int)font.MeasureString(text).Y + 4;
                }

                if (healAmountToDisplay != -1)
                {
                    Utility.drawWithShadow(
                        batch,
                        Game1.mouseCursors,
                        new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom, yPos + Game1.tileSize / 4),
                        new Rectangle(healAmountToDisplay < 0 ? 140 : 0, 428, 10, 10),
                        Color.White,
                        0.0f,
                        Vector2.Zero,
                        3f,
                        false,
                        0.95f);
                    Utility.drawTextWithShadow(
                        batch, Game1.content.LoadString("Strings\\UI:ItemHover_Energy", new object[] { ((healAmountToDisplay > 0 ? "+" : "") + healAmountToDisplay) }),
                        font,
                        new Vector2(xPos + Game1.tileSize / 4 + 34 + Game1.pixelZoom, yPos + Game1.tileSize / 4 + 8),
                        Game1.textColor);
                    yPos += 34;

                    if (healAmountToDisplay > 0)
                    {
                        Utility.drawWithShadow(
                            batch,
                            Game1.mouseCursors,
                            new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom, yPos + Game1.tileSize / 4),
                            new Rectangle(0, 438, 10, 10),
                            Color.White,
                            0,
                            Vector2.Zero,
                            3,
                            false,
                            0.95f);

                        Utility.drawTextWithShadow(
                            batch,
                            Game1.content.LoadString(
                                "Strings\\UI:ItemHover_Health",
                                new object[] { "+" + (healAmountToDisplay * 0.4) }),
                            font,
                            new Vector2(xPos + Game1.tileSize / 4 + 34 + Game1.pixelZoom, yPos + Game1.tileSize / 4 + 8),
                            Game1.textColor);

                        yPos += 34;
                    }
                }

                if (buffIconsToDisplay != null)
                {
                    for (int i = 0; i < buffIconsToDisplay.Length; ++i)
                    {
                        String buffIcon = buffIconsToDisplay[i];
                        if (buffIcon != "0")
                        {
                            Utility.drawWithShadow(
                                batch,
                                Game1.mouseCursors,
                                new Vector2(xPos + Game1.tileSize / 4 + Game1.pixelZoom, yPos + Game1.tileSize / 4),
                                new Rectangle(10 + i * 10, 428, 10, 10),
                                Color.White,
                                0, Vector2.Zero,
                                3,
                                false,
                                0.95f);

                            string textToDraw = (buffIcon.SafeParseInt32() > 0 ? "+" : string.Empty) + buffIcon + " ";

                            //if (i <= 10)
                            //    textToDraw = Game1.content.LoadString("Strings\\UI:ItemHover_Buff" + i, new object[] { textToDraw });

                            Utility.drawTextWithShadow(
                                batch,
                                textToDraw,
                                font,
                                new Vector2(xPos + Game1.tileSize / 4 + 34 + Game1.pixelZoom, yPos + Game1.tileSize / 4 + 8),
                                Game1.textColor);
                            yPos += 34;
                        }
                    }
                }

                if (hoveredItem != null &&
                    hoveredItem.attachmentSlots() > 0)
                {
                    yPos += 16;
                    hoveredItem.drawAttachments(batch, xPos + Game1.tileSize / 4, yPos);
                    if (moneyAmountToDisplayAtBottom > -1)
                        yPos += Game1.tileSize * hoveredItem.attachmentSlots();
                }

                if (moneyAmountToDisplayAtBottom > -1)
                {

                }

                result = new Vector2(xPos, yPositionToReturn);
            }

            return result;
        }
    }
#endif


#if false
using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

//Created by Musbah Sinno

//Resources:Got GetHoveredItemFromMenu and DrawHoverTextbox from a CJB mod and modified them to suit my needs.
//          They also inspired me to make GetHoveredItemFromToolbar, so thank you CJB
//https://github.com/CJBok/SDV-Mods/blob/master/CJBShowItemSellPrice/StardewCJB.cs

namespace StardewValleyBundleTooltips
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        //Needed to make sure essential variables are loaded before running what needs them
        bool isLoaded = false;

        // check if a mod is loaded
        bool isCJBSellItemPriceLoaded;

        private bool isUiInfoSuiteLoaded; //Check to see if UiInfoSuiteIsLoaded

        Item toolbarItem;
        List<int> itemsInBundles;
        Dictionary<int, int[][]> bundles;
        Dictionary<int, string[]> bundleNamesAndSubNames;

        string language = "";


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            isCJBSellItemPriceLoaded = this.Helper.ModRegistry.IsLoaded("CJBok.ShowItemSellPrice");
            isUiInfoSuiteLoaded = this.Helper.ModRegistry.IsLoaded("Cdaragorn.UiInfoSuite");

            //Events
            helper.Events.GameLoop.SaveLoaded += SaveEvents_AfterLoad;
            helper.Events.Display.RenderedHud += GraphicsEvents_OnPostRenderHudEvent;
            helper.Events.Display.RenderingHud += GraphicsEvents_OnPreRenderHudEvent;
            helper.Events.Display.RenderedActiveMenu += GraphicsEvents_OnPostRenderGuiEvent;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>The method invoked when the player presses a keyboard button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        /// 
        private void SaveEvents_AfterLoad(object sender, SaveLoadedEventArgs e)
        {
            //This will be filled with the itemIDs of every item in every bundle (for a fast search without details)
            itemsInBundles = new List<int>();
            bundles = getBundles();

            //remove duplicates
            itemsInBundles = new HashSet<int>(itemsInBundles).ToList();

            isLoaded = true;
        }

        private void GraphicsEvents_OnPreRenderHudEvent(object sender, RenderingHudEventArgs e)
        {
            //I have to get it on preRendering because it gets set to null post
            toolbarItem = GetHoveredItemFromToolbar();
        }

        private void GraphicsEvents_OnPostRenderHudEvent(object sender, RenderedHudEventArgs e)
        {
            if (isLoaded && !Game1.MasterPlayer.mailReceived.Contains("JojaMember") && Game1.activeClickableMenu == null && toolbarItem != null)
            {
                PopulateHoverTextBoxAndDraw(toolbarItem,true);
                toolbarItem = null;
            }
        }

        private void GraphicsEvents_OnPostRenderGuiEvent(object sender, RenderedActiveMenuEventArgs e)
        {
            if (isLoaded && !Game1.MasterPlayer.mailReceived.Contains("JojaMember") && Game1.activeClickableMenu != null)
            {
                Item item = this.GetHoveredItemFromMenu(Game1.activeClickableMenu);
                if (item != null)
                    PopulateHoverTextBoxAndDraw(item,false);
            }
        }

        private void PopulateHoverTextBoxAndDraw(Item item, bool isItFromToolbar)
        {
            StardewValley.Locations.CommunityCenter communityCenter = Game1.getLocationFromName("CommunityCenter") as StardewValley.Locations.CommunityCenter;

            List<int[]> itemInfo = new List<int[]>();
            Dictionary<string, List<string>> descriptions = new Dictionary<string, List<string>>();

            foreach (int itemInBundles in itemsInBundles)
            {
                if (item.ParentSheetIndex == itemInBundles)
                {
                    foreach (KeyValuePair<int, int[][]> bundle in bundles)
                    {
                        for (int i = 0; i < bundle.Value.Length; i++)
                        {
                            //Getting the item name because the bundle itself doesn't actually make sure that the correct item is being placed
                            //(parentSheetIndex of object can overlap with another item from another sheet)
                            string itemName = "";
                            if (Game1.objectInformation.ContainsKey(bundle.Value[i][0]))
                            {
                                if(language == "")
                                    itemName = Game1.objectInformation[bundle.Value[i][0]].Split('/')[0];
                                else
                                    itemName = Game1.objectInformation[bundle.Value[i][0]].Split('/')[4];
                            }

                            var isItemInBundleSlot = communityCenter.bundles[bundle.Key][bundle.Value[i][3]];
                            if ((item is StardewValley.Object) && item.Stack != 0 && bundle.Value[i] != null && bundle.Value[i][0] == item.ParentSheetIndex && itemName == item.DisplayName && bundle.Value[i][2] <= ((StardewValley.Object)item).Quality)
                            {
                                if(!isItemInBundleSlot)
                                {
                                    //Saving i to check if the items are the same or not later on
                                    itemInfo.Add(new int[] {bundle.Key,bundle.Value[i][1],i});
                                    descriptions[bundleNamesAndSubNames[bundle.Key][0]] = new List<string>();
                                }
                            }
                        }
                    }
                }
            }

            foreach (int[] info in itemInfo)
            {
                string bundleName = bundleNamesAndSubNames[info[0]][0];
                string bundleSubName = bundleNamesAndSubNames[info[0]][1];
                int quantity = info[1];

                descriptions[bundleName].Add(bundleSubName + " | Qty: " + quantity);
            }

            if (descriptions.Count > 0)
            {
                string tooltipText = "";
                int count = 0;

                foreach (KeyValuePair<string, List<string>> keyValuePair in descriptions)
                {
                    if (count > 0)
                        tooltipText += "\n";

                    tooltipText += keyValuePair.Key;
                    foreach(string value in keyValuePair.Value)
                    {
                        tooltipText += "\n    " + value;
                    }
                    count++;
                    
                }

                this.DrawHoverTextBox(Game1.smallFont, tooltipText, isItFromToolbar , item.Stack);
            }
        }

        private Item GetHoveredItemFromMenu(IClickableMenu menu)
        {
            // game menu
            if (menu is GameMenu gameMenu)
            {
                IClickableMenu page = this.Helper.Reflection.GetField<List<IClickableMenu>>(gameMenu, "pages").GetValue()[gameMenu.currentTab];
                if (page is InventoryPage)
                    return this.Helper.Reflection.GetField<Item>(page, "hoveredItem").GetValue();
            }
            // from inventory UI (so things like shops and so on)
            else if (menu is MenuWithInventory inventoryMenu)
            {
                return inventoryMenu.hoveredItem;
            }

            return null;
        }

        private Item GetHoveredItemFromToolbar()
        {
            foreach (IClickableMenu menu in Game1.onScreenMenus)
            {
                if (menu is Toolbar toolbar)
                {
                    return this.Helper.Reflection.GetField<Item>(menu, "hoverItem").GetValue();
                }
            }

            return null;
        }

        private void DrawHoverTextBox(SpriteFont font, string description, bool isItFromToolbar, int itemStack)
        {
            Vector2 stringLength = font.MeasureString(description);
            int width = (int)stringLength.X + Game1.tileSize / 2 + 40;
            int height = (int)stringLength.Y + Game1.tileSize / 3 + 5;

            int x = (int)(Mouse.GetState().X / Game1.options.zoomLevel) - Game1.tileSize / 2 - width;
            int y = (int)(Mouse.GetState().Y / Game1.options.zoomLevel) + Game1.tileSize / 2;

            //So that the tooltips don't overlap
            if ((isCJBSellItemPriceLoaded || isUiInfoSuiteLoaded) && !isItFromToolbar)
            {
                if (itemStack > 1)
                    y += 95;
                else
                    y += 55;
            }   

            if (x < 0)
                x = 0;

            if (y + height > Game1.graphics.GraphicsDevice.Viewport.Height)
                y = Game1.graphics.GraphicsDevice.Viewport.Height - height;

            IClickableMenu.drawTextureBox(Game1.spriteBatch, Game1.menuTexture, new Rectangle(0, 256, 60, 60), x, y, width, height, Color.White);
            Utility.drawTextWithShadow(Game1.spriteBatch, description, font, new Vector2(x + Game1.tileSize / 4, y + Game1.tileSize / 4), Game1.textColor);
        }

        private Dictionary<int, int[][]> getBundles()
        {
            switch (LocalizedContentManager.CurrentLanguageCode)
            {
                case LocalizedContentManager.LanguageCode.ja:
                    language = ".ja-JP";
                    break;
                case LocalizedContentManager.LanguageCode.ru:
                    language = ".ru-RU";
                    break;
                case LocalizedContentManager.LanguageCode.pt:
                    language = ".pt-BR";
                    break;
                case LocalizedContentManager.LanguageCode.es:
                    language = ".es-ES";
                    break;
                case LocalizedContentManager.LanguageCode.de:
                    language = ".de-DE";
                    break;
                case LocalizedContentManager.LanguageCode.zh:
                    language = ".zh-CN";
                    break;
            }

            Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Bundles" + language);

            Dictionary<int, int[][]> bundles = new Dictionary<int, int[][]>();
            bundleNamesAndSubNames = new Dictionary<int, string[]>();

            foreach (KeyValuePair<string, string> keyValuePair in dictionary)
            {
                //format of the values are itemID itemAmount itemQuality

                //if bundleIndex is between 23 and 26, then they're vault bundles so don't add to dictionary

                string[] split = keyValuePair.Key.Split('/');
                string bundleName = split[0];

                string bundleSubName;

                if (language == "")
                    bundleSubName = keyValuePair.Value.Split('/')[0];
                else
                    bundleSubName = keyValuePair.Value.Split('/')[4];

                int bundleIndex = Convert.ToInt32(split[1]);
                if (!(bundleIndex >= 23 && bundleIndex <= 26))
                {
                    //creating an array for the bundle names
                    string[] bundleNames = new string[] {bundleName,bundleSubName} ;

                    //creating an array of items[i][j] , i is the item index, j=0 itemId, j=1 itemAmount, j=2 itemQuality, j=3 order of the item for it's own bundle
                    string[] allItems = keyValuePair.Value.Split('/')[2].Split(' ');
                    int allItemsLength = allItems.Length / 3;
                    
                    int[][] items = new int[allItemsLength][];

                    int j = 0;
                    int i = 0;
                    while(j< allItemsLength)
                    {
                        items[j] = new int[4];
                        items[j][0] = Convert.ToInt32(allItems[0 + i]);
                        items[j][1] = Convert.ToInt32(allItems[1 + i]);
                        items[j][2] = Convert.ToInt32(allItems[2 + i]);
                        items[j][3] = i/3;

                        itemsInBundles.Add(items[j][0]);
                        i = i + 3;
                        j++;
                    }

                    bundles.Add(bundleIndex, items);
                    bundleNamesAndSubNames.Add(bundleIndex, bundleNames);
                }
            }

            return bundles;
        }
    }
}
#endif
