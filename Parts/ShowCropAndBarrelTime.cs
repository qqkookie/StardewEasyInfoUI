using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Objects;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace EasyInfoUI
{
    internal class ShowCropAndBarrelTime : IDisposable
    {
        // private readonly Dictionary<int, String> _indexOfCropNames = new Dictionary<int, string>();

        private Vector2 CurrentTile = Vector2.Zero;
        private string HoverText;

        private int[] WildCrops =  {    // object index of wild crops
            16, 18, 20, 22, 396, 398, 402, 404, 406, 408, 410, 412, 414, 416, 418 };

        static string[] TreeNames = {
            "Oak Tree/Maple Tree/Pine Tree/Coconut Tree/Mushroom Tree",     // en
             "樫の木/カエデ/松の木/ココナッツの木/キノコの木",                  // ja
            "Дуб/Клен/Cосну/Кокос/Грибное Дерево",                          // ru
            "橡树/枫树/松树/椰子树/蘑菇树",                                   // zh
            "Carvalho/Ácer/Pinheiro/Coqueiro/Árvore de Cogumelo",           // pt
            "Roble/Arce/Pino/Cocotero/Árbol de Setas",                      // es
            "Eiche/Ahornbaum/Kiefer/Kokosnussbaum/Pilzbaum",                // de
            "Oak Tree/Maple Tree/Pine Tree/Coconut Tree/Mushroom Tree",     // th Not Translated.
            "Chêne/Érable/Pin/Cocotier/Champignon arbre",                   // fr
            "떡갈 나무/단풍 나무/소나무/코코넛 나무/버섯 나무",                 // ko
            "Quercia/Acero/Pino/Palma da Cocco/Fungo Albero",               // it
            "Meşe Ağacı/Akçaağaç/Çam Ağacı/Hindistan Cevizi Ağacı/Mantar Ağacı",    // tr
            "Tölgyfa/Juharfa/Fenyőfa/Kókuszfa/Gombafa",                     // hu
        };

        internal ShowCropAndBarrelTime()
        {
        }

        internal void ToggleOption(bool showCropAndBarrelTimes)
        {
            ModEntry.Events.Display.RenderedWorld -= OnRenderedWorld;
            ModEntry.Events.Input.CursorMoved -= OnCursorMoved;

            if (showCropAndBarrelTimes)
            {
                ModEntry.Events.Display.RenderedWorld += OnRenderedWorld;
                ModEntry.Events.Input.CursorMoved += OnCursorMoved;
            }
        }

        public void Dispose()
        {
            ToggleOption(false);
        }

        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            string Trans(string key) => ModEntry.Translation.Get(key);

            if (!Context.IsPlayerFree || Game1.currentLocation == null
                || CurrentTile == Game1.currentCursorTile)
                return;

            HoverText = String.Empty;

            // get tile under cursor
            CurrentTile = Game1.currentCursorTile;

            StardewValley.Object obj = null;

            if (Game1.currentLocation.Objects != null)
                Game1.currentLocation.Objects.TryGetValue(CurrentTile, out obj);

            if (obj != null)
            {
                // machines and casks
                if (obj.bigCraftable.Value)
                {
                    if (obj.MinutesUntilReady > 0 && obj.heldObject.Value != null && obj.Name != "Heater")
                    {
                        HoverText = obj.heldObject.Value.DisplayName;

                        if (obj is Cask cask)
                        {
                            HoverText += ": " + String.Format(Trans(LanguageKeys.DaysToMature),
                                (int)(cask.daysToMature.Value / cask.agingRate.Value) + 0.5f);
                        }
                        else
                        {
                            int days = obj.MinutesUntilReady / 60 / 24;
                            int hours = obj.MinutesUntilReady / 60 % 24;
                            int minutes = obj.MinutesUntilReady % 60;

                            if (days > 0)
                                HoverText += $": {days} {Trans("label.days")} {(hours + minutes / 30)} {Trans("label.hours")}";
                            else if (hours > 0)
                                HoverText += $": {hours} {Trans("label.hours")} {minutes} {Trans("label.minutes")}";
                            else
                                HoverText += $": {minutes} {Trans("label.minutes")}";
                        }
                    }
                }
                else if (obj.Type.Equals("Basic") && Array.IndexOf(this.WildCrops, obj.ParentSheetIndex) >= 0)
                {
                    HoverText = obj.DisplayName;
                }
                else if (obj is IndoorPot pot1 && pot1.hoeDirt.Value.crop != null)
                {
                    HoverText = this.CropIndexToName(pot1.hoeDirt.Value.crop.indexOfHarvest.Value); // maybe overwritten later
                }
                else
                    return;
            }

            TerrainFeature currentTerrain = null;

            if (obj is IndoorPot pot && pot.hoeDirt.Value != null)
                currentTerrain = pot.hoeDirt.Value;
            else if (Game1.currentLocation.terrainFeatures != null)
                Game1.currentLocation.terrainFeatures.TryGetValue(CurrentTile, out currentTerrain);

            if (currentTerrain != null)
            {
                if (currentTerrain is HoeDirt hoeDirt)
                {
                    if (hoeDirt.crop != null && !hoeDirt.crop.dead.Value)
                    {
                        Crop crop = hoeDirt.crop;
                        int crop_index = crop.isWildSeedCrop()
                            ? crop.whichForageCrop.Value : crop.indexOfHarvest.Value;

                        if (crop_index == 0)
                            return;

                        HoverText = this.CropIndexToName(crop_index) + ": ";

                        if (crop.fullyGrown.Value && crop.dayOfCurrentPhase.Value > 0)
                        {
                            HoverText += Trans(LanguageKeys.ReadyToHarvest);
                        }
                        else
                        {
                            int days = -crop.dayOfCurrentPhase.Value;

                            for (int i = crop.currentPhase.Value; i < crop.phaseDays.Count - 1; ++i)
                                days += crop.phaseDays[i];

                            HoverText += days + " " + Trans("label.days");
                        }

                        /*
                        // String hoverText = String.Empty;
                        //if ( _indexOfCropNames.TryGetValue(hoeDirt.crop.indexOfHarvest.Value, out string value))
                        //    hoverText = value;

                        //if (String.IsNullOrEmpty(hoverText))
                        {
                            hoverText = new StardewValley.Object(new Debris(hoeDirt.crop.indexOfHarvest.Value, Vector2.Zero, Vector2.Zero).chunkType.Value, 1).DisplayName;
                            _indexOfCropNames.Add(hoeDirt.crop.indexOfHarvest.Value, hoverText);
                        }
                        */
                    }
                }

                else if (currentTerrain is FruitTree fruit)
                {
                    HoverText = this.CropIndexToName(fruit.indexOfFruit.Value);
                    if (fruit.daysUntilMature.Value > 0)
                        HoverText += ": " + String.Format(Trans(LanguageKeys.DaysToMature), fruit.daysUntilMature.Value);

                    // var text = new StardewValley.Object(new Debris(fruittree.indexOfFruit.Value, Vector2.Zero, Vector2.Zero).chunkType.Value, 1).DisplayName;
                    // text += Environment.NewLine + tree.daysUntilMature.Value + " " + _helper.SafeGetString(LanguageKeys.DaysToMature);
                }

                else if (currentTerrain is Tree wildtree)
                {
                    int kind = -1;
                    switch (wildtree.treeType.Value)
                    {
                        case Tree.bushyTree: case Tree.winterTree1: kind = 0; break;
                        case Tree.leafyTree: case Tree.winterTree2: kind = 1; break;
                        case Tree.pineTree:             kind = 2; break;
                        case Tree.palmTree:             kind = 3; break;
                        case Tree.mushroomTree:         kind = 4; break;
                    }
                    if (kind >= 0)
                        HoverText = TreeNames[(int)LocalizedContentManager.CurrentLanguageCode].Split('/')[kind];
                }
                else
                    return;

            }

            // for mill
            else if (Game1.currentLocation is BuildableGameLocation buildableLocation
                && buildableLocation.getBuildingAt(CurrentTile) is Mill mill
                && mill.input.Value != null && !mill.input.Value.isEmpty())
            {
                foreach (var item in mill.input.Value.items)
                {
                    if (item != null && !String.IsNullOrEmpty(item.Name) && item.Stack > 0
                        && (item.Name == "Wheat" || item.Name == "Beet"))
                        HoverText += $"{item.Stack} {item.DisplayName.ToLower()}  ";
                }
            }
        }

        private string CropIndexToName(int objid)
        {
            return (Game1.objectInformation.TryGetValue(objid, out string objinfo))
                ? objinfo.Split('/')[4] : string.Empty;
        }

        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            var font = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh)
                    ? Game1.dialogueFont : Game1.smallFont;

            // draw hover tooltip
            if (!String.IsNullOrWhiteSpace(HoverText))
                IClickableMenu.drawHoverText(e.SpriteBatch, HoverText, font);
        }

    }
}
