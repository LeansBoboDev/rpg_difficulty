using System.Collections.Generic;
using OpenConfiguration;
using Vintagestory.API.Common;

namespace RPGDifficulty;

public class BlacklistConfigurations
{
    public bool enableBlacklist = true;
    public Dictionary<string, double> Distance = [];
    public Dictionary<string, double> Height = [];
    public Dictionary<string, double> Age = [];
}

public static partial class Configuration
{
    public static BlacklistConfigurations Blacklist = new();

    // Only used to pull enableBlacklist out of base.json without dragging the
    // Distance/Height/Age dictionaries (which live in their own files) into that file's backfill.
    private class BlacklistToggle { public bool enableBlacklist = true; }

    private static void LoadBlacklist(ICoreAPI api)
    {
        Blacklist = new BlacklistConfigurations
        {
            enableBlacklist = ConfigManager.LoadModConfig<BlacklistToggle>(api, "RPGDifficulty", "base", RPGDifficultyModSystem.Logger, "rpgdifficulty:config/base.json").enableBlacklist,
            Distance = ConfigManager.LoadModConfig<Dictionary<string, double>>(api, "RPGDifficulty", "blacklistdistance", RPGDifficultyModSystem.Logger, "rpgdifficulty:config/blacklistdistance.json"),
            Height = ConfigManager.LoadModConfig<Dictionary<string, double>>(api, "RPGDifficulty", "blacklistheight", RPGDifficultyModSystem.Logger, "rpgdifficulty:config/blacklistheight.json"),
            Age = ConfigManager.LoadModConfig<Dictionary<string, double>>(api, "RPGDifficulty", "blacklistage", RPGDifficultyModSystem.Logger, "rpgdifficulty:config/blacklistage.json"),
        };
    }
}
