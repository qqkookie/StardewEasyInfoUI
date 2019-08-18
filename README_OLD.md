# UI INFO SUITE 1.X by Cdaragorn

This is description on old pre-2.0 version of [UI Info Suits](https://www.nexusmods.com/stardewvalley/mods/1150)

Version ~1.7.23 uploaded on 10 January 2019 by Cdaragorn

## Intro

Provides helpful information about things around you designed to help you be aware of what's going on without feeling like you're cheating.

This mod is a rewrite of Demiacle's UiModSuite found [here.](https://www.nexusmods.com/stardewvalley/mods/1023/) I've rewritten it mainly to try to make it easier to deal with bugs and add future improvements, and because his no longer works.

I have the same basic goals with this mod as the original did. I don't want it to feel like it's cheating, but rather just giving information to help you know where things are around you and what you might want to do each day.

There is also a new tab added to the options menu that allows turning each individual mod on or off whenever you want.

I have also added basic support for all languages in the game now. Please keep in mind that I don't speak all these languages, so right now the translations have all come from Google translate. I'd appreciate any help putting together better translations for them. You can edit the language json files and play around with the translations. You can even load your changes while the game is running by typing reload_i18n into the SMAPI console window.

## Features

The current features include:
* Display an icon that represents the current days luck
* Display experience point gains
* Display a dynamic experience bar that changes based on your current tool or location
* Display more accurate heart levels
* Display more information on item mouse overs, including items that are still needed for bundles
* View calendar and quest billboard anywhere
* Display icons over animals that need petting
* Display crop type and days until harvest on mouseover
* Display icon when animal has item yield (milk, wool)
* Sprinkler, scarecrow, beehive and junimo hut ranges
* Display npc locations on map
* Skip the intro by pressing the Escape key
* Display an icon for Queen of Sauce when she is airing a recipe you don't already know
* Display an icon when Clint is upgrading one of your tools. Icon will tell you how long until the tool is finished and shows you which tool you are upgrading.

## Installation

1. Make sure you have downloaded and installed the latest version of SMAPI.﻿
2. Unzip the mod folder into Stardew Valley/Mods
3. Run the game using SMAPI

### Requirements

SMAPI 2.1 or newer

## Known Issues

* NPC Locations are not working on the beach. To get this feature working in each area I have to map the area to the minimap's dimensions which does take some time. I have this on my list to get done, but for now it will just show all NPC's in that area in the same spot.
* Resizing the game window while the mod's options page in the main menu is selected causes an exception and may cause a game crash. So far I only have reports of the game crashing in Mac OS, but the same exception is generated in windows and it causes the menu graphics to glitch a little. Just close the menu and reopen it to fix that. I'm not sure if I'll ever be able to fix this problem but I am continuing to look into it. The problem is I need a way to catch the resize event before the game does, which so far I have not been able to do. The easiest way to avoid this is to select one of the games normal tabs before resizing the window.

## Source code
I've put my code up on github if anyone would like to take a look at it and just in case I'm ever away when the mod stops working.

https://github.com/cdaragorn/Ui-Info-Suite

Please endorse if you like this mod. It really helps out the author

## Changelogs

* Version 1.6.3
  - Fixed null reference check
* Version 1.6.2
  - Fixed null reference exception
  - Fixed Chest being marked as part of the Ocean Fish community bundle
* Version 1.6.1
  - Removed unnecessary check
  - Reduced SMAPI requirement to 2.0
  - Changed .NET version to 4.5
* Version 1.6.0
  - Added range information for Junimo huts. Range is shown when hovering the mouse over a hut.
* Version 1.5.1
  - Fixed some problems with item values
  - Fixed an issue with Krobus's location on the map
  - Added hover information for Mill. You can now see how much wheat and beets you have milling right now.
* Version 1.5.0
  - Added icon when you're having Clint upgrade a tool for you. Tells you how long until the tool is finished. Icon will show the actual tool you are upgrading.
  - Made recipe icon disappear when you learn the recipe
* Version 1.4.0
  - Added sell price information for equipment and possibly some other items that were missing it before.
  - Improved Portuguese translation (Many thanks to Mazzons!)
  - Added language support for all the mod option text
* Version 1.3.5
  - Fixed Show NPC Locations not defaulting to on.
* Version 1.3.4
  - Added support for updated version messages in SMAPI
  - Added check for null file name when trying to save custom settings
* Version 1.3.3
  - Fixed bug in sound name when scrolling up on custom mod options page
  - Made custom mod options button show when showing menu backgrounds
* Version 1.3.2
  - Fixed mod breaking if player name has characters that are invalid for a file name in it.
* Version 1.3.1
  - Fixed issues with hardware cursor
  - Fixed item hover information not showing up when using calendar but disabling custom information
  - Changed NPC locations to default to on.
* Version 1.3.0
  - Added exact location of NPC's when in forest
  - Added hover information on map when you hover over NPC face. Helps when more than one are in the same location.
* Version 1.2.8
  - Fixed pet and milk icon positions over farm animals when outside.
* Version 1.2.7
  - Changed Item Hover Information to just add its own window instead of redrawing everything the game already draws for you.
* Version 1.2.6
  - Fixed Options page select sound playing even when options not showing
  - Added improved Chinese translations
* Version 1.2.5
  - Fixed Queen of Sauce icon showing up on first Wednesday
  - Changed language support to use SMAPI's new json files method
  - Added more logging
  - Changed method used to load LevelUp sound file
* Version 1.2.4
  - Added Queen of Sauce back in for Wednesdays.
* Version 1.2.3
  - Fixed Queen of Sauce icon adding the recipe to the player without watching the show.
  - Removed Queen of Sauce Rerun episode checks. There's no way to know what episode it will be without causing the player to learn the recipe.
* Version 1.2.2
  - Fixed "Harvest Price" text
  - Made birthday icon disappear after you give the birthday villager a gift
  - First attempt to fix cask times
* Version 1.2.1
  - Finished language support for all strings in this mod
  - Removed Gus icon. It wasn't what I thought it meant.
  - Fixed Rarecrow showing as part of a community center bundle
* Version 1.2.0
  - Fixed spring onions showing up as copper ore
  - Added icons for queen of sauce and Gus when they have recipes you don't know yet
* Version 1.1.3
  - Fixed showing community bundle on items that qualify.
* Version 1.1.2
  - Fixed language support
* Version 1.1.1
  - Fixed several map locations
  - Added () around stack value for clarity
  - Added first attempt at showing exact NPC locations on map. Only works for town right now.
* Version 1.1.0
  - Added language support
* Version 1.0.12
  - Improved item value draw position to account for different font sizes
  - Added hover information to fruit trees to show when they will mature
* Version 1.0.11
  - Fixed custom menu not resizing when zoom options changed.
  - Fixed Sebastian's room and the Saloon not existing as map locations.
* Version 1.0.10
  - Fixed null reference exception when using gamepad outside of menus.
* Version 1.0.9
  - Made ranges for scarecrow and sprinklers show range on all placed items as well as the currently held one.
  - Tried a better check for heaters in crop times.
* Version 1.0.8
  - Fixed heaters showing growth time
  - Attempt to fix some crops showing incorrect times
  - Added check for null
* Version 1.0.7
  - Fixed loading sound file for level up animation
* Version 1.0.6
  - Fixed display of positive speed modifier on weapons
* Version 1.0.5
  - Added new sound for level up and fixed level up animation
* Version 1.0.4
  - Made skipping the intro only happen if you press the Escape key
* Version 1.0.3
  - Added viewing range for Bee house
* Version 1.0.2
  - Removed debug output
  - Added controller support to new UI controls
* Version 1.0.1
  - Fixed not showing item info from items in toolbar
* Version 1.0
  - Initial version


-------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------

# UiModSuite Version 1.0 by Demiacle

This is description of [OLD UiModSuite Version 1.0](https://www.nexusmods.com/stardewvalley/mods/1023/) uploaded on 21 March 2017 by Demiacle.

## Intro

Improves and adds a huge amount of visual feedback, great mod for a first play through!

UiModSuite(Version 1) - The main idea here is to give useful feedback without going into the details of the games mechanics and without feeling like a cheat! I've seen a lot of mods that get too technical and here I will be avoiding that. Features include

## Features

* Display an icon that represents the current days luck
* Display experience point gains
* Display a dynamic experience bar that changes based on your current tool or location
* Display more accurate heart levels ( hearts are filled in based on how close you are to gaining one )
* Display more information on item mouseover, including items that are still needed for bundles
* View calendar and quest billboard anywhere
* Display icons over animals that need petting
* Display crop type and days until harvest on mouseover
* Display icon when animal has item yield ( milk/wool )
* Sprinkler and scarecrow range finder
* Display npcs locations on map

### Possible improvements - ON HOLD

Display list of gifts already given to NPCs and their likeness towards them

## Config

* **`keysForBarrelAndCropTimes`** : key press to display barrel and crop times
canRightClickForBarrelAndCropTimes: allow right click to display barrel and crop time

* **`Sprinkler`** : the range for sprinkler and scarecrows, defaults to the standard ranges but can be edited in case another mod alters this behavior
## Compatibility

No compatibility issues have been reported

Additionally if you wish to remove a feature, there is a menu tab added to the in game menu which will allow you to completely remove any feature you do not wish to use.
