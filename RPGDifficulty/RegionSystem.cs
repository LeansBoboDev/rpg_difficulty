using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace RPGDifficulty;

public static class RegionSystem
{
    private static ICoreServerAPI _api;

    public static void Initialize(ICoreServerAPI api)
    {
        _api = api;
    }

    public static string GetRockTypeAtPosition(double posX, double posZ)
    {
        int x = (int)posX;
        int z = (int)posZ;
        int chunkSize = GlobalConstants.ChunkSize;

        IMapChunk mapChunk = _api.World.BlockAccessor.GetMapChunkAtBlockPos(new BlockPos(x, 0, z));
        if (mapChunk?.TopRockIdMap == null) return "unknown";

        int localX = GameMath.Mod(x, chunkSize);
        int localZ = GameMath.Mod(z, chunkSize);
        int rockBlockId = mapChunk.TopRockIdMap[localZ * chunkSize + localX];

        Block rockBlock = _api.World.GetBlock(rockBlockId);
        return rockBlock?.Code?.SecondCodePart() ?? "unknown";
    }

    public static string GetSurfaceTypeAtPosition(double posX, double posZ)
    {
        int x = (int)posX;
        int z = (int)posZ;
        int surfaceY = Math.Max(1, _api.World.BlockAccessor.GetTerrainMapheightAt(new BlockPos(x, 0, z)));

        ClimateCondition climate = _api.World.BlockAccessor.GetClimateAt(
            new BlockPos(x, surfaceY, z),
            EnumGetClimateMode.WorldGenValues
        );

        if (climate == null) return "unknown";

        if (surfaceY >= Configuration.StatusRegion.mountainAltitudeThreshold)
            return "mountain";

        // Thresholds from blocklayers.json worldgen:
        // L1 Soil: minFertility 0.22 → fertile ground (soil)
        // L1 Infertile Soil: maxFertility 0.25 → gravel (temp < 8) or sand (temp >= 8)
        if (climate.Fertility >= 0.22f)
            return "soil";
        if (climate.Temperature >= 8f)
            return "sand";
        return "gravel";
    }

    public static int GetRegionLevel(double posX, double posZ, string rockType = null, string surfaceType = null)
    {
        if (!Configuration.StatusRegion.enableRegionSystem) return 0;

        rockType ??= GetRockTypeAtPosition(posX, posZ);
        surfaceType ??= GetSurfaceTypeAtPosition(posX, posZ);

        int minLevel = Configuration.StatusRegion.regionMinLevel;
        int maxLevel = Configuration.StatusRegion.regionMaxLevel;
        bool isInitialRegion = false;
        double distanceFromSpawn = -1;

        EntityPos spawnPos = _api.World.DefaultSpawnPosition;
        if (spawnPos != null)
        {
            double dx = posX - spawnPos.X;
            double dz = posZ - spawnPos.Z;
            distanceFromSpawn = Math.Sqrt(dx * dx + dz * dz);

            if (distanceFromSpawn <= Configuration.StatusRegion.initialRegionRadius)
            {
                isInitialRegion = true;
                minLevel = Configuration.StatusRegion.initialRegionMinLevel;
                maxLevel = Configuration.StatusRegion.initialRegionMaxLevel;
            }
            else if (Configuration.StatusRegion.enableDistanceBands)
            {
                int band = (int)(distanceFromSpawn / Configuration.StatusRegion.distanceBandSizeInBlocks);
                minLevel += band * Configuration.StatusRegion.levelMinPerBand;
                maxLevel += band * Configuration.StatusRegion.levelMaxPerBand;

                int cap = Configuration.StatusRegion.distanceBandMaxLevelCap;
                if (cap >= 0)
                {
                    maxLevel = Math.Min(maxLevel, cap);
                    minLevel = Math.Min(minLevel, cap);
                }
            }
        }

        int regionX = (int)Math.Floor(posX / Configuration.StatusRegion.regionSizeInBlocks);
        int regionZ = (int)Math.Floor(posZ / Configuration.StatusRegion.regionSizeInBlocks);
        int seed = _api.World.Seed ^ (regionX * 123456789) ^ (regionZ * 987654321);
        int level = new Random(seed).Next(minLevel, maxLevel + 1);

        RPGDifficultyModSystem.Logger.LogDebug(
            $"[Region] Tile=({regionX},{regionZ}) RockType={rockType} SurfaceType={surfaceType} | " +
            $"DistanceFromSpawn={distanceFromSpawn:F0} | " +
            $"InitialRegion={isInitialRegion} | " +
            $"LevelRange=[{minLevel}-{maxLevel}] | " +
            $"Level={level}"
        );

        return level;
    }
}
