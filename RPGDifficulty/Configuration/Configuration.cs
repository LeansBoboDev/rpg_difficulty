using System.Collections.Generic;
using Vintagestory.API.Common;

namespace RPGDifficulty;

#pragma warning disable CA2211
public static partial class Configuration
{
    internal static void Load(ICoreAPI api)
    {
        LoadBase(api);
        LoadStatusVariation(api);
        LoadStatusHeight(api);
        LoadStatusDistance(api);
        LoadStatusAge(api);
        LoadSpawnConditions(api);
        LoadStatusRegion(api);
        LoadWhitelist(api);
        LoadBlacklist(api);
    }

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
            RPGDifficultyModSystem.Logger.LogDebug($"{entityCode} is on blacklist, ignoring stats {statType}");
            return false;
        }
        if (Whitelist.enableWhitelist)
        {
            if (whitelist.ContainsKey(entityCode))
            {
                RPGDifficultyModSystem.Logger.LogDebug($"{entityCode} is on whitelist, increasing stats {statType}");
                return true;
            }
            RPGDifficultyModSystem.Logger.LogDebug($"{entityCode} is not on whitelist, ignoring stats {statType}");
            return false;
        }
        return true;
    }
}
