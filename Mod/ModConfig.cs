using StardewModdingAPI;

namespace EasyUI
{
    class ModConfig
    {
        /*
        public string[] KeysForBarrelAndCropTimes { get; set; } = new string[]
        {
            SButton.LeftShift.ToString()
        };

        public bool CanRightClickForBarrelAndCropTimes { get; set; } = true;
        */

        /// <summary>Icon Luck of Day: Show HUD messages of today's weather and luck when new day begins.</summary>
        public bool ShowTodayMessage { get; set; } = true;

        /// <summary>Monster health bar: Show health bar only for the monster which player scored enough kills.</summary>
        public bool RequireKillScore { get; set; }

        /// <summary>Monster health bar: Reversed color scheme: red to green</summary>
        public bool ReverseColorScheme { get; set; }

        /// <summary>Show location name: Show location name as HUD message, instead of on screen display.</summary>
        public bool ShowLocationMessage { get; set; }

        /// <summary>Show Buried Items: Show buried clay tile.</summary>
        public bool ShowBuriedClay { get; set; }

        /// <summary>Show Buried Items: Show mine ladder stone.</summary>
        public bool ShowMineLadder { get; set; } = true;

    }
}
