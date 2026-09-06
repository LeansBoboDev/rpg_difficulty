# RPG Difficulty
Monster and Creatures increases their status getting away from the spawn, getting deeper in the caves and by the world aging, highly configurable

## Dependencies
- [Open Configuration](https://mods.vintagestory.at/openconfiguration)

Features:
- Blacklist
- Whitelist
- Life status increase
- Damage status increase
- Loot drop status increase
- Increase by distance
- Increase by height
- Increase by world age
- Status variations
- Condition to spawn based on distance/height/age
- [Level UP](https://mods.vintagestory.at/levelup) Mod increase experience based on distance/height/age
- [SpawnersAPI](https://mods.vintagestory.at/spawnersapi) Mod compatibility to ignore spawn conditions

Everything is configurable, check the [wiki](https://github.com/LeandroTheDev/rpg_difficulty/wiki) for more informations about configurations

### Observations
For the health system to work, we change the base max health when new entity spawn, the  entity will not lose that health if the mod is removed.

For the damage system to work we change the damage from the entity attack AI, removing the mod will remove the damage.

For the harvest system to work correctly the mod ``overwrites`` the harvest system and increase the multiply result, if the mod is removed the additional multiply will no longer exist.

The mod is very compatible with other mods, any incompatibility you can contact me.

Make a backup of the world before adding this mod, any error can drastically increase the life of a creature which can ruin your gameplay.

### Considerations
By default the blacklist with "game:player" is enabled, because of course we dont wanna get the players to receive the buffs from distance when entering in the server, but if you want feel free to remove it.

# About RPG Difficulty
RPG Difficulty is open source project and can easily be accessed on the github, all contents from this mod is completly free.

If you want to contribute into the project you can access the project github and make your pull request.

You are free to fork the project and make your own version of RPG Difficulty, as long the name is changed.

Inspirations: 
- Minecraft RpgDifficulty mod

# Building
- Install .NET in your system, open terminal type: ``dotnet new install VintageStory.Mod.Templates``
- Create a template with the name ``RPGDifficulty``: ``dotnet new vsmod --AddSolutionFile -o RPGDifficulty``
- [Clone the repository](https://github.com/LeandroTheDev/rpg_difficulty/archive/refs/heads/main.zip)
- Copy the ``CakeBuild`` and ``build.ps1`` or ``build.sh`` and paste inside the repository

Now you can build using the ``build.ps1`` or ``build.sh`` file

FTM License
