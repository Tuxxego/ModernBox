using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModernBoxRewrite
{
    internal static class ProductionService
    {
        private static readonly Dictionary<string, float> LastProduction = new Dictionary<string, float>(StringComparer.Ordinal);
        private static readonly Dictionary<int, int> AirAlternation = new Dictionary<int, int>();
        private static readonly Dictionary<int, float> LastResources = new Dictionary<int, float>();
        private static readonly Dictionary<int, float> LastConstruction = new Dictionary<int, float>();
        private static float _tick;
        private static string _humanDefaultNames;
        private static string _orcDefaultNames;
        private static string _elfDefaultNames;
        private static string _dwarfDefaultNames;
        private static bool? _lastMirvEnabled;
        private static bool? _lastIdeologyEnabled;
        internal static long CompletedCycles { get; private set; }

        internal static void Update(float elapsed)
        {
            if (World.world == null || World.world.isPaused()) return;
            _tick += elapsed;
            if (_tick < 5f) return;
            _tick = 0f;
            ApplyDynamicSettings();
            ActorsAndBuildingsRegistry.RepairOrphanedUnits();
            foreach (City city in World.world.cities.list)
            {
                if (!ModernProgression.IsHumanCity(city) || city.kingdom == null) continue;
                TryModernConstruction(city);
                GenerateResources(city);
                TryProduceOne(city);
            }
            CompletedCycles++;
        }

        private static void TryModernConstruction(City city)
        {
            int key = city.GetHashCode();
            float last;
            if (LastConstruction.TryGetValue(key, out last) && Time.time - last < 5f) return;
            if (city.under_construction_building != null) return;

            ProgressionTier tier = ModernProgression.GetTier(city);

            // Establish each unlocked one-per-city producer before adding more
            // unlimited civilian structures.
            foreach (BuildingSpec spec in ContentRegistry.Buildings
                .Where(candidate => !candidate.Civilian)
                .OrderBy(candidate => candidate.Id == "MissileSilo" ? 0 : 1))
            {
                if (tier < spec.Tier || city.countBuildingsOfID(spec.Id) >= spec.Limit) continue;
                if (TryNativeBuild(city, spec.Id))
                {
                    LastConstruction[key] = Time.time;
                    return;
                }
            }

            // Earlier ordinary houses may branch into a modern residence only when
            // the modern target is affordable. Their shared assets are restored in a
            // finally block, preserving the normal house chain when it is not.
            if (TryEarlyModernHouseUpgrade(city))
            {
                LastConstruction[key] = Time.time;
                return;
            }

            // The terminal ordinary house has no further vanilla stage, so it uses a
            // permanent native upgrade link to the modern residence.
            string sourceId = ActorsAndBuildingsRegistry.HumanHouseUpgradeSourceId;
            List<Building> sources = string.IsNullOrEmpty(sourceId) ? null : city.getBuildingListOfID(sourceId);
            if (sources != null)
            {
                foreach (Building source in sources.ToArray())
                {
                    if (source == null || !source.isAlive() || source.isUnderConstruction()) continue;
                    if (ai.behaviours.CityBehBuild.upgradeBuilding(source, city))
                    {
                        LastConstruction[key] = Time.time;
                        return;
                    }
                }
            }

            if (tier < ProgressionTier.Urban) return;
            BuildingSpec civilian = ContentRegistry.Buildings
                .Where(spec => spec.Civilian && tier >= spec.Tier)
                .OrderBy(spec => city.countBuildingsOfID(spec.Id))
                .ThenBy(spec => spec.Id, StringComparer.Ordinal)
                .FirstOrDefault();
            if (civilian != null && TryNativeBuild(city, civilian.Id)) LastConstruction[key] = Time.time;
        }

        private static bool TryEarlyModernHouseUpgrade(City city)
        {
            BuildingAsset target = AssetManager.buildings.get("modernbuilding");
            if (target == null || !city.hasEnoughResourcesFor(target.cost)) return false;

            foreach (string sourceId in ActorsAndBuildingsRegistry.HumanHouseBranchUpgradeSourceIds)
            {
                List<Building> sources = city.getBuildingListOfID(sourceId);
                if (sources == null) continue;
                foreach (Building source in sources.ToArray())
                {
                    if (source == null || !source.isAlive() || source.isUnderConstruction() || source.asset == null) continue;
                    BuildingAsset sourceAsset = source.asset;
                    bool originalCanUpgrade = sourceAsset.can_be_upgraded;
                    string originalUpgradeTarget = sourceAsset.upgrade_to;
                    try
                    {
                        sourceAsset.can_be_upgraded = true;
                        sourceAsset.upgrade_to = target.id;
                        if (ai.behaviours.CityBehBuild.upgradeBuilding(source, city)) return true;
                    }
                    finally
                    {
                        sourceAsset.can_be_upgraded = originalCanUpgrade;
                        sourceAsset.upgrade_to = originalUpgradeTarget;
                    }
                }
            }
            return false;
        }

        private static bool TryNativeBuild(City city, string buildingId)
        {
            BuildingAsset asset = AssetManager.buildings.get(buildingId);
            if (asset == null || !city.hasEnoughResourcesFor(asset.cost)) return false;
            return ai.behaviours.CityBehBuild.tryToBuild(city, asset) != null;
        }

        private static void TryProduceOne(City city)
        {
            ProgressionTier tier = ModernProgression.GetTier(city);
            if (tier == ProgressionTier.None) return;
            int cap = Mathf.Clamp(city.getPopulationPeople() / 10, 4, 30);
            if (CountModernUnits(city) >= cap) return;

            foreach (FactorySpec factory in ContentRegistry.Factories)
            {
                if (tier < factory.Tier || !ModernBoxSettings.Get(factory.SettingKey)) continue;
                Building producer = FindUsableBuilding(city, factory.BuildingId);
                if (producer == null) continue;
                string timerKey = city.GetHashCode() + "|" + factory.BuildingId;
                float last;
                if (LastProduction.TryGetValue(timerKey, out last) && Time.time - last < factory.Interval) continue;
                if (!city.hasEnoughResourcesFor(factory.UnitCost)) continue;

                string unitId = SelectUnit(city, factory);
                WorldTile spawnTile = producer.door_tile ?? city.getTile(false);
                if (spawnTile == null) continue;
                Actor actor = null;
                string stage = "create actor";
                try
                {
                    actor = World.world.units.createNewUnit(unitId, spawnTile, true, factory.UnitIds.Length > 1 ? 1.5f : 0.35f, null);
                    if (actor == null) return;
                    stage = "assign city";
                    actor.setCity(city);
                    stage = "assign kingdom";
                    actor.setKingdom(city.kingdom);
                    stage = "initialize unit runtime state";
                    ActorsAndBuildingsRegistry.EnsureUnitRuntimeState(actor);
                    stage = "assign producer";
                    actor.setHomeBuilding(producer);
                    stage = "assign combat role";
                    AssignCombatRole(city, actor);
                    if (actor.city != city || actor.kingdom != city.kingdom || !actor.is_profession_warrior)
                        throw new InvalidOperationException("Produced unit did not retain its city, kingdom, and warrior role.");
                    stage = "spend resources";
                    city.spendResourcesForBuildingAsset(factory.UnitCost);
                    LastProduction[timerKey] = Time.time;
                    ModernBoxDiagnostics.Info(city + " produced " + unitId + " at " + factory.BuildingId + ".");
                    return;
                }
                catch (Exception exception)
                {
                    ModernBoxDiagnostics.Error("Factory production failed for " + factory.BuildingId + " during '" + stage + "': " + exception);
                    if (actor != null && actor.isAlive())
                    {
                        try { actor.die(true, AttackType.Other, false, false); }
                        catch (Exception cleanupException) { ModernBoxDiagnostics.Error("Factory cleanup failed for " + unitId + ": " + cleanupException); }
                    }
                    return;
                }
            }
        }

        private static void AssignCombatRole(City city, Actor actor)
        {
            if (actor.asset != null && actor.asset.id == "Soldier")
            {
                city.makeWarrior(actor);
                return;
            }

            // Vehicles use their ActorAsset.default_attack and do not use city
            // equipment. City.makeWarrior() attempts to equip an empty weapon
            // slot, which is invalid for itemless aircraft such as the F22.
            actor.setProfession(UnitProfession.Warrior, true);
            city.status.warriors_current++;
        }

        private static string SelectUnit(City city, FactorySpec factory)
        {
            if (factory.UnitIds.Length == 1) return factory.UnitIds[0];
            int key = city.GetHashCode();
            int index;
            AirAlternation.TryGetValue(key, out index);
            string selected = factory.UnitIds[index % factory.UnitIds.Length];
            AirAlternation[key] = index + 1;
            return selected;
        }

        private static Building FindUsableBuilding(City city, string id)
        {
            List<Building> buildings = city.getBuildingListOfID(id);
            if (buildings == null) return null;
            return buildings.FirstOrDefault(building => building != null && building.isAlive() && building.isUsable() && !building.isUnderConstruction());
        }

        private static int CountModernUnits(City city)
        {
            int count = 0;
            foreach (Actor actor in World.world.units)
            {
                if (actor != null && actor.isAlive() && actor.city == city && actor.asset != null && ModernBoxCatalog.UnitIds.Contains(actor.asset.id)) count++;
            }
            return count;
        }

        private static void GenerateResources(City city)
        {
            int key = city.GetHashCode();
            float last;
            if (LastResources.TryGetValue(key, out last) && Time.time - last < 60f) return;
            LastResources[key] = Time.time;

            int factoryCount = 0;
            foreach (string id in ModernBoxCatalog.FactoryBuildingIds) factoryCount += city.countBuildingsOfID(id);
            int schools = city.countBuildingsOfID("school");
            int commerce = city.countBuildingsOfID("casino") + city.countBuildingsOfID("restaurant") + city.countBuildingsOfID("mall");
            int strategic = city.countBuildingsOfID("AirFactory") + city.countBuildingsOfID("MissileSilo");
            if (factoryCount > 0) city.addResourcesToRandomStockpile("Parts", Math.Min(3, factoryCount));
            if (schools > 0) city.addResourcesToRandomStockpile("CyberWareParts", Math.Min(5, schools));
            if (strategic > 0) city.addResourcesToRandomStockpile("Xenium", Math.Min(2, strategic));
            if (commerce > 0) city.addResourcesToRandomStockpile("gold", Math.Min(10, commerce));
        }

        internal static void ApplyDynamicSettings()
        {
            BuildingAsset silo = AssetManager.buildings.get("MissileSilo");
            if (silo != null) silo.tower = ModernBoxSettings.Get("NukeOption");

            bool mirvEnabled = ModernBoxSettings.Get("MIRVOption");
            foreach (EquipmentSpec spec in ContentRegistry.Equipment)
            {
                if (!ModernBoxCatalog.MirvIds.Contains(spec.Id)) continue;
                EquipmentAsset mirv = AssetManager.items.get(spec.Id);
                if (mirv == null) continue;
                mirv.projectile = mirvEnabled ? spec.Projectile : "modernbox_bullet";
                mirv.base_stats["damage"] = mirvEnabled ? spec.Damage : 0f;
                mirv.equipment_value = mirvEnabled ? spec.Value : 0;
            }
            if (_lastMirvEnabled != mirvEnabled)
            {
                _lastMirvEnabled = mirvEnabled;
                int removed = mirvEnabled ? 0 : PurgeDisabledMirvs();
                ModernBoxDiagnostics.Info("Nuclear MIRV weapons are " + (mirvEnabled
                    ? "enabled"
                    : "disabled; removed " + removed + " equipped or stored MIRV item(s)") + ".");
            }

            bool ideologyEnabled = ModernBoxSettings.Get("IdeologiesOption");
            string[] ideologies = { "Capitalist", "Communist", "Liberal", "Conservative", "Fascist", "Democratic", "Technocrat", "Luddite", "Environmental Steward", "Anarchist", "Primalism" };
            foreach (string id in ideologies)
            {
                ActorTrait trait = AssetManager.traits.get(id);
                if (trait == null) continue;
                trait.rate_birth = ideologyEnabled ? 37 : 0;
                trait.rate_inherit = ideologyEnabled ? 100 : 0;
            }
            if (_lastIdeologyEnabled != ideologyEnabled)
            {
                _lastIdeologyEnabled = ideologyEnabled;
                int assigned = ideologyEnabled ? EquipmentAndTraitsRegistry.BackfillDefaultIdeologies() : 0;
                ModernBoxDiagnostics.Info("Default ideologies are " + (ideologyEnabled
                    ? "enabled; assigned " + assigned + " missing ideology trait(s)"
                    : "disabled") + ".");
            }

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
            if (human != null) human.name_template_unit = ModernBoxSettings.Get("namesOption") ? "Modern_Names" : _humanDefaultNames;
            bool otherNames = ModernBoxSettings.Get("othernamesOption");
            if (orc != null) orc.name_template_unit = otherNames ? "Modern_Orc_Names" : _orcDefaultNames;
            if (elf != null) elf.name_template_unit = otherNames ? "Modern_Elf_Names" : _elfDefaultNames;
            if (dwarf != null) dwarf.name_template_unit = otherNames ? "Modern_Dwarf_Names" : _dwarfDefaultNames;
        }

        internal static bool IsEquipmentEnabled(string id, City city)
        {
            ProgressionTier required;
            if (!ModernBoxCatalog.EquipmentTiers.TryGetValue(id, out required)) return true;
            if (!ModernProgression.IsHumanCity(city) || ModernProgression.GetTier(city) < required) return false;
            if (ModernBoxCatalog.MirvIds.Contains(id)) return ModernBoxSettings.Get("MIRVOption");
            if (id.StartsWith("Pipe", StringComparison.Ordinal) || id == "Musket") return ModernBoxSettings.Get("PipeGunOption");
            if (id == "Sandevistan" || id == "TurboBooster") return ModernBoxSettings.Get("CyberwareOption");
            if (id == "Meth" || id == "Crack") return ModernBoxSettings.Get("DrugsOption");
            return ModernBoxSettings.Get("GunOption");
        }

        internal static bool IsMirvItem(Item item)
        {
            EquipmentAsset asset = item == null ? null : item.getAsset();
            return asset != null && ModernBoxCatalog.MirvIds.Contains(asset.id);
        }

        internal static int RemoveDisabledMirvsFromCityStorage(City city)
        {
            if (ModernBoxSettings.Get("MIRVOption") || city == null || World.world == null || World.world.items == null) return 0;
            List<long> weapons = city.getEquipmentList(EquipmentType.Weapon);
            if (weapons == null || weapons.Count == 0) return 0;

            int removed = 0;
            for (int index = weapons.Count - 1; index >= 0; index--)
            {
                Item item = World.world.items.get(weapons[index]);
                if (item != null && !IsMirvItem(item)) continue;
                // Also discard a stale ID whose backing item no longer exists.
                // City.giveItem dereferences it without a null check in build 719.
                weapons.RemoveAt(index);
                if (item != null)
                {
                    item.clearCity();
                    World.world.items.removeObject(item);
                }
                removed++;
            }
            return removed;
        }

        private static int PurgeDisabledMirvs()
        {
            if (World.world == null || World.world.items == null) return 0;
            int removed = 0;

            if (World.world.units != null)
            {
                foreach (Actor actor in World.world.units)
                {
                    ActorEquipmentSlot slot = actor == null || actor.equipment == null ? null : actor.equipment.weapon;
                    Item item = slot == null ? null : slot.getItem();
                    if (!IsMirvItem(item)) continue;
                    slot.takeAwayItem();
                    actor.setItemSpriteRenderDirty();
                    actor.setStatsDirty();
                    World.world.items.removeObject(item);
                    removed++;
                }
            }

            if (World.world.cities != null)
            {
                foreach (City city in World.world.cities.list) removed += RemoveDisabledMirvsFromCityStorage(city);
            }
            return removed;
        }
    }
}
