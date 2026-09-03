using System;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using OpenConfiguration;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
namespace RPGDifficulty;

public class Initialization : ModSystem
{
    readonly Overwrite overwriter = new();
    static internal ICoreServerAPI serverAPI;
    internal static ModLogger Logger = ModLogger.None;
    public static EntityPos DefaultSpawnPosition { get; private set; }
    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        serverAPI = api;

        // Create the timer only with levelup compatibility
        if (api.ModLoader.IsModEnabled("levelup"))
        {
            Task.Run(() =>
            {
                Logger.Log("LevelUP is enabled, registering 'OnExperienceIncrease' event");
                LevelUP.Server.ExperienceEvents.OnExperienceIncrease += LevelUPOnExperienceIncrease;
                LevelUP.Server.LevelKnifeEvents.OnKnifeHarvested += OnKnifeHarvested;
            });
        }

        // Timer to get world spawn position
        {
            var timer = new System.Timers.Timer(200)
            {
                AutoReset = true,
                Enabled = true
            };
            timer.Elapsed += (_, _) =>
            {
                try
                {
                    DefaultSpawnPosition = api.World.DefaultSpawnPosition;
                    timer.Stop();
                    timer.Dispose();
                }
                catch (Exception) { }
            };
        }
    }

    private static void OnKnifeHarvested(IPlayer byPlayer, Entity harvestedEntity, ref float number)
    {
        // Check if player exist and options is enabled
        if (byPlayer == null || (Configuration.Status.lootStatsIncreaseEveryDistance == 0 && Configuration.Status.lootStatsIncreaseEveryHeight == 0 && Configuration.Status.lootStatsIncreaseEveryAge == 0)) return;

        // Get the final droprate
        float dropRate = (float)Configuration.Status.baseHarvest + (float)harvestedEntity.Attributes.GetDouble("RPGDifficultyLootStatsIncreaseDistance");
        dropRate += (float)harvestedEntity.Attributes.GetDouble("RPGDifficultyLootStatsIncreaseHeight");
        dropRate += (float)harvestedEntity.Attributes.GetDouble("RPGDifficultyLootStatsIncreaseAge");

        if (Configuration.Status.enableStatusVariation)
            dropRate *= (float)harvestedEntity.Attributes.GetDouble("RPGDifficultyStatusVariation");

        number += dropRate;

        Logger.LogDebug($"{byPlayer.PlayerName} harvested any entity with knife, multiply drop: {number}");
    }

    private static void LevelUPOnExperienceIncrease(IPlayer player, string type, ref ulong amount)
    {
        int statsIncreaseDistance = 0;
        int statsIncreaseHeight = 0;
        int statsIncreaseAge = 0;

        // Stats increasing
        {
            // Coordinates
            double entityX = player.Entity.Pos.X - serverAPI.World.DefaultSpawnPosition.X;
            double entityZ = player.Entity.Pos.Z - serverAPI.World.DefaultSpawnPosition.Z;
            double entityY = player.Entity.Pos.Y;

            // XZ Coordinates translations
            if (entityX < 0) entityX = Math.Abs(entityX);
            if (entityZ < 0) entityZ = Math.Abs(entityZ);

            // Distance calculation
            if (Configuration.Status.enableStatusIncreaseByDistance)
            {
                statsIncreaseDistance = (int)(Math.Floor(entityX / Configuration.Status.increaseStatsEveryDistance) +
                                              Math.Floor(entityZ / Configuration.Status.increaseStatsEveryDistance));
            }

            // Height Calculation
            if (Configuration.Status.enableStatusIncreaseByHeight)
            {
                double heightDifference = Configuration.Status.baseStatusHeight - entityY;
                if (heightDifference > 0)
                {
                    statsIncreaseHeight = (int)Math.Floor(heightDifference / Configuration.Status.increaseStatsEveryDownHeight);
                }
            }


            // Age Calculation
            if (Configuration.Status.enableStatusIncreaseByAge)
            {
                statsIncreaseAge = Configuration.GetStatusByWorldAge(serverAPI);
            }
        }

        Logger.LogDebug($"[EXPERIENCE] Before: {amount}");
        // Increasing experience gain
        amount += (ulong)Math.Round(amount *
            (
                (Configuration.Status.levelUPExperienceIncreaseEveryDistance * statsIncreaseDistance) +
                (Configuration.Status.levelUPExperienceIncreaseEveryHeight * statsIncreaseHeight) +
                (Configuration.Status.levelUPExperienceIncreaseEveryAge * statsIncreaseAge)
            ));
        Logger.LogDebug($"[EXPERIENCE] After: {amount}");
    }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        Logger = new ModLogger(api.Logger, "RPGDifficulty", extendedLoggingEnabled: true);
        Logger.Log($"Running on Version: {Mod.Info.Version}");

        // Overwrite native functions
        overwriter.OverwriteNativeFunctions();
        // Disable GenerateDrops patch, because levelup already overwrite it
        if (api.ModLoader.IsModEnabled("levelup"))
        {
            MethodInfo target = AccessTools.Method(typeof(EntityBehaviorHarvestable), "GenerateDrops");
            overwriter.instance.Unpatch(
                target,
                HarmonyPatchType.Prefix,
                overwriter.instance.Id
            );
            Logger.Log("GenerateDrops unpatched because levelup already patch it");
        }
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        Configuration.Load(api);
        Logger.ExtendedLoggingEnabled = Configuration.Status.enableExtendedLog;
        Logger.Log("Configuration set");
    }

    public override double ExecuteOrder()
    {
        return 1.1;
    }


    public override void Dispose()
    {
        base.Dispose();
        overwriter.instance?.UnpatchAll();
    }

    private static readonly Random random = new();
    public static void SetEntityStats(Entity entity)
    {
        // Disclcaimer: for some reason the spawn position takes way too long to load, so in first loads we need to ignore it unfurtunally
        // serverAPI.World.DefaultSpawnPosition.X
        if (DefaultSpawnPosition == null) return;

        // Ignore non creature
        if (!entity.IsCreature) return;

        bool increaseByDistance = Configuration.BlackWhiteListCheckForDistance(entity.Code.ToString());
        bool increaseByHeight = Configuration.BlackWhiteListCheckForHeight(entity.Code.ToString());
        bool increaseByAge = Configuration.BlackWhiteListCheckForAge(entity.Code.ToString());

        entity.Attributes.SetBool("RPGDifficultyAlreadySet", true);

        // Function for increasing entity stats
        void increaseStats()
        {
            int statsIncreaseDistance = 0;
            int statsIncreaseHeight = 0;
            int statsIncreaseAge = 0;

            // Stats increasing
            {
                // Coordinates
                double entityX = entity.Pos.X - DefaultSpawnPosition.X;
                double entityZ = entity.Pos.Z - DefaultSpawnPosition.Z;
                double entityY = entity.Pos.Y;

                // XZ Coordinates translations
                if (entityX < 0) entityX = Math.Abs(entityX);
                if (entityZ < 0) entityZ = Math.Abs(entityZ);

                entity.Attributes.SetDouble("RPGDifficultyEntitySpawnDistance", entityX + entityZ);
                entity.Attributes.SetDouble("RPGDifficultyEntitySpawnHeight", entityY);
                entity.Attributes.SetDouble("RPGDifficultyEntitySpawnAge", (int)serverAPI.World.Calendar.ElapsedDays);

                // Distance calculation
                if (Configuration.Status.enableStatusIncreaseByDistance)
                {
                    statsIncreaseDistance = (int)(Math.Floor(entityX / Configuration.Status.increaseStatsEveryDistance) +
                                                  Math.Floor(entityZ / Configuration.Status.increaseStatsEveryDistance));
                }

                // Height Calculation
                if (Configuration.Status.enableStatusIncreaseByHeight)
                {
                    double heightDifference = Configuration.Status.baseStatusHeight - entityY;
                    if (heightDifference > 0)
                    {
                        statsIncreaseHeight = (int)Math.Floor(heightDifference / Configuration.Status.increaseStatsEveryDownHeight);
                    }
                }

                // Age Calculation
                if (Configuration.Status.enableStatusIncreaseByAge)
                {
                    statsIncreaseAge = Configuration.GetStatusByWorldAge(serverAPI);
                }
            }

            // Verification if is a creature and alive
            if (entity.IsCreature && entity.Alive)
            {
                // Getting variation
                double variation = 0;
                if (Configuration.Status.enableStatusVariation)
                {
                    variation = Configuration.Status.minimumVariableStatusAverage + (Configuration.Status.maxVariableStatusAverage - Configuration.Status.minimumVariableStatusAverage) * random.NextDouble();
                    variation = Math.Round(variation, 2);
                    entity.Attributes.SetDouble("RPGDifficultyStatusVariation", variation);
                }

                double healthDistance = Configuration.Status.lifeStatsIncreaseEveryDistance * statsIncreaseDistance;
                if (healthDistance > Configuration.Status.maximumLifeStatusIncreasedByDistance)
                    healthDistance = Configuration.Status.maximumLifeStatusIncreasedByDistance;

                double healthHeight = Configuration.Status.lifeStatsIncreaseEveryHeight * statsIncreaseHeight;
                if (healthHeight > Configuration.Status.maximumLifeStatusIncreasedByHeight)
                    healthHeight = Configuration.Status.maximumLifeStatusIncreasedByHeight;

                double healthAge = Configuration.Status.lifeStatsIncreaseEveryAge * statsIncreaseAge;
                if (healthAge > Configuration.Status.maximumLifeStatusIncreasedByAge)
                    healthAge = Configuration.Status.maximumLifeStatusIncreasedByAge;

                // Setting health variables
                if (increaseByDistance)
                    entity.Attributes.SetDouble("RPGDifficultyHealthStatsIncreaseDistance", healthDistance);
                if (increaseByHeight)
                    entity.Attributes.SetDouble("RPGDifficultyHealthStatsIncreaseHeight", healthHeight);
                if (increaseByAge)
                    entity.Attributes.SetDouble("RPGDifficultyHealthStatsIncreaseAge", healthAge);

                double damageDistance = Configuration.Status.damageStatsIncreaseEveryDistance * statsIncreaseDistance;
                if (damageDistance > Configuration.Status.maximumDamageStatusIncreasedByDistance)
                    damageDistance = Configuration.Status.maximumDamageStatusIncreasedByDistance;

                double damageHeight = Configuration.Status.damageStatsIncreaseEveryHeight * statsIncreaseHeight;
                if (damageHeight > Configuration.Status.maximumDamageStatusIncreasedByHeight)
                    damageHeight = Configuration.Status.maximumDamageStatusIncreasedByHeight;

                double damageAge = Configuration.Status.damageStatsIncreaseEveryAge * statsIncreaseAge;
                if (damageAge > Configuration.Status.maximumDamageStatusIncreasedByAge)
                    damageAge = Configuration.Status.maximumDamageStatusIncreasedByAge;

                // Setting damage variables
                if (increaseByDistance)
                    entity.Attributes.SetDouble("RPGDifficultyDamageStatsIncreaseDistance", damageDistance);
                if (increaseByHeight)
                    entity.Attributes.SetDouble("RPGDifficultyDamageStatsIncreaseHeight", damageHeight);
                if (increaseByAge)
                    entity.Attributes.SetDouble("RPGDifficultyDamageStatsIncreaseAge", damageAge);

                double lootDistance = Configuration.Status.lootStatsIncreaseEveryDistance * statsIncreaseDistance;
                if (lootDistance > Configuration.Status.maximumLootStatusIncreasedByDistance)
                    lootDistance = Configuration.Status.maximumLootStatusIncreasedByDistance;

                double lootHeight = Configuration.Status.lootStatsIncreaseEveryHeight * statsIncreaseHeight;
                if (lootHeight > Configuration.Status.maximumLootStatusIncreasedByHeight)
                    lootHeight = Configuration.Status.maximumLootStatusIncreasedByHeight;

                double lootAge = Configuration.Status.lootStatsIncreaseEveryAge * statsIncreaseAge;
                if (lootAge > Configuration.Status.maximumLootStatusIncreasedByAge)
                    lootAge = Configuration.Status.maximumLootStatusIncreasedByAge;

                // Setting damage variables
                if (increaseByDistance)
                    entity.Attributes.SetDouble("RPGDifficultyLootStatsIncreaseDistance", lootDistance);
                if (increaseByHeight)
                    entity.Attributes.SetDouble("RPGDifficultyLootStatsIncreaseHeight", lootHeight);
                if (increaseByAge)
                    entity.Attributes.SetDouble("RPGDifficultyLootStatsIncreaseAge", lootAge);

                Logger.LogDebug($"{entity.Code} health percentage: {healthDistance + healthHeight + healthAge} damage percentage: {damageDistance + damageHeight + damageAge} loot percentage: {lootDistance + lootHeight + lootAge}, variation: {variation}");
            }
        }

        // List Check
        if (increaseByDistance || increaseByHeight || increaseByAge)
            increaseStats();
    }

    public static bool ShouldEntitySpawn(Entity entity)
    {
        // Swiping every condition
        foreach (SpawnCondition condition in Configuration.Status.entitySpawnConditions)
        {
            // Check if the spawn condition is for this entity
            if (condition.code != entity.Code.ToString())
                continue;

            // Check SpawnersApi condition
            if (condition.ignoreConditionsForSpawnersAPI && entity.Attributes.GetBool("SpawnersAPI_Is_From_Spawner"))
                continue;

            // Getting the entity condition values
            double distance = entity.Attributes.GetDouble("RPGDifficultyEntitySpawnDistance", -1);
            double height = entity.Attributes.GetDouble("RPGDifficultyEntitySpawnHeight", -1);
            double age = entity.Attributes.GetDouble("RPGDifficultyEntitySpawnAge", -1);

            // Distance check
            if (condition.minimumDistanceToSpawn != -1 && distance < condition.minimumDistanceToSpawn)
            {
                Logger.LogDebug($"not in minimum distance: {condition.minimumDistanceToSpawn}, actual: {distance}");
                return false;
            }
            if (condition.maximumDistanceToSpawn != -1 && distance > condition.maximumDistanceToSpawn)
            {
                Logger.LogDebug($"not in maximum distance: {condition.maximumDistanceToSpawn}, actual: {distance}");
                return false;
            }

            // Height check
            if (condition.minimumHeightToSpawn != -1 && height < condition.minimumHeightToSpawn)
            {
                Logger.LogDebug($"not in minimum height: {condition.minimumHeightToSpawn}, actual: {height}");
                return false;
            }
            if (condition.maximumHeightToSpawn != -1 && height > condition.maximumHeightToSpawn)
            {
                Logger.LogDebug($"not in maximum height: {condition.maximumHeightToSpawn}, actual: {height}");
                return false;
            }

            // Age check
            if (condition.minimumAgeToSpawn != -1 && age < condition.minimumAgeToSpawn)
            {
                Logger.LogDebug($"not in minimum age: {condition.minimumAgeToSpawn}, actual: {age}");
                return false;
            }
            if (condition.maximumAgeToSpawn != -1 && age > condition.maximumAgeToSpawn)
            {
                Logger.LogDebug($"not in maximum age: {condition.maximumAgeToSpawn}, actual: {age}");
                return false;
            }
        }
        return true;
    }
}