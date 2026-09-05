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

public class SpawnConditionsConfigurations
{
    public List<SpawnCondition> entitySpawnConditions = [];
}

public static partial class Configuration
{
    public static SpawnConditionsConfigurations SpawnConditions = new();

    private static void LoadSpawnConditions(ICoreAPI api)
        => SpawnConditions = ConfigManager.LoadModConfig<SpawnConditionsConfigurations>(api, "RPGDifficulty", "spawnconditions", RPGDifficultyModSystem.Logger);
}
