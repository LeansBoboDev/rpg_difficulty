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

public static partial class Configuration
{
    public static StatusConfigurations Status = new();

    private static void LoadStatus(ICoreAPI api)
        => Status = ConfigManager.LoadModConfig<StatusConfigurations>(api, "RPGDifficulty", "base", RPGDifficultyModSystem.Logger, "rpgdifficulty:config/base.json");

    public static int GetStatusByWorldAge(ICoreAPI serverAPI)
        => (int)serverAPI.World.Calendar.ElapsedDays / Status.increaseStatsEveryWorldDays;
}
