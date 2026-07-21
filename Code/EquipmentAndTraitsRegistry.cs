using System;
using System.Collections.Generic;
using System.Linq;
using NCMS.Utils;
using tools;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    internal static class EquipmentAndTraitsRegistry
    {
        private const string ItemNameGeneratorId = "ModernBox_Item_Names";

        internal static readonly string[] IdeologyIds =
        {
            "Dynastic", "Mercantile", "Peoplewoven", "Martial", "Chaosvolt"
        };

        private static readonly HashSet<string> SapientSpeciesIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "human", "orc", "elf", "dwarf"
        };
        private static bool _ideologyLoadHooksRegistered;

        internal static void RegisterResourcesAndProjectiles()
        {
            RegisterResource("Parts", "ui/Icons/Factories", "common_metals", 9999, 12, 6);
            RegisterResource("CyberWareParts", "ui/Icons/SolarPoweredCyberBody", "common_metals", 5000, 16, 8);
            RegisterResource("Xenium", "ui/Icons/Xeno", "common_metals", 2000, 24, 12);

            TerraformOptions nukerTerraform = CreateTerraform("modernbox_nuker_terraform", 10000, 43, true);
            AssetManager.terraform.add(nukerTerraform);
            RegisterProjectile("NUKER", "effects/projectiles/NUKER/0", "modernbox_nuker_terraform", 43, "fx_explosion_nuke_atomic", 150f, 0.35f);
            OriginalM2Projectiles.Register();

            // A silo impact should be a real build-719 atomic bomb, including the
            // flash delay, mushroom cloud, vanilla radius-30 terrain destruction,
            // fire, wasteland and 10,000 damage. Merely attaching the final atomic
            // explosion sprite to a projectile skips the NukeFlash sequence and is
            // why the former strike looked much weaker than the normal nuke power.
            ProjectileAsset nuker = AssetManager.projectiles.get("NUKER");
            nuker.texture_shadow = "shadows/projectiles/shadow_ball";
            nuker.impact_actions = TriggerVanillaAtomicNuke;

            RegisterMirvProjectile("modernbox_mirv_budget", 5, 500);
            RegisterMirvProjectile("modernbox_mirv_decent", 12, 1500);
            RegisterMirvProjectile("modernbox_mirv", 25, 3000);
            RegisterMirvProjectile("modernbox_mirv_bomb", 40, 5000);
            RegisterMirvProjectile("modernbox_mirv_strong", 60, 10000);
            OriginalM2Projectiles.RegisterAttackAssets();
        }

        private static void RegisterResource(string id, string icon, string gameplayTemplateId, int maximum, int tradeBound, int tradeGive)
        {
            ResourceAsset gameplayTemplate = AssetManager.resources.get(gameplayTemplateId);
            if (gameplayTemplate == null || gameplayTemplate.gameplay_sprites == null || gameplayTemplate.gameplay_sprites.Length == 0)
                throw new InvalidOperationException("Missing gameplay sprite template for resource " + id + ": " + gameplayTemplateId);
            const string iconPrefix = "ui/Icons/";
            string iconId = icon.StartsWith(iconPrefix, StringComparison.OrdinalIgnoreCase) ? icon.Substring(iconPrefix.Length) : icon;
            ResourceAsset resource = new ResourceAsset
            {
                id = id,
                path_icon = iconId,
                // UI icons are much larger than ground-resource sprites. Reusing a
                // vanilla metal ground sprite prevents Parts/Xenium drops from being
                // rendered as screen-sized cogwheels while preserving custom UI art.
                path_gameplay_sprite = gameplayTemplate.path_gameplay_sprite,
                full_sprite_path = gameplayTemplate.full_sprite_path,
                gameplay_sprites = gameplayTemplate.gameplay_sprites,
                type = ResType.Strategic,
                maximum = maximum,
                storage_max = maximum,
                stack_size = 100,
                trade_bound = tradeBound,
                trade_give = tradeGive,
                trade_cost = 2,
                supply_bound_give = tradeBound,
                supply_bound_take = tradeBound / 2,
                // ButtonResource expects this field to name a TooltipAsset, not the
                // resource itself. Use WorldBox's shared city-resource tooltip and
                // pass the resource ID through the button's TooltipData as normal.
                tooltip = "city_resource"
            };
            AssetManager.resources.add(resource);
            ModernLocalization.Add(id, id == "CyberWareParts" ? "Cyberware Parts" : id);
        }

        private static TerraformOptions CreateTerraform(string id, int damage, int strength, bool nuclear)
        {
            return new TerraformOptions
            {
                id = id,
                damage = damage,
                damage_buildings = true,
                remove_tornado = true,
                remove_frozen = true,
                remove_fire = false,
                flash = nuclear,
                shake = true,
                shake_duration = nuclear ? 0.45f : 0.2f,
                shake_interval = 0.02f,
                shake_intensity = nuclear ? 0.25f : 0.08f,
                apply_force = true,
                force_power = nuclear ? 3f : 1f,
                explode_tile = true,
                explosion_pixel_effect = true,
                explode_and_set_random_fire = nuclear,
                transform_to_wasteland = nuclear,
                explode_strength = strength,
                applies_to_high_flyers = true,
                attack_type = AttackType.Explosion
            };
        }

        private static void RegisterProjectile(string id, string texture, string terraform, int radius, string effect, float speed, float scale)
        {
            bool animatedProjectile = id == "NUKER" || id == "MIRVartillery" || id.EndsWith("bigplasma", StringComparison.Ordinal);
            ProjectileAsset projectile = new ProjectileAsset
            {
                id = id,
                texture = texture,
                animated = animatedProjectile,
                animation_speed = 0.08f,
                speed = speed,
                speed_random = 0f,
                // Build 719 compares these fields against String.Empty instead of using
                // IsNullOrEmpty. A null value therefore falls through to an asset lookup
                // with a null key when the projectile lands.
                terraform_option = terraform ?? string.Empty,
                terraform_range = radius,
                end_effect = effect ?? string.Empty,
                end_effect_scale = radius >= 40 ? 1.3f : 0.6f,
                sound_launch = string.Empty,
                sound_impact = string.Empty,
                trail_effect_id = string.Empty,
                texture_shadow = string.Empty,
                look_at_target = true,
                hit_shake = radius > 0,
                shake_duration = 0.15f,
                shake_interval = 0.02f,
                shake_intensity = 0.08f,
                scale_start = scale,
                scale_target = scale,
                trigger_on_collision = true,
                can_be_collided = true,
                can_be_blocked = false,
                can_be_left_on_ground = false,
                draw_light_area = radius > 0,
                draw_light_size = radius > 0 ? 0.4f : 0f
            };
            if (projectile.animated)
            {
                string framePath = id == "NUKER" ? "effects/projectiles/NUKER" :
                    id == "MIRVartillery" ? "effects/projectiles/MIRVartillery" : texture;
                projectile.frames = Resources.LoadAll<Sprite>(framePath);
            }
            else
            {
                Sprite frame = Resources.Load<Sprite>(texture);
                projectile.frames = frame == null ? new Sprite[0] : new[] { frame };
            }
            if (projectile.frames == null || projectile.frames.Length == 0 || projectile.frames[0] == null)
                throw new InvalidOperationException("Missing projectile render frame for " + id + " at " + texture + ".");
            AssetManager.projectiles.add(projectile);
        }

        private static bool TriggerVanillaAtomicNuke(BaseSimObject source, BaseSimObject target, WorldTile tile)
        {
            if (tile == null) return false;
            if (World.world != null) World.world.startShake(0.3f, 0.01f, 2f, true, true);
            EffectsLibrary.spawn("fx_nuke_flash", tile, "atomic_bomb", null, 0f, -1f, -1f, null);

            // Projectile.targetReached treats false as "impact handled" and skips
            // its ordinary end effect/terraform pass. The NukeFlash now owns the
            // complete explosion, avoiding a second smaller blast underneath it.
            return false;
        }

        private static void RegisterMirvProjectile(string id, int radius, int damage)
        {
            string terraformId = id + "_terraform";
            AssetManager.terraform.add(CreateTerraform(terraformId, damage, radius, true));
            RegisterProjectile(id, "effects/projectiles/NUKER/0", terraformId, radius, radius >= 40 ? "fx_explosion_nuke_atomic" : "fx_explosion_middle", 22f, radius >= 40 ? 0.35f : 0.22f);
        }

        internal static void RegisterEquipment()
        {
            RegisterItemNameGenerator();
            foreach (EquipmentSpec spec in ContentRegistry.Equipment)
            {
                string template = spec.Type == EquipmentType.Weapon ? "$range" :
                    spec.Type == EquipmentType.Ring ? "$ring" :
                    spec.Type == EquipmentType.Armor ? "$armor" :
                    spec.Type == EquipmentType.Helmet ? "$helmet" :
                    spec.Type == EquipmentType.Boots ? "$boots" : "$amulet";
                EquipmentAsset item = AssetManager.items.clone(spec.Id, template);
                item.id = spec.Id;
                item.translation_key = spec.Id;
                // ItemAsset.getRandomNameTemplate assumes this list is non-null, even
                // for ordinary-quality equipment when a generated modifier requests a
                // name. The generic $range/$ring/$amulet bases do not provide one.
                item.name_templates = new List<string> { ItemNameGeneratorId };
                item.equipment_type = spec.Type;
                item.equipment_subtype = spec.Type == EquipmentType.Weapon ? "stick" :
                    spec.Type == EquipmentType.Ring ? "ring" : spec.Type.ToString().ToLowerInvariant();
                // The loot-rain/equipment editor groups items by group_id, not by the
                // crafting subtype above.  Keep melee weapons in the sword section and
                // expose every projectile weapon (guns, blasters, launchers and MIRVs)
                // in the game's Firearms section.
                item.group_id = EquipmentEditorGroup(spec);
                item.show_in_meta_editor = true;
                item.show_in_knowledge_window = true;
                item.show_for_unlockables_ui = true;
                item.can_be_given = true;
                item.material = "basic";
                item.metallic = spec.Type == EquipmentType.Weapon;
                item.colored = false;
                item.projectile = spec.Projectile;
                item.path_slash_animation = "effects/slashes/slash_punch";
                item.path_icon = ItemIcon(spec.Id);
                // Never use a toolbar icon as a MIRV's world/held sprite.  The
                // toolbar art includes its large square button background and
                // becomes enormous when the actor renderer treats it as an item.
                // The rewrite ships dedicated tiny M1 gameplay sprites for all
                // five MIRVs, so point at those directly instead of relying on a
                // filesystem probe that can run before NML finalises ModFolder.
                item.path_gameplay_sprite = HasItemSprite(spec.Id) ? FirstItemSpritePath(spec.Id) : ItemIcon(spec.Id);
                item.gameplay_sprites = LoadItemSprites(spec.Id);
                PreloadHeldItemSprites(item);
                item.base_stats["damage"] = spec.Damage;
                item.base_stats["range"] = spec.Range;
                item.base_stats["attack_speed"] = spec.AttackSpeed;
                item.base_stats["accuracy"] = spec.Accuracy;
                item.base_stats["targets"] = 1f;
                item.base_stats["critical_chance"] = 0.12f;
                item.base_stats["projectiles"] = spec.Id == "PipeShotgun" ? 6f : 1f;
                foreach (KeyValuePair<string, float> pair in spec.BaseStats)
                {
                    if (AssetManager.base_stats_library.get(pair.Key) == null)
                    {
                        ModernBoxDiagnostics.Warn("Skipped obsolete M2 equipment stat '" + pair.Key + "'.");
                        continue;
                    }
                    item.base_stats[pair.Key] = pair.Value;
                }
                item.equipment_value = spec.Value;
                item.rigidity_rating = spec.Type == EquipmentType.Weapon ? 4 : 2;
                item.quality = Rarity.R0_Normal;
                item.setCost(0, spec.Resource1, spec.Resource1Cost, spec.Resource2, spec.Resource2Cost);
                item.minimum_city_storage_resource_1 = Math.Max(1, spec.Resource1Cost);
                // Keep the manually loaded M1 material sprites. The vanilla pool preloader
                // otherwise overwrites them by looking for a single combined texture path.
                item.is_pool_weapon = false;
                item.pool_rate = spec.Tier >= ProgressionTier.Strategic ? 3 : 12;
                ApplyAccessoryStats(item, spec.Id);
                if (spec.Type == EquipmentType.Weapon)
                {
                    if (!AssetManager.items.pot_weapon_assets_all.Contains(item)) AssetManager.items.pot_weapon_assets_all.Add(item);
                    if (!AssetManager.items.pot_weapon_assets_unlocked.Contains(item)) AssetManager.items.pot_weapon_assets_unlocked.Add(item);
                }
                AddToCraftingSubtype(item);
                ModernBoxCatalog.EquipmentTiers[spec.Id] = spec.Tier;
                ModernLocalization.Add(spec.Id, spec.DisplayName);
                ModernLocalization.Add(spec.Id + "_description", string.Empty);
                ModernLocalization.Add("item_" + spec.Id, spec.DisplayName);
                ModernLocalization.Add("item_" + spec.Id + "_description", string.Empty);
                string normalized = spec.Id.ToLowerInvariant();
                ModernLocalization.Add("item_" + normalized, spec.DisplayName);
                ModernLocalization.Add("item_" + normalized + "_description", string.Empty);
            }
        }

        private static string EquipmentEditorGroup(EquipmentSpec spec)
        {
            if (spec.Type == EquipmentType.Weapon)
                return string.IsNullOrEmpty(spec.Projectile) ? "sword" : "firearm";
            if (spec.Type == EquipmentType.Ring) return "ring";
            if (spec.Type == EquipmentType.Armor) return "armor";
            if (spec.Type == EquipmentType.Helmet) return "helmet";
            if (spec.Type == EquipmentType.Boots) return "boots";
            return "amulet";
        }

        private static void PreloadHeldItemSprites(EquipmentAsset item)
        {
            if (item.gameplay_sprites == null) return;
            foreach (Sprite sprite in item.gameplay_sprites)
            {
                if (sprite == null) continue;
                DynamicSprites.preloadItemSprite(sprite, null);
            }
        }

        private static void RegisterItemNameGenerator()
        {
            if (AssetManager.name_generator.has(ItemNameGeneratorId))
            {
                EnsureCurrentNameGeneratorFields(AssetManager.name_generator.get(ItemNameGeneratorId));
                return;
            }
            NameGeneratorAsset names = new NameGeneratorAsset { id = ItemNameGeneratorId };
            names.addPartGroup("Guardian,Vanguard,Sentinel,Defender,Thunder,Lightning,Storm,Liberty,Victory,Iron,Steel,Crimson,Black,Silver,Golden");
            names.addPartGroup(" ");
            names.addPartGroup("Rifle,Carbine,Cannon,Launcher,Repeater,Sidearm,Weapon,System,Prototype,Mark");
            names.addTemplate("part_group");
            EnsureCurrentNameGeneratorFields(names);
            AssetManager.name_generator.add(names);
        }

        private static void AddToCraftingSubtype(EquipmentAsset item)
        {
            List<EquipmentAsset> pool;
            if (!AssetManager.items.equipment_by_subtypes.TryGetValue(item.equipment_subtype, out pool))
            {
                pool = new List<EquipmentAsset>();
                AssetManager.items.equipment_by_subtypes[item.equipment_subtype] = pool;
            }
            if (!pool.Contains(item)) pool.Add(item);
        }

        private static Sprite[] LoadItemSprites(string id)
        {
            List<Sprite> loaded = new List<Sprite>();
            foreach (string path in ItemSpritePaths(id))
            {
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite != null) loaded.Add(sprite);
            }
            if (loaded.Count > 0) return loaded.ToArray();
            if (ModernBoxCatalog.MirvIds.Contains(id))
            {
                // A missing gameplay sprite must never fall back to ui/Icons/MIRV:
                // that is a full button image, not a world-scale weapon sprite.
                Sprite missile = Resources.Load<Sprite>("effects/projectiles/NUKER/0");
                return missile == null ? new Sprite[0] : new[] { missile };
            }
            Sprite fallback = Resources.Load<Sprite>(ItemIcon(id));
            if (fallback != null) return new[] { fallback };
            return new Sprite[0];
        }

        private static bool HasItemSprite(string id)
        {
            return ItemSpritePaths(id).Length > 0;
        }

        private static string FirstItemSpritePath(string id)
        {
            string[] paths = ItemSpritePaths(id);
            return paths.Length == 0 ? ItemIcon(id) : paths[0];
        }

        private static string[] ItemSpritePaths(string id)
        {
            string folder = System.IO.Path.Combine(ModernBoxMod.ModFolder, "GameResources", "ItemTextures");
            if (!System.IO.Directory.Exists(folder)) return new string[0];
            string prefix = "w_" + id + "_";
            return System.IO.Directory.GetFiles(folder, "*.png", System.IO.SearchOption.TopDirectoryOnly)
                .Where(path => System.IO.Path.GetFileName(path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => ItemMaterialOrder(System.IO.Path.GetFileNameWithoutExtension(path)))
                .ThenBy(path => path, StringComparer.Ordinal)
                .Select(path => "ItemTextures/" + System.IO.Path.GetFileNameWithoutExtension(path))
                .ToArray();
        }

        private static int ItemMaterialOrder(string name)
        {
            string suffix = name.Substring(name.LastIndexOf('_') + 1).ToLowerInvariant();
            switch (suffix)
            {
                case "base": return 0;
                case "iron": return 1;
                case "copper": return 2;
                case "bronze": return 3;
                case "silver": return 4;
                case "steel": return 5;
                case "mythril": return 6;
                case "adamantine": return 7;
                default: return 20;
            }
        }

        private static string ItemIcon(string id)
        {
            string supplied = SuppliedItemIcon(id);
            if (!string.IsNullOrEmpty(supplied)) return supplied;
            if (id.IndexOf("MIRV", StringComparison.OrdinalIgnoreCase) >= 0)
                return "ui/Icons/MIRV";
            if (id == "Sandevistan" || id == "TurboBooster") return "ui/Icons/SolarPoweredCyberBody";
            if (id == "Meth" || id == "Crack") return "ui/Icons/Drugs";
            return id.StartsWith("Pipe", StringComparison.Ordinal) || id == "Musket" ? "ui/Icons/lowfirearm" : "ui/Icons/firearm";
        }

        private static string SuppliedItemIcon(string id)
        {
            string folder = System.IO.Path.Combine(ModernBoxMod.ModFolder, "GameResources", "ui", "Icons", "items");
            if (!System.IO.Directory.Exists(folder)) return null;
            string prefix = "icon_" + id + "_";
            string file = System.IO.Directory.GetFiles(folder, "*.png", System.IO.SearchOption.TopDirectoryOnly)
                .Where(path => System.IO.Path.GetFileName(path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => ItemMaterialOrder(System.IO.Path.GetFileNameWithoutExtension(path)))
                .ThenBy(path => path, StringComparer.Ordinal)
                .FirstOrDefault();
            return file == null ? null : "ui/Icons/items/" + System.IO.Path.GetFileNameWithoutExtension(file);
        }

        private static void ApplyAccessoryStats(EquipmentAsset item, string id)
        {
            if (id == "Sandevistan")
            {
                item.base_stats["speed"] = 100f;
                item.base_stats["attack_speed"] = 80f;
                item.base_stats["stamina"] = 25f;
            }
            else if (id == "TurboBooster")
            {
                item.base_stats["speed"] = 60f;
                item.base_stats["stamina"] = 100f;
            }
            else if (id == "Meth")
            {
                item.base_stats["speed"] = 40f;
                item.base_stats["damage"] = 25f;
                item.base_stats["health"] = -15f;
            }
            else if (id == "Crack")
            {
                item.base_stats["attack_speed"] = 35f;
                item.base_stats["armor"] = 20f;
                item.base_stats["health"] = -30f;
            }
        }

        internal static void RegisterTraits()
        {
            ActorTraitGroupAsset vehicleGroup = new ActorTraitGroupAsset { id = "ModernBox", name = "ModernBox", color = "#6E7B8B", show_counter = true };
            ActorTraitGroupAsset ideologyGroup = new ActorTraitGroupAsset { id = "IdeologyBox", name = "Ideologies", color = "#D4AF37", show_counter = true };
            ModernLocalization.Add("ModernBox", "ModernBox");
            ModernLocalization.Add("Ideologies", "Ideologies");
            AssetManager.trait_groups.add(vehicleGroup);
            AssetManager.trait_groups.add(ideologyGroup);

            RegisterVehicleTrait("Jet", "ui/Icons/Plane", "Aircraft platform.", 0f);
            RegisterVehicleTrait("Vehicle", "ui/Icons/tabIconModernWarfare", "A ModernBox vehicle.", -100f);
            RegisterVehicleTrait("MIRVBoat", "ui/Icons/Boat", "ModernBox naval military unit.", -100f);
            RegisterVehicleTrait("Helicopter", "ui/Icons/Heli", "Rotary-wing aircraft.", -100f);
            RegisterVehicleTrait("Tank", "ui/Icons/Tank", "Armored ground vehicle.", -100f);
            RegisterVehicleTrait("Railgun", "ui/Icons/Railgun", "Railgun platform.", -100f);
            RegisterVehicleTrait("Humvee", "ui/Icons/Humvee", "Light military vehicle.", -100f);
            RegisterVehicleTrait("Zeppelin", "ui/Icons/Airship", "Large airship.", -100f);
            RegisterVehicleTrait("spawnedvehicle", "ui/Icons/tabIconModernWarfare", "Produced ModernBox military unit.", -100f);
            RegisterVehicleTrait("SupportRole", "ui/Icons/SolarPoweredCyberBody", "Military support role.", -100f);

            // These traits drove M2's non-space simulation systems. Their
            // callbacks are implemented by M2LegacyBehaviorService so the
            // original behavior remains bounded and save-safe on build 719.
            RegisterLegacyBehaviorTrait("Unitpotential", "ui/Icons/UnitpotentialIcon", "Allows the expanded civilization unit roster and veteran vehicle upgrades.");
            RegisterLegacyBehaviorTrait("Potential", "ui/Icons/PotentialIcon", "May evolve into a stronger M2 form when its conditions are met.");
            RegisterLegacyBehaviorTrait("AssimilatorSpawner", "ui/Icons/AssimilatorSpawner", "Creates Assimilator production cores.");
            RegisterLegacyBehaviorTrait("IceTowerSpawner", "ui/Icons/IceTowerSpawner", "Creates one Ice Walker production or defense tower per chunk during winter.");
            RegisterLegacyBehaviorTrait("Walker_Titan", "ui/Icons/Walker_TitanIcon", "An Ice Walker titan that leaves a functioning walker spawner behind.");
            RegisterLegacyBehaviorTrait("zombie_spawner", "ui/Icons/NightInfusedZombie", "An evolved zombie capable of seeding corpse piles.");
            RegisterLegacyBehaviorTrait("SolarPoweredCyberBody", "ui/Icons/SolarPoweredCyberBody", "Assimilator machinery whose performance follows the world era.");
            RegisterLegacyBehaviorTrait("frozenmachinary", "ui/Icons/frozenmachinary", "Machinery impaired by extreme cold.");
            RegisterLegacyBehaviorTrait("unpoweredmachinery", "ui/Icons/unpoweredmachinery", "Machinery operating without enough power.");
            RegisterLegacyBehaviorTrait("Freezer", "ui/Icons/FreezerIcon", "Freezes targets and survives extreme cold.");
            RegisterLegacyZombieTrait("NightInfusedZombie", "ui/Icons/NightInfusedZombie", 40f, 50f, 0f, 0f);
            RegisterLegacyZombieTrait("ChaosZombie", "ui/Icons/ChaosZombie", 20f, 30f, 0f, 0.3f);
            RegisterLegacyZombieTrait("FrostedZombie", "ui/Icons/FrostedZombie", -30f, -50f, 0f, 0f);
            RegisterLegacyZombieTrait("ScorchedZombie", "ui/Icons/ScorchedZombie", -100f, 0f, -100f, 0f);

            foreach (string id in IdeologyIds)
            {
                ActorTrait trait = new ActorTrait
                {
                    id = id,
                    base_stats = new BaseStats(),
                    path_icon = IdeologyIcon(id),
                    group_id = "IdeologyBox",
                    type = TraitType.Other,
                    can_be_given = true,
                    can_be_removed = true,
                    can_be_cured = false,
                    rate_birth = ModernBoxSettings.Get("IdeologiesOption") ? 37 : 0,
                    rate_inherit = ModernBoxSettings.Get("IdeologiesOption") ? 100 : 0,
                    rate_acquire_grow_up = 0,
                    is_mutation_box_allowed = true
                };
                ApplyIdeologyStats(trait, id);
                foreach (string opposite in IdeologyIds) if (opposite != id) trait.addOpposite(opposite);
                AddTraitLocale(id, id, "A ModernBox political ideology.");
                trait.cached_sprite = LoadWorldSafeTraitIcon(trait.path_icon, id);
                AssetManager.traits.add(trait);
                trait.unlock(true);
            }
            RegisterIdeologyLoadHooks();
        }

        private static void ApplyIdeologyStats(ActorTrait trait, string id)
        {
            if (id == "Dynastic")
            {
                trait.base_stats["lifespan"] = 30f; trait.base_stats["intelligence"] = -2f;
                trait.base_stats["diplomacy"] = 10f; trait.base_stats["stewardship"] = 10f;
                trait.base_stats["loyalty_mood"] = 15f; trait.base_stats["offspring"] = -0.15f;
            }
            else if (id == "Mercantile")
            {
                trait.base_stats["intelligence"] = 5f; trait.base_stats["diplomacy"] = 10f;
                trait.base_stats["opinion"] = 10f; trait.base_stats["loyalty_mood"] = 15f; trait.base_stats["cities"] = -3f;
            }
            else if (id == "Peoplewoven")
            {
                trait.base_stats["intelligence"] = 5f; trait.base_stats["warfare"] = 10f;
                trait.base_stats["diplomacy"] = 5f; trait.base_stats["stewardship"] = 3f;
                trait.base_stats["opinion"] = 10f; trait.base_stats["loyalty_mood"] = -50f; trait.base_stats["cities"] = 3f;
            }
            else if (id == "Martial")
            {
                trait.base_stats["intelligence"] = 5f; trait.base_stats["warfare"] = 20f;
                trait.base_stats["diplomacy"] = -10f; trait.base_stats["stewardship"] = 5f;
                trait.base_stats["opinion"] = -20f; trait.base_stats["loyalty_mood"] = -50f; trait.base_stats["cities"] = 3f;
            }
            else
            {
                trait.base_stats["lifespan"] = -10f; trait.base_stats["attack_speed"] = 15f;
                trait.base_stats["intelligence"] = -5f; trait.base_stats["warfare"] = 20f;
                trait.base_stats["diplomacy"] = -500f; trait.base_stats["stewardship"] = -400f;
                trait.base_stats["opinion"] = -800f; trait.base_stats["loyalty_mood"] = -10000f; trait.base_stats["cities"] = -100f;
            }
        }

        private static void RegisterIdeologyLoadHooks()
        {
            if (_ideologyLoadHooksRegistered) return;
            foreach (string speciesId in SapientSpeciesIds)
            {
                ActorAsset species = AssetManager.actor_library.get(speciesId);
                if (species != null) species.action_on_load += EnsureLoadedIdeology;
            }
            _ideologyLoadHooksRegistered = true;
        }

        private static void EnsureLoadedIdeology(Actor actor)
        {
            EnsureDefaultIdeology(actor);
        }

        internal static bool EnsureDefaultIdeology(Actor actor)
        {
            return EnsureDefaultIdeology(actor, actor == null ? null : actor.asset);
        }

        internal static bool EnsureDefaultIdeology(Actor actor, ActorAsset species)
        {
            if (!ModernBoxSettings.Get("IdeologiesOption") || actor == null || species == null ||
                !SapientSpeciesIds.Contains(species.id)) return false;
            foreach (string ideologyId in IdeologyIds)
            {
                if (actor.hasTrait(ideologyId)) return false;
            }

            // Stable assignment avoids perturbing WorldBox's simulation RNG and
            // gives an even distribution across long-lived populations.
            long actorId = actor.getID();
            int index = (int)(actorId % IdeologyIds.Length);
            if (index < 0) index += IdeologyIds.Length;
            return actor.addTrait(IdeologyIds[index], true);
        }

        internal static int BackfillDefaultIdeologies()
        {
            if (!ModernBoxSettings.Get("IdeologiesOption") || World.world == null || World.world.units == null) return 0;
            int assigned = 0;
            foreach (Actor actor in World.world.units)
            {
                if (EnsureDefaultIdeology(actor)) assigned++;
            }
            return assigned;
        }

        private static void RegisterVehicleTrait(string id, string icon, string description, float fertility)
        {
            ActorTrait trait = new ActorTrait
            {
                id = id,
                base_stats = new BaseStats(),
                path_icon = icon,
                group_id = "ModernBox",
                type = TraitType.Other,
                can_be_given = false,
                can_be_removed = false,
                can_be_cured = false,
                rate_birth = 0,
                rate_inherit = 0,
                rate_acquire_grow_up = 0
            };
            AddTraitLocale(id, id, description);
            trait.cached_sprite = LoadWorldSafeTraitIcon(trait.path_icon, id);
            AssetManager.traits.add(trait);
        }

        private static void RegisterLegacyBehaviorTrait(string id, string icon, string description)
        {
            if (AssetManager.traits.get(id) != null) return;
            ActorTrait trait = new ActorTrait
            {
                id = id,
                base_stats = new BaseStats(),
                path_icon = icon,
                group_id = "ModernBox",
                type = TraitType.Other,
                can_be_given = true,
                can_be_removed = true,
                can_be_cured = false,
                rate_birth = 0,
                rate_inherit = 0,
                rate_acquire_grow_up = 0
            };
            if (id == "Unitpotential") trait.action_death = M2LegacyBehaviorService.OnUnitPotentialDeath;
            else if (id == "Walker_Titan") trait.action_death = M2LegacyBehaviorService.OnWalkerTitanDeath;
            else if (id == "zombie_spawner") trait.action_death = M2LegacyBehaviorService.OnZombieSpawnerDeath;
            AddTraitLocale(id, FriendlyLegacyTraitName(id), description);
            trait.cached_sprite = LoadWorldSafeTraitIcon(trait.path_icon, id);
            AssetManager.traits.add(trait);
        }

        private static void RegisterLegacyZombieTrait(string id, string icon, float speed, float attackSpeed, float accuracy, float healthMultiplier)
        {
            RegisterLegacyBehaviorTrait(id, icon, "An environmental mutation of the M2 zombie infection.");
            ActorTrait trait = AssetManager.traits.get(id);
            if (trait == null) return;
            trait.base_stats["speed"] = speed;
            trait.base_stats["attack_speed"] = attackSpeed;
            trait.base_stats["accuracy"] = accuracy;
            // Legacy mod_health no longer exists. A small flat bonus preserves
            // ChaosZombie's toughness without writing an invalid stat ID.
            if (healthMultiplier > 0f) trait.base_stats["health"] = healthMultiplier * 100f;
        }

        private static string FriendlyLegacyTraitName(string id)
        {
            switch (id)
            {
                case "Unitpotential": return "Unit Potential";
                case "AssimilatorSpawner": return "Assimilator Spawner";
                case "IceTowerSpawner": return "Ice Tower Spawner";
                case "Walker_Titan": return "Walker Titan";
                case "zombie_spawner": return "Zombie Spawner";
                case "SolarPoweredCyberBody": return "Solar Powered Cyber Body";
                case "frozenmachinary": return "Frozen Machinery";
                case "unpoweredmachinery": return "Unpowered Machinery";
                default: return id;
            }
        }

        private static Sprite LoadWorldSafeTraitIcon(string path, string traitId)
        {
            Sprite source = Resources.Load<Sprite>(path);
            if (source == null) throw new InvalidOperationException("Missing trait icon for " + traitId + " at " + path + ".");

            // Conversation topics render trait sprites directly in world space.
            // M1's source icons range from 200px to 1920px and use Unity's default
            // 100 PPU, making them several tiles wide. Preserve the exact texture
            // for UI use while normalizing its world bounds to a vanilla-like 0.32.
            const float targetWorldSize = 0.32f;
            float longestEdge = Mathf.Max(source.rect.width, source.rect.height);
            float pixelsPerUnit = Mathf.Max(1f, longestEdge / targetWorldSize);
            Vector2 pivot = new Vector2(
                source.rect.width <= 0f ? 0.5f : source.pivot.x / source.rect.width,
                source.rect.height <= 0f ? 0.5f : source.pivot.y / source.rect.height);
            Sprite normalized = Sprite.Create(source.texture, source.rect, pivot, pixelsPerUnit);
            normalized.name = source.name + "_modernbox_world_safe";
            return normalized;
        }

        private static void AddTraitLocale(string id, string name, string description)
        {
            string normalized = id.ToLowerInvariant().Replace(' ', '_');
            ModernLocalization.Add("trait_" + id, name);
            ModernLocalization.Add("trait_" + id + "_description", description);
            ModernLocalization.Add("trait_" + normalized, name);
            ModernLocalization.Add("trait_" + normalized + "_description", description);
        }

        private static string IdeologyIcon(string id) { return "ui/Icons/" + id; }

        internal static void RegisterNames()
        {
            AddNames("Modern_human_Names", "Arthur,Samantha,William,Michael,Nancy,Robert,Natasha,Iris,Grace,Viktor,Bradley,Francesco,Magnus,Marc,Jerome,Angel,Dexter,Mitchell,Russell,Walker,Harper,Pearce,George,Archer,John,Finn,Lucas,Charles,Martin", "Tucker,Ford,Mitchell,Russell,Walker,Harper,Pearce,Stephenson,Erickson,King,Larson,Goodwin,Garner,Bonaparte,Dubois,Duval,Richard,Marino,Rommel,Weber,Braun,Wagner,Lee,Liang,Putin,Garcia,Williams");
            AddNames("Modern_orc_Names", "Grommash,Thrakka,Grulok,Durgar,Morgash,Drakka,Krusk,Gornak,Thokk,Roktar,Azog,Garrosh", "Bloodaxe,Ironhide,Skullcrusher,Blackfang,Stonefist,Doomhammer,Ironskull,Warblade");
            AddNames("Modern_elf_Names", "Lirael,Eledrin,Thalindra,Elowen,Galadriel,Lorandor,Ilyndor,Faelarion,Elanor,Lorien", "Silverleaf,Moonshadow,Starwhisper,Windrunner,Nightbloom,Frostfall,Sunblade,Swiftarrow");
            AddNames("Modern_dwarf_Names", "Balin,Thorin,Dwalin,Gimli,Fili,Kili,Gloin,Durin,Brokk,Eitri,Hrothgar", "Ironbeard,Stoneforge,Graniteheart,Hammerstrike,Steelhelm,Oakenshield,Fireforge");
            AddNames("Modern_Names", "Arthur,Samantha,William,Michael,Nancy,Robert,Natasha,Iris,Grace,Viktor,Bradley,Francesco,Magnus", "Tucker,Ford,Mitchell,Russell,Walker,Harper,Pearce,King,Larson,Goodwin");
            AddCodeNames("Jet_Names", "F-,V-,X-,J-,S-");
            AddCodeNames("Humvee_Names", "H-");
            AddCodeNames("MIRV_Names", "M-,R-,V-");

            // These three IDs were vanilla NameGeneratorAssets in the pre-2025
            // game used by original M2. In 0.51.2 they can still appear in old
            // content definitions/name sets while no generator asset exists.
            // Register complete local fallbacks so boats, the orc war turtle and
            // armored wolves can always generate a name safely.
            AddNames("human_name", "Arthur,Samantha,William,Michael,Nancy,Robert,Natasha,Iris,Grace,Viktor,Bradley,Francesco,Magnus,Marc,Jerome,Angel,Dexter,George,Archer,John,Finn,Lucas,Charles,Martin", "Tucker,Ford,Mitchell,Russell,Walker,Harper,Pearce,Stephenson,Erickson,King,Larson,Goodwin,Garner,Richard,Marino,Weber,Braun,Wagner,Lee,Garcia,Williams");
            AddNames("orc_name", "Grommash,Thrakka,Grulok,Durgar,Morgash,Drakka,Krusk,Gornak,Thokk,Roktar,Azog,Garrosh", "Bloodaxe,Ironhide,Skullcrusher,Blackfang,Stonefist,Doomhammer,Ironskull,Warblade");
            AddNames("wolf_name", "Fang,Claw,Howl,Shadow,Ash,Storm,Frost,Night,Red,Grey,Black,White", "Wolf,Hunter,Runner,Stalker,Biter,Howler,Prowler,Tracker");
        }

        private static void AddNames(string id, string first, string last)
        {
            if (AssetManager.name_generator.has(id))
            {
                EnsureCurrentNameGeneratorFields(AssetManager.name_generator.get(id));
                RegisterNameSet(id);
                return;
            }
            NameGeneratorAsset names = new NameGeneratorAsset { id = id };
            names.addPartGroup(first);
            names.addPartGroup(" ");
            names.addPartGroup(last);
            names.addTemplate("part_group");
            EnsureCurrentNameGeneratorFields(names);
            AssetManager.name_generator.add(names);
            RegisterNameSet(id);
        }

        private static void AddCodeNames(string id, string prefix)
        {
            if (AssetManager.name_generator.has(id))
            {
                EnsureCurrentNameGeneratorFields(AssetManager.name_generator.get(id));
                RegisterNameSet(id);
                return;
            }
            NameGeneratorAsset names = new NameGeneratorAsset { id = id };
            names.addPartGroup(prefix);
            names.addPartGroup("10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40");
            names.addPartGroup("A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z");
            names.addTemplate("part_group");
            EnsureCurrentNameGeneratorFields(names);
            AssetManager.name_generator.add(names);
            RegisterNameSet(id);
        }

        private static void EnsureCurrentNameGeneratorFields(NameGeneratorAsset names)
        {
            if (names == null) return;
            // Build 719's NameGenerator.getName always scans this array for female
            // names, even when the selected template only uses part_group. Legacy
            // M2 generators left it null because the pre-2025 implementation did
            // not require it, causing a delayed crash when an unnamed female actor
            // first crafted an item or opened a name-dependent UI.
            if (names.vowels == null || names.vowels.Length == 0)
                names.vowels = new[] { "a", "e", "i", "o", "u", "y", "A", "E", "I", "O", "U", "Y" };
        }

        private static void RegisterNameSet(string id)
        {
            NameSetAsset set = AssetManager.name_sets.get(id);
            if (set == null)
            {
                set = new NameSetAsset { id = id };
                AssetManager.name_sets.add(set);
            }
            set.unit = id;
            set.kingdom = id;
            set.city = id;
            set.clan = id;
            set.culture = id;
            set.family = id;
            set.language = id;
            set.religion = id;
        }
    }
}

