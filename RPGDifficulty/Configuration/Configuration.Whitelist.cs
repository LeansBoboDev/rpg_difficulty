using System.Collections.Generic;
using OpenConfiguration;
using Vintagestory.API.Common;

namespace RPGDifficulty;

public class WhitelistConfigurations
{
    public bool enableWhitelist = false;
    public Dictionary<string, double> Distance = [];
    public Dictionary<string, double> Height = [];
    public Dictionary<string, double> Age = [];
}

public static partial class Configuration
{
    public static WhitelistConfigurations Whitelist = new();

    // Only used to pull enableWhitelist out of base.json without dragging the
    // Distance/Height/Age dictionaries (which live in their own files) into that file's backfill.
    private class WhitelistToggle { public bool enableWhitelist = false; }

    private static void LoadWhitelist(ICoreAPI api)
    {
        Whitelist = new WhitelistConfigurations
        {
            enableWhitelist = ConfigManager.LoadModConfig<WhitelistToggle>(api, "RPGDifficulty", "base", RPGDifficultyModSystem.Logger).enableWhitelist,
            Distance = ConfigManager.LoadModConfig<Dictionary<string, double>>(api, "RPGDifficulty", "whitelistdistance", RPGDifficultyModSystem.Logger, "rpgdifficulty:config/whitelistdistance.json"),
            Height = ConfigManager.LoadModConfig<Dictionary<string, double>>(api, "RPGDifficulty", "whitelistheight", RPGDifficultyModSystem.Logger, "rpgdifficulty:config/whitelistheight.json"),
            Age = ConfigManager.LoadModConfig<Dictionary<string, double>>(api, "RPGDifficulty", "whitelistage", RPGDifficultyModSystem.Logger, "rpgdifficulty:config/whitelistage.json"),
        };
    }
}
