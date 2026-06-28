# ModernBox-5

Modernboxxing time.

## Required loader

This build is meant to be used with `NeoModLoader.dll` version `1.2.0.1`.

If someone launches the mod with a different NeoModLoader build, the mod may fail to load or throw startup errors.

## Install

1. Copy `NeoModLoader.dll` version `1.2.0.1` into:
   `WorldBox\worldbox_Data\StreamingAssets\mods`
2. Copy the full `M5TrainsUpdateBeta` folder into that same `mods` folder.
3. Start WorldBox with experimental mode enabled.

The final layout should look like this:

```text
WorldBox
└─ worldbox_Data
   └─ StreamingAssets
      └─ mods
         ├─ NeoModLoader.dll
         └─ M5TrainsUpdateBeta
```

## Added

Trains: you can now paint and spawn trains via the cars tab. Cars are still heavy beta and mostly decorative right now.

Kaijus: 28 more kaijus have been added to the mod.

## QOL

- Optimisations and FPS patches: ModernBox should now run much smoother on moderately populated worlds.
- Removed ocean biomes because they were draining performance.
- Added caching for assets.
- Removed the effects in the M5 tab because they were a major source of lag.
- Removed space loading on startup. It now only loads when you open its menu.
- Kaijus are cached and refresh less often.
- Removed achievement background logic that was wasting performance.
- Improved zombies from a personal fixed build and merged the result into this mod.

- ## HOW TO USE TRAINS
- Rails do automatically build themselves and trains do spawn. But it's logic is awful. It is much more better you place the rails themselves. Use the spawn rail power and set the brush size to the lowest one. use the finger power to spread it it must be in a horizontal or vertical line. it cant be layered. Then spawn the train and it should work as intended.
