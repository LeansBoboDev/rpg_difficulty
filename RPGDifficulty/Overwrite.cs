using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;
using Vintagestory.API.Common.Entities;
using Vintagestory.Server;
using System.Reflection;
using System.Threading.Tasks;

namespace RPGDifficulty;

class Overwrite
{
    public Harmony instance;
    public void OverwriteNativeFunctions()
    {
        if (!Harmony.HasAnyPatches("rpgdifficulty"))
        {
            instance = new Harmony("rpgdifficulty");
            instance.PatchCategory("rpgdifficulty");
            RPGDifficultyModSystem.Logger.Log("Damage interaction has been overwrited");
        }
        else
        {
            RPGDifficultyModSystem.Logger.Log("RPGDifficulty overwriter has already patched, probably by the singleplayer server");
        }
    }
}

#pragma warning disable IDE0060
[HarmonyPatchCategory("rpgdifficulty")]
class DamageInteraction
{
    // Overwrite Damage Interaction
    [HarmonyPatch(typeof(AiTaskMeleeAttack), MethodType.Constructor, [typeof(EntityAgent), typeof(JsonObject), typeof(JsonObject)])]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.VeryHigh)]
    public static void LoadConfig(AiTaskMeleeAttack __instance, EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
    {
        if (!entity.Alive) return;

        // Check if should spawn entity
        if (!RPGDifficultyModSystem.ShouldEntitySpawn(entity))
        {
            RPGDifficultyModSystem.Logger.LogDebug($"Entity removed by ShouldEntitySpawn: {entity.GetName()}");

            RPGDifficultyModSystem.serverAPI?.World.DespawnEntity(entity, new()
            {
                Reason = EnumDespawnReason.Removed
            });
            return;
        }

        // Checking if the entity already have the calculation
        if (!entity.Attributes.GetBool("RPGDifficultyAlreadySet"))
        {
            RPGDifficultyModSystem.Logger.LogDebug($"Calculating entity status: {entity.Code}");
            RPGDifficultyModSystem.SetEntityStats(entity);
        }

        // Single player / Lan treatment
        if (entity.SidedProperties == null) return;

        #region health
        if (!entity.Attributes.GetBool("RPGDifficultyHealthAlreadySet"))
        {
            EntityBehaviorHealth entityLifeStats = entity.GetBehavior<EntityBehaviorHealth>();

            void updateEntityHealth()
            {
                double healthPercentage = 0;
                healthPercentage += entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseDistance");
                healthPercentage += entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseHeight");
                healthPercentage += entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseAge");
                if (healthPercentage > 0)
                {

                    float oldBaseMaxHealth = entityLifeStats.BaseMaxHealth;
                    float oldMaxHealth = entityLifeStats.MaxHealth;
                    float oldHealth = entityLifeStats.Health;

                    if (oldBaseMaxHealth > 1 && oldMaxHealth > 1 && oldHealth > 1)
                    {
                        entityLifeStats.BaseMaxHealth += (int)Math.Round(entityLifeStats.BaseMaxHealth * healthPercentage);
                        if (Configuration.StatusVariation.enableStatusVariation)
                            entityLifeStats.BaseMaxHealth *= (float)entity.Attributes.GetDouble("RPGDifficultyStatusVariation");
                        entityLifeStats.MaxHealth += (int)Math.Round(entityLifeStats.MaxHealth * healthPercentage);
                        if (Configuration.StatusVariation.enableStatusVariation)
                            entityLifeStats.MaxHealth *= (float)entity.Attributes.GetDouble("RPGDifficultyStatusVariation");
                        entityLifeStats.Health += (int)Math.Round(entityLifeStats.Health * healthPercentage);
                        if (Configuration.StatusVariation.enableStatusVariation)
                            entityLifeStats.Health *= (float)entity.Attributes.GetDouble("RPGDifficultyStatusVariation");

                        if (entityLifeStats.Health < 1)
                        {
                            RPGDifficultyModSystem.Logger.LogError("------------------------");
                            RPGDifficultyModSystem.Logger.LogError($"ERROR: Entity health calculations goes really wrong: {entity.GetName()}, ");
                            RPGDifficultyModSystem.Logger.LogError($"Distance: {entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseDistance")}");
                            RPGDifficultyModSystem.Logger.LogError($"Height: {entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseHeight")}");
                            RPGDifficultyModSystem.Logger.LogError($"Age: {entity.Attributes.GetDouble("RPGDifficultyHealthStatsIncreaseAge")}");
                            RPGDifficultyModSystem.Logger.LogError($"Health Percentage: {healthPercentage}");
                            RPGDifficultyModSystem.Logger.LogError($"Base Max Health: {entityLifeStats.BaseMaxHealth}");
                            RPGDifficultyModSystem.Logger.LogError($"Max Health: {entityLifeStats.MaxHealth}");
                            RPGDifficultyModSystem.Logger.LogError($"Health: {entityLifeStats.Health}");
                            RPGDifficultyModSystem.Logger.LogError($"Old Base Max Health: {oldBaseMaxHealth}");
                            RPGDifficultyModSystem.Logger.LogError($"Old Max Health: {oldMaxHealth}");
                            RPGDifficultyModSystem.Logger.LogError($"Old Health: {oldHealth}");

                            entityLifeStats.BaseMaxHealth = oldBaseMaxHealth;
                            entityLifeStats.MaxHealth = oldMaxHealth;
                            entityLifeStats.Health = oldHealth;

                            RPGDifficultyModSystem.Logger.LogError("Resetting calculations to the previous");
                            RPGDifficultyModSystem.Logger.LogError("------------------------");
                        }
                        else
                        {
                            RPGDifficultyModSystem.Logger.LogDebug($"[LoadConfig] {entity.Code} health updated to: {entityLifeStats.MaxHealth}");
                            // Health status can only be set once, otherwise will be updated every world start or entity reload
                            entity.Attributes.SetBool("RPGDifficultyHealthAlreadySet", true);
                        }
                    }
                }
            }

            // Check existance
            if (entityLifeStats != null)
            {
                if (
                    entityLifeStats.BaseMaxHealth > 0f &&
                    entityLifeStats.MaxHealth > 0f &&
                    entityLifeStats.Health > 0f
                )
                {
                    updateEntityHealth();
                }
                else
                {
                    // Entity health is not set yet for some reason, we wait it...
                    Task.Run(async () =>
                    {
                        // Changing Health Stats
                        EntityBehaviorHealth entityLifeStats = entity.GetBehavior<EntityBehaviorHealth>();

                        for (int i = 0; i < 5; i++)
                        {
                            await Task.Delay(500);

                            if (
                                entityLifeStats.BaseMaxHealth > 0f &&
                                entityLifeStats.MaxHealth > 0f &&
                                entityLifeStats.Health > 0f
                            )
                            {
                                updateEntityHealth();
                                break;
                            }

                            if (i == 4)
                            {
                                RPGDifficultyModSystem.Logger.LogError($"Could not setup entity health after 5 tries: {entity.GetName()}");
                            }
                        }
                    });
                }
            }
        }
        #endregion

        #region damage
        float damage = taskConfig["damage"].AsFloat(2f);
        if (damage >= 0f)
        {

            // Increase the damage
            damage += (float)(damage * entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseDistance"));
            damage += (float)(damage * entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseHeight"));
            damage += (float)(damage * entity.Attributes.GetDouble("RPGDifficultyDamageStatsIncreaseAge"));

            // Variation
            if (Configuration.StatusVariation.enableStatusVariation)
                damage *= (float)entity.Attributes.GetDouble("RPGDifficultyStatusVariation");

            FieldInfo protectedDamage = AccessTools.Field(typeof(AiTaskMeleeAttack), "damage");
            protectedDamage.SetValue(__instance, damage);

            RPGDifficultyModSystem.Logger.LogDebug($"[LoadConfig] Entity damage updated to: {protectedDamage.GetValue(__instance)}");
        }
        #endregion
    }

    // Overwrite Entity Spawn, why not use server api event?
    // Because I prefer the entity to be removed before it is even present in the world
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ServerMain), "SpawnEntity", [typeof(Entity), typeof(EntityProperties)])]
    public static bool SpawnEntity(Entity entity, EntityProperties type)
    {
        if (!RPGDifficultyModSystem.ShouldEntitySpawn(entity))
        {
            RPGDifficultyModSystem.Logger.LogDebug($"Entity removed by ShouldEntitySpawn: {entity.GetName()}");
            return false;
        }

        // Checking if the entity already have the calculation
        if (!entity.Attributes.GetBool("RPGDifficultyAlreadySet"))
            RPGDifficultyModSystem.SetEntityStats(entity);

        return true;
    }

    // Overwrite Knife Harvesting
    [HarmonyPrefix]
    [HarmonyPatch(typeof(EntityBehaviorHarvestable), "GenerateDrops")]
    public static void GenerateDropsStart(EntityBehaviorHarvestable __instance, IPlayer byPlayer)
    {
        // Check if player exist and options is enabled
        if (byPlayer != null && Configuration.StatusDistance.lootStatsIncreaseEveryDistance == 0 && Configuration.StatusHeight.lootStatsIncreaseEveryHeight == 0) return;

        // Get the final droprate
        float dropRate = (float)__instance.entity.Attributes.GetDouble("RPGDifficultyLootStatsIncreaseDistance");
        dropRate += (float)__instance.entity.Attributes.GetDouble("RPGDifficultyLootStatsIncreaseHeight");
        dropRate += (float)__instance.entity.Attributes.GetDouble("RPGDifficultyLootStatsIncreaseAge");

        if (Configuration.StatusVariation.enableStatusVariation)
            dropRate *= (float)__instance.entity.Attributes.GetDouble("RPGDifficultyStatusVariation");

        // Don't worry, it will be reseted automatically by the game
        // 1 means 100%, luckly your base harvest in config is 0.0 so no changes needed
        byPlayer.Entity.Stats.Set("animalLootDropRate", "animalLootDropRate", dropRate - 1f);

        RPGDifficultyModSystem.Logger.LogDebug($"{byPlayer.PlayerName} harvested any entity with knife, multiply drop: {byPlayer.Entity.Stats.GetBlended("animalLootDropRate")}");
    }
}