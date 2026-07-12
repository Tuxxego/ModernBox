using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NCMS.Utils;
using tools;
using UnityEngine;

namespace ModernBoxRewrite
{
    internal static class ActorsAndBuildingsRegistry
    {
        private const string FallbackKingdomId = "ModernKingdom";
        private const string HumanHouseUpgradeOrderId = "order_modernbox_house_upgrade";
        private static readonly Dictionary<string, string> SpawnPowerActors = new Dictionary<string, string>(StringComparer.Ordinal);
        internal static string HumanHouseUpgradeSourceId { get; private set; }
        internal static readonly List<string> HumanHouseBranchUpgradeSourceIds = new List<string>();

        internal static void RegisterUnits()
        {
            RegisterFallbackKingdom();
            foreach (ModernUnitSpec spec in ContentRegistry.Units)
            {
                ActorAsset actor = AssetManager.actor_library.clone(spec.Id, "$basic_unit$");
                actor.name_locale = spec.Id;
                actor.collective_term = "units";
                actor.use_phenotypes = false;
                actor.is_humanoid = false;
                actor.has_avatar_prefab = false;
                actor.has_advanced_textures = false;
                actor.texture_asset = new ActorTextureSubAsset("actors/" + spec.Id + "/", false);
                actor.texture_asset.shadow_texture = "unitShadow_6";
                actor.texture_asset.shadow_texture_egg = "unitShadow_6";
                actor.texture_asset.shadow_texture_baby = "unitShadow_6";
                actor.texture_asset.shadow_size = new Vector2(2.5f, 1.25f);
                actor.texture_asset.shadow_size_egg = actor.texture_asset.shadow_size;
                actor.texture_asset.shadow_size_baby = actor.texture_asset.shadow_size;
                actor.shadow_texture = "unitShadow_6";
                actor.animation_walk = FindFrames(spec.Id, "walk", new[] { "walk_0" });
                actor.animation_idle = FindFrames(spec.Id, "idle", actor.animation_walk);
                actor.animation_swim = FindFrames(spec.Id, "swim", actor.animation_walk);
                actor.animation_walk_speed = spec.Flying ? 0.08f : 0.14f;
                actor.animation_idle_speed = actor.animation_walk_speed;
                actor.animation_swim_speed = actor.animation_walk_speed;
                actor.default_attack = spec.Attack;
                actor.base_stats["health"] = spec.Health;
                actor.base_stats["speed"] = spec.Speed;
                actor.base_stats["armor"] = spec.Armor;
                actor.base_stats["damage"] = spec.Damage;
                actor.base_stats["attack_speed"] = spec.AttackSpeed;
                actor.base_stats["range"] = spec.Range;
                actor.base_stats["scale"] = spec.Scale;
                actor.base_stats["accuracy"] = 90f;
                actor.base_stats["mass"] = spec.Flying ? 15f : 35f;
                actor.base_stats["lifespan"] = spec.Id == "Soldier" ? 150f : 1000f;
                actor.nutrition_max = spec.Id == "Soldier" ? 10000 : 1000000;
                actor.actor_size = spec.Id == "Soldier" ? ActorSize.S13_Human : ActorSize.S16_Buffalo;
                actor.flying = spec.Flying;
                actor.very_high_flyer = spec.Flying;
                // WorldBox's normal death sequence rotates the body onto its side,
                // then darkens/fades it. update_z is essential for aircraft: without
                // it a dead flyer stays above the ground forever, so the fade stage
                // never begins and the last animation frame appears frozen.
                actor.update_z = true;
                actor.death_animation_angle = true;
                actor.special_dead_animation = false;
                actor.action_dead_animation = null;
                actor.force_land_creature = !spec.Flying;
                actor.force_ocean_creature = false;
                actor.damaged_by_ocean = false;
                actor.die_on_blocks = false;
                actor.ignore_blocks = spec.Flying;
                actor.move_from_block = !spec.Flying;
                actor.unit_other = true;
                actor.civ = false;
                actor.kingdom_id_wild = FallbackKingdomId;
                actor.kingdom_id_civilization = string.Empty;
                actor.count_as_unit = true;
                actor.skip_fight_logic = false;
                actor.can_attack_buildings = true;
                actor.can_level_up = true;
                actor.can_receive_traits = spec.Id == "Soldier";
                actor.can_edit_traits = spec.Id == "Soldier";
                actor.can_edit_equipment = spec.Id == "Soldier";
                actor.use_items = spec.Id == "Soldier";
                actor.take_items = spec.Id == "Soldier";
                actor.can_have_subspecies = false;
                actor.create_family_at_spawn = false;
                actor.family_limit = 0;
                actor.follow_herd = false;
                actor.source_meat = false;
                actor.has_soul = spec.Id == "Soldier";
                actor.immune_to_injuries = true;
                actor.inspect_children = false;
                actor.inspect_generation = false;
                actor.inspect_sex = false;
                actor.inspect_stats = true;
                actor.inspect_kills = true;
                actor.inspect_experience = true;
                actor.inspect_home = true;
                actor.can_be_inspected = true;
                actor.visible_on_minimap = true;
                actor.name_template_unit = spec.NameTemplate;
                actor.name_template_sets = new[] { spec.NameTemplate };
                actor.job = new[] { "attacker" };
                actor.job_citizen = new[] { "attacker" };
                actor.job_kingdom = new[] { "attacker" };
                actor.job_attacker = new[] { "attacker" };
                actor.icon = spec.IconPath;
                actor.color_hex = "#7F8C8D";
                // Build 719's selected-unit avatar assumes every warrior owns an
                // equipment container, including actors that never equip items.
                // The basic unit template leaves it null for itemless vehicles.
                actor.action_on_load += EnsureUnitRuntimeState;
                foreach (string trait in spec.Traits) actor.addTrait(trait);
                ModernLocalization.Add(spec.Id, FriendlyName(spec.Id));
                RegisterSpawnPower(spec);
            }
            RepairOrphanedUnits();
        }

