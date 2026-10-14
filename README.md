# ModernBox 2

Welcome to the Modern and Space Age. Guns, vehicles, drugs, casinos, MIRVs and space travel.

This is M2 ported to WorldBox 0.51.2 so it runs on the new game. The aim was to keep it as close to the original M2 as possible.

## Required loader

Made and tested with `NeoModLoader.dll` version `1.2.0.1` on WorldBox 0.51.2 (build 719). Other versions might work but no promises.

## Install

1. Copy `NeoModLoader.dll` into:
   `WorldBox\worldbox_Data\StreamingAssets\mods`
2. Copy the `ModernBox-M2` folder into that same `mods` folder.
3. Start WorldBox. The mod shows up as the M2 tab.

```text
WorldBox
└─ worldbox_Data
   └─ StreamingAssets
      └─ mods
         ├─ NeoModLoader.dll
         └─ ModernBox-M2
```

## Tech and eras

0.51.2 removed culture tech, so the port brings back M2's own version of it. Every human, orc, elf and dwarf culture researches techs on its own, with the same costs and speed as the old game. Once they research far enough they move into the next era:

- Renaissance at culture level 55
- Industrial at 65
- Modern at 80
- Future at 90

Each culture moves at its own pace, so you can have a future empire next to a medieval one. Buildings, guns, armor, ships and barracks units unlock with the same techs as in M2.

Research is slow, like it was in the original. Smart city leaders and lots of cities speed it up. If you don't want to wait there's a research speed button in the Technologies window (1x is the original speed, it goes up to 10x).

The Technologies button is in the M2 tab under the Medieval Units button. It shows a culture's era, what it's researching and which techs it has. Hover over an icon to see what it does.

Old saves get their cultures caught up to the era the world was in.

## Different from the original

Some things couldn't be done the same way on 0.51.2:

- The heroes, trading, storage and smith era techs don't do anything because 0.51.2 has nothing for them to change. They still count towards the culture level.
- The old vanilla techs only set the research speed. 0.51.2 handles housing, zones and weapons its own way.
- Human cities upgrade their houses and halls by tech like in M2. 0.51.2 normally also wants a big hall first, which stopped cities from ever reaching the modern houses.
- Cities keep 10 metal back from crafting so they can still afford buildings. Without this, soldiers spent all of it on guns and cities stopped growing.
- The eraser drop uses a different sprite. The original one isn't in the files.
- Some color variants and sounds aren't in yet.

## Bugs

If something breaks, send the `error_*.log` from `AppData\LocalLow\mkarpenko\WorldBox\logs` along with what you were doing.
