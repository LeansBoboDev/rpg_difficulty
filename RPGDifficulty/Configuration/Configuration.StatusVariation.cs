using OpenConfiguration;
using Vintagestory.API.Common;

namespace RPGDifficulty;

public class StatusVariationConfigurations
{
    public bool enableStatusVariation = true;
    public double minimumVariableStatusAverage = 0.5;
    public double maxVariableStatusAverage = 1.5;
}

public static partial class Configuration
{
    public static StatusVariationConfigurations StatusVariation = new();

    private static void LoadStatusVariation(ICoreAPI api)
        => StatusVariation = ConfigManager.LoadModConfig<StatusVariationConfigurations>(api, "RPGDifficulty", "variation", RPGDifficultyModSystem.Logger);
}
