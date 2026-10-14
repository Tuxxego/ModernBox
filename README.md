# ModernBox

Welcome to the Modern Age. Guns, vehicles, drugs, casinos and MIRVs.

This is ModernBox M1 (10.0) ported to WorldBox 0.51.2 so it runs on the new game. The aim was to keep it as close to the original M1 as possible, bugs included.

## Required loader

Made with `NeoModLoader.dll` version `1.2.0.1` on WorldBox 0.51.2 (build 719). Other versions might work but no promises.

## Install

1. Copy `NeoModLoader.dll` into:
   `WorldBox\worldbox_Data\StreamingAssets\mods`
2. Copy the `ModernBox-M1` folder into that same `mods` folder.
3. Take ModernBox 2 out of the `mods` folder if it's there. Both use the same units and buildings, so only one can be loaded.
4. Start WorldBox. The mod shows up as the ModernBox tab.

```text
WorldBox
└─ worldbox_Data
   └─ StreamingAssets
      └─ mods
         ├─ NeoModLoader.dll
         └─ ModernBox-M1
```

## Tech

0.51.2 removed culture tech, so the port brings it back. Every human, orc, elf and dwarf culture researches techs on its own, with the same costs and speed as the old game. The culture level is the number of techs it has, and the M1 techs unlock at the same levels as before:

- Casino (casinos, restaurants, malls, schools, modern buildings) at level 40
- Pipe Guns at 55
- Modern Military (Modern Barracks, much bigger armies) at 60
- Guns, Tanks, Helicopters, Fighter Jets, MIRV Bombers, F55, Drones, Nukes and Short Range MIRVs at 65
- Cyberware, Decent Range MIRVs and Gunships at 70
- Long Range MIRVs at 80
- Nuclear MIRVs at 90

Research is slow, like it was in the original. Smart city leaders speed it up.

The Tech button under Developer Mode in the ModernBox tab shows each culture's level, what it is researching and which ModernBox techs it has. The original didn't need this button because the old game had its own culture tech window.

## Kept from the original on purpose

These look like bugs, but the original M1 did them too:

- A city only ever builds one M1 building (plus one missile silo).
- Cities never build the Tank, Airship, Railgun or Missile System factories.
- Drones are never made.
- Factory toggles only work on factories built or loaded after you switch them on.
- Missile silos can only be built after the nuke toggle is switched on, and switching it off does nothing until you restart.
- Turning off modern names for other races doesn't change the dwarves back.
- The Random bomb's sizes don't match the bombs they're named after.
- Reset to defaults only takes effect after a restart.
- Cultures run out of techs to research before the last unlocks. Humans stop at level 79 and dwarves at 78, so Long Range and Nuclear MIRVs are never researched. Elves stop at 64 and orcs at 62, so they never get guns or vehicles.

## Different from the original

Some things couldn't be done the same way on 0.51.2:

- Cyberware and drugs are made like rings. In the original they came out of the weapon list.
- In the original, a city that picked a gun it couldn't make yet made nothing that time. Here it makes a normal item instead.
- Fertility, max children and zone range don't exist in 0.51.2, so the stats that used them (drugs, Primalism, the Vehicle trait) do nothing.
- The saved factory, Modern Militaries and ideology toggles work as soon as the game starts. In the original they did nothing until you clicked them again, so Modern Barracks always made soldiers and ideologies were always on.
- Developer mode only writes extra lines to the game log. The original sent errors to a private Discord channel.
- The original's log file writer and start-up sound aren't included. The sound never played in the original.
- The eraser drop uses a different sprite. The original one isn't in the game anymore.

## Bugs

If something breaks, send the `error_*.log` from `AppData\LocalLow\mkarpenko\WorldBox\logs` along with what you were doing.
