using System;
using System.Collections.Generic;
using NCMS.Utils;
using tools;
using UnityEngine;

namespace ModernBoxRewrite
{
    internal static class EquipmentAndTraitsRegistry
    {
        private const string ItemNameGeneratorId = "ModernBox_Item_Names";
        internal const string MissileSystemAttackId = "modernbox_missile_system_mirv";

        private static readonly string[] IdeologyIds =
        {
            "Capitalist", "Communist", "Liberal", "Conservative", "Fascist", "Democratic",
            "Technocrat", "Luddite", "Environmental Steward", "Anarchist", "Primalism"
        };

        private static readonly HashSet<string> SapientSpeciesIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "human", "orc", "elf", "dwarf"
        };
        private static bool _ideologyLoadHooksRegistered;

        internal static void RegisterResourcesAndProjectiles()
        {
            RegisterResource("Parts", "ui/Icons/Factories", "common_metals", 9999, 12, 6);
            RegisterResource("CyberWareParts", "ui/Icons/Cyberware", "common_metals", 5000, 16, 8);
            RegisterResource("Xenium", "ui/Icons/Xeno", "common_metals", 2000, 24, 12);

            TerraformOptions blastTerraform = CreateTerraform("modernbox_blast_terraform", 550, 5, false);
            TerraformOptions nukerTerraform = CreateTerraform("modernbox_nuker_terraform", 10000, 43, true);
            AssetManager.terraform.add(blastTerraform);
            AssetManager.terraform.add(nukerTerraform);

            RegisterProjectile("modernbox_bullet", "ItemTextures/w_bullet", string.Empty, 0, "fx_hit", 45f, 0.16f);
            RegisterProjectile("modernbox_gunship_bullet", "ItemTextures/w_bullet", "modernbox_blast_terraform", 2, "fx_explosion_small", 55f, 0.2f);
            RegisterProjectile("modernbox_blast", "ItemTextures/w_bullet", "modernbox_blast_terraform", 5, "fx_explosion_middle", 28f, 0.25f);
            RegisterProjectile("NUKER", "effects/projectiles/NUKER/0", "modernbox_nuker_terraform", 43, "fx_explosion_nuke_atomic", 18f, 0.35f);

            RegisterMirvProjectile("modernbox_mirv_budget", 5, 500);
            RegisterMirvProjectile("modernbox_mirv_decent", 12, 1500);
            RegisterMirvProjectile("modernbox_mirv", 25, 3000);
            RegisterMirvProjectile("modernbox_mirv_bomb", 40, 5000);
            RegisterMirvProjectile("modernbox_mirv_strong", 60, 10000);
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
            ProjectileAsset projectile = new ProjectileAsset
            {
                id = id,
                texture = texture,
                animated = id == "NUKER",
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
                can_be_blocked = id == "modernbox_bullet",
                can_be_left_on_ground = false,
                draw_light_area = radius > 0,
                draw_light_size = radius > 0 ? 0.4f : 0f
            };
            if (projectile.animated)
            {
                projectile.frames = Resources.LoadAll<Sprite>("effects/projectiles/NUKER");
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
                string template = spec.Type == EquipmentType.Weapon ? "$range" : (spec.Type == EquipmentType.Ring ? "$ring" : "$amulet");
                EquipmentAsset item = AssetManager.items.clone(spec.Id, template);
                item.id = spec.Id;
                item.translation_key = spec.Id;
                // ItemAsset.getRandomNameTemplate assumes this list is non-null, even
                // for ordinary-quality equipment when a generated modifier requests a
                // name. The generic $range/$ring/$amulet bases do not provide one.
                item.name_templates = new List<string> { ItemNameGeneratorId };
                item.equipment_type = spec.Type;
                item.equipment_subtype = spec.Type == EquipmentType.Weapon ? "stick" : (spec.Type == EquipmentType.Ring ? "ring" : "amulet");
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
                item.path_gameplay_sprite = ModernBoxCatalog.MirvIds.Contains(spec.Id) || HasItemSprite(spec.Id)
                    ? "ItemTextures/w_" + spec.Id + "_base"
                    : ItemIcon(spec.Id);
                item.gameplay_sprites = LoadItemSprites(spec.Id);
                PreloadHeldItemSprites(item);
                item.base_stats["damage"] = spec.Damage;
                item.base_stats["range"] = spec.Range;
                item.base_stats["attack_speed"] = spec.AttackSpeed;
                item.base_stats["accuracy"] = spec.Accuracy;
                item.base_stats["targets"] = 1f;
                item.base_stats["critical_chance"] = 0.12f;
                item.base_stats["projectiles"] = spec.Id == "PipeShotgun" ? 6f : 1f;
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
                ModernLocalization.Add(spec.Id + "_description", "ModernBox equipment. Requires " + spec.Tier + " human-city progression.");
                ModernLocalization.Add("item_" + spec.Id, spec.DisplayName);
                ModernLocalization.Add("item_" + spec.Id + "_description", "ModernBox equipment. Requires " + spec.Tier + " human-city progression.");
                string normalized = spec.Id.ToLowerInvariant();
                ModernLocalization.Add("item_" + normalized, spec.DisplayName);
                ModernLocalization.Add("item_" + normalized + "_description", "ModernBox equipment. Requires " + spec.Tier + " human-city progression.");
            }
            RegisterMissileSystemAttack();
        }

