using System.Collections.Generic;
using OpenConfiguration;
using Vintagestory.API.Common;

namespace RPGDifficulty;

public class SpawnCondition
{
    public string code = "";
    public long minimumDistanceToSpawn = -1;
    public long maximumDistanceToSpawn = -1;
    public long minimumHeightToSpawn = -1;
    public long maximumHeightToSpawn = -1;
    public long minimumAgeToSpawn = -1;
    public long maximumAgeToSpawn = -1;
    public bool ignoreConditionsForSpawnersAPI = false;
}

public class StatusConfigurations
{
    public double baseHarvest = 0.0;

    public bool enableStatusVariation = true;
    public double minimumVariableStatusAverage = 0.5;
    public double maxVariableStatusAverage = 1.5;

    public bool enableStatusIncreaseByHeight = true;
    public int increaseStatsEveryDownHeight = 10;
    public int baseStatusHeight = 60;
    public double lifeStatsIncreaseEveryHeight = 0.1;
    public double damageStatsIncreaseEveryHeight = 0.1;
    public double lootStatsIncreaseEveryHeight = 0.1;
    public double maximumLifeStatusIncreasedByHeight = 2.0;
    public double maximumDamageStatusIncreasedByHeight = 2.0;
    public double maximumLootStatusIncreasedByHeight = 2.0;
    public double levelUPExperienceIncreaseEveryHeight = 0.1;

    public bool enableStatusIncreaseByDistance = true;
    public int increaseStatsEveryDistance = 500;
    public double lifeStatsIncreaseEveryDistance = 0.1;
    public double damageStatsIncreaseEveryDistance = 0.1;
    public double lootStatsIncreaseEveryDistance = 0.1;
    public double maximumLifeStatusIncreasedByDistance = 10.0;
    public double maximumDamageStatusIncreasedByDistance = 10.0;
    public double maximumLootStatusIncreasedByDistance = 10.0;
    public double levelUPExperienceIncreaseEveryDistance = 0.1;

    public bool enableStatusIncreaseByAge = true;
    public int increaseStatsEveryWorldDays = 5;
    public double lifeStatsIncreaseEveryAge = 0.1;
    public double damageStatsIncreaseEveryAge = 0.1;
    public double lootStatsIncreaseEveryAge = 0.1;
    public double maximumLifeStatusIncreasedByAge = 2.0;
    public double maximumDamageStatusIncreasedByAge = 2.0;
    public double maximumLootStatusIncreasedByAge = 2.0;
    public double levelUPExperienceIncreaseEveryAge = 0.1;

    public List<SpawnCondition> entitySpawnConditions = [];
    public bool enableExtendedLog = true;
}

public class WhitelistConfigurations
{
    public bool enableWhitelist = false;
    public Dictionary<string, double> Distance = [];
    public Dictionary<string, double> Height = [];
    public Dictionary<string, double> Age = [];
}

public class BlacklistConfigurations
{
    public bool enableBlacklist = true;
    public Dictionary<string, double> Distance = [];
    public Dictionary<string, double> Height = [];
    public Dictionary<string, double> Age = [];
}

#pragma warning disable CA2211
public static class Configuration
{
    public static StatusConfigurations Status = new();
    public static WhitelistConfigurations Whitelist = new();
    public static BlacklistConfigurations Blacklist = new();

    // Only used to pull enableWhitelist/enableBlacklist out of base.json without dragging the
    // Distance/Height/Age dictionaries (which live in their own files) into that file's backfill.
    private class WhitelistToggle { public bool enableWhitelist = false; }
    private class BlacklistToggle { public bool enableBlacklist = true; }

    internal static void Load(ICoreAPI api)
    {
        ModLogger logger = new(api.Logger, "RPGDifficulty");

        Status = ConfigManager.LoadModConfig<StatusConfigurations>(api, "RPGDifficulty", "base", logger, "rpgdifficulty:config/base.json");

        Whitelist = new WhitelistConfigurations
        {
            enableWhitelist = ConfigManager.LoadModConfig<WhitelistToggle>(api, "RPGDifficulty", "base", logger, "rpgdifficulty:config/base.json").enableWhitelist,
            Distance = ConfigManager.LoadModConfig<Dictionary<string, double>>(api, "RPGDifficulty", "whitelistdistance", logger, "rpgdifficulty:config/whitelistdistance.json"),
            Height = ConfigManager.LoadModConfig<Dictionary<string, double>>(api, "RPGDifficulty", "whitelistheight", logger, "rpgdifficulty:config/whitelistheight.json"),
            Age = ConfigManager.LoadModConfig<Dictionary<string, double>>(api, "RPGDifficulty", "whitelistage", logger, "rpgdifficulty:config/whitelistage.json"),
        };

        Blacklist = new BlacklistConfigurations
        {
            enableBlacklist = ConfigManager.LoadModConfig<BlacklistToggle>(api, "RPGDifficulty", "base", logger, "rpgdifficulty:config/base.json").enableBlacklist,
            Distance = ConfigManager.LoadModConfig<Dictionary<string, double>>(api, "RPGDifficulty", "blacklistdistance", logger, "rpgdifficulty:config/blacklistdistance.json"),
            Height = ConfigManager.LoadModConfig<Dictionary<string, double>>(api, "RPGDifficulty", "blacklistheight", logger, "rpgdifficulty:config/blacklistheight.json"),
            Age = ConfigManager.LoadModConfig<Dictionary<string, double>>(api, "RPGDifficulty", "blacklistage", logger, "rpgdifficulty:config/blacklistage.json"),
        };
    }

    public static int GetStatusByWorldAge(ICoreAPI serverAPI)
        => (int)serverAPI.World.Calendar.ElapsedDays / Status.increaseStatsEveryWorldDays;

    /// Returns false for NO status increase, true for status increase
    public static bool BlackWhiteListCheckForDistance(string entityCode)
        => BlackWhiteListCheck(entityCode, Whitelist.Distance, Blacklist.Distance, "distance");

    /// Returns false for NO status increase, true for status increase
    public static bool BlackWhiteListCheckForHeight(string entityCode)
        => BlackWhiteListCheck(entityCode, Whitelist.Height, Blacklist.Height, "height");

    /// Returns false for NO status increase, true for status increase
    public static bool BlackWhiteListCheckForAge(string entityCode)
        => BlackWhiteListCheck(entityCode, Whitelist.Age, Blacklist.Age, "age");

    private static bool BlackWhiteListCheck(string entityCode, Dictionary<string, double> whitelist, Dictionary<string, double> blacklist, string statType)
    {
        if (Blacklist.enableBlacklist && blacklist.ContainsKey(entityCode))
        {
            Initialization.Logger.LogDebug($"{entityCode} is on blacklist, ignoring stats {statType}");
            return false;
        }
        if (Whitelist.enableWhitelist)
        {
            if (whitelist.ContainsKey(entityCode))
            {
                Initialization.Logger.LogDebug($"{entityCode} is on whitelist, increasing stats {statType}");
                return true;
            }
            Initialization.Logger.LogDebug($"{entityCode} is not on whitelist, ignoring stats {statType}");
            return false;
        }
        return true;
    }
}
