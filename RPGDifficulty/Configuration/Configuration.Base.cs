using OpenConfiguration;
using Vintagestory.API.Common;

namespace RPGDifficulty;

public class BaseConfigurations
{
    public bool enableExtendedLog = false;
}

public static partial class Configuration
{
    public static BaseConfigurations Base = new();

    private static void LoadBase(ICoreAPI api)
        => Base = ConfigManager.LoadModConfig<BaseConfigurations>(api, "RPGDifficulty", "base", RPGDifficultyModSystem.Logger);
}
