# ModernBox M1 Rewrite - WorldBox 0.51.2

This is a clean NML rewrite of ModernBox M1 for WorldBox **0.51.2 (build 558)**.

## Install

1. Disable or remove every other ModernBox edition. The original content IDs intentionally collide.
2. Copy this entire folder into `worldbox_Data/StreamingAssets/mods/`.
3. Keep NML 1.2.0.1 or the matching experimental build installed.
4. Start WorldBox and open the ModernBox toolbar tab.

## Human-city progression

| Tier | Population | Buildings | Main unlocks |
|---|---:|---:|---|
| Urban | 50 | 16 | Unlimited casino, restaurant, mall, school, and modern-residence construction |
| Industrial | 75 | 18 | Pipe weapons, ModernBarracks, Soldier |
| Modern | 100 | 20 | Normal factories, vehicles, aircraft, guns, Budget MIRV |
| Advanced | 130 | 23 | Railgun/Gunship/Missile System, cyberware, drugs, Decent MIRV |
| Strategic | 170 | 26 | Strategic AirFactory, MIRVBomber, CargoPlane, MIRV weapons |
| Nuclear | 220 | 30 | MissileSilo and STRONGMIRV |

Only human architecture receives these build orders. All five civilian buildings have an effectively unlimited direct build-order count. Ordinary human houses at stages 1 through 4 may also branch into a modern residence whenever the city can afford the modern target; their original vanilla upgrade destinations are restored immediately, so cities that cannot afford that branch can still reach the final ordinary house normally. The final ordinary house retains a native modern-residence upgrade. Each factory, ModernBarracks, and MissileSilo remains limited to one of that type per city, matching M1's factory-order limit.

## Factory economy

The production service checks cities every five seconds and produces at most one unit per city per check. A city must meet the progression, factory, toggle, resource, interval, and military-cap requirements. Resources are spent only after a unit has been created and assigned successfully.

Factories generate bounded Parts, schools generate CyberWareParts, and AirFactory/MissileSilo generate Xenium. Casinos, restaurants, and malls generate bounded gold.

| Production class | Interval | Cost |
|---|---:|---|
| Soldier | 30 seconds | 1 common metal, 1 gold |
| Light vehicle | 45 seconds | 1 wood, 1 common metal, 1 gold |
| Armor | 60 seconds | 1 stone, 3 common metals, 2 gold |
| Normal aircraft | 75 seconds | 1 wood, 2 common metals, 2 gold |
| Advanced aircraft | 90 seconds | 1 stone, 4 common metals, 4 gold |
| MIRV bomber / strategic aircraft | 120 seconds | 1 stone, 6 common metals, 8 gold |

The ModernBox military cap per city is `clamp(population / 10, 4, 30)`. AirFactory alternates between MIRVBomber and CargoPlane.

## Controls

Open the **ModernBox** toolbar tab. Its page buttons switch between **Industry**, **Units**, **Bombs**, **Equipment**, and **Settings**. Industry controls factory and silo production toggles; Units contains all 13 manual spawn powers; Bombs contains all ten custom drops; Equipment controls guns, pipe guns, cyberware, drugs, ideologies, and name sets; Settings opens the credits, complete settings, and bounded diagnostics windows.

## Exact-scale bombs

MOAB 50, Cobalt 120, Ultron 100, Death 100, Xenium 400, Mini 5, Proton 786, Jupiter 1486, and Eraser 1000. Random uses the original M1 radius set. Huge areas are processed over multiple frames; the radius is never reduced. Test Jupiter, Eraser, Proton, and Xenium only on disposable worlds; exact-scale jobs can take time to finish on large maps.

## Included content

- 13 units and manual spawn powers
- 18 finished buildings
- 26 guns, 5 MIRV weapons, 2 cyberware accessories, and 2 drug accessories
- Parts, CyberWareParts, and Xenium resources
- 8 vehicle traits and 11 ideology traits
- Modern human/orc/elf/dwarf and vehicle name generators
- 10 custom bombs with complete DropAsset registration
- Paginated in-game tab, settings, credits/info, bounded diagnostics, and optional local startup audio

## Conflicts

Do not enable this rewrite alongside M1, M5, or any other ModernBox variant. The rewrite checks core original IDs during startup and stops with a clear error if another edition registered them first.
