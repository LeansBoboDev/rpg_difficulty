using OpenConfiguration;
using Vintagestory.API.Common;

namespace RPGDifficulty;

public class StatusRegionConfigurations
{
    public bool enableRegionSystem = true;

    // Size in blocks of each region tile in the world
    public int regionSizeInBlocks = 512;

    // Difficulty level range for regular regions
    public int regionMinLevel = 0;
    public int regionMaxLevel = 15;

    // Per-stat modifier applied per level point in a region
    public double lifeModifierPerLevel = 0.05;
    public double damageModifierPerLevel = 0.05;
    public double lootModifierPerLevel = 0.05;
    public double levelUPExperienceModifierPerLevel = 0.03;

    // Interval in milliseconds to check if a player crossed into a new region
    public int regionCheckIntervalMs = 2000;

    // Altitude threshold in blocks above which terrain is classified as mountain
    public int mountainAltitudeThreshold = 160;

    // Spawn protection: radius in blocks where level is restricted
    public int initialRegionRadius = 3500;

    // Difficulty level range restricted for the initial/spawn region
    public int initialRegionMinLevel = 0;
    public int initialRegionMaxLevel = 15;

    // Distance bands: every distanceBandSizeInBlocks beyond spawn, level range shifts up
    public bool enableDistanceBands = false;
    public int distanceBandSizeInBlocks = 1000;
    public int levelMinPerBand = 0;
    public int levelMaxPerBand = 5;

    // Cap for distance band levels; -1 = disabled
    public int distanceBandMaxLevelCap = -1;
}

public static partial class Configuration
{
    public static StatusRegionConfigurations StatusRegion = new();

    private static void LoadStatusRegion(ICoreAPI api)
        => StatusRegion = ConfigManager.LoadModConfig<StatusRegionConfigurations>(api, "RPGDifficulty", "region", RPGDifficultyModSystem.Logger);
}
