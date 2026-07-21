# ModernBox-2

Welcome to the Modern and (not for now) Space Age.

# Required loader

This build is meant to be used with `NeoModLoader.dll` version `1.2.0.1`.
If someone launches the mod with a different NeoModLoader build, the mod may fail to load or throw startup errors.

# Install

1. Copy `NeoModLoader.dll` version `1.2.0.1` into:
`WorldBox_Data\StreamingAssets\mods`
2. Copy the full `modernbox-m2` folder into that same `mods` folder.
3. Start WorldBox with experimental mode enabled.
The final layout should look like this:

```text
WorldBox
└─ worldbox_Data
   └─ StreamingAssets
      └─ mods
         ├─ NeoModLoader.dll
         └─ M2Port
```


## Culture progression

The rewrite uses the *Standard** pace: the world starts Medieval and rolls a saved 50-200 world-year interval for each transition. Every supported civilization advances together, and reloading a save does not reroll the world's era dates.

| Transition | Standard interval |
| --- | ---: |
| Medieval to Renaissance | 50-200 world years |
| Renaissance to Industrial | 50-200 world years |
| Industrial to Modern | 50-200 world years |
| Modern to Future | 50-200 world years |

This is based only on total world history year, not culture age, or population. Buildings, armies, factories, equipment, and race/era appearances use the current world era.

Casinos, restaurants, malls, schools, and modern buildings are directly buildable and unlimited after Renaissance. Every individual factory and MissileSilo is limited to one per city. Existing too-advanced buildings in an old save are left intact, but no new early upgrades or construction are allowed.

## Production and resources

Factory production is free where original M2 declared no unit price. Boat actors retain their declared wood/gold cost. Production runs in bounded five-second city passes and allows up to 40 M2 vehicles per city.

Dock progression is upgrade-only: a terminal native dock can become a Renaissance dock in Renaissance, an Industrial dock in Industrial, and a Modern dock in Modern. Each dock upgrade costs one gold. Upgraded docks spend one wood plus one gold for each M2 boat.

- Commerce buildings produce one gold every 30 seconds, capped at 500 per city.
- Schools produce one CyberWareParts every 60 seconds, capped at 100.
- Factories produce one Parts every 30 seconds, capped at 200.
- Air, Terran, and P9000 factories plus MissileSilo produce one Xenium every 120 seconds, capped at 50.

Equipment without a complete original recipe uses era fallbacks: Renaissance `1 wood + 1 common metal + 1 gold`; Industrial `2 common metals + 2 gold`; Modern `2 Parts + 2 common metals + 3 gold`; Future `2 Parts + 1 Xenium + 4 gold`. Cyberware uses `2 CyberWareParts + 1 Parts + 2 gold`; MIRV and MIRVBomb use `3/5 Parts + 2/4 Xenium + 5/10 gold`.


## Included and deferred scope

Included: four-civilization era armies, land vehicles, aircraft, future/Goliath units, naval actors, factories, equipment, traits, five ideologies, names, resources, bosses, bounded invasions, Alien Jungle as a normal-map biome, custom bombs, and missile silos.

Deferred: the star map, generated planets and stars, colony ships, planet transfers, copy/paste-world features, planet-only biome wrappers, and Universal Destruction. No deferred space system initializes or modifies saves in this build.