        private static void RegisterMissileSystemAttack()
        {
            EquipmentAsset mirv = AssetManager.items.get("MIRV");
            if (mirv == null) throw new InvalidOperationException("Cannot register MissileSystem launcher before the MIRV equipment asset.");

            // The original M1 MissileSystem always used MIRV as its built-in
            // attack. Give it a private clone so the player-facing MIRV crafting
            // toggle cannot replace the vehicle projectile with a gun round.
            EquipmentAsset launcher = AssetManager.items.clone(MissileSystemAttackId, "MIRV");
            launcher.id = MissileSystemAttackId;
            launcher.translation_key = "MIRV";
            launcher.projectile = "modernbox_mirv";
            launcher.is_pool_weapon = false;
            launcher.pool_rate = 0;
            launcher.equipment_value = 0;
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
            if (AssetManager.name_generator.has(ItemNameGeneratorId)) return;
            NameGeneratorAsset names = new NameGeneratorAsset { id = ItemNameGeneratorId };
            names.addPartGroup("Guardian,Vanguard,Sentinel,Defender,Thunder,Lightning,Storm,Liberty,Victory,Iron,Steel,Crimson,Black,Silver,Golden");
            names.addPartGroup(" ");
            names.addPartGroup("Rifle,Carbine,Cannon,Launcher,Repeater,Sidearm,Weapon,System,Prototype,Mark");
            names.addTemplate("part_group");
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
            Sprite basic = Resources.Load<Sprite>("ItemTextures/w_" + id + "_base");
            Sprite iron = Resources.Load<Sprite>("ItemTextures/w_" + id + "_iron");
            if (basic != null && iron != null) return new[] { basic, iron };
            if (basic != null) return new[] { basic };
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
            return System.IO.File.Exists(System.IO.Path.Combine(ModernBoxMod.ModFolder, "GameResources", "ItemTextures", "w_" + id + "_base.png"));
        }

        private static string ItemIcon(string id)
        {
            if (id.IndexOf("MIRV", StringComparison.OrdinalIgnoreCase) >= 0)
                return id == "BudgetMIRV" ? "ui/Icons/BudgetMIRV" : (id == "DecentMIRV" ? "ui/Icons/DecentMIRV" : "ui/Icons/MIRV");
            if (id == "Sandevistan" || id == "TurboBooster") return "ui/Icons/Cyberware";
            if (id == "Meth" || id == "Crack") return "ui/Icons/Drugs";
            return id.StartsWith("Pipe", StringComparison.Ordinal) || id == "Musket" ? "ui/Icons/lowfirearm" : "ui/Icons/firearm";
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
            RegisterVehicleTrait("MIRVBoat", "ui/Icons/Boat", "Legacy M1 marker trait. Original M1 did not implement a MIRV boat or modern dock.", -100f);
            RegisterVehicleTrait("Helicopter", "ui/Icons/Heli", "Rotary-wing aircraft.", -100f);
            RegisterVehicleTrait("Tank", "ui/Icons/Tank", "Armored ground vehicle.", -100f);
            RegisterVehicleTrait("Railgun", "ui/Icons/Railgun", "Railgun platform.", -100f);
            RegisterVehicleTrait("Humvee", "ui/Icons/Humvee", "Light military vehicle.", -100f);
            RegisterVehicleTrait("Zeppelin", "ui/Icons/Airship", "Large airship.", -100f);

            Dictionary<string, float[]> stats = new Dictionary<string, float[]>(StringComparer.Ordinal)
            {
                { "Capitalist", new[] { 0f, 5f, 3f, 0f } }, { "Communist", new[] { 7f, 1f, 8f, 0f } },
                { "Liberal", new[] { 0f, 3f, 2f, 5f } }, { "Conservative", new[] { 3f, 1f, 2f, 4f } },
                { "Fascist", new[] { 15f, -7f, 8f, 3f } }, { "Democratic", new[] { 20f, 15f, 5f, 15f } },
                { "Technocrat", new[] { 2f, 5f, 4f, 10f } }, { "Luddite", new[] { -3f, -5f, 1f, -7f } },
                { "Environmental Steward", new[] { -5f, 5f, 2f, 10f } }, { "Anarchist", new[] { 5f, -25f, -8f, 2f } },
                { "Primalism", new[] { 12f, -10f, -3f, -8f } }
            };
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
                float[] values = stats[id];
                trait.base_stats["warfare"] = values[0];
                trait.base_stats["diplomacy"] = values[1];
                trait.base_stats["cities"] = values[2];
                trait.base_stats["stewardship"] = values[3];
                foreach (string opposite in IdeologyIds) if (opposite != id) trait.addOpposite(opposite);
                trait.cached_sprite = LoadWorldSafeTraitIcon(trait.path_icon, id);
                AssetManager.traits.add(trait);
                trait.unlock(true);
                AddTraitLocale(id, id, "A ModernBox political ideology.");
            }
            RegisterIdeologyLoadHooks();
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
            trait.cached_sprite = LoadWorldSafeTraitIcon(trait.path_icon, id);
            AssetManager.traits.add(trait);
            AddTraitLocale(id, id, description);
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

