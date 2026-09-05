using OpenConfiguration;
using Vintagestory.API.Common;

namespace RPGDifficulty;

public class AgeStatusConfigurations
{
    public bool enableStatusIncreaseByAge = true;
    public int increaseStatsEveryWorldDays = 5;
    public double lifeStatsIncreaseEveryAge = 0.1;
    public double damageStatsIncreaseEveryAge = 0.1;
    public double lootStatsIncreaseEveryAge = 0.1;
    public double maximumLifeStatusIncreasedByAge = 2.0;
    public double maximumDamageStatusIncreasedByAge = 2.0;
    public double maximumLootStatusIncreasedByAge = 2.0;
    public double levelUPExperienceIncreaseEveryAge = 0.1;
}

public static partial class Configuration
{
    public static AgeStatusConfigurations StatusAge = new();

    private static void LoadStatusAge(ICoreAPI api)
        => StatusAge = ConfigManager.LoadModConfig<AgeStatusConfigurations>(api, "RPGDifficulty", "age", RPGDifficultyModSystem.Logger);

    public static int GetStatusByWorldAge(ICoreAPI serverAPI)
        => (int)serverAPI.World.Calendar.ElapsedDays / StatusAge.increaseStatsEveryWorldDays;
}
