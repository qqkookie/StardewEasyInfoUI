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

        /// <summary>Monster health bar: show health bar only for the monster which player scored enough kills.</summary>
        public bool RequireKillScore { get; set; }

        /// <summary>Monster health bar: Reversed color scheme: red to green</summary>
        public bool ReverseColorScheme { get; set; }

        /// <summary>Current location: show as popup message, instead of on screen display</summary>
        public bool ShowLocationPopUp { get; set; }

        /// <summary>Disable showing message of today's weather and luck.</summary>
        public bool DisableTodayMessage { get; set; }

        /// <summary>Show buried clay tile when Show Buried Items.</summary>
        public bool ShowBuriedClay { get; set; }

        /// <summary>Show mine ladder stone when Show Buried Items.</summary>
        public bool ShowMineLadder { get; set; } = true;

    }
}