        private static string IdeologyIcon(string id)
        {
            if (id == "Fascist") return "ui/Icons/Facist";
            if (id == "Environmental Steward") return "ui/Icons/EnvironmentalSteward";
            return "ui/Icons/" + id;
        }

        internal static void RegisterNames()
        {
            AddNames("Modern_Names", "Arthur,Samantha,William,Michael,Nancy,Robert,Natasha,Iris,Grace,Viktor,Bradley,Francesco,Magnus,Marc,Jerome,Angel,Dexter,Mitchell,Russell,Walker,Harper,Pearce,George,Archer,John,Finn,Lucas,Charles,Martin", "Tucker,Ford,Mitchell,Russell,Walker,Harper,Pearce,Stephenson,Erickson,King,Larson,Goodwin,Garner,Bonaparte,Dubois,Duval,Richard,Marino,Rommel,Weber,Braun,Wagner,Lee,Liang,Putin,Garcia,Williams");
            AddNames("Modern_Orc_Names", "Grommash,Thrakka,Grulok,Durgar,Morgash,Drakka,Krusk,Gornak,Thokk,Roktar,Azog,Garrosh", "Bloodaxe,Ironhide,Skullcrusher,Blackfang,Stonefist,Doomhammer,Ironskull,Warblade");
            AddNames("Modern_Elf_Names", "Lirael,Eledrin,Thalindra,Elowen,Galadriel,Lorandor,Ilyndor,Faelarion,Elanor,Lorien", "Silverleaf,Moonshadow,Starwhisper,Windrunner,Nightbloom,Frostfall,Sunblade,Swiftarrow");
            AddNames("Modern_Dwarf_Names", "Balin,Thorin,Dwalin,Gimli,Fili,Kili,Gloin,Durin,Brokk,Eitri,Hrothgar", "Ironbeard,Stoneforge,Graniteheart,Hammerstrike,Steelhelm,Oakenshield,Fireforge");
            AddCodeNames("Jet_Names", "F-,V-,X-,J-,S-");
            AddCodeNames("Humvee_Names", "H-");
            AddCodeNames("MIRV_Names", "M-,R-,V-");
        }

        private static void AddNames(string id, string first, string last)
        {
            NameGeneratorAsset names = new NameGeneratorAsset { id = id };
            names.addPartGroup(first);
            names.addPartGroup(" ");
            names.addPartGroup(last);
            names.addTemplate("part_group");
            AssetManager.name_generator.add(names);
            RegisterNameSet(id);
        }

        private static void AddCodeNames(string id, string prefix)
        {
            NameGeneratorAsset names = new NameGeneratorAsset { id = id };
            names.addPartGroup(prefix);
            names.addPartGroup("10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40");
            names.addPartGroup("A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z");
            names.addTemplate("part_group");
            AssetManager.name_generator.add(names);
            RegisterNameSet(id);
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
