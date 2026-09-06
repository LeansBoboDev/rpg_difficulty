using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace RPGDifficulty;

public class RegionInfo
{
    public int RegionX { get; init; }
    public int RegionZ { get; init; }
    public int Level { get; init; }
    public bool IsInitialRegion { get; init; }
    public string RockType { get; init; }
    public string SurfaceType { get; init; }
}

public static class RegionAPI
{
    /// <summary>
    /// Fired when a player crosses into a different region (rock type or surface type boundary).
    /// </summary>
    public static event Action<IServerPlayer, RegionInfo, RegionInfo> OnPlayerEnterRegion;

    private static ICoreServerAPI _api;
    private static readonly Dictionary<string, (string rockType, string surfaceType)> _playerRegionKeys = new();

    internal static void Initialize(ICoreServerAPI api)
    {
        _api = api;
        api.Event.PlayerNowPlaying += OnPlayerJoin;
        api.Event.PlayerDisconnect += OnPlayerLeave;
        api.Event.RegisterGameTickListener(CheckPlayerRegions, Configuration.StatusRegion.regionCheckIntervalMs);
    }

    private static void OnPlayerJoin(IServerPlayer player)
    {
        double posX = player.Entity.Pos.X;
        double posZ = player.Entity.Pos.Z;
        string rockType = RegionSystem.GetRockTypeAtPosition(posX, posZ);
        string surfaceType = RegionSystem.GetSurfaceTypeAtPosition(posX, posZ);
        _playerRegionKeys[player.PlayerUID] = (rockType, surfaceType);
    }

    private static void OnPlayerLeave(IServerPlayer player)
    {
        _playerRegionKeys.Remove(player.PlayerUID);
    }

    private static void CheckPlayerRegions(float dt)
    {
        foreach (IPlayer playerBase in _api.World.AllOnlinePlayers)
        {
            if (playerBase is not IServerPlayer player) continue;
            if (player.Entity == null) continue;

            double posX = player.Entity.Pos.X;
            double posZ = player.Entity.Pos.Z;
            string newRockType = RegionSystem.GetRockTypeAtPosition(posX, posZ);
            string newSurfaceType = RegionSystem.GetSurfaceTypeAtPosition(posX, posZ);

            if (!_playerRegionKeys.TryGetValue(player.PlayerUID, out var current))
            {
                _playerRegionKeys[player.PlayerUID] = (newRockType, newSurfaceType);
                continue;
            }

            if (current.rockType == newRockType && current.surfaceType == newSurfaceType) continue;

            RegionInfo oldInfo = BuildRegionInfo(posX, posZ, current.rockType, current.surfaceType);
            RegionInfo newInfo = BuildRegionInfo(posX, posZ, newRockType, newSurfaceType);

            _playerRegionKeys[player.PlayerUID] = (newRockType, newSurfaceType);

            RPGDifficultyModSystem.Logger.LogDebug(
                $"[RegionAPI] {player.PlayerName} entered region rockType={newRockType} surfaceType={newSurfaceType} " +
                $"level={newInfo.Level} initialRegion={newInfo.IsInitialRegion}"
            );

            OnPlayerEnterRegion?.Invoke(player, oldInfo, newInfo);
        }
    }

    private static RegionInfo BuildRegionInfo(double posX, double posZ, string rockType, string surfaceType)
    {
        int regionSize = _api.World.BlockAccessor.RegionSize;
        int regionX = (int)(posX / regionSize);
        int regionZ = (int)(posZ / regionSize);

        bool isInitial = false;
        EntityPos spawnPos = _api.World.DefaultSpawnPosition;
        if (spawnPos != null)
        {
            double dx = posX - spawnPos.X;
            double dz = posZ - spawnPos.Z;
            isInitial = Math.Sqrt(dx * dx + dz * dz) <= Configuration.StatusRegion.initialRegionRadius;
        }

        return new RegionInfo
        {
            RegionX = regionX,
            RegionZ = regionZ,
            RockType = rockType,
            SurfaceType = surfaceType,
            Level = RegionSystem.GetRegionLevel(posX, posZ, rockType, surfaceType),
            IsInitialRegion = isInitial
        };
    }
}
