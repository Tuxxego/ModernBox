using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NCMS.Utils;
using tools;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    internal static class ActorsAndBuildingsRegistry
    {
        private const string FallbackKingdomId = "ModernKingdom";
        private const string InvasionKingdomId = "ModernBoxM2Invasion";
        internal const string AssimilatorKingdomId = "ModernBoxM2Assimilators";
        internal const string WalkerKingdomId = "ModernBoxM2Walkers";
        internal const string UndeadKingdomId = "ModernBoxM2Undead";
        internal const string NativeUndeadKingdomId = "undead";
        internal const string CrusaderKingdomId = "ModernBoxM2Crusaders";
        private const string InvasionActorFlag = "modernbox_m2_invasion_actor";
        private const string FactionFlagPrefix = "modernbox_m2_faction_";
        private const string NavalTraitMigrationFlag = "modernbox_m2_naval_trait_migrated";
        private static readonly HashSet<string> NaturalAlienJungleActors = new HashSet<string>(StringComparer.Ordinal)
        {
            "alienwisp", "pantherax", "pterax", "rhinokinglor", "geckoid", "peones", "xenodogo"
        };
        private static readonly Dictionary<string, string> SpawnPowerActors = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, Sprite> BuildingRenderFallbacks = new Dictionary<string, Sprite>(StringComparer.Ordinal);
        internal static readonly Dictionary<string, string> UpgradeSources = new Dictionary<string, string>(StringComparer.Ordinal);
        internal static string HumanHouseUpgradeSourceId { get; private set; }
        internal static readonly List<string> HumanHouseBranchUpgradeSourceIds = new List<string>();

        internal static void RegisterUnits()
        {
            RegisterFallbackKingdom();
            RegisterInvasionKingdoms();
            MissileSystemService.RegisterDecision();
            foreach (ModernUnitSpec spec in ContentRegistry.Units)
            {
                string baseId = spec.Boat ? CurrentBoatBase(spec.Id) : spec.BaseAsset;
                if (string.IsNullOrEmpty(baseId) || AssetManager.actor_library.get(baseId) == null) baseId = "$basic_unit$";
                ActorAsset sourceActor = AssetManager.actor_library.get(baseId);
                ActorAsset actor = AssetManager.actor_library.clone(spec.Id, baseId);
                bool mechanicalUnit = IsMechanicalSpec(spec);
                string inheritedWildKingdom = actor.kingdom_id_wild;
                ActorTextureSubAsset inheritedTextureAsset = actor.texture_asset;
                string[] inheritedWalkAnimation = actor.animation_walk;
                string[] inheritedIdleAnimation = actor.animation_idle;
                string[] inheritedSwimAnimation = actor.animation_swim;
                float inheritedWalkAnimationSpeed = actor.animation_walk_speed;
                float inheritedIdleAnimationSpeed = actor.animation_idle_speed;
                float inheritedSwimAnimationSpeed = actor.animation_swim_speed;
                string inheritedIcon = actor.icon;
                actor.name_locale = spec.Id;
                actor.collective_term = "units";
                actor.use_phenotypes = false;
                actor.is_humanoid = spec.Humanoid;
                if (spec.Humanoid && !string.IsNullOrEmpty(spec.Race))
                {
                    // A city can promote a produced M2 soldier when its previous
                    // ruler dies. CityBehBuild then resolves construction through
                    // the leader asset's build-order template. $basic_unit$ has no
                    // civilization template, so copy the matching native race's
                    // template onto each M2 humanoid asset at registration time.
                    ActorAsset raceActor = AssetManager.actor_library.get(spec.Race);
                    if (raceActor != null && !string.IsNullOrEmpty(raceActor.build_order_template_id))
                        actor.build_order_template_id = raceActor.build_order_template_id;
                    if (raceActor != null && !string.IsNullOrEmpty(raceActor.banner_id))
                        actor.banner_id = raceActor.banner_id;
                }
                actor.has_avatar_prefab = false;
                actor.has_advanced_textures = false;
                string textureFolder = string.IsNullOrEmpty(spec.TextureFolder) ? spec.Id : spec.TextureFolder;
                bool inheritCurrentWalkerArt = textureFolder == "t_walker";
                if (inheritCurrentWalkerArt)
                {
                    // M2's newwalker and normalwalker reused the pre-2025
                    // "walker" actor and "actors/t_walker" sheet. Both were
                    // removed in WorldBox 0.51.2; cold_one is the current engine
                    // successor. Reuse the source texture object itself rather than
                    // its freshly cloned metadata. The source object has already
                    // loaded its shadows and registered its packed sprites; loading
                    // a second copy after atlas creation makes PixelBag read packed
                    // sprite coordinates against the original texture and crash.
                    actor.texture_asset = sourceActor == null ? inheritedTextureAsset : sourceActor.texture_asset;
                    actor.has_advanced_textures = sourceActor != null && sourceActor.has_advanced_textures;
                    actor.animation_walk = inheritedWalkAnimation;
                    actor.animation_idle = inheritedIdleAnimation;
                    actor.animation_swim = inheritedSwimAnimation;
                    if (string.IsNullOrEmpty(inheritedIcon) && inheritedTextureAsset != null &&
                        inheritedWalkAnimation != null && inheritedWalkAnimation.Length > 0)
                    {
                        inheritedIcon = inheritedTextureAsset.texture_path_main + "/" + inheritedWalkAnimation[0];
                    }
                    if (!string.IsNullOrEmpty(inheritedIcon))
                        spec.IconPath = inheritedIcon.IndexOf('/') >= 0 ? inheritedIcon : "ui/Icons/" + inheritedIcon;
                }
                else
                {
                    actor.texture_asset = new ActorTextureSubAsset("actors/" + textureFolder + "/", false);
                    // M2's actor art uses the legacy flat layout
                    // GameResources/actors/<id>/walk_0.png rather than the current
                    // GameResources/actors/<id>/main/walk_0.png layout. Build 719's
                    // ActorTextureSubAsset constructor probes the current layout and,
                    // when it cannot find /main, falls back to /heads_male. Point the
                    // main texture path back at M2's actual flat folder explicitly.
                    actor.texture_asset.texture_path_main = "actors/" + textureFolder;
                    actor.texture_asset.texture_path_baby = null;
                    actor.texture_asset.shadow_texture = "unitShadow_6";
                    actor.texture_asset.shadow_texture_egg = "unitShadow_6";
                    actor.texture_asset.shadow_texture_baby = "unitShadow_6";
                    actor.texture_asset.shadow_size = new Vector2(2.5f, 1.25f);
                    actor.texture_asset.shadow_size_egg = actor.texture_asset.shadow_size;
                    actor.texture_asset.shadow_size_baby = actor.texture_asset.shadow_size;
                    actor.animation_walk = FindFrames(textureFolder, "walk", new[] { "walk_0" });
                    actor.animation_idle = FindFrames(textureFolder, "idle", actor.animation_walk);
                    actor.animation_swim = FindFrames(textureFolder, "swim", actor.animation_walk);
                }
                ApplyOriginalM2Animations(actor, spec, inheritedIdleAnimation);
                actor.shadow_texture = "unitShadow_6";
                // Original M2 never supplied custom animation timing. It inherited
                // the source actor's timing for aircraft, ground vehicles and boats.
                // The former rewrite values (especially 0.08 for aircraft) could
                // leave large looping sprites apparently frozen on one trail frame.
                actor.animation_walk_speed = inheritedWalkAnimationSpeed;
                actor.animation_idle_speed = inheritedIdleAnimationSpeed;
                actor.animation_swim_speed = inheritedSwimAnimationSpeed;
                // Vehicles in original M2 slide across the map as rigid machines.
                // The current basic-unit template adds a creature hop whose vertical
                // bob is especially obvious on large tanks and hover vehicles.
                bool rigidVehicle = !spec.Boat && spec.Traits != null && spec.Traits.Contains("spawnedvehicle");
                if (rigidVehicle)
                {
                    actor.disable_jump_animation = true;
                    actor.animation_speed_based_on_walk_speed = false;
                    ApplyOriginalGroundVehicleIdle(actor, spec);
                }
                if (spec.Boat)
                {
                    // Current native boats decouple their frame clock from movement
                    // speed. Without this, an idle or slowly turning M2 ship can stay
                    // on walk_0 even though its original swim sequence is present.
                    actor.disable_jump_animation = true;
                    actor.animation_speed_based_on_walk_speed = false;
                }
                actor.default_attack = CurrentAttackId(spec.Attack);
                actor.base_stats["health"] = spec.Health;
                actor.base_stats["speed"] = spec.Speed;
                actor.base_stats["armor"] = spec.Armor;
                actor.base_stats["damage"] = spec.Damage;
                actor.base_stats["attack_speed"] = spec.AttackSpeed;
                actor.base_stats["range"] = spec.Range;
                if (spec.Projectiles > 0f) actor.base_stats["projectiles"] = spec.Projectiles;
                // Most original M2 boats did not set scale at all; they inherited
                // it from _boat. Only the six actors below had explicit overrides.
                if (!spec.Boat || HasExplicitOriginalBoatScale(spec.Id))
                    actor.base_stats["scale"] = spec.Scale;
                actor.base_stats["accuracy"] = 90f;
                actor.base_stats["mass"] = spec.Flying ? 15f : 35f;
                actor.base_stats["lifespan"] = spec.Id == "Soldier" ? 150f : 1000f;
                // Mechanical actors do not use the biological nutrition system.
                // Preserve the cloned actor's harmless stored capacity instead of
                // giving vehicles a huge creature-food reserve. Whether nutrition
                // is active is controlled by subspecies; EnsureUnitRuntimeState
                // deliberately keeps vehicles out of civilization subspecies.
                actor.nutrition_max = sourceActor == null ? 100 : sourceActor.nutrition_max;
                actor.actor_size = spec.Boat ? ActorSize.S17_Dragon : (spec.Id == "Soldier" ? ActorSize.S13_Human : ActorSize.S16_Buffalo);
                if (spec.Boat) actor.cost = new ConstructionCost(1, 0, 0, 1);
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
                actor.can_be_killed_by_stuff = true;
                actor.force_land_creature = !spec.Flying && !spec.Boat;
                actor.force_ocean_creature = spec.Boat;
                if (spec.Boat)
                {
                    // Current docks resolve three built-in boat categories through
                    // the city's architecture. M2 docks instead carry explicit M2
                    // actor IDs. Give every naval actor the real boat runtime and a
                    // unique counter key; the scoped dock resolver below maps that
                    // key back to this exact actor asset.
                    actor.is_boat = true;
                    actor.boat_type = spec.Id;
                    actor.is_boat_transport = !IsCivilianBoat(spec.Id);
                    actor.draw_boat_mark = true;
                    actor.draw_boat_mark_big = actor.is_boat_transport;
                }
                actor.damaged_by_ocean = false;
                actor.die_on_blocks = false;
                actor.ignore_blocks = spec.Flying;
                actor.move_from_block = !spec.Flying && !spec.Boat;
                actor.unit_other = true;
                actor.civ = false;
                // Build 719 assumes every live actor has a kingdom during meta and
                // chunk indexing.  M2's creature templates often clone from a
                // minimal unit with no default wild kingdom, which leaves manually
                // spawned invasion actors orphaned and crashes the next sim tick.
                // Ordinary Alien Jungle fauna should retain the natural wild
                // faction supplied by its vanilla base actor. Creature powers
                // and automatic invasion batches are moved into the dedicated
                // hostile invasion faction after spawning.
                actor.kingdom_id_wild = spec.Role == M2UnitRole.Creature && !string.IsNullOrEmpty(inheritedWildKingdom)
                    ? inheritedWildKingdom
                    : FallbackKingdomId;
                actor.kingdom_id_civilization = string.Empty;
                actor.count_as_unit = true;
                actor.skip_fight_logic = spec.Boat && IsCivilianBoat(spec.Id);
                actor.can_attack_buildings = true;
                actor.can_level_up = true;
                actor.can_receive_traits = spec.Humanoid;
                actor.can_edit_traits = spec.Humanoid;
                actor.can_edit_equipment = spec.Equipment;
                actor.use_items = spec.Equipment;
                actor.take_items = spec.Equipment;
                // $basic_unit$ can inherit a default-weapon entry which is not a
                // current EquipmentAsset. ActorManager tries to create that weapon
                // before action_on_load runs, crashing walker spawns/evolution.
                // M2 actors already carry an explicit default_attack; real equipment
                // is supplied later by the city crafting system.
                actor.default_weapons = Array.Empty<string>();
                if (mechanicalUnit)
                {
                    // Original M2's _mob attacker/jet jobs and M5's baseWarUnit
                    // decision set contain no sleep decision. Build 719's basic
                    // unit template can contribute the new sleep-cycle decisions,
                    // so copy and filter the list instead of mutating the shared
                    // template collection.
                    actor.decision_ids = actor.decision_ids == null
                        ? new List<string>()
                        : actor.decision_ids.Where(id => string.IsNullOrEmpty(id) ||
                            id.IndexOf("sleep", StringComparison.OrdinalIgnoreCase) < 0).ToList();
                }
                // Flat M2 actor sheets do not contain phenotype skin atlases. Keep
                // automatic species creation disabled or build 719 calls
                // Subspecies.createSkins while loading a save and aborts that actor.
                // EnsureUnitRuntimeState assigns an existing city's subspecies
                // directly after Actor.loadFromSave has restored the city link.
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
                // M2's values are direct NameGenerator IDs. Build 719 interprets
                // name_template_sets as NameSetAsset IDs and dereferences a missing
                // set while hovering/selecting units. Keep the direct template and
                // do not opt these actors into the newer NameSet pipeline.
                actor.name_template_sets = null;
                // Actor.nextJobActor assumes all five arrays are non-null once an
                // actor belongs to a sapient subspecies. Keep native boats on their
                // inherited "decision" job; military/creature actors use attacker.
                // job_baby is also populated even though M2 units have no baby form,
                // because legacy saves can contain age-zero actor data.
                string[] runtimeJobs;
                if (spec.Id == "MissileSystem")
                    runtimeJobs = new[] { "decision" };
                else if (spec.Boat && actor.job != null && actor.job.Length > 0)
                    runtimeJobs = actor.job;
                else
                    runtimeJobs = new[] { "attacker" };
                actor.job = runtimeJobs;
                actor.job_baby = runtimeJobs;
                actor.job_citizen = runtimeJobs;
                actor.job_kingdom = runtimeJobs;
                actor.job_attacker = runtimeJobs;
                if (spec.Id == "MissileSystem") MissileSystemService.ConfigureActor(actor);
                // ActorAsset.getIconPath always prepends "ui/Icons/" in build 719.
                // Generated M2 unit icons are usually actor-frame paths, so storing
                // that full path in ActorAsset.icon creates the impossible path
                // ui/Icons/actors/... and returns null in family-map rendering.
                // Keep a valid native icon ID here; the scoped lazy fallback patch
                // supplies the actor's own walk frame when an M2 icon is requested.
                actor.icon = NormalizeActorIconId(spec.IconPath, inheritedIcon);
                actor.color_hex = "#7F8C8D";
                // Flat legacy M2 sprites have no separate head atlas. Build 719's
                // generic selected-unit avatar still attempts to index one unless
                // an override is supplied, producing the repeated head-array and
                // avatar-frame exceptions seen when an M2 unit is selected.
                string avatarPath = actor.texture_asset == null || actor.animation_walk == null || actor.animation_walk.Length == 0
                    ? null
                    : actor.texture_asset.texture_path_main + "/" + actor.animation_walk[0];
                if (!string.IsNullOrEmpty(avatarPath))
                {
                    actor.get_override_avatar_frames = pActor =>
                    {
                        Sprite sprite = SpriteTextureLoader.getSprite(avatarPath);
                        return sprite == null ? Array.Empty<Sprite>() : new[] { sprite };
                    };
                    actor.has_override_avatar_frames = true;
                }
                // Build 719's selected-unit avatar assumes every warrior owns an
                // equipment container, including actors that never equip items.
                // The basic unit template leaves it null for itemless vehicles.
                actor.action_on_load += EnsureUnitRuntimeState;
                foreach (string trait in spec.Traits)
                {
                    // Original M2 registered MIRVBoat as an assignable trait but did
                    // not grant it to every naval actor. Generated rewrite specs did.
                    if (spec.Boat && trait == "MIRVBoat") continue;
                    actor.addTrait(trait);
                }
                ModernLocalization.Add(spec.Id, FriendlyName(spec.Id));
                RegisterSpawnPower(spec);
            }
            M2LegacyBehaviorService.AttachActorTraits();
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

        private static void RegisterInvasionKingdoms()
        {
            RegisterHostileWildKingdom(InvasionKingdomId, new[] { "civ", "human", "orc", "elf", "dwarf", CrusaderKingdomId });
            RegisterHostileWildKingdom(AssimilatorKingdomId, new[] { "civ", "human", "orc", "elf", "dwarf", CrusaderKingdomId });
            RegisterHostileWildKingdom(WalkerKingdomId, new[] { "civ", "human", "orc", "elf", "dwarf", CrusaderKingdomId });
            RegisterHostileWildKingdom(UndeadKingdomId, new[] { "civ", "human", "orc", "elf", "dwarf", CrusaderKingdomId });
            RegisterCrusaderKingdom();

            // M2's Vatican force is a counter-invasion against the zombie
            // outbreak, so hostility must work in both directions. All normal
            // and evolved M2 zombies deliberately share the game's native undead
            // faction; add only the crusader tag and preserve every native undead
            // enemy/friendly relationship already supplied by WorldBox.
            KingdomAsset nativeUndead = AssetManager.kingdoms.get(NativeUndeadKingdomId);
            if (nativeUndead != null)
            {
                nativeUndead.addEnemyTag(CrusaderKingdomId);
                nativeUndead._cached_enemies.Clear();
            }
        }

        private static void RegisterHostileWildKingdom(string id, IEnumerable<string> enemies)
        {
            KingdomAsset invasion = AssetManager.kingdoms.get(id);
            if (invasion == null) invasion = AssetManager.kingdoms.clone(id, "neutral");
            invasion.id = id;
            invasion.civ = false;
            invasion.nomads = false;
            invasion.neutral = false;
            invasion.nature = false;
            invasion.abandoned = false;
            invasion.mobs = true;
            invasion.brain = true;
            invasion.count_as_danger = true;
            invasion.friendship_for_everyone = false;
            invasion.always_attack_each_other = false;
            invasion.units_always_looking_for_enemies = true;
            invasion.force_look_all_chunks = true;
            invasion.friendly_tags.Clear();
            invasion.enemy_tags.Clear();
            invasion.list_tags.Clear();
            invasion._cached_enemies.Clear();
            invasion.addTag(id);
            foreach (string enemy in enemies) invasion.addEnemyTag(enemy);
            EnsureWildKingdom(id);
        }

        private static void RegisterCrusaderKingdom()
        {
            // Original M2's base crusader inherited the plague doctor's "good"
            // faction, and all three evolved crusaders explicitly used SK.good.
            // A neutral-derived asset with no friendly tags defaults to treating
            // every unlisted kingdom as a foe in build 719, which made humans and
            // crusaders attack one another. Recreate the good-faction relationship
            // while retaining a separate hidden kingdom for save-safe ownership.
            KingdomAsset crusaders = AssetManager.kingdoms.get(CrusaderKingdomId);
            if (crusaders == null) crusaders = AssetManager.kingdoms.clone(CrusaderKingdomId, "plague_doctor");
            crusaders.id = CrusaderKingdomId;
            crusaders.civ = false;
            crusaders.nomads = false;
            crusaders.neutral = false;
            crusaders.nature = false;
            crusaders.abandoned = false;
            crusaders.mobs = true;
            crusaders.brain = true;
            crusaders.count_as_danger = false;
            crusaders.friendship_for_everyone = false;
            crusaders.always_attack_each_other = false;
            crusaders.units_always_looking_for_enemies = true;
            crusaders.force_look_all_chunks = true;
            crusaders.friendly_tags.Clear();
            crusaders.enemy_tags.Clear();
            crusaders.list_tags.Clear();
            crusaders._cached_enemies.Clear();

            // These two tags make the relationship reciprocal: civilizations
            // recognize crusaders as good, and crusaders recognize civilizations.
            crusaders.addTag(CrusaderKingdomId);
            crusaders.addTag("good");
            crusaders.addFriendlyTag(CrusaderKingdomId);
            crusaders.addFriendlyTag("good");
            crusaders.addFriendlyTag("neutral");
            crusaders.addFriendlyTag("civ");
            crusaders.addFriendlyTag("human");
            crusaders.addFriendlyTag("elf");
            crusaders.addFriendlyTag("dwarf");

            crusaders.addEnemyTag(InvasionKingdomId);
            crusaders.addEnemyTag(AssimilatorKingdomId);
            crusaders.addEnemyTag(WalkerKingdomId);
            crusaders.addEnemyTag(UndeadKingdomId);
            crusaders.addEnemyTag(NativeUndeadKingdomId);
            EnsureWildKingdom(CrusaderKingdomId);

            // Remove any foe result cached by an older rewrite revision. Existing
            // saved actors keep their kingdom ID, but immediately inherit the
            // corrected asset relationships after the mod is reloaded.
            foreach (string id in new[] { "human", "elf", "dwarf", "nomads_human", "nomads_elf", "nomads_dwarf" })
            {
                KingdomAsset civilization = AssetManager.kingdoms.get(id);
                if (civilization != null) civilization._cached_enemies.Clear();
            }
        }

        private static Kingdom EnsureFallbackKingdom()
        {
            if (World.world == null || World.world.kingdoms_wild == null) return null;
            Kingdom fallback = World.world.kingdoms_wild.get(FallbackKingdomId);
            if (fallback == null)
            {
                KingdomAsset asset = AssetManager.kingdoms.get(FallbackKingdomId);
                fallback = asset == null ? null : World.world.kingdoms_wild.newWildKingdom(asset);
            }
            InitializeFallbackKingdomData(fallback);
            return fallback;
        }

        private static void InitializeFallbackKingdomData(Kingdom fallback)
        {
            if (fallback == null || fallback.data == null) return;
            // Selected-unit banners resolve their background through
            // KingdomData.original_actor_asset. Hidden kingdoms created after the
            // base game's initialization do not populate it automatically. A
            // stable core species supplies a valid inspection background without
            // changing the wild ownership or combat role of the M2 actor.
            if (string.IsNullOrEmpty(fallback.data.original_actor_asset))
                fallback.data.original_actor_asset = "human";
            // Wild kingdoms start with the default numeric banner fields, but old
            // rewrite saves can retain indices generated against a different banner
            // set. Their presentation identity is deliberately the human fallback,
            // so index zero is always a stable member of that set.
            fallback.data.banner_background_id = 0;
            fallback.data.banner_icon_id = 0;
        }

        internal static bool UsesM2KingdomPresentation(Kingdom kingdom)
        {
            if (kingdom == null) return false;
            string kingdomId = kingdom.asset == null ? null : kingdom.asset.id;
            if (kingdomId == FallbackKingdomId || kingdomId == InvasionKingdomId ||
                kingdomId == AssimilatorKingdomId || kingdomId == WalkerKingdomId ||
                kingdomId == UndeadKingdomId || kingdomId == CrusaderKingdomId)
                return true;
            return kingdom.king != null && kingdom.king.asset != null &&
                   ModernBoxCatalog.UnitIds.Contains(kingdom.king.asset.id);
        }

        internal static void RepairKingdomBanner(Kingdom kingdom)
        {
            if (!UsesM2KingdomPresentation(kingdom) || kingdom.data == null) return;
            ActorAsset identity = kingdom.getActorAsset();
            if (identity == null || string.IsNullOrEmpty(identity.banner_id))
            {
                identity = AssetManager.actor_library.get("human");
                kingdom.data.original_actor_asset = "human";
            }
            if (identity == null || string.IsNullOrEmpty(identity.banner_id)) return;
            BannerAsset banner = AssetManager.kingdom_banners_library.get(identity.banner_id);
            if (banner == null) return;
            if (banner.backgrounds == null || banner.backgrounds.Count == 0)
                return;
            if (kingdom.data.banner_background_id < 0 ||
                kingdom.data.banner_background_id >= banner.backgrounds.Count)
                kingdom.data.banner_background_id = 0;
            if (banner.icons != null && banner.icons.Count > 0 &&
                (kingdom.data.banner_icon_id < 0 || kingdom.data.banner_icon_id >= banner.icons.Count))
                kingdom.data.banner_icon_id = 0;
        }

        private static Kingdom EnsureWildKingdom(string id)
        {
            if (World.world == null || World.world.kingdoms_wild == null) return null;
            Kingdom invasion = World.world.kingdoms_wild.get(id);
            if (invasion == null)
            {
                KingdomAsset asset = AssetManager.kingdoms.get(id);
                invasion = asset == null ? null : World.world.kingdoms_wild.newWildKingdom(asset);
            }
            InitializeFallbackKingdomData(invasion);
            return invasion;
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
            M2LegacyBehaviorService.EnsureOriginalZombieBalloonRuntime(actor);

            ModernUnitSpec unitSpec = actor.asset == null
                ? null
                : ContentRegistry.Units.Find(candidate => candidate.Id == actor.asset.id);
            bool mechanicalUnit = IsMechanicalSpec(unitSpec);
            if (mechanicalUnit && actor.subspecies != null)
            {
                // Original M2 vehicles inherited _boat/_mob and had no biological
                // subspecies. The former rewrite assigned the owning city's
                // subspecies to every M2 actor, enabling hunger, starvation and
                // age-related scale changes on ships and other machines. Remove
                // that accidental relationship from existing saves as well as new
                // actors, while leaving evolving invasion creatures untouched.
                Subspecies oldSubspecies = actor.subspecies;
                if (World.world != null && World.world.subspecies != null)
                    World.world.subspecies.unitDied(oldSubspecies);
                actor.subspecies = null;
                if (actor.data != null) actor.data.subspecies = 0L;
                actor.setStatsDirty();
                actor.updateStats();
                actor.finishScale();
            }
            if (mechanicalUnit)
            {
                // Repair actors saved while an older rewrite revision had allowed
                // the current game's biological sleeping status onto machines.
                if (actor.hasStatus("sleeping")) actor.stopSleeping();
            }
            else if (!mechanicalUnit && actor.asset != null && actor.asset.is_humanoid &&
                     actor.city != null && actor.subspecies == null)
            {
                Subspecies citySubspecies = actor.city.getMainSubspecies();
                if (citySubspecies != null) actor.setSubspecies(citySubspecies);
            }
            if (actor.asset != null && actor.asset.is_boat && actor.data != null &&
                !actor.data.hasFlag(NavalTraitMigrationFlag))
            {
                // Remove the erroneous old rewrite default once. A player can still
                // assign MIRVBoat manually afterwards, as in original M2.
                if (actor.hasTrait("MIRVBoat")) actor.removeTrait("MIRVBoat");
                actor.data.addFlag(NavalTraitMigrationFlag);
            }
            string factionId = GetMarkedFaction(actor);
            bool invasionActor = actor.data != null && actor.data.hasFlag(InvasionActorFlag);
            if (string.IsNullOrEmpty(factionId) && actor.asset != null)
            {
                factionId = FactionForActorId(actor.asset.id);
                if (string.IsNullOrEmpty(factionId) && !NaturalAlienJungleActors.Contains(actor.asset.id))
                {
                    if (unitSpec != null && unitSpec.Role == M2UnitRole.Creature) factionId = InvasionKingdomId;
                }
            }
            if (string.IsNullOrEmpty(factionId) && invasionActor) factionId = InvasionKingdomId;
            if (!string.IsNullOrEmpty(factionId))
            {
                MarkFaction(actor, factionId);
                Kingdom invasion = EnsureWildKingdom(factionId);
                if (invasion != null && actor.kingdom != invasion) actor.setKingdom(invasion);
            }
            else if (actor.kingdom == null)
            {
                Kingdom fallback = EnsureFallbackKingdom();
                if (fallback != null) actor.setKingdom(fallback);
            }
        }

        private static bool IsMechanicalSpec(ModernUnitSpec spec)
        {
            return spec != null && !spec.Humanoid && spec.Role != M2UnitRole.Creature;
        }

        internal static bool IsNonSleepingMechanicalActor(Actor actor)
        {
            if (actor == null || actor.asset == null) return false;
            return IsMechanicalSpec(ContentRegistry.Units.Find(candidate => candidate.Id == actor.asset.id));
        }

        internal static void MakeInvasionActor(Actor actor)
        {
            if (actor == null) return;
            string factionId = actor.asset == null ? InvasionKingdomId : FactionForActorId(actor.asset.id);
            MakeInvasionActor(actor, string.IsNullOrEmpty(factionId) ? InvasionKingdomId : factionId);
        }

        internal static void MakeInvasionActor(Actor actor, string factionId)
        {
            if (actor == null) return;
            if (string.IsNullOrEmpty(factionId)) factionId = InvasionKingdomId;
            MarkFaction(actor, factionId);
            Kingdom invasion = EnsureWildKingdom(factionId);
            if (invasion != null) actor.setKingdom(invasion);
            EnsureUnitRuntimeState(actor);
        }

        internal static void EnsureInvasionIdentity(Actor actor)
        {
            if (actor == null || actor.asset == null) return;
            string factionId = FactionForActorId(actor.asset.id);
            if (!string.IsNullOrEmpty(factionId)) MakeInvasionActor(actor, factionId);
        }

        internal static Kingdom GetInvasionKingdom(string factionId)
        {
            return EnsureWildKingdom(string.IsNullOrEmpty(factionId) ? InvasionKingdomId : factionId);
        }

        internal static bool IsM2InvasionActor(Actor actor)
        {
            if (actor == null) return false;
            if (actor.data != null && actor.data.hasFlag(InvasionActorFlag)) return true;
            if (!string.IsNullOrEmpty(GetMarkedFaction(actor))) return true;
            string kingdomId = actor.kingdom == null || actor.kingdom.asset == null ? null : actor.kingdom.asset.id;
            return kingdomId == InvasionKingdomId || kingdomId == AssimilatorKingdomId ||
                kingdomId == WalkerKingdomId || kingdomId == UndeadKingdomId || kingdomId == CrusaderKingdomId;
        }

        private static void MarkFaction(Actor actor, string factionId)
        {
            if (actor.data == null) return;
            actor.data.addFlag(InvasionActorFlag);
            actor.data.addFlag(FactionFlagPrefix + factionId);
        }

        private static string GetMarkedFaction(Actor actor)
        {
            if (actor == null || actor.data == null) return null;
            // M2 cloned every evolved zombie from the native zombie actor, whose
            // wild kingdom is "undead". An older rewrite revision marked those
            // actors as ModernBoxM2Undead instead. That split one infection into
            // two wild kingdoms and made evolved zombies fight ordinary zombies.
            // Prefer the actor asset's native faction even when an old save still
            // contains the obsolete ModernBox faction flag.
            if (actor.asset != null && actor.asset.id.StartsWith("zombie", StringComparison.OrdinalIgnoreCase))
                return NativeUndeadKingdomId;
            foreach (string id in new[] { AssimilatorKingdomId, WalkerKingdomId, UndeadKingdomId, CrusaderKingdomId, InvasionKingdomId })
                if (actor.data.hasFlag(FactionFlagPrefix + id)) return id;
            return null;
        }

        internal static string FactionForActorId(string actorId)
        {
            if (string.IsNullOrEmpty(actorId)) return null;
            if (actorId == "basecrusader" || actorId.StartsWith("crusader", StringComparison.OrdinalIgnoreCase)) return CrusaderKingdomId;
            if (actorId.StartsWith("zombie", StringComparison.OrdinalIgnoreCase)) return NativeUndeadKingdomId;
            if (actorId.StartsWith("assimil", StringComparison.OrdinalIgnoreCase) || actorId == "helilator" || actorId == "assizeppelin") return AssimilatorKingdomId;
            if (actorId.IndexOf("walker", StringComparison.OrdinalIgnoreCase) >= 0 || actorId == "icedracoid" || actorId == "buffrost") return WalkerKingdomId;
            return null;
        }

        private static string[] FindFrames(string actorId, string prefix, string[] fallback)
        {
            string folder = Path.Combine(ModernBoxMod.ModFolder, "GameResources", "actors", actorId);
            if (!Directory.Exists(folder)) return fallback;
            string[] result = Directory.GetFiles(folder, prefix + "_*.png")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => name.IndexOf("_head", StringComparison.OrdinalIgnoreCase) < 0 && name.IndexOf("_item", StringComparison.OrdinalIgnoreCase) < 0)
                .OrderBy(FrameNumber)
                .ThenBy(name => name, StringComparer.Ordinal)
                .ToArray();
            return result.Length == 0 ? fallback : result;
        }

        private static int FrameNumber(string name)
        {
            if (string.IsNullOrEmpty(name)) return int.MaxValue;
            int separator = name.LastIndexOf('_');
            int number;
            return separator >= 0 && int.TryParse(name.Substring(separator + 1), out number) ? number : int.MaxValue;
        }

        private static string NormalizeActorIconId(string requestedPath, string inheritedIcon)
        {
            const string iconPrefix = "ui/Icons/";
            if (!string.IsNullOrEmpty(requestedPath) && requestedPath.StartsWith(iconPrefix, StringComparison.Ordinal))
                return requestedPath.Substring(iconPrefix.Length);
            if (!string.IsNullOrEmpty(inheritedIcon) && inheritedIcon.StartsWith(iconPrefix, StringComparison.Ordinal))
                return inheritedIcon.Substring(iconPrefix.Length);
            return inheritedIcon ?? string.Empty;
        }

        private static bool HasExplicitOriginalBoatScale(string actorId)
        {
            return actorId == "human_industrial_trading" ||
                   actorId == "human_modern_battleship" ||
                   actorId == "human_modern_gunboat" ||
                   actorId == "human_modern_corvette1" ||
                   actorId == "human_modern_corvette2" ||
                   actorId == "human_modern_submarine";
        }

        private static void ApplyOriginalM2Animations(ActorAsset actor, ModernUnitSpec spec, string[] inheritedBoatIdle)
        {
            if (spec.Flying)
            {
                ApplyOriginalAircraftAnimations(actor, spec.Id);
                return;
            }

            if (!spec.Boat)
            {
                switch (spec.Id)
                {
                    // Both railgun definitions deliberately used three of their
                    // four supplied movement frames in LandVehicles.cs.
                    case "Railgun":
                    case "OmegaRailgun":
                        actor.animation_walk = new[] { "walk_0", "walk_1", "walk_2" };
                        actor.animation_swim = new[] { "swim_0", "swim_1", "swim_2" };
                        break;
                    // These repeated frames are intentional timing beats in the
                    // original source, rather than extra image files.
                    case "catapulta":
                    case "orcatapulta":
                        actor.animation_idle = new[] { "idle_0", "idle_1", "idle_2", "idle_1" };
                        break;
                    case "ogreunit":
                    case "Soldier":
                        actor.animation_swim = new[] { "swim_0", "swim_1", "swim_0", "swim_1" };
                        break;
                }
                return;
            }

            string[] fourSwim = { "swim_0", "swim_1", "swim_2", "swim_3" };
            string[] walkZero = { "walk_0" };
            switch (spec.Id)
            {
                case "orcwarturtle":
                    actor.animation_walk = walkZero;
                    actor.animation_swim = new[] { "swim_0", "swim_1", "swim_2" };
                    break;
                case "human_renaissance_trading":
                case "fishing_boat_renaissance":
                case "fishing_boat_industrial":
                case "fishing_boat_modern":
                    actor.animation_walk = fourSwim;
                    actor.animation_swim = fourSwim;
                    break;
                case "human_modern_trading":
                    actor.animation_walk = walkZero;
                    actor.animation_swim = new[] { "swim_0", "swim_1", "swim_2" };
                    break;
                case "human_modern_submarine":
                    actor.animation_walk = walkZero;
                    actor.animation_swim = new[]
                    {
                        "swim_0", "swim_1", "swim_2", "swim_3", "swim_4", "swim_5",
                        "swim_6", "swim_7", "swim_8", "swim_9", "swim_10"
                    };
                    break;
                default:
                    actor.animation_walk = walkZero;
                    actor.animation_swim = fourSwim;
                    break;
            }

            // NavalVehicles.cs never assigned animation_idle, so original M2
            // retained the cloned boat idle sequence. Preserve that behavior.
            actor.animation_idle = inheritedBoatIdle != null && inheritedBoatIdle.Length > 0
                ? inheritedBoatIdle
                : actor.animation_walk;
        }

        private static void ApplyOriginalAircraftAnimations(ActorAsset actor, string actorId)
        {
            string[] fourWalk = { "walk_0", "walk_1", "walk_2", "walk_3" };
            switch (actorId)
            {
                case "Heli":
                case "Drone":
                    actor.animation_walk = fourWalk;
                    actor.animation_idle = fourWalk;
                    actor.animation_swim = fourWalk;
                    break;
                case "Gunship":
                    actor.animation_swim = new[] { "walk_1", "walk_2" };
                    break;
                case "Zeppelin":
                    actor.animation_idle = new[] { "walk_0" };
                    break;
                case "MIRVBomber":
                    actor.animation_walk = NumberedFrames("idle", 20);
                    break;
                case "FighterJet":
                case "FighterJet1":
                case "F55FighterJet":
                case "F55FighterJet1":
                    actor.animation_walk = NumberedFrames("idle", 10);
                    break;
            }
        }

        private static string[] NumberedFrames(string prefix, int count)
        {
            string[] frames = new string[count];
            for (int i = 0; i < count; i++) frames[i] = prefix + "_" + i;
            return frames;
        }

        private static void ApplyOriginalGroundVehicleIdle(ActorAsset actor, ModernUnitSpec spec)
        {
            if (spec.Flying || actor.animation_walk == null || actor.animation_walk.Length == 0) return;

            // These three explicitly animate through their walking sequence while
            // idle in original M2. Actors with real idle art already received it
            // from FindFrames; the remaining original ground vehicles use walk_0.
            if (spec.Id == "P9000" || spec.Id == "EliteP9000" || spec.Id == "davincitank" ||
                spec.Id == "fairelf" || spec.Id == "fairydragon")
            {
                actor.animation_idle = actor.animation_walk;
                return;
            }

            string textureFolder = string.IsNullOrEmpty(spec.TextureFolder) ? spec.Id : spec.TextureFolder;
            string idleFolder = Path.Combine(ModernBoxMod.ModFolder, "GameResources", "actors", textureFolder);
            bool hasIdleArt = Directory.Exists(idleFolder) && Directory.GetFiles(idleFolder, "idle_*.png").Length > 0;
            if (!hasIdleArt && spec.Id != "AbramTank" && spec.Id != "HumanTitanElite")
                actor.animation_idle = new[] { actor.animation_walk[0] };
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
            ModernLocalization.Add(powerId + "_description", spec.Role == M2UnitRole.Creature
                ? "Spawn the wild M2 creature " + FriendlyName(spec.Id) + "."
                : "Spawn a ModernBox " + FriendlyName(spec.Id) + " inside a living civilized kingdom city. Empty land, ruins, and wild factions are rejected.");
        }

        private static bool SpawnUnit(WorldTile tile, string powerId)
        {
            string actorId;
            if (tile == null || !SpawnPowerActors.TryGetValue(powerId, out actorId)) return false;
            ModernUnitSpec spec = ContentRegistry.Units.Find(candidate => candidate.Id == actorId);
            if (spec != null && spec.Role == M2UnitRole.Creature)
            {
                Actor creature = World.world.units.spawnNewUnit(actorId, tile, true, true, spec.Flying ? 3f : 0f, null, false, true);
                MakeInvasionActor(creature);
                return creature != null;
            }
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
            UpgradeSources.Clear();
            foreach (BuildingSpec spec in ContentRegistry.Buildings)
            {
                string baseId = ResolveBuildingBase(spec);
                BuildingAsset building = AssetManager.buildings.clone(spec.Id, baseId);
                building.id = spec.Id;
                building.sprite_path = "buildings/" + (string.IsNullOrEmpty(spec.SourceId) ? spec.Id : spec.SourceId);
                building.main_path = building.sprite_path;
                building.setAtlasID("buildings", "buildings");
                building.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
                if (building.atlas_asset == null) throw new InvalidOperationException("Building recolor atlas is unavailable for " + spec.Id + ".");
                building.city_building = true;
                building.group = spec.Civilian ? "modernbox_m2_civilian" : (spec.UpgradeOnly ? "modernbox_m2_era" : "modernbox_m2_industry");
                // The city limit is counted by BuildingAsset.type, not by asset ID.
                // A unique type makes limit=1 apply independently to each factory
                // while keeping civilian int.Max orders genuinely unlimited.
                if (!spec.UpgradeOnly) building.type = "modernbox_type_" + spec.Id;
                // Upgrade-chain assets clone the current native terminal building.
                // Preserve that exact footprint: Building.upgradeBuilding rejects a
                // target with a different footprint when the extra coastal/urban
                // tiles are occupied. The former generic 2x2 replacement made dock
                // modernization stall forever in otherwise eligible cities.
                if (!spec.UpgradeOnly)
                    building.fundament = spec.Tower ? new BuildingFundament(4, 2, 2, 0) : new BuildingFundament(2, 2, 2, 0);
                building.cost = spec.Cost;
                building.priority = spec.UpgradeOnly ? 2500 : (spec.Tower ? 3750 : 69999);
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
                building.build_prefer_replace_house = !spec.Tower && !spec.UpgradeOnly;
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
                if (spec.Tower)
                {
                    building.tower = true;
                    building.tower_projectile = spec.Id == "MissileSilo" ? "NUKER" : OriginalTowerProjectile(spec.Era);
                    building.tower_projectile_amount = spec.Type == "watch_tower" && spec.Era == M2Era.Renaissance ? 4 : 1;
                    building.tower_projectile_offset = 4f;
                    building.tower_projectile_reload = spec.Id == "MissileSilo" ? 32f : 64f;
                    building.tower_attack_buildings = true;
                }
                if (spec.Type == "dock") building.boat_types = DockBoats(spec.Era, spec.Race);
                building.loadBuildingSprites();
                EnsureBuildingRenderSprites(building);
                ModernLocalization.Add(spec.Id, FriendlyName(spec.Id));
                ModernLocalization.Add(spec.Id + "_description", BuildingDescription(spec));
            }
            RegisterLegacySpawnerAndScrapBuildings();
            LinkEraUpgradeChains();
            AddCivilizationBuildOrders();
        }

        private static void RegisterLegacySpawnerAndScrapBuildings()
        {
            RegisterLegacySpawnerBuilding("missilecybercore", "tumor", "assimilator", AssimilatorKingdomId, 3500f);
            RegisterLegacySpawnerBuilding("icewatchtower", "$building_civ_human$", "icedracoid", WalkerKingdomId, 200f);
            RegisterLegacySpawnerBuilding("newicetower", "$building_civ_human$", "newwalker", WalkerKingdomId, 500f);
            RegisterLegacySpawnerBuilding("walkercorpse", "$building_civ_human$", "fly", WalkerKingdomId, 10000f);
            RegisterLegacySpawnerBuilding("pileofcorpses", "$building_civ_human$", "zombie", NativeUndeadKingdomId, 1000f);

            // M2 has two different ice structures. The common newicetower is a
            // walker production spawner; the rare icewatchtower is an armed
            // defensive tower and does not produce units. Their old registration
            // cloned different legacy templates, so spell the distinction out for
            // build 719 instead of inheriting city-building behavior.
            BuildingAsset watchtower = AssetManager.buildings.get("icewatchtower");
            if (watchtower != null)
            {
                watchtower.spawn_units = false;
                watchtower.spawn_units_asset = null;
                watchtower.housing_slots = 0;
                watchtower.can_units_live_here = false;
                watchtower.ice_tower = true;
                watchtower.tower = true;
                watchtower.tower_projectile = "frostbolt";
                watchtower.tower_projectile_amount = 1;
                watchtower.tower_projectile_offset = 10f;
                watchtower.tower_projectile_reload = 2f;
                watchtower.tower_attack_buildings = true;
            }
            BuildingAsset missileCore = AssetManager.buildings.get("missilecybercore");
            if (missileCore != null)
            {
                missileCore.tower = true;
                missileCore.tower_projectile = "cybermissileprojectile";
                missileCore.tower_projectile_amount = 10;
                missileCore.tower_projectile_offset = 2f;
                missileCore.tower_attack_buildings = true;
            }
            BuildingAsset productionTower = AssetManager.buildings.get("newicetower");
            if (productionTower != null)
            {
                productionTower.ice_tower = true;
                productionTower.tower = false;
                productionTower.spawn_units = true;
                productionTower.spawn_units_asset = "newwalker";
            }

            foreach (string scrapId in ContentRegistry.Units
                .Where(candidate => !string.IsNullOrEmpty(candidate.ScrapBuilding))
                .Select(candidate => candidate.ScrapBuilding)
                .Distinct(StringComparer.Ordinal))
            {
                if (AssetManager.buildings.get(scrapId) != null) continue;
                BuildingAsset scrap = AssetManager.buildings.clone(scrapId, "$building_civ_human$");
                ConfigureLegacyWorldBuilding(scrap, scrapId, 350f);
                scrap.spawn_units = false;
                scrap.spawn_units_asset = null;
                scrap.housing_slots = 0;
                scrap.can_units_live_here = false;
                scrap.burnable = true;
                scrap.can_be_demolished = true;
                scrap.can_be_abandoned = false;
                ModernLocalization.Add(scrapId, FriendlyName(scrapId));
                ModernLocalization.Add(scrapId + "_description", "The remains of a destroyed ModernBox unit.");
            }
        }

        private static void RegisterLegacySpawnerBuilding(string id, string baseId, string unitId, string kingdomId, float health)
        {
            if (AssetManager.buildings.get(id) != null) return;
            if (AssetManager.buildings.get(baseId) == null) baseId = "$building_civ_human$";
            BuildingAsset spawner = AssetManager.buildings.clone(id, baseId);
            ConfigureLegacyWorldBuilding(spawner, id, health);
            spawner.kingdom = kingdomId;
            spawner.civ_kingdom = kingdomId;
            spawner.spawn_units = true;
            spawner.spawn_units_asset = unitId;
            // UnitSpawner uses housing_slots as its live-unit cap. Eight keeps
            // every core dangerous without allowing old worlds to snowball
            // into an unbounded resident list.
            spawner.housing_slots = 8;
            spawner.can_units_live_here = true;
            spawner.burnable = id == "pileofcorpses";
            spawner.can_be_demolished = true;
            spawner.can_be_abandoned = false;
            ModernLocalization.Add(id, FriendlyName(id));
            ModernLocalization.Add(id + "_description", id == "icewatchtower"
                ? "A rare armed Ice Walker watchtower created during winter."
                : id == "newicetower"
                    ? "An Ice Walker tower that produces evolving new walkers."
                    : "A hostile M2 invasion spawner.");
        }

        private static void ConfigureLegacyWorldBuilding(BuildingAsset building, string spriteId, float health)
        {
            building.id = spriteId;
            building.sprite_path = "buildings/" + spriteId;
            building.main_path = building.sprite_path;
            building.setAtlasID("buildings", "buildings");
            building.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
            building.city_building = false;
            building.group = "modernbox_m2_world_structures";
            building.type = "modernbox_type_" + spriteId;
            building.fundament = new BuildingFundament(1, 0, 1, 0);
            // Hostile spawners and vehicle scraps are not constructed by cities,
            // but BehRemoveRuins still reads the cost of every Building_Civ target
            // before destroying it. Some legacy templates carry a null cost, which
            // turns an otherwise valid cleanup task into a simulation-stopping NRE.
            building.cost = new ConstructionCost(0, 0, 0, 0);
            building.base_stats["health"] = health;
            building.base_stats["size"] = 1f;
            building.can_be_upgraded = false;
            building.can_be_placed_on_liquid = false;
            building.can_be_living_house = false;
            building.has_sprites_main = true;
            building.has_sprites_ruin = true;
            building.has_ruin_state = true;
            building.has_ruins_graphics = true;
            building.loadBuildingSprites();
            EnsureBuildingRenderSprites(building);
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

            // Keep a stable, already-loaded sprite for the renderer. Build 719 can
            // occasionally select an empty animation slot for custom vegetation or
            // legacy structures after its later preload/finalization pass. The
            // calculateMainSprite postfix uses this only when the normal result is
            // null, so valid construction, ruin, disabled, and animated frames are
            // never replaced.
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

            // Build 719 always reads this field while a building is under
            // construction, even when has_sprite_construction is false. Most M2
            // art has only main frames, so use the first valid main frame.
            if (building.building_sprites.construction == null)
                building.building_sprites.construction = fallback;
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

        private static string[] DockBoats(M2Era era, string race)
        {
            if (era == M2Era.Renaissance)
            {
                if (race == "orc") return new[] { "orcwarturtle", "human_renaissance_battleship", "human_renaissance_trading", "fishing_boat_renaissance", "human_renaissance_corvette1", "human_renaissance_corvette2" };
                return new[] { "human_renaissance_battleship", "human_renaissance_trading", "fishing_boat_renaissance", "human_renaissance_corvette1", "human_renaissance_corvette2" };
            }
            if (era == M2Era.Industrial)
                return new[] { "human_industrial_battleship", "human_industrial_trading", "fishing_boat_industrial", "human_industrial_corvette1", "human_industrial_corvette2" };
            return new[] { "human_modern_battleship", "human_modern_trading", "fishing_boat_modern", "human_modern_corvette1", "human_modern_corvette2", "human_modern_submarine" };
        }

        private static string ResolveBuildingBase(BuildingSpec spec)
        {
            if (!spec.UpgradeOnly) return "$building_civ_human$";
            BuildingAsset native = ResolveNativeSource(spec.Race, spec.Type);
            return native == null ? "$building_civ_human$" : native.id;
        }

        private static void LinkEraUpgradeChains()
        {
            foreach (BuildingUpgradeSpec upgrade in ContentRegistry.Upgrades)
            {
                BuildingAsset target = AssetManager.buildings.get(upgrade.TargetId);
                BuildingAsset source = upgrade.SourceId.StartsWith("$native_", StringComparison.Ordinal)
                    ? ResolveNativeSource(upgrade.Race, NativeChain(upgrade.SourceId))
                    : AssetManager.buildings.get(upgrade.SourceId);
                if (source == null || target == null)
                    throw new InvalidOperationException("Missing M2 upgrade chain asset " + upgrade.SourceId + " -> " + upgrade.TargetId + ".");
                source.can_be_upgraded = true;
                source.upgrade_to = target.id;
                target.upgraded_from = source.id;
                target.upgrade_level = source.upgrade_level + 1;
                target.can_be_upgraded = !string.IsNullOrEmpty(ContentRegistry.Buildings.Find(candidate => candidate.Id == target.id).UpgradeTo);
                target.upgrade_to = ContentRegistry.Buildings.Find(candidate => candidate.Id == target.id).UpgradeTo ?? string.Empty;
                UpgradeSources[target.id] = source.id;
            }
        }

        private static void AddCivilizationBuildOrders()
        {
            foreach (string race in ModernBoxCatalog.SupportedRaces)
            {
                ActorAsset species = AssetManager.actor_library.get(race);
                if (species == null || species.architecture_asset == null) continue;
                CityBuildOrderAsset orders = CreatePrivateBuildOrders(species, race);
                if (orders == null) continue;
                foreach (BuildingSpec spec in ContentRegistry.Buildings)
                {
                    if (spec.UpgradeOnly) continue;
                    string orderId = "order_m2_" + race + "_" + spec.Id;
                    species.architecture_asset.addBuildingOrderKey(orderId, spec.Id);
                    if (orders.list.Any(order => string.Equals(order.id, orderId, StringComparison.Ordinal))) continue;
                    int population, count;
                    ModernProgression.GetRequirements((ProgressionTier)(int)spec.Era, out population, out count);
                    BuildOrder order = orders.addBuilding(orderId, spec.Limit, population, count, false, false, 0);
                    order.requirements_types = new[] { "type_bonfire" };
                }
                foreach (BuildingUpgradeSpec upgrade in ContentRegistry.Upgrades.Where(candidate => candidate.Race == race))
                {
                    string sourceId;
                    if (!UpgradeSources.TryGetValue(upgrade.TargetId, out sourceId)) continue;
                    string orderId = "order_m2_upgrade_" + upgrade.TargetId;
                    species.architecture_asset.addBuildingOrderKey(orderId, sourceId);
                    if (!orders.list.Any(order => order.id == orderId)) orders.addUpgrade(orderId, int.MaxValue, 0, 0, false, false, 0);
                }
                // The generation cache is normally prepared during the game's
                // library post-init, before NML mods register their additions.
                // Rebuild it now so every copied vanilla order and M2 order is
                // available to newly founded cities.
                orders.prepareForAssetGeneration();
            }
            ModernBoxDiagnostics.Info("Added M2 construction and upgrade orders to human, orc, elf, and dwarf architectures.");
        }

        private static CityBuildOrderAsset CreatePrivateBuildOrders(ActorAsset species, string race)
        {
            string privateId = "build_order_modernbox_m2_" + race;
            CityBuildOrderAsset privateOrders = AssetManager.city_build_orders.get(privateId);
            if (privateOrders == null)
            {
                CityBuildOrderAsset nativeOrders = AssetManager.city_build_orders.get(species.build_order_template_id);
                if (nativeOrders == null) return null;

                privateOrders = new CityBuildOrderAsset { id = privateId };
                // BuildOrder entries are immutable definitions once the base game
                // finishes initialization, so sharing the vanilla entries is safe.
                // The list container itself must be private: the four core species
                // use a shared vanilla template in build 719, and adding race-only
                // IDs to that list makes other architectures throw KeyNotFound.
                privateOrders.list.AddRange(nativeOrders.list);
                AssetManager.city_build_orders.add(privateOrders);
            }
            species.build_order_template_id = privateId;
            return privateOrders;
        }

        private static string NativeChain(string placeholder)
        {
            string value = placeholder.Trim('$');
            int prefix = value.IndexOf("native_", StringComparison.Ordinal);
            if (prefix >= 0) value = value.Substring(prefix + 7);
            int last = value.LastIndexOf('_');
            return last > 0 ? value.Substring(0, last) : value;
        }

        private static BuildingAsset ResolveNativeSource(string race, string chain)
        {
            ActorAsset species = AssetManager.actor_library.get(race);
            ArchitectureAsset architecture = species == null ? null : species.architecture_asset;
            if (architecture == null) return null;
            BuildingAsset best = null;
            foreach (KeyValuePair<string, string> pair in architecture.building_ids_for_construction)
            {
                BuildingAsset candidate = AssetManager.buildings.get(pair.Value);
                if (candidate == null) continue;
                string id = candidate.id.ToLowerInvariant();
                string type = (candidate.type ?? string.Empty).ToLowerInvariant();
                bool match = id.Contains(chain) || type.Contains(chain) ||
                    (chain == "dock" && (id.Contains("dock") || type.Contains("dock"))) ||
                    (chain == "hall" && (id.Contains("hall") || type.Contains("hall")));
                if (!match) continue;
                HashSet<string> visited = new HashSet<string>(StringComparer.Ordinal);
                while (candidate != null && visited.Add(candidate.id))
                {
                    best = candidate;
                    if (string.IsNullOrEmpty(candidate.upgrade_to)) break;
                    BuildingAsset next = AssetManager.buildings.get(candidate.upgrade_to);
                    if (next == null) break;
                    candidate = next;
                }
                if (best != null) return best;
            }
            string[] candidates = chain == "dock"
                ? new[] { "fishing_docks_" + race, "docks_" + race }
                : new[] { chain + "_" + race, chain };
            foreach (string id in candidates)
            {
                BuildingAsset asset = AssetManager.buildings.get(id);
                if (asset != null) return asset;
            }
            return null;
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

        private static string CurrentAttackId(string legacyId)
        {
            switch (legacyId)
            {
                case "base": return "base_attack";
                default: return legacyId;
            }
        }

        private static string OriginalTowerProjectile(M2Era era)
        {
            switch (era)
            {
                case M2Era.Renaissance: return "cannonballprojectile";
                case M2Era.Industrial: return "shotgun_bullet";
                case M2Era.Modern: return "artilleryshell";
                case M2Era.Future: return "big_plasma_bomb";
                default: return "arrow";
            }
        }

        private static string CurrentBoatBase(string actorId)
        {
            // Build 719 registers the concrete fishing actor as boat_fishing. There
            // is no $boat_fishing$ template; using it falls through to $basic_unit$
            // and produces tiny land actors with no native boat runtime.
            if (actorId.StartsWith("fishing_boat_", StringComparison.Ordinal)) return "boat_fishing";
            if (actorId.EndsWith("_trading", StringComparison.Ordinal)) return "$boat_trading$";
            return "$boat_transport$";
        }

        private static bool IsCivilianBoat(string actorId)
        {
            return actorId.StartsWith("fishing_boat_", StringComparison.Ordinal) ||
                actorId.EndsWith("_trading", StringComparison.Ordinal);
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

