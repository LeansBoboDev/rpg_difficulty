using OpenConfiguration;
using Vintagestory.API.Common;

namespace RPGDifficulty;

public class DistanceStatusConfigurations
{
    public bool enableStatusIncreaseByDistance = true;
    public int increaseStatsEveryDistance = 500;
    public double lifeStatsIncreaseEveryDistance = 0.03;
    public double damageStatsIncreaseEveryDistance = 0.03;
    public double lootStatsIncreaseEveryDistance = 0.03;
    public double maximumLifeStatusIncreasedByDistance = 10.0;
    public double maximumDamageStatusIncreasedByDistance = 10.0;
    public double maximumLootStatusIncreasedByDistance = 10.0;
    public double levelUPExperienceIncreaseEveryDistance = 0.1;
}

public static partial class Configuration
{
    public static DistanceStatusConfigurations StatusDistance = new();

    private static void LoadStatusDistance(ICoreAPI api)
        => StatusDistance = ConfigManager.LoadModConfig<DistanceStatusConfigurations>(api, "RPGDifficulty", "distance", RPGDifficultyModSystem.Logger);
}
