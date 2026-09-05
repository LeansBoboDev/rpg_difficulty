using OpenConfiguration;
using Vintagestory.API.Common;

namespace RPGDifficulty;

public class HeightStatusConfigurations
{
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
}

public static partial class Configuration
{
    public static HeightStatusConfigurations StatusHeight = new();

    private static void LoadStatusHeight(ICoreAPI api)
        => StatusHeight = ConfigManager.LoadModConfig<HeightStatusConfigurations>(api, "RPGDifficulty", "height", RPGDifficultyModSystem.Logger);
}