        private static void RegisterFallbackKingdom()
        {
            KingdomAsset fallback = AssetManager.kingdoms.get(FallbackKingdomId);
            if (fallback == null)
            {
                // The abstract kingdom template has no color asset in build 719.
                // Clone the concrete neutral wild kingdom so createWildKingdom()
                // always receives a fully initialized color and tag collection.
                fallback = AssetManager.kingdoms.clone(FallbackKingdomId, "neutral");
                fallback.id = FallbackKingdomId;
                fallback.neutral = false;
                fallback.nature = false;
                fallback.abandoned = false;
                fallback.addTag(FallbackKingdomId);
                fallback.addTag("civ");
            }
            EnsureFallbackKingdom();
        }

        private static Kingdom EnsureFallbackKingdom()
        {
            if (World.world == null || World.world.kingdoms_wild == null) return null;
            Kingdom fallback = World.world.kingdoms_wild.get(FallbackKingdomId);
            if (fallback != null) return fallback;
            KingdomAsset asset = AssetManager.kingdoms.get(FallbackKingdomId);
            return asset == null ? null : World.world.kingdoms_wild.newWildKingdom(asset);
        }

        internal static void RepairOrphanedUnits()
        {
            if (World.world == null || World.world.units == null) return;
            Kingdom fallback = EnsureFallbackKingdom();
            foreach (Actor actor in World.world.units)
            {
                if (actor == null || actor.asset == null) continue;
                if (!ModernBoxCatalog.UnitIds.Contains(actor.asset.id)) continue;
                if (actor.kingdom == null && fallback != null) actor.setKingdom(fallback);
                EnsureUnitRuntimeState(actor);
            }
        }

        internal static void EnsureUnitRuntimeState(Actor actor)
        {
            if (actor == null) return;
            if (actor.equipment == null) actor.equipment = new ActorEquipment();
        }

        private static string[] FindFrames(string actorId, string prefix, string[] fallback)
        {
            string folder = Path.Combine(ModernBoxMod.ModFolder, "GameResources", "actors", actorId);
            if (!Directory.Exists(folder)) return fallback;
            string[] result = Directory.GetFiles(folder, prefix + "_*.png")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => name.IndexOf("_head", StringComparison.OrdinalIgnoreCase) < 0 && name.IndexOf("_item", StringComparison.OrdinalIgnoreCase) < 0)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            return result.Length == 0 ? fallback : result;
        }

        private static void RegisterSpawnPower(ModernUnitSpec spec)
        {
            string powerId = "modernbox_spawn_" + spec.Id;
            GodPower power = AssetManager.powers.clone(powerId, "$template_spawn_actor$");
            power.id = powerId;
            power.name = powerId;
            power.type = PowerActionType.PowerSpawnActor;
            power.actor_asset_id = spec.Id;
            power.rank = PowerRank.Rank0_free;
            power.show_unit_stats_overview = true;
            power.show_spawn_effect = true;
            power.multiple_spawn_tip = true;
            power.actor_spawn_height = spec.Flying ? 4f : 0f;
            power.path_icon = spec.IconPath;
            power.click_action = SpawnUnit;
            ActorAsset actor = AssetManager.actor_library.get(spec.Id);
            if (actor != null) actor.power_id = powerId;
            SpawnPowerActors[powerId] = spec.Id;
            ModernLocalization.Add(powerId, "Spawn " + FriendlyName(spec.Id));
            ModernLocalization.Add(powerId + "_description", "Spawn a ModernBox " + FriendlyName(spec.Id) + " inside a living civilized kingdom city. Empty land, ruins, and wild factions are rejected.");
        }

