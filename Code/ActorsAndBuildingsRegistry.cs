using System;
using System.Collections.Generic;
using System.Linq;
using ai.behaviours;
using UnityEngine;

namespace ModernBoxM1Rewrite
{
    internal static class ActorsAndBuildingsRegistry
    {
        internal const string ModernKingdomId = "ModernKingdom";
        internal const string NomadsModernKingdomId = "nomads_ModernKingdom";
        internal const string JetRaceKingdomId = "JetRaceKingdom";
        // Every M1 building except the silo kept the template's empty type, so they all
        // counted as one type and a city could only ever have one of them.
        internal const string SharedBuildingType = "modernbox_m1_building";
        private const string JetJobId = "jet_job";
        private const string JetTaskId = "jet";
        private static readonly Dictionary<string, string> SpawnPowerActors = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, Sprite> BuildingRenderFallbacks = new Dictionary<string, Sprite>(StringComparer.Ordinal);

        // Buttonz.cs spawn powers. F55 had a power but never got a button; Soldier, Drone
        // and CargoPlane had none.
        private static readonly Dictionary<string, string> OriginalSpawnPowers = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "spawn_mirv_bomber", "MIRVBomber" }, { "spawn_airship", "Zeppelin" }, { "heli_spawn", "Heli" },
            { "gunship_spawn", "Gunship" }, { "spawn_tank", "Tank" }, { "spawn_humvee", "Humvee" },
            { "spawn_F22", "F22" }, { "spawn_F55", "F55" }, { "spawn_Railgun", "Railgun" },
            { "spawn_MissileSystem", "MissileSystem" }
        };

        internal static void RegisterUnits()
        {
            RegisterKingdoms();
            RegisterJetJob();
            EquipmentAndTraitsRegistry.RegisterUnitAttacks();
            foreach (ModernUnitSpec spec in ContentRegistry.Units) RegisterUnit(spec);
            foreach (KeyValuePair<string, string> pair in OriginalSpawnPowers) RegisterSpawnPower(pair.Key, pair.Value);
            RepairUnits();
        }

        private static void RegisterUnit(ModernUnitSpec spec)
        {
            ActorAsset source = AssetManager.actor_library.get("$basic_unit$");
            ActorAsset actor = AssetManager.actor_library.clone(spec.Id, "$basic_unit$");
            actor.name_locale = spec.Id;
            actor.collective_term = "units";
            actor.use_phenotypes = false;
            // The old game recoloured every unit's magenta team pixels; 0.51.2 only does it on request.
            actor.need_colored_sprite = true;
            actor.is_humanoid = spec.Humanoid;
            if (spec.Humanoid)
            {
                // A Soldier can end up leading a city. CityBehBuild then reads the leader's
                // build orders, which $basic_unit$ doesn't have, so borrow the human ones.
                ActorAsset raceActor = AssetManager.actor_library.get(spec.Race);
                if (raceActor != null && !string.IsNullOrEmpty(raceActor.build_order_template_id))
                    actor.build_order_template_id = raceActor.build_order_template_id;
                if (raceActor != null && !string.IsNullOrEmpty(raceActor.banner_id))
                    actor.banner_id = raceActor.banner_id;
            }
            actor.has_avatar_prefab = false;
            actor.has_advanced_textures = false;
            // M1's art uses the old flat layout GameResources/actors/<id>/walk_0.png.
            actor.texture_asset = new ActorTextureSubAsset("actors/" + spec.Id + "/", false);
            actor.texture_asset.texture_path_main = "actors/" + spec.Id;
            actor.texture_asset.texture_path_baby = null;
            actor.texture_asset.shadow_texture = "unitShadow_6";
            actor.texture_asset.shadow_texture_egg = "unitShadow_6";
            actor.texture_asset.shadow_texture_baby = "unitShadow_6";
            actor.texture_asset.shadow_size = new Vector2(2.5f, 1.25f);
            actor.texture_asset.shadow_size_egg = actor.texture_asset.shadow_size;
            actor.texture_asset.shadow_size_baby = actor.texture_asset.shadow_size;
            actor.shadow_texture = "unitShadow_6";
            actor.animation_walk = spec.Walk;
            actor.animation_idle = spec.Idle;
            actor.animation_swim = spec.Swim;
            // _mob had disableJumpAnimation, so vehicles slide instead of hopping.
            actor.disable_jump_animation = true;
            if (!spec.Humanoid) actor.animation_speed_based_on_walk_speed = false;

            actor.default_attack = spec.Attack == "base" ? "base_attack" : EquipmentAndTraitsRegistry.AttackId(spec.Attack);
            actor.base_stats["health"] = spec.Health;
            actor.base_stats["speed"] = spec.Speed * M1OriginalStats.SpeedScale;
            actor.base_stats["damage"] = spec.Damage;
            actor.base_stats["attack_speed"] = M1AttackSpeed.ForUnit(spec.AttackSpeed, spec.Attack);
            // Anything the original didn't set keeps the old ActorAsset default.
            actor.base_stats["armor"] = spec.Armor >= 0f ? spec.Armor : 0f;
            actor.base_stats["range"] = spec.Range >= 0f ? spec.Range : 1f;
            actor.base_stats["scale"] = spec.Scale >= 0f ? spec.Scale : 0.1f;
            actor.base_stats["knockback"] = 1f;
            actor.base_stats["targets"] = 1f;
            // The old engine never read accuracy, so every unit gets the same middling value.
            actor.base_stats["accuracy"] = 90f;
            actor.base_stats["warfare"] = spec.Warfare;
            // _mob had no max_age, which meant it never died of old age.
            actor.base_stats["lifespan"] = 0f;
            actor.nutrition_max = source == null ? 100 : source.nutrition_max;
            actor.actor_size = spec.Humanoid ? ActorSize.S13_Human : ActorSize.S16_Buffalo;
            actor.flying = spec.Flying;
            actor.very_high_flyer = spec.Flying;
            // Dead flyers need update_z to fall, or their fade never starts.
            actor.update_z = true;
            actor.death_animation_angle = true;
            actor.special_dead_animation = false;
            actor.action_dead_animation = null;
            actor.can_be_killed_by_stuff = true;
            actor.force_land_creature = !spec.Flying;
            actor.force_ocean_creature = false;
            actor.damaged_by_ocean = false;
            actor.die_on_blocks = spec.DieOnBlocks;
            actor.ignore_blocks = spec.IgnoreBlocks;
            actor.move_from_block = spec.MoveFromBlock;
            actor.unit_other = true;
            actor.civ = false;
            actor.kingdom_id_wild = ModernKingdomId;
            actor.kingdom_id_civilization = string.Empty;
            actor.count_as_unit = true;
            actor.skip_fight_logic = false;
            actor.can_attack_buildings = true;
            actor.can_level_up = true;
            actor.can_receive_traits = spec.Humanoid;
            actor.can_edit_traits = spec.Humanoid;
            actor.can_edit_equipment = spec.Equipment;
            actor.use_items = spec.Equipment;
            actor.take_items = spec.Equipment;
            actor.default_weapons = spec.DefaultWeapons;
            if (!spec.Humanoid)
            {
                // _mob's jobs had no sleeping; keep the newer sleep decisions off the vehicles.
                actor.decision_ids = actor.decision_ids == null
                    ? new List<string>()
                    : actor.decision_ids.Where(id => string.IsNullOrEmpty(id) || id.IndexOf("sleep", StringComparison.OrdinalIgnoreCase) < 0).ToList();
            }
            // Flat M1 sheets have no phenotype skins, so no subspecies are made for these units.
            actor.can_have_subspecies = false;
            actor.has_baby_form = false;
            actor.create_family_at_spawn = false;
            actor.family_limit = 0;
            actor.follow_herd = false;
            actor.source_meat = false;
            actor.has_soul = spec.Humanoid;
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
            actor.name_template_sets = null;
            string[] jobs = { spec.JetJob ? JetJobId : "attacker" };
            actor.job = jobs;
            actor.job_baby = jobs;
            actor.job_citizen = jobs;
            actor.job_kingdom = jobs;
            actor.job_attacker = jobs;
            // _mob units made no sounds.
            actor.sound_spawn = null;
            actor.sound_attack = null;
            actor.sound_idle = null;
            actor.sound_idle_loop = null;
            actor.sound_death = null;
            // ActorAsset.getIconPath always prepends ui/Icons/, so keep a valid native icon id;
            // M1ActorIconFallbackPatch hands out the unit's own first frame instead.
            actor.icon = source == null ? string.Empty : source.icon;
            actor.color_hex = "#33724D";
            string avatarPath = "actors/" + spec.Id + "/" + spec.Walk[0];
            actor.get_override_avatar_frames = pActor =>
            {
                Sprite sprite = SpriteTextureLoader.getSprite(avatarPath);
                return sprite == null ? Array.Empty<Sprite>() : new[] { sprite };
            };
            actor.has_override_avatar_frames = true;
            actor.action_on_load += EnsureUnitRuntimeState;
            foreach (string trait in spec.Traits)
                if (AssetManager.traits.get(trait) != null) actor.addTrait(trait);
            ModernLocalization.Add(spec.Id, spec.Id);
        }

        // ModernKingdoms.cs: a hidden kingdom that is hostile to every civilization and to
        // itself; units belong to it only until they join a city.
        private static void RegisterKingdoms()
        {
            string[] enemies = { ModernKingdomId, NomadsModernKingdomId, "human", "orc", "elf", "dwarf" };
            RegisterModernKingdom(ModernKingdomId, false, enemies);
            RegisterModernKingdom(NomadsModernKingdomId, true, enemies);
            KingdomAsset nomads = AssetManager.kingdoms.get(NomadsModernKingdomId);
            nomads.list_tags.Remove(NomadsModernKingdomId);
            nomads.addTag(ModernKingdomId);

            foreach (string race in new[] { "human", "orc", "elf", "dwarf" })
            {
                KingdomAsset civ = AssetManager.kingdoms.get(race);
                if (civ != null)
                {
                    civ.addEnemyTag(ModernKingdomId);
                    civ._cached_enemies.Clear();
                }
                KingdomAsset nomad = AssetManager.kingdoms.get("nomads_" + race);
                if (nomad != null)
                {
                    nomad.addEnemyTag(ModernKingdomId);
                    nomad.addEnemyTag(NomadsModernKingdomId);
                    nomad._cached_enemies.Clear();
                }
            }

            // Aircraft.cs made this one but no unit ever used it.
            if (AssetManager.kingdoms.get(JetRaceKingdomId) == null)
            {
                KingdomAsset jetRace = AssetManager.kingdoms.clone(JetRaceKingdomId, "neutral");
                jetRace.id = JetRaceKingdomId;
                jetRace.neutral = false;
                jetRace.nomads = true;
                jetRace.mobs = true;
                jetRace.friendly_tags.Clear();
                jetRace.enemy_tags.Clear();
                jetRace.list_tags.Clear();
                jetRace.addTag(JetRaceKingdomId);
                foreach (string race in new[] { "human", "dwarf", "elf", "orc" }) jetRace.addFriendlyTag(race);
            }
            EnsureModernKingdom();
        }

        private static void RegisterModernKingdom(string id, bool nomads, IEnumerable<string> enemies)
        {
            // Cloned from the neutral kingdom so createWildKingdom gets a colour asset.
            KingdomAsset kingdom = AssetManager.kingdoms.get(id) ?? AssetManager.kingdoms.clone(id, "neutral");
            kingdom.id = id;
            kingdom.neutral = false;
            kingdom.nature = false;
            kingdom.abandoned = false;
            kingdom.concept = false;
            kingdom.nomads = nomads;
            kingdom.count_as_danger = true;
            kingdom.friendly_tags.Clear();
            kingdom.enemy_tags.Clear();
            kingdom.list_tags.Clear();
            kingdom._cached_enemies.Clear();
            kingdom.addTag("civ");
            kingdom.addTag(id);
            foreach (string enemy in enemies) kingdom.addEnemyTag(enemy);
        }

        internal static Kingdom EnsureModernKingdom()
        {
            if (World.world == null || World.world.kingdoms_wild == null) return null;
            Kingdom kingdom = World.world.kingdoms_wild.get(ModernKingdomId);
            if (kingdom == null)
            {
                KingdomAsset asset = AssetManager.kingdoms.get(ModernKingdomId);
                if (asset == null) return null;
                if (asset.default_kingdom_color == null)
                {
                    // 0.51.2 wild kingdoms need a colour; the old game didn't.
                    ColorAsset color = ColorAsset.tryMakeNewColorAsset("#8C8C8C");
                    if (color != null)
                    {
                        color.id = "kingdom_library_color_" + asset.id;
                        asset.default_kingdom_color = color;
                    }
                }
                kingdom = World.world.kingdoms_wild.newWildKingdom(asset);
            }
            if (kingdom != null && kingdom.data != null)
            {
                // The unit inspector reads its banner through this; hidden kingdoms leave it empty.
                if (string.IsNullOrEmpty(kingdom.data.original_actor_asset)) kingdom.data.original_actor_asset = "human";
                kingdom.data.banner_background_id = 0;
                kingdom.data.banner_icon_id = 0;
            }
            return kingdom;
        }

        internal static bool UsesModernKingdomBanner(Kingdom kingdom)
        {
            if (kingdom == null) return false;
            if (kingdom.asset != null && kingdom.asset.id == ModernKingdomId) return true;
            return kingdom.king != null && kingdom.king.asset != null && ModernBoxCatalog.UnitIds.Contains(kingdom.king.asset.id);
        }

        // NewJobs.cs: fly a random 8-direction path while looking for a fight.
        private static void RegisterJetJob()
        {
            if (AssetManager.tasks_actor.get(JetTaskId) == null)
            {
                BehaviourTaskActor task = new BehaviourTaskActor { id = JetTaskId, in_combat = true, locale_key = "task_unit_fight" };
                AssetManager.tasks_actor.add(task);
                task.addBeh(new BehFindRandomTile8Directions());
                task.addBeh(new BehGoToTileTarget());
                task.addBeh(new BehFightCheckEnemyIsOk());
                task.addBeh(new BehFindRandomTile8Directions());
                task.addBeh(new BehGoToTileTarget());
                task.addBeh(new BehFindRandomTile8Directions());
                task.addBeh(new BehGoToTileTarget());
                task.addBeh(new BehRestartTask());
            }
            if (AssetManager.job_actor.get(JetJobId) == null)
            {
                ActorJob job = new ActorJob { id = JetJobId };
                AssetManager.job_actor.add(job);
                job.addTask(JetTaskId);
            }
        }

        internal static void RepairUnits()
        {
            if (World.world == null || World.world.units == null) return;
            Kingdom modern = EnsureModernKingdom();
            foreach (Actor actor in World.world.units)
            {
                if (actor == null || actor.asset == null || !ModernBoxCatalog.UnitIds.Contains(actor.asset.id)) continue;
                if (actor.kingdom == null && modern != null) actor.setKingdom(modern);
                EnsureUnitRuntimeState(actor);
            }
        }

        internal static void EnsureUnitRuntimeState(Actor actor)
        {
            if (actor == null) return;
            // The selected-unit panel assumes every warrior has an equipment container.
            if (actor.equipment == null) actor.equipment = new ActorEquipment();
            ModernUnitSpec spec = actor.asset == null ? null : ContentRegistry.FindUnit(actor.asset.id);
            if (spec == null) return;
            if (!spec.Humanoid)
            {
                // Vehicles have no body: no subspecies (hunger, ageing) and no sleep.
                if (actor.subspecies != null)
                {
                    if (World.world != null && World.world.subspecies != null) World.world.subspecies.unitDied(actor.subspecies);
                    actor.subspecies = null;
                    if (actor.data != null) actor.data.subspecies = 0L;
                    actor.setStatsDirty();
                }
                if (actor.hasStatus("sleeping")) actor.stopSleeping();
            }
            else if (actor.city != null && actor.subspecies == null)
            {
                Subspecies citySubspecies = actor.city.getMainSubspecies();
                if (citySubspecies != null) actor.setSubspecies(citySubspecies);
            }
            if (actor.kingdom == null)
            {
                Kingdom modern = EnsureModernKingdom();
                if (modern != null) actor.setKingdom(modern);
            }
        }

        internal static bool IsVehicle(Actor actor)
        {
            if (actor == null || actor.asset == null) return false;
            ModernUnitSpec spec = ContentRegistry.FindUnit(actor.asset.id);
            return spec != null && !spec.Humanoid;
        }

        private static void RegisterSpawnPower(string powerId, string unitId)
        {
            ModernUnitSpec spec = ContentRegistry.FindUnit(unitId);
            GodPower power = AssetManager.powers.clone(powerId, "$template_spawn_actor$");
            power.id = powerId;
            power.name = powerId;
            power.type = PowerActionType.PowerSpawnActor;
            power.actor_asset_id = unitId;
            power.rank = PowerRank.Rank0_free;
            power.show_unit_stats_overview = true;
            power.show_spawn_effect = true;
            power.multiple_spawn_tip = true;
            // _spawnActor: every M1 spawn power kept the template's height of 3.
            power.actor_spawn_height = 3f;
            power.path_icon = spec == null ? null : spec.IconPath;
            power.click_action = SpawnUnit;
            ActorAsset actor = AssetManager.actor_library.get(unitId);
            if (actor != null) actor.power_id = powerId;
            SpawnPowerActors[powerId] = unitId;
        }

        // Buttonz.action_spawn_jet: only inside a city, and the unit joins that city's kingdom.
        // Buttonz.spawnUnit played the spawn whoosh and effect and made the unit miracle born.
        private static bool SpawnUnit(WorldTile tile, string powerId)
        {
            string actorId;
            if (tile == null || !SpawnPowerActors.TryGetValue(powerId, out actorId)) return false;
            City city = tile.zone_city;
            if (city == null || city.isRekt() || city.kingdom == null)
            {
                WorldTip.showNow("You must spawn this vehicle within a kingdom.", false, "top", 3f);
                return false;
            }
            MusicBox.playSound("event:/SFX/UNIQUE/SpawnWhoosh", tile.pos.x, tile.pos.y, false, false);
            EffectsLibrary.spawn("fx_spawn", tile);
            GodPower power = AssetManager.powers.get(powerId);
            float height = power == null ? 3f : power.actor_spawn_height;
            Actor actor = World.world.units.spawnNewUnit(actorId, tile, true, true, height, null, false, true);
            if (actor == null) return false;
            actor.setCity(city);
            actor.setKingdom(city.kingdom);
            EnsureUnitRuntimeState(actor);
            actor.setProfession(UnitProfession.Warrior, true);
            return true;
        }

        internal static void RegisterBuildingsAndOrders()
        {
            foreach (BuildingSpec spec in ContentRegistry.Buildings) RegisterBuilding(spec);
            AddBuildOrders();
        }

        private static void RegisterBuilding(BuildingSpec spec)
        {
            BuildingAsset building = AssetManager.buildings.clone(spec.Id, "$building_civ_human$");
            building.id = spec.Id;
            building.sprite_path = "buildings/" + spec.Id;
            building.main_path = building.sprite_path;
            building.setAtlasID("buildings", "buildings");
            building.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
            if (building.atlas_asset == null) throw new InvalidOperationException("Building atlas is unavailable for " + spec.Id + ".");
            building.city_building = true;
            building.group = "modernbox_m1";
            building.type = spec.Tower ? M1Silo.SiloId : SharedBuildingType;
            building.fundament = spec.Tower ? new BuildingFundament(4, 2, 2, 0) : new BuildingFundament(2, 2, 2, 0);
            building.cost = spec.Cost;
            building.priority = spec.Priority;
            building.base_stats["health"] = spec.Health;
            if (!spec.LivingHouse) building.base_stats["size"] = 1f;
            building.burnable = spec.Burnable;
            building.build_place_single = spec.PlaceSingle;
            building.build_place_batch = !spec.PlaceSingle;
            building.build_road_to = !spec.Tower;
            building.can_be_upgraded = false;
            building.can_be_abandoned = true;
            building.can_be_demolished = true;
            building.has_ruin_state = true;
            building.has_ruins_graphics = true;
            building.has_sprites_ruin = true;
            building.has_sprite_construction = true;
            building.has_sprites_main = true;
            // Units come from ProductionService, which copies the old UnitSpawner.
            building.spawn_units = false;
            building.spawn_units_asset = null;
            building.housing_slots = spec.Housing;
            building.can_units_live_here = spec.Housing > 0;
            building.can_be_living_house = spec.LivingHouse && spec.Housing > 0;
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
            EnsureBuildingRenderSprites(building);
            // Commerce.cs gave the buildings no names, so the game showed their ids.
            ModernLocalization.Add(spec.Id, spec.Id);
            ModernLocalization.Add(spec.Id + "_description", string.Empty);
        }

        internal static void EnsureBuildingRenderSprites(BuildingAsset building)
        {
            if (building == null || building.building_sprites == null)
                throw new InvalidOperationException("Building sprites were not initialized.");
            Sprite fallback = null;
            List<BuildingAnimationData> animations = building.building_sprites.animation_data;
            if (animations != null)
            {
                foreach (BuildingAnimationData animation in animations)
                {
                    if (animation == null || animation.main == null) continue;
                    fallback = animation.main.FirstOrDefault(sprite => sprite != null);
                    if (fallback != null) break;
                }
            }
            if (fallback == null)
                throw new InvalidOperationException("Building " + building.id + " has no usable main sprite at " + building.sprite_path + ".");
            // M1's buildings only ship main frames; every other state reuses the first one.
            BuildingRenderFallbacks[building.id] = fallback;
            if (animations != null)
            {
                foreach (BuildingAnimationData animation in animations)
                {
                    if (animation == null) continue;
                    animation.main = RepairSpriteArray(animation.main, fallback);
                    animation.main_disabled = RepairSpriteArray(animation.main_disabled, fallback);
                    animation.special = RepairSpriteArray(animation.special, fallback);
                    animation.ruins = RepairSpriteArray(animation.ruins, fallback);
                    animation.spawn = RepairSpriteArray(animation.spawn, fallback);
                }
            }
            if (building.building_sprites.construction == null) building.building_sprites.construction = fallback;
            building.has_sprite_construction = true;
        }

        internal static bool TryGetBuildingRenderFallback(BuildingAsset building, out Sprite sprite)
        {
            sprite = null;
            return building != null && !string.IsNullOrEmpty(building.id) &&
                   BuildingRenderFallbacks.TryGetValue(building.id, out sprite) && sprite != null;
        }

        private static Sprite[] RepairSpriteArray(Sprite[] sprites, Sprite fallback)
        {
            if (sprites == null || sprites.Length == 0) return new[] { fallback };
            for (int index = 0; index < sprites.Length; index++)
                if (sprites[index] == null) sprites[index] = fallback;
            return sprites;
        }

        // Commerce.cs added every order to kingdom_base, which all four civ races shared,
        // each one limit 1 from 50 people and 16 buildings.
        private static void AddBuildOrders()
        {
            foreach (string race in ModernBoxCatalog.SupportedRaces)
            {
                ActorAsset species = AssetManager.actor_library.get(race);
                if (species == null || species.architecture_asset == null) continue;
                CityBuildOrderAsset orders = CreatePrivateBuildOrders(species, race);
                if (orders == null) continue;
                foreach (BuildingSpec spec in ContentRegistry.Buildings)
                    species.architecture_asset.addBuildingOrderKey(OrderId(race, spec.Id), spec.Id);
                foreach (BuildingSpec spec in ContentRegistry.Buildings)
                {
                    if (!spec.HasOrder) continue;
                    string orderId = OrderId(race, spec.Id);
                    if (!orders.list.Any(order => order.id == orderId)) orders.addBuilding(orderId, 1, 50, 16, false, false, 0);
                }
                M1Silo.RegisterOrder(race, orders);
                // The generation cache was built before mods load, so rebuild it.
                orders.prepareForAssetGeneration();
            }
        }

        internal static string OrderId(string race, string buildingId)
        {
            return "order_m1_" + race + "_" + buildingId;
        }

        // The four core species share one vanilla template in 0.51.2, and other species use
        // it too. Race-specific order ids in that shared list make the others throw, so each
        // race gets its own copy.
        private static CityBuildOrderAsset CreatePrivateBuildOrders(ActorAsset species, string race)
        {
            string privateId = "build_order_modernbox_m1_" + race;
            CityBuildOrderAsset privateOrders = AssetManager.city_build_orders.get(privateId);
            if (privateOrders == null)
            {
                CityBuildOrderAsset nativeOrders = AssetManager.city_build_orders.get(species.build_order_template_id);
                if (nativeOrders == null) return null;
                privateOrders = new CityBuildOrderAsset { id = privateId };
                privateOrders.list.AddRange(nativeOrders.list);
                AssetManager.city_build_orders.add(privateOrders);
            }
            species.build_order_template_id = privateId;
            return privateOrders;
        }

        // The M1 building an order builds, for the city's race (null for other orders).
        internal static BuildingSpec SpecForOrder(BuildOrder order, City city)
        {
            if (order == null || string.IsNullOrEmpty(order.id) || !order.id.StartsWith("order_m1_", StringComparison.Ordinal)) return null;
            string prefix = "order_m1_" + ModernBoxCatalog.GetRace(city) + "_";
            return order.id.StartsWith(prefix, StringComparison.Ordinal) ? ContentRegistry.FindBuilding(order.id.Substring(prefix.Length)) : null;
        }
    }
}
