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
        internal static readonly string[] IdeologyIds =
        {
            "Dynastic", "Mercantile", "Peoplewoven", "Martial", "Chaosvolt"
        };

        internal static readonly HashSet<string> SapientSpeciesIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "human", "orc", "elf", "dwarf"
        };

        internal static void RegisterResourcesAndProjectiles()
        {
            RegisterResource("Parts", "ui/Icons/Factories", "common_metals", 9999, 12, 6);
            RegisterResource("CyberWareParts", "ui/Icons/SolarPoweredCyberBody", "common_metals", 5000, 16, 8);
            RegisterResource("Xenium", "ui/Icons/Xeno", "common_metals", 2000, 24, 12);

            // Original M2 NUKERExplode: radius 30, 20000 damage, fire, no wasteland.
            M2NoBlastStun.Register("modernbox_nuker_terraform");
            AssetManager.terraform.add(new TerraformOptions
            {
                id = "modernbox_nuker_terraform",
                flash = true,
                damage_buildings = true,
                damage = 20000,
                apply_force = true,
                applies_to_high_flyers = true,
                explode_and_set_random_fire = true,
                explode_tile = true,
                explosion_pixel_effect = true,
                explode_strength = 1,
                transform_to_wasteland = false,
                shake = true,
                remove_ruins = false,
                remove_tornado = false,
                attack_type = AttackType.Explosion
            });
            RegisterProjectile("NUKER", "effects/projectiles/NUKER/0", "modernbox_nuker_terraform", 30, "fx_explosion_nuke_atomic", 5.5f, 0.3f);
            OriginalM2Projectiles.Register();

            ProjectileAsset nuker = AssetManager.projectiles.get("NUKER");
            // Resourcez.cs: speed 5.5, parabolic, no shadow, sprite not turned toward the target.
            nuker.look_at_target = false;
            OriginalM2Projectiles.LegacyArcProjectileIds.Add("NUKER");
            // The old Projectile.targetReached always spawned the end effect at 0.25.
            nuker.end_effect_scale = 0.25f;

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
                can_be_collided = false,
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

        private static void RegisterMirvProjectile(string id, int radius, int damage)
        {
            string terraformId = id + "_terraform";
            AssetManager.terraform.add(CreateTerraform(terraformId, damage, radius, true));
            M2NoBlastStun.Register(terraformId);
            RegisterProjectile(id, "effects/projectiles/NUKER/0", terraformId, radius, radius >= 40 ? "fx_explosion_nuke_atomic" : "fx_explosion_middle", 22f, radius >= 40 ? 0.35f : 0.22f);
        }

        internal static void RegisterEquipment()
        {
            foreach (EquipmentSpec spec in ContentRegistry.Equipment)
            {
                bool melee = ContentRegistry.IsMeleeEquipment(spec.Id);
                string template = spec.Type == EquipmentType.Weapon ? (melee ? "$melee" : "$range") :
                    spec.Type == EquipmentType.Ring ? "$ring" :
                    spec.Type == EquipmentType.Armor ? "$armor" :
                    spec.Type == EquipmentType.Helmet ? "$helmet" :
                    spec.Type == EquipmentType.Boots ? "$boots" : "$amulet";
                EquipmentAsset item = AssetManager.items.clone(spec.Id, template);
                item.id = spec.Id;
                item.translation_key = spec.Id;
                // ItemAsset.getRandomNameTemplate assumes this list is non-null.
                item.name_templates = new List<string> { NameTemplate(spec) };
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
                item.path_slash_animation = SlashAnimation(spec.Id);
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
                foreach (KeyValuePair<string, float> pair in spec.BaseStats)
                {
                    float material;
                    spec.MaterialStats.TryGetValue(pair.Key, out material);
                    SetItemStat(item, pair.Key, pair.Value + material);
                }
                foreach (KeyValuePair<string, float> pair in spec.MaterialStats)
                    if (!spec.BaseStats.ContainsKey(pair.Key)) SetItemStat(item, pair.Key, pair.Value);
                item.equipment_value = spec.Value;
                item.rigidity_rating = spec.Type == EquipmentType.Weapon ? 4 : 2;
                item.quality = spec.Id == "malorian" ? Rarity.R3_Legendary : Rarity.R0_Normal;
                item.setCost(0, spec.Resource1, spec.Resource1Cost, spec.Resource2, spec.Resource2Cost);
                item.minimum_city_storage_resource_1 = spec.MinimumStorage > 0 ? spec.MinimumStorage : Math.Max(1, spec.Resource1Cost);
                // Keep the manually loaded M1 material sprites. The vanilla pool preloader
                // otherwise overwrites them by looking for a single combined texture path.
                item.is_pool_weapon = false;
                item.pool_rate = spec.Tier >= ProgressionTier.Strategic ? 3 : 12;
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
            RegisterPristineVariants();
            RegisterUnitAttacks();
        }

        // The pristine set could be made from steel, mythril or adamantine. The old engine
        // took the best material the city had the tech and resources for, and the item
        // got that material's armor and value. 0.51.2 items have one cost and one set of
        // stats, so the mythril and adamantine pieces are their own items.
        private static readonly string[] PristinePieces = { "pristinearmor", "pristineboots", "pristinehelmet" };

        private sealed class PristineMaterial
        {
            internal string Id;
            internal string Tech;
            internal float Armor;
            internal int Value;
        }

        // Best first, like the old engine's search from the end of ItemArmorMaterialLibrary.
        private static readonly PristineMaterial[] PristineMaterials =
        {
            new PristineMaterial { Id = "adamantine", Tech = "material_adamantine", Armor = 10f, Value = 70 },
            new PristineMaterial { Id = "mythril", Tech = "material_mythril", Armor = 8f, Value = 50 }
        };

        private const int SteelArmorValue = 40;

        private static void RegisterPristineVariants()
        {
            foreach (string piece in PristinePieces)
            {
                EquipmentSpec spec = ContentRegistry.Equipment.Find(candidate => candidate.Id == piece);
                if (spec == null || AssetManager.items.get(piece) == null) continue;
                foreach (PristineMaterial material in PristineMaterials)
                {
                    string id = piece + "_" + material.Id;
                    EquipmentAsset item = AssetManager.items.clone(id, piece);
                    item.id = id;
                    item.translation_key = piece;
                    item.show_in_knowledge_window = false;
                    item.show_for_unlockables_ui = false;
                    item.pool_rate = 0;
                    item.setCost(0, material.Id, 1, "none", 0);
                    item.minimum_city_storage_resource_1 = 0;
                    item.equipment_value = spec.Value - SteelArmorValue + material.Value;
                    item.base_stats = new BaseStats();
                    foreach (KeyValuePair<string, float> pair in spec.BaseStats)
                        SetItemStat(item, pair.Key, pair.Key == "armor" ? pair.Value + material.Armor : pair.Value);
                    if (!spec.BaseStats.ContainsKey("armor")) SetItemStat(item, "armor", material.Armor);
                }
            }
        }

        // The pristine piece the city makes: adamantine or mythril when it has the tech and
        // the resource, otherwise steel.
        private static EquipmentAsset PristineChoice(EquipmentAsset steel, City city)
        {
            if (steel == null || Array.IndexOf(PristinePieces, steel.id) < 0 || city == null) return steel;
            foreach (PristineMaterial material in PristineMaterials)
            {
                if (!M2Tech.Has(city, material.Tech) || city.getResourcesAmount(material.Id) < 1) continue;
                EquipmentAsset variant = AssetManager.items.get(steel.id + "_" + material.Id);
                if (variant != null) return variant;
            }
            return steel;
        }

        private static void SetItemStat(EquipmentAsset item, string key, float value)
        {
            // The old engine never read accuracy.
            if (key == "accuracy") return;
            // Old max_age (years) is 0.51.2's lifespan.
            string current = key == "max_age" ? "lifespan" : key;
            if (AssetManager.base_stats_library.get(current) == null)
            {
                ModernBoxDiagnostics.Warn("Skipped obsolete M2 equipment stat '" + key + "'.");
                return;
            }
            item.base_stats[current] = key == "attack_speed" ? M2AttackSpeed.Delta(value, M2AttackSpeed.OldHumanBase)
                : key == "speed" ? value * M2OriginalStats.SpeedScale
                : key == "critical_damage_multiplier" ? M2AttackSpeed.CritMultiplier(value) : value;
        }

        // The old engine merged a unit's built-in attack item without any material, so
        // units that attack with a craftable gun get a copy of it without the material stats.
        internal const string AttackPrefix = "modernbox_m2_attack_";

        internal static string AttackId(string itemId)
        {
            return itemId != null && ModernBoxCatalog.EquipmentIds.Contains(itemId) ? AttackPrefix + itemId : itemId;
        }

        private static void RegisterUnitAttacks()
        {
            HashSet<string> done = new HashSet<string>(StringComparer.Ordinal);
            foreach (ModernUnitSpec unit in ContentRegistry.Units)
            {
                EquipmentSpec spec = unit.Attack == null ? null : ContentRegistry.Equipment.Find(candidate => candidate.Id == unit.Attack);
                if (spec == null || !done.Add(spec.Id)) continue;
                string id = AttackId(spec.Id);
                EquipmentAsset attack = AssetManager.items.clone(id, spec.Id);
                attack.id = id;
                attack.translation_key = spec.Id;
                attack.show_in_meta_editor = false;
                attack.show_in_knowledge_window = false;
                attack.show_for_unlockables_ui = false;
                attack.can_be_given = false;
                attack.is_pool_weapon = false;
                attack.pool_rate = 0;
                AssetManager.items.pot_weapon_assets_all.Remove(attack);
                AssetManager.items.pot_weapon_assets_unlocked.Remove(attack);
                attack.base_stats = new BaseStats();
                foreach (KeyValuePair<string, float> pair in spec.BaseStats) SetItemStat(attack, pair.Key, pair.Value);
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

        private static string SlashAnimation(string id)
        {
            if (id == "shieldedaxe") return "effects/slashes/slash_axe";
            if (id == "shieldedhammer") return "effects/slashes/slash_hammer";
            if (id == "shieldedspear") return "effects/slashes/slash_spear";
            if (id == "shieldedsword" || id == "chainsaw" || id.EndsWith("lightsaber", StringComparison.Ordinal))
                return "effects/slashes/slash_sword";
            return "effects/slashes/slash_punch";
        }

        // Same vanilla name generators M2 assigned to each item.
        private static string NameTemplate(EquipmentSpec spec)
        {
            switch (spec.Id)
            {
                case "shieldedaxe": return "axe_name";
                case "shieldedhammer": return "hammer_name";
                case "shieldedspear": return "spear_name";
            }
            if (spec.Type == EquipmentType.Weapon) return ContentRegistry.IsMeleeEquipment(spec.Id) ? "sword_name" : "bow_name";
            if (spec.Type == EquipmentType.Armor) return "armor_name";
            if (spec.Type == EquipmentType.Helmet) return "helmet_name";
            if (spec.Type == EquipmentType.Boots) return "boots_name";
            return "ring_name";
        }

        // Port of M2's crafting choices: City_TryProduceItem_EpochPatch +
        // CustomArmorEquipmentIDPatch forced the culture's era armor piece, and
        // AddPreferredWeaponToCivRaces put M2 weapons in every civ race's pool.
        // getItemAssetToCraft walks the list from the end, so M2 weapons go last
        // in random order and are tried first.
        internal static void ApplyM2CraftingChoice(List<EquipmentAsset> items, City city)
        {
            if (items == null || items.Count == 0 || !ModernProgression.IsSupportedCity(city)) return;
            // City_TryProduceItem_EpochPatch picked the tier from Future/MilitaryModern/Firearms/Renaissance.
            M2Era era = M2Tech.MilitaryEra(city.culture);
            if (era < M2Era.Renaissance) return;
            EquipmentType type = items[0].equipment_type;
            if (type == EquipmentType.Armor || type == EquipmentType.Helmet || type == EquipmentType.Boots)
            {
                EquipmentSpec tierPiece = ContentRegistry.Equipment.Find(spec => spec.Type == type && spec.Era == era);
                if (tierPiece == null || !ProductionService.IsEquipmentEnabled(tierPiece.Id, city)) return;
                EquipmentAsset asset = PristineChoice(AssetManager.items.get(tierPiece.Id), city);
                if (asset == null) return;
                items.Clear();
                items.Add(asset);
                return;
            }
            if (type != EquipmentType.Weapon) return;
            items.RemoveAll(item => ModernBoxCatalog.EquipmentIds.Contains(item.id));
            List<EquipmentAsset> modern = new List<EquipmentAsset>();
            foreach (EquipmentSpec spec in ContentRegistry.Equipment)
            {
                if (spec.Type != EquipmentType.Weapon || !ContentRegistry.PreferredWeapons.Contains(spec.Id)) continue;
                if (!ProductionService.IsEquipmentEnabled(spec.Id, city)) continue;
                EquipmentAsset asset = AssetManager.items.get(spec.Id);
                if (asset != null) modern.Add(asset);
            }
            for (int i = modern.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                EquipmentAsset swap = modern[i];
                modern[i] = modern[j];
                modern[j] = swap;
            }
            items.AddRange(modern);
        }

        internal static void RegisterTraits()
        {
            ActorTraitGroupAsset vehicleGroup = new ActorTraitGroupAsset { id = "ModernBox", name = "ModernBox", color = "#6E7B8B", show_counter = true };
            ActorTraitGroupAsset ideologyGroup = new ActorTraitGroupAsset { id = "IdeologyBox", name = "Ideologies", color = "#D4AF37", show_counter = true };
            ModernLocalization.Add("ModernBox", "ModernBox");
            ModernLocalization.Add("Ideologies", "Ideologies");
            AssetManager.trait_groups.add(vehicleGroup);
            AssetManager.trait_groups.add(ideologyGroup);

            // Original M2 vehicle traits are stat-free markers (all stats 0).
            RegisterVehicleTrait("exhausted", "ui/Icons/Humvee", "exhausted");
            RegisterVehicleTrait("spawnedvehicle", "ui/Icons/Humvee", "spawnedvehicle");
            RegisterVehicleTrait("Jet", "ui/Icons/Plane", "Fly through the sky...");
            RegisterVehicleTrait("MIRVBoat", "ui/Icons/Boat", "He's a armored boy");
            RegisterVehicleTrait("Helicopter", "ui/Icons/Heli", "HELICOPTER HELICOPTER BRRRRRRRRRRRR");
            RegisterVehicleTrait("Tank", "ui/Icons/Tank", "Armoured Vehicle.");
            RegisterVehicleTrait("Railgun", "ui/Icons/Railgun", "Armoured Vehicle.");
            RegisterVehicleTrait("Humvee", "ui/Icons/Humvee", "Awesome military vehicle.");
            RegisterVehicleTrait("Zeppelin", "ui/Icons/Airship", "Big airship.");
            RegisterVehicleTrait("SupportRole", "ui/Icons/SolarPoweredCyberBody", "Heals and cures nearby allies");
            ActorTrait supportRole = AssetManager.traits.get("SupportRole");
            supportRole.action_special_effect = M2VehicleBehaviours.FriendlyAuraEffect;
            supportRole.special_effect_interval = 5f;
            // The original left SupportRole givable and removable, with the vanilla healing aura icon.
            supportRole.can_be_given = true;
            supportRole.can_be_removed = true;
            Sprite healingAura = SpriteTextureLoader.getSprite("ui/Icons/actor_traits/iconHealingAura");
            if (healingAura != null)
            {
                supportRole.path_icon = "ui/Icons/actor_traits/iconHealingAura";
                supportRole.cached_sprite = healingAura;
            }

            // These traits drove M2's non-space simulation systems. Their
            // callbacks are implemented by M2LegacyBehaviorService so the
            // original behavior remains bounded and save-safe on build 719.
            RegisterLegacyBehaviorTrait("Unitpotential", "ui/Icons/UnitpotentialIcon", "Allows expanded unit roster for civs. Cool stuff");
            ActorTrait unitPotential = AssetManager.traits.get("Unitpotential");
            if (unitPotential != null)
            {
                unitPotential.type = TraitType.Negative;
                unitPotential.can_be_given = false;
                unitPotential.can_be_removed = false;
                unitPotential.base_stats["offspring"] = -99999f;
                unitPotential.action_special_effect = M2VehicleBehaviours.NomadHandlerEffect;
            }
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
                    type = TraitType.Positive,
                    can_be_given = true,
                    can_be_removed = true,
                    can_be_cured = false,
                    rate_birth = 0,
                    rate_inherit = 0,
                    rate_acquire_grow_up = 0,
                    is_mutation_box_allowed = true
                };
                ApplyIdeologyStats(trait, id);
                foreach (string opposite in IdeologyIds) if (opposite != id) trait.addOpposite(opposite);
                AddTraitLocale(id, id, IdeologyDescription(id));
                trait.cached_sprite = LoadWorldSafeTraitIcon(trait.path_icon, id);
                AssetManager.traits.add(trait);
                trait.unlock(true);
            }
            M2Ideologies.Register();
        }

        private static string IdeologyDescription(string id)
        {
            switch (id)
            {
                case "Dynastic": return "LONG LIVE THE KING!!!!";
                case "Mercantile": return "The very best prices, the yugest golden toilets, and the bestest hairstyles!";
                case "Peoplewoven": return "WE HAVE COME TO THE ONLY PLACE NOT YET CORRUPTED BY MERCANTILES, SPACEBOX!";
                case "Martial": return "TUXXEDAN TECHNOLOGY IS THE BEST!!!";
                default: return "THE CHAINS ARE BROKEN, THE PEOPLE RISE!";
            }
        }

        private static void ApplyIdeologyStats(ActorTrait trait, string id)
        {
            if (id == "Dynastic")
            {
                trait.base_stats["lifespan"] = 30f; trait.base_stats["intelligence"] = -2f;
                trait.base_stats["diplomacy"] = 10f; trait.base_stats["stewardship"] = 10f;
                trait.base_stats["loyalty_traits"] = 15f; trait.base_stats["multiplier_offspring"] = -0.15f;
            }
            else if (id == "Mercantile")
            {
                trait.base_stats["intelligence"] = 5f; trait.base_stats["diplomacy"] = 10f;
                trait.base_stats["opinion"] = 10f; trait.base_stats["loyalty_traits"] = 15f; trait.base_stats["cities"] = -3f;
            }
            else if (id == "Peoplewoven")
            {
                trait.base_stats["intelligence"] = 5f; trait.base_stats["warfare"] = 10f;
                trait.base_stats["diplomacy"] = 5f; trait.base_stats["stewardship"] = 3f;
                trait.base_stats["opinion"] = 10f; trait.base_stats["loyalty_traits"] = -50f; trait.base_stats["cities"] = 3f;
            }
            else if (id == "Martial")
            {
                trait.base_stats["intelligence"] = 5f; trait.base_stats["warfare"] = 20f;
                trait.base_stats["diplomacy"] = -10f; trait.base_stats["stewardship"] = 5f;
                trait.base_stats["opinion"] = -20f; trait.base_stats["loyalty_traits"] = -50f; trait.base_stats["cities"] = 3f;
            }
            else
            {
                trait.base_stats["lifespan"] = -10f; trait.base_stats["attack_speed"] = M2AttackSpeed.Delta(15f, M2AttackSpeed.OldHumanBase);
                trait.base_stats["intelligence"] = -5f; trait.base_stats["warfare"] = 20f;
                trait.base_stats["diplomacy"] = -500f; trait.base_stats["stewardship"] = -400f;
                trait.base_stats["opinion"] = -800f; trait.base_stats["loyalty_traits"] = -10000f; trait.base_stats["cities"] = -100f;
            }
        }

        internal static bool EnsureDefaultIdeology(Actor actor)
        {
            // Original birth roll: each ideology is rolled once at birth with its
            // own chance (10%, Chaosvolt 8%). Opposites block a second ideology.
            if (!ModernBoxSettings.Get("IdeologiesOption") || actor == null || actor.asset == null ||
                !SapientSpeciesIds.Contains(actor.asset.id)) return false;
            bool added = false;
            foreach (string ideologyId in IdeologyIds)
                if (Randy.randomChance(M2Ideologies.BirthRate(ideologyId) / 100f) && actor.addTrait(ideologyId)) added = true;
            return added;
        }

        private static void RegisterVehicleTrait(string id, string icon, string description)
        {
            ActorTrait trait = new ActorTrait
            {
                id = id,
                base_stats = new BaseStats(),
                path_icon = icon,
                group_id = "ModernBox",
                type = TraitType.Negative,
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
            trait.base_stats["speed"] = speed * M2OriginalStats.SpeedScale;
            trait.base_stats["attack_speed"] = M2AttackSpeed.Delta(attackSpeed, M2AttackSpeed.OldZombieBase);
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
            AddNames("Modern_Names", M2Names.HumanFirst, M2Names.HumanLast);
            AddNames("Modern_Orc_Names", M2Names.OrcFirst, M2Names.OrcLast);
            AddNames("Modern_Elf_Names", M2Names.ElfFirst, M2Names.ElfLast);
            AddNames("Modern_Dwarf_Names", M2Names.DwarfFirst, M2Names.DwarfLast);
            // Aliases used by the era soldier actors.
            AddNames("Modern_human_Names", M2Names.HumanFirst, M2Names.HumanLast);
            AddNames("Modern_orc_Names", M2Names.OrcFirst, M2Names.OrcLast);
            AddNames("Modern_elf_Names", M2Names.ElfFirst, M2Names.ElfLast);
            AddNames("Modern_dwarf_Names", M2Names.DwarfFirst, M2Names.DwarfLast);
            AddCodeNames("Jet_Names", "F-,V-,X-,J-,S-");
            AddCodeNames("Humvee_Names", "H-");
            AddCodeNames("MIRV_Names", "F-,V-,X-,J-,S-");

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