        private static bool SpawnUnit(WorldTile tile, string powerId)
        {
            string actorId;
            if (tile == null || !SpawnPowerActors.TryGetValue(powerId, out actorId)) return false;
            City city = tile.zone_city;
            if (city == null || city.isRekt() || city.kingdom == null || city.kingdom.wild || !city.kingdom.isCiv())
            {
                ModernBoxDiagnostics.Warn("Rejected manual " + actorId + " spawn outside a living civilized kingdom city.");
                return false;
            }
            Actor actor = World.world.units.spawnNewUnit(actorId, tile, true, true, 0f, null, false, true);
            if (actor == null) return false;
            actor.setCity(city);
            actor.setKingdom(city.kingdom);
            EnsureUnitRuntimeState(actor);
            actor.setProfession(UnitProfession.Warrior, true);
            return true;
        }

        internal static void RegisterBuildingsAndOrders()
        {
            foreach (BuildingSpec spec in ContentRegistry.Buildings)
            {
                BuildingAsset building = AssetManager.buildings.clone(spec.Id, "$building_civ_human$");
                building.id = spec.Id;
                building.sprite_path = "buildings/" + spec.Id;
                building.main_path = building.sprite_path;
                building.setAtlasID("buildings", "buildings");
                building.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
                if (building.atlas_asset == null) throw new InvalidOperationException("Building recolor atlas is unavailable for " + spec.Id + ".");
                building.city_building = true;
                building.group = spec.Civilian ? "modernbox_civilian" : "modernbox_industry";
                // The city limit is counted by BuildingAsset.type, not by asset ID.
                // A unique type makes limit=1 apply independently to each factory
                // while keeping civilian int.Max orders genuinely unlimited.
                building.type = "modernbox_type_" + spec.Id;
                building.fundament = spec.Tower ? new BuildingFundament(4, 2, 2, 0) : new BuildingFundament(2, 2, 2, 0);
                building.cost = spec.Cost;
                building.priority = spec.Id == "ModernBarracks" ? 89893289 : (spec.Tower ? 3750 : 69999);
                building.base_stats["health"] = spec.Tower ? 2500f : 3000f;
                building.base_stats["size"] = 1f;
                // In build 719 both single and batch placement add zone-wide exclusion
                // rules. Modern structures rely on the ordinary footprint check so a
                // mature city can use any genuinely open tiles.
                building.build_place_single = spec.Tower;
                building.build_place_batch = false;
                // Mature cities often have no unused 2x2 footprint. Native replacement
                // placement lets modernization replace an ordinary house instead of
                // permanently stalling every eligible order.
                building.build_prefer_replace_house = !spec.Tower;
                building.build_road_to = !spec.Tower;
                building.can_be_upgraded = false;
                building.can_be_abandoned = true;
                building.can_be_demolished = true;
                building.burnable = !spec.Tower;
                building.has_ruin_state = true;
                building.has_ruins_graphics = true;
                building.has_sprites_ruin = true;
                building.has_sprite_construction = true;
                building.has_sprites_main = true;
                building.spawn_units = false;
                building.spawn_units_asset = null;
                building.housing_slots = spec.Housing;
                building.can_units_live_here = spec.Housing > 0;
                building.can_be_living_house = spec.Housing > 0;
                if (spec.Id == "casino" || spec.Id == "restaurant" || spec.Id == "mall") building.addResource("gold", 2, false);
                if (spec.Id == "school") building.addResource("CyberWareParts", 1, false);
                if (spec.Tower)
                {
                    building.tower = true;
                    building.tower_projectile = "NUKER";
                    building.tower_projectile_amount = 1;
                    building.tower_projectile_offset = 4f;
                    building.tower_projectile_reload = 64f;
                    building.tower_attack_buildings = true;
                }
                building.loadBuildingSprites();
                ModernLocalization.Add(spec.Id, FriendlyName(spec.Id));
                ModernLocalization.Add(spec.Id + "_description", BuildingDescription(spec));
            }
            AddHumanBuildOrders();
        }

