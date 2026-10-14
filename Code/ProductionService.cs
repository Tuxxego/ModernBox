using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    internal static class ProductionService
    {
        // Original M2: barracks spawn a placeholder unit that becomes an era unit
        // (UnitSpawner: every 10 game seconds, at most 5 living units per barracks),
        // and a city stops at 40 produced ("spawnedvehicle") units.
        private const int CityProducedUnitCap = 40;
        private const int BarracksUnitLimit = 5;
        private const float BarracksSpawnInterval = 10f;
        private const float TransformationChance = 0.5f;
        private const int TransformationAttempts = 20;
        private static readonly Dictionary<long, float> BarracksTimers = new Dictionary<long, float>();
        private static float _gameTick;
        private static readonly Dictionary<int, float> LastConstruction = new Dictionary<int, float>();

        // Building ids and recycled City objects repeat in the next world.
        internal static void ResetWorldState()
        {
            BarracksTimers.Clear();
            LastConstruction.Clear();
        }
        private static float _tick;
        private static string _humanDefaultNames;
        private static string _orcDefaultNames;
        private static string _elfDefaultNames;
        private static string _dwarfDefaultNames;
        internal static long CompletedCycles { get; private set; }

        internal static void Update(float elapsed)
        {
            if (World.world == null || World.world.isPaused()) return;
            _tick += elapsed;
            if (_tick < 5f) return;
            _tick = 0f;

            ModernProgression.UpdateCultures();
            ApplyDynamicSettings();
            ActorsAndBuildingsRegistry.RepairOrphanedUnits();
            foreach (City city in World.world.cities.list.ToArray())
            {
                if (!ModernProgression.IsSupportedCity(city)) continue;
                if (ModernBoxSettings.Get("ConstructionOption")) TryConstruction(city);
            }
            CompletedCycles++;
        }

        private static void TryConstruction(City city)
        {
            int cityKey = city.GetHashCode();
            float last;
            float now = (float)World.world.getCurWorldTime();
            if (LastConstruction.TryGetValue(cityKey, out last) && now - last < 5f) return;
            if (TryTierUpgrade(city) || TryEraUpgrade(city)) LastConstruction[cityKey] = now;
        }

        // The original only gated the vanilla house/hall tiers by culture tech (house_tier_N,
        // Renaissance for house 5 and hall 2). 0.51.2 also wants a tier 2 hall, which needs a
        // statue, mine and barracks at once, so cities stalled below the M2 era chains.
        private const int MaxTierUpgrades = 3;

        private static bool TryTierUpgrade(City city)
        {
            if (ModernProgression.GetRace(city) != "human") return false;
            List<KeyValuePair<int, Building>> candidates = new List<KeyValuePair<int, Building>>();
            foreach (Building building in city.buildings)
            {
                if (building == null || !building.isAlive() || building.isUnderConstruction() || building.asset == null) continue;
                string id = building.asset.id;
                bool house = id.StartsWith("house_human_", StringComparison.Ordinal);
                if (!house && !id.StartsWith("hall_human_", StringComparison.Ordinal)) continue;
                string next = building.asset.upgrade_to;
                if (string.IsNullOrEmpty(next) || !next.StartsWith(house ? "house_human_" : "hall_human_", StringComparison.Ordinal)) continue;
                if (AssetManager.buildings.get(next) == null) continue;
                int tier;
                if (!int.TryParse(next.Substring(next.LastIndexOf('_') + 1), out tier)) continue;
                string tech = house ? (tier >= 5 ? "Renaissance" : "house_tier_" + tier) : (tier >= 2 ? "Renaissance" : "house_tier_3");
                if (!M2Tech.Has(city, tech)) continue;
                // Halls first, then the lowest houses.
                candidates.Add(new KeyValuePair<int, Building>(house ? tier : tier - 10, building));
            }
            int done = 0;
            foreach (KeyValuePair<int, Building> candidate in candidates.OrderBy(pair => pair.Key))
            {
                if (ai.behaviours.CityBehBuild.upgradeBuilding(candidate.Value, city) && ++done >= MaxTierUpgrades) break;
            }
            return done > 0;
        }

        private static bool TryEraUpgrade(City city)
        {
            string race = ModernProgression.GetRace(city);
            if (!ActorsAndBuildingsRegistry.IsActiveUpgradeRace(race)) return false;
            foreach (BuildingUpgradeSpec upgrade in ContentRegistry.Upgrades
                .Where(candidate => candidate.Race == race && M2TechGates.CityAllowsBuilding(city, candidate.TargetId, candidate.Era))
                .OrderByDescending(candidate => candidate.Era))
            {
                string sourceId;
                if (!ActorsAndBuildingsRegistry.UpgradeSources.TryGetValue(upgrade.TargetId, out sourceId)) continue;
                BuildingAsset target = AssetManager.buildings.get(upgrade.TargetId);
                if (target == null || !city.hasEnoughResourcesFor(target.cost)) continue;
                List<Building> sources = city.getBuildingListOfID(sourceId);
                if (sources == null) continue;
                foreach (Building source in sources.ToArray())
                {
                    if (source == null || !source.isAlive() || source.isUnderConstruction() || source.asset == null) continue;
                    // upgradeBuilding dereferences the asset's upgrade_to without a null check.
                    if (source.asset.upgrade_to != upgrade.TargetId) continue;
                    if (ai.behaviours.CityBehBuild.upgradeBuilding(source, city)) return true;
                }
            }
            return false;
        }


        // Runs on game time (from the world update), so it keeps pace with the game speed.
        internal static void GameUpdate(float elapsed)
        {
            _gameTick += elapsed;
            if (_gameTick < 1f) return;
            float step = _gameTick;
            _gameTick = 0f;
            if (World.world == null || World.world.cities == null || !ModernBoxSettings.Get("FactoriesOption")) return;
            foreach (City city in World.world.cities.list.ToArray())
            {
                if (!ModernProgression.IsSupportedCity(city)) continue;
                try { TryBarracksProduction(city, step); }
                catch (Exception exception) { ModernBoxDiagnostics.Error("M2 barracks failed for " + city.name + ": " + exception); }
            }
        }

        private static void TryBarracksProduction(City city, float step)
        {
            int produced = CountProducedUnits(city);
            if (produced >= CityProducedUnitCap) return;
            // Traits.GetRoleType: barracks units follow Future/MilitaryModern/Firearms/Renaissance,
            // and the medieval ones need building_barracks.
            M2Era era = M2Tech.MilitaryEra(city.culture);
            if (era == M2Era.Medieval && !M2Tech.Has(city, "building_barracks")) return;
            string race = ModernProgression.GetRace(city);
            foreach (Building barracks in city.buildings.ToArray())
            {
                if (barracks == null || !barracks.isAlive() || barracks.isUnderConstruction() ||
                    barracks.asset == null || barracks.asset.type != "type_barracks") continue;
                long key = barracks.id;
                float timer;
                if (!BarracksTimers.TryGetValue(key, out timer)) timer = 1f;
                timer -= step;
                if (timer > 0f)
                {
                    BarracksTimers[key] = timer;
                    continue;
                }
                BarracksTimers[key] = BarracksSpawnInterval;
                if (CountUnitsFrom(city, barracks) >= BarracksUnitLimit) continue;
                string unitId = RollBarracksUnit(race, era);
                if (string.IsNullOrEmpty(unitId) || AssetManager.actor_library.get(unitId) == null) continue;
                if (SpawnProducedUnit(city, barracks, unitId) && ++produced >= CityProducedUnitCap) return;
            }
        }

        private static string RollBarracksUnit(string race, M2Era era)
        {
            // The original placeholder re-rolled its role on every trait tick and
            // transformed with a 50% chance once the roll had candidates.
            for (int attempt = 0; attempt < TransformationAttempts; attempt++)
            {
                string[] candidates = OriginalBarracksCandidates(race, era, RollRole(era, UnityEngine.Random.value));
                if (candidates.Length == 0 || UnityEngine.Random.value >= TransformationChance) continue;
                return candidates[UnityEngine.Random.Range(0, candidates.Length)];
            }
            return null;
        }

        private static M2UnitRole RollRole(M2Era era, float roll)
        {
            // Same threshold order as Traits.GetRoleType; thresholds the original
            // could never reach are left out.
            switch (era)
            {
                case M2Era.Future:
                    if (roll < 0.05f) return M2UnitRole.Titan;
                    if (roll < 0.20f) return M2UnitRole.Support;
                    if (roll < 0.30f) return M2UnitRole.Air;
                    break;
                case M2Era.Modern:
                    if (roll < 0.30f) return M2UnitRole.Air;
                    break;
                case M2Era.Industrial:
                    if (roll < 0.20f) return M2UnitRole.Air;
                    if (roll < 0.40f) return M2UnitRole.Heavy;
                    break;
                case M2Era.Renaissance:
                    if (roll < 0.10f) return M2UnitRole.Air;
                    if (roll < 0.20f) return M2UnitRole.Support;
                    if (roll < 0.30f) return M2UnitRole.Heavy;
                    break;
                default:
                    if (roll < 0.20f) return M2UnitRole.Support;
                    if (roll < 0.30f) return M2UnitRole.Heavy;
                    break;
            }
            return M2UnitRole.Offensive;
        }

        internal static string[] OriginalBarracksCandidates(string race, M2Era era, M2UnitRole role)
        {
            // Traits.CartTransformations from original M2.
            switch (role)
            {
                case M2UnitRole.Offensive:
                    switch (era)
                    {
                        case M2Era.Future:
                            string infantry = race == "orc" ? "spaceork" : "SpaceMarine";
                            return new[] { infantry, "Terran", "teslatruckgun", "atst", infantry, "artilleryatst", "atstsniper" };
                        case M2Era.Modern: return new[] { "modernhumvee", "wwartillery" };
                        case M2Era.Industrial: return new[] { "Humvee", "wwartillery" };
                        case M2Era.Renaissance:
                            if (race == "orc") return new[] { "ogreunit", "orccannon", "armoredwolf" };
                            if (race == "dwarf") return new[] { "dwarfcannon" };
                            if (race == "elf") return new[] { "treant", "elfcannon" };
                            return new[] { "humancavalry", "humancannon" };
                        default:
                            if (race == "orc") return new[] { "ogreunit", "armoredwolf" };
                            if (race == "dwarf") return new[] { "golemgem" };
                            if (race == "elf") return new[] { "treant" };
                            return new[] { "humancavalry" };
                    }
                case M2UnitRole.Heavy:
                    switch (era)
                    {
                        case M2Era.Future: return new[] { "P9000", "dreadnaught", "Railgun", "baseMA9000" };
                        case M2Era.Modern: return new[] { "Tank", "MissileSystem", "wheeledtank" };
                        case M2Era.Industrial: return new[] { "AbramTank", "shermanww", "tankie", "genericwwtank", "landship", "bigtankww" };
                        case M2Era.Renaissance: return new[] { "davincitank" };
                        default:
                            if (race == "orc") return new[] { "orcatapulta" };
                            if (race == "dwarf") return new[] { "santaguin" };
                            if (race == "elf") return new[] { "woolyrhino" };
                            return new[] { "catapulta", "batteringram" };
                    }
                case M2UnitRole.Support:
                    switch (era)
                    {
                        case M2Era.Future: return new[] { "AT9000", "supportatst" };
                        case M2Era.Modern: return new[] { "modernsupporttruck" };
                        case M2Era.Industrial: return new[] { "wwsupporttruck" };
                        default:
                            if (race == "orc") return new[] { "orcwarlock" };
                            if (race == "dwarf") return new[] { "dwarfdoctor" };
                            if (race == "elf") return new[] { era == M2Era.Renaissance ? "fairelf" : "fairydragon" };
                            return new[] { "humanpaladin" };
                    }
                case M2UnitRole.Air:
                    switch (era)
                    {
                        case M2Era.Future: return new[] { "HeliELite", "eliteGunship", "TIEfighter", "EliteBomber" };
                        case M2Era.Modern: return new[] { "Heli", "MIRVBomber", "FighterJet", "F55FighterJet" };
                        case M2Era.Industrial: return new[] { "Zeppelin", "EliteZeppelin", "americanbomberww", "biplane", "fighterww" };
                        case M2Era.Renaissance:
                            if (race == "orc") return new[] { "orccannon", "armoredwolf" };
                            if (race == "dwarf") return new[] { "Gunship" };
                            if (race == "elf") return new[] { "bigfaerydragon" };
                            return new[] { "balloonunit" };
                    }
                    break;
                case M2UnitRole.Titan:
                    if (era == M2Era.Future) return new[] { "HumanTitan" };
                    break;
            }
            return Array.Empty<string>();
        }

        private static bool SpawnProducedUnit(City city, Building barracks, string unitId)
        {
            ModernUnitSpec unit = ContentRegistry.FindUnit(unitId);
            WorldTile tile = barracks.door_tile ?? barracks.current_tile ?? city.getTile(false);
            if (unit == null || tile == null) return false;
            Actor actor = null;
            try
            {
                actor = World.world.units.createNewUnit(unitId, tile, true, unit.Flying ? 1.5f : 0.35f, null);
                if (actor == null) return false;
                actor.setCity(city);
                actor.setKingdom(city.kingdom);
                ActorsAndBuildingsRegistry.EnsureUnitRuntimeState(actor);
                actor.setHomeBuilding(barracks);
                AssignCombatRole(city, actor, unit);
                actor.addTrait("spawnedvehicle", true);
                if (actor.city != city || actor.kingdom != city.kingdom || !actor.is_profession_warrior)
                    throw new InvalidOperationException("M2 unit ownership or warrior assignment failed.");
                EffectsLibrary.spawn("fx_spawn", actor.current_tile);
                return true;
            }
            catch (Exception exception)
            {
                ModernBoxDiagnostics.Error("M2 production failed for " + unitId + ": " + exception);
                if (actor != null && actor.isAlive()) actor.die(true, AttackType.Other, false, false);
                return false;
            }
        }

        private static void AssignCombatRole(City city, Actor actor, ModernUnitSpec unit)
        {
            if (unit.Humanoid) city.makeWarrior(actor);
            else
            {
                actor.setProfession(UnitProfession.Warrior, true);
                city.status.warriors_current++;
            }
        }

        private static int CountUnitsFrom(City city, Building barracks)
        {
            int count = 0;
            foreach (Actor actor in city.units)
                if (actor != null && actor.isAlive() && actor.home_building == barracks && actor.hasTrait("spawnedvehicle")) count++;
            return count;
        }

        private static int CountProducedUnits(City city)
        {
            int count = 0;
            foreach (Actor actor in city.units)
                if (actor != null && actor.isAlive() && actor.hasTrait("spawnedvehicle")) count++;
            return count;
        }

        internal static void ApplyDynamicSettings()
        {
            BuildingAsset silo = AssetManager.buildings.get(SiloLaunchEvents.SiloId);
            if (silo != null) silo.tower = true;
            SiloLaunchEvents.SyncOrders();
            M2Ideologies.ApplyRates(ModernBoxSettings.Get("IdeologiesOption"));
            ApplyNames();
        }

        private static void ApplyNames()
        {
            ActorAsset human = AssetManager.actor_library.get("human");
            ActorAsset orc = AssetManager.actor_library.get("orc");
            ActorAsset elf = AssetManager.actor_library.get("elf");
            ActorAsset dwarf = AssetManager.actor_library.get("dwarf");
            if (_humanDefaultNames == null)
            {
                _humanDefaultNames = human == null ? null : human.name_template_unit;
                _orcDefaultNames = orc == null ? null : orc.name_template_unit;
                _elfDefaultNames = elf == null ? null : elf.name_template_unit;
                _dwarfDefaultNames = dwarf == null ? null : dwarf.name_template_unit;
            }
            bool humanNames = ModernBoxSettings.Get("namesOption");
            bool otherNames = ModernBoxSettings.Get("othernamesOption");
            if (human != null) human.name_template_unit = humanNames ? "Modern_Names" : _humanDefaultNames;
            if (orc != null) orc.name_template_unit = otherNames ? "Modern_Orc_Names" : _orcDefaultNames;
            if (elf != null) elf.name_template_unit = otherNames ? "Modern_Elf_Names" : _elfDefaultNames;
            if (dwarf != null) dwarf.name_template_unit = otherNames ? "Modern_Dwarf_Names" : _dwarfDefaultNames;
        }

        internal static bool IsEquipmentEnabled(string id, City city)
        {
            M2Era required;
            if (!ModernBoxCatalog.EquipmentEras.TryGetValue(id, out required)) return true;
            if (!M2TechGates.CityAllowsItem(city, id, required)) return false;
            string materialTech;
            if (ContentRegistry.MaterialTechs.TryGetValue(id, out materialTech) && !M2Tech.Has(city, materialTech)) return false;
            if (!ModernBoxSettings.Get("EquipmentOption")) return false;
            if (id.StartsWith("Pipe", StringComparison.Ordinal)) return ModernBoxSettings.Get("PipeGunOption");
            if (id == "Sandevistan" || id == "TurboBooster") return ModernBoxSettings.Get("CyberwareOption");
            if (id == "Meth" || id == "Crack") return ModernBoxSettings.Get("DrugsOption");
            return ModernBoxSettings.Get("GunOption");
        }
    }
}