        private static void AddHumanBuildOrders()
        {
            ActorAsset human = AssetManager.actor_library.get(ModernBoxCatalog.Human);
            if (human == null || human.architecture_asset == null) throw new InvalidOperationException("Human architecture is not linked.");
            CityBuildOrderAsset orders = AssetManager.city_build_orders.get(human.build_order_template_id);
            if (orders == null) throw new InvalidOperationException("Human build order template is missing: " + human.build_order_template_id);
            foreach (BuildingSpec spec in ContentRegistry.Buildings)
            {
                string orderId = "order_" + spec.Id;
                human.architecture_asset.addBuildingOrderKey(orderId, spec.Id);
                if (orders.list.Any(order => string.Equals(order.id, orderId, StringComparison.Ordinal))) continue;
                int population, buildings;
                ModernProgression.GetRequirements(spec.Tier, out population, out buildings);
                BuildOrder orderAsset = orders.addBuilding(orderId, spec.Limit, population, buildings, false, false, 0);
                orderAsset.requirements_types = new[] { "type_bonfire" };
            }
            AddModernHouseUpgrade(human, orders);
            ModernBoxDiagnostics.Info("Added ModernBox build orders only to the human architecture. Civilian buildings are unlimited and industry limit=1.");
        }

        private static void AddModernHouseUpgrade(ActorAsset human, CityBuildOrderAsset orders)
        {
            BuildingAsset modernBuilding = AssetManager.buildings.get("modernbuilding");
            List<BuildingAsset> houseChain = FindHumanHouseChain(human.architecture_asset);
            BuildingAsset source = houseChain.Count == 0 ? null : houseChain[houseChain.Count - 1];
            if (modernBuilding == null || source == null)
                throw new InvalidOperationException("Could not create the human house -> modernbuilding upgrade path.");

            // Earlier houses keep their original upgrade_to values. The production
            // service can temporarily branch stages 1..n-1 into modernbuilding when
            // the city can afford it, then immediately restores the vanilla chain.
            HumanHouseBranchUpgradeSourceIds.Clear();
            for (int i = 1; i < houseChain.Count - 1; i++)
                HumanHouseBranchUpgradeSourceIds.Add(houseChain[i].id);

            // The terminal house has no remaining vanilla destination, so its native
            // upgrade can safely point at the modern residence permanently.
            HumanHouseUpgradeSourceId = source.id;
            source.can_be_upgraded = true;
            source.upgrade_to = modernBuilding.id;
            modernBuilding.upgraded_from = source.id;
            modernBuilding.upgrade_level = source.upgrade_level + 1;
            modernBuilding.can_be_upgraded = false;
            modernBuilding.upgrade_to = string.Empty;

            human.architecture_asset.addBuildingOrderKey(HumanHouseUpgradeOrderId, source.id);
            BuildOrder upgradeOrder = orders.list.FirstOrDefault(order => order.id == HumanHouseUpgradeOrderId);
            if (upgradeOrder == null)
            {
                // Zero thresholds make every terminal human house eligible immediately.
                // The target's original construction cost is still charged by the
                // native upgrade routine, and the order repeats for every source house.
                orders.addUpgrade(HumanHouseUpgradeOrderId, int.MaxValue, 0, 0, false, false, 0);
            }
            ModernBoxDiagnostics.Info("Enabled modern-house branches from ordinary stages " +
                string.Join(",", HumanHouseBranchUpgradeSourceIds.ToArray()) + " and terminal stage " + source.id + ".");
        }

        private static List<BuildingAsset> FindHumanHouseChain(ArchitectureAsset architecture)
        {
            List<BuildingAsset> best = new List<BuildingAsset>();
            HashSet<string> visited = new HashSet<string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> pair in architecture.building_ids_for_construction)
            {
                BuildingAsset current = AssetManager.buildings.get(pair.Value);
                if (current == null || current.type != "type_house") continue;
                visited.Clear();
                List<BuildingAsset> chain = new List<BuildingAsset>();
                while (current != null && current.type == "type_house" && visited.Add(current.id))
                {
                    chain.Add(current);
                    BuildingAsset next = string.IsNullOrEmpty(current.upgrade_to) ? null : AssetManager.buildings.get(current.upgrade_to);
                    if (next == null || next.type != "type_house") break;
                    current = next;
                }
                if (chain.Count > best.Count) best = chain;
            }
            return best;
        }

        internal static string FriendlyName(string id)
        {
            switch (id)
            {
                case "modernbuilding": return "Modern Building";
                case "ModernBarracks": return "Modern Barracks";
                case "AirFactory": return "Strategic Air Factory";
                case "BoiFactory": return "Missile System Factory";
                case "MissileSystem": return "Missile System";
                case "MIRVBomber": return "MIRV Bomber";
                case "CargoPlane": return "Cargo Plane";
                default:
                    string value = id.Replace("Factory", " Factory");
                    return char.ToUpperInvariant(value[0]) + value.Substring(1);
            }
        }

        private static string BuildingDescription(BuildingSpec spec)
        {
            if (spec.Id == "modernbuilding") return "An unlimited high-density residence that can be built directly or created by upgrading an ordinary human house.";
            if (spec.Civilian) return "An unlimited modern civilian building for human cities.";
            if (spec.Tower) return "A one-per-city nuclear missile silo.";
            return "A one-per-city ModernBox production building.";
        }
    }
}
