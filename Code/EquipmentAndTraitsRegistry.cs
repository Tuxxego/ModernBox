using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ModernBoxM1Rewrite
{
    internal static class EquipmentAndTraitsRegistry
    {
        // Ideologies.cs, in the order they were added. The old birth roll went through the
        // trait list in this order, so Capitalist was the most common.
        internal static readonly string[] IdeologyIds =
        {
            "Capitalist", "Communist", "Liberal", "Conservative", "Fascist", "Democratic",
            "Technocrat", "Luddite", "Environmental Steward", "Anarchist", "Primalism"
        };

        internal static readonly HashSet<string> SapientSpeciesIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "human", "orc", "elf", "dwarf"
        };

        // Old stats that 0.51.2 no longer has; items and traits skip them.
        private static readonly HashSet<string> DroppedStats = new HashSet<string>(StringComparer.Ordinal)
        {
            "fertility", "max_children", "zone_range"
        };

        // Resourcez.cs: three strategic resources that nothing in M1 ever produced or used.
        internal static void RegisterResourcesAndProjectiles()
        {
            RegisterResource("Parts", "Factories", 999);
            RegisterResource("CyberWareParts", "Cyberware", 999);
            RegisterResource("Xenium", "Xeno", 9999);
            M1Projectiles.Register();
        }

        private static void RegisterResource(string id, string icon, int maximum)
        {
            if (AssetManager.resources.get(id) != null) return;
            // The original icons (resources/iconResParts etc.) were never shipped, and a UI icon
            // is far too big for the ground, so the ground sprite is borrowed from common metals.
            ResourceAsset metals = AssetManager.resources.get("common_metals");
            if (metals == null || metals.gameplay_sprites == null || metals.gameplay_sprites.Length == 0)
                throw new InvalidOperationException("Missing gameplay sprite template for resource " + id + ".");
            AssetManager.resources.add(new ResourceAsset
            {
                id = id,
                path_icon = icon,
                path_gameplay_sprite = metals.path_gameplay_sprite,
                full_sprite_path = metals.full_sprite_path,
                gameplay_sprites = metals.gameplay_sprites,
                type = ResType.Strategic,
                maximum = maximum,
                storage_max = maximum,
                stack_size = 100,
                trade_bound = 200,
                trade_give = 50,
                trade_cost = 2,
                supply_bound_give = 200,
                supply_bound_take = 100,
                // ButtonResource reads this as a TooltipAsset id.
                tooltip = "city_resource"
            });
            ModernLocalization.Add(id, id);
        }

        internal static void RegisterEquipment()
        {
            foreach (EquipmentSpec spec in ContentRegistry.Equipment)
            {
                string template = spec.Type == EquipmentType.Weapon ? "$range" : "$ring";
                EquipmentAsset item = AssetManager.items.clone(spec.Id, template);
                item.id = spec.Id;
                item.translation_key = spec.Id;
                // ItemAsset.getRandomNameTemplate assumes this list is non-null.
                item.name_templates = NameTemplates(spec.NameTemplate);
                item.equipment_type = spec.Type;
                item.equipment_subtype = spec.Type == EquipmentType.Weapon ? "stick" : "ring";
                item.group_id = spec.Type == EquipmentType.Weapon ? "firearm" : "ring";
                item.show_in_meta_editor = true;
                item.show_in_knowledge_window = true;
                item.show_for_unlockables_ui = true;
                item.can_be_given = true;
                item.material = "basic";
                item.metallic = true;
                item.colored = false;
                item.projectile = spec.Projectile;
                item.path_slash_animation = "effects/slashes/slash_punch";
                item.path_icon = ItemIcon(spec);
                string[] sprites = ItemSpritePaths(spec.Id);
                item.path_gameplay_sprite = sprites.Length > 0 ? sprites[0] : item.path_icon;
                item.gameplay_sprites = LoadSprites(sprites);
                foreach (Sprite sprite in item.gameplay_sprites)
                    if (sprite != null) DynamicSprites.preloadItemSprite(sprite, null);
                item.base_stats = new BaseStats();
                foreach (KeyValuePair<string, float> pair in spec.BaseStats)
                {
                    float material;
                    spec.MaterialStats.TryGetValue(pair.Key, out material);
                    SetItemStat(item, spec, pair.Key, pair.Value + material);
                }
                foreach (KeyValuePair<string, float> pair in spec.MaterialStats)
                    if (!spec.BaseStats.ContainsKey(pair.Key)) SetItemStat(item, spec, pair.Key, pair.Value);
                item.equipment_value = spec.Value;
                item.rigidity_rating = spec.Type == EquipmentType.Weapon ? 4 : 2;
                item.quality = spec.Legendary ? Rarity.R3_Legendary : Rarity.R0_Normal;
                item.setCost(0, spec.Resource1, spec.Resource1Cost, spec.Resource2, spec.Resource2Cost);
                item.minimum_city_storage_resource_1 = spec.MinimumStorage;
                // Keep the M1 sprites loaded above. The vanilla pool preloader would replace
                // them by looking for one combined texture.
                item.is_pool_weapon = false;
                item.pool_rate = 0;
                if (spec.Type == EquipmentType.Weapon)
                {
                    if (!AssetManager.items.pot_weapon_assets_all.Contains(item)) AssetManager.items.pot_weapon_assets_all.Add(item);
                    if (!AssetManager.items.pot_weapon_assets_unlocked.Contains(item)) AssetManager.items.pot_weapon_assets_unlocked.Add(item);
                }
                AddToCraftingSubtype(item);
                ModernLocalization.Add(spec.Id, spec.DisplayName);
                ModernLocalization.Add(spec.Id + "_description", string.Empty);
                ModernLocalization.Add("item_" + spec.Id, spec.DisplayName);
                ModernLocalization.Add("item_" + spec.Id + "_description", string.Empty);
            }
        }

        // The old game merged a unit's defaultAttack item without any material, so units
        // get a copy of their attack item without the iron bonus.
        internal static string AttackId(string itemId)
        {
            return ModernBoxCatalog.EquipmentIds.Contains(itemId) ? "modernbox_m1_attack_" + itemId : itemId;
        }

        internal static void RegisterUnitAttacks()
        {
            HashSet<string> done = new HashSet<string>(StringComparer.Ordinal);
            foreach (ModernUnitSpec unit in ContentRegistry.Units)
            {
                EquipmentSpec spec = ContentRegistry.FindEquipment(unit.Attack);
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
                attack.base_stats = new BaseStats();
                foreach (KeyValuePair<string, float> pair in spec.BaseStats) SetItemStat(attack, spec, pair.Key, pair.Value);
            }
        }

        // guns.cs: sword_name#30 (or bow_name#30), sword_name_king#3 and the five weapon_name_*
        // templates. The #N weights are written out as repeats, like 0.51.2 does it.
        private static List<string> NameTemplates(string main)
        {
            List<string> templates = new List<string>();
            if (main != "sword_name" && main != "bow_name")
            {
                templates.Add(main);
                return templates;
            }
            for (int i = 0; i < 30; i++) templates.Add(main);
            for (int i = 0; i < 3; i++) templates.Add("sword_name_king");
            templates.Add("weapon_name_city");
            templates.Add("weapon_name_kingdom");
            templates.Add("weapon_name_culture");
            templates.Add("weapon_name_enemy_king");
            templates.Add("weapon_name_enemy_kingdom");
            return templates;
        }

        private static void SetItemStat(EquipmentAsset item, EquipmentSpec spec, string key, float value)
        {
            switch (key)
            {
                // The old engine never read accuracy.
                case "accuracy": return;
                case "knockback_reduction": M1OriginalStats.ItemKnockbackReduction[spec.Id] = value; return;
                case "max_age": key = "lifespan"; break;
                case "attack_speed": value = M1AttackSpeed.Delta(value, M1AttackSpeed.OldHumanBase); break;
                case "speed": value *= M1OriginalStats.SpeedScale; break;
                case "critical_damage_multiplier": value = M1AttackSpeed.CritMultiplier(value); break;
            }
            if (DroppedStats.Contains(key) || AssetManager.base_stats_library.get(key) == null) return;
            item.base_stats[key] = value;
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

        // The original drew each gun with its iron sprite (addgunSprite(id, "iron")).
        private static string[] ItemSpritePaths(string id)
        {
            string folder = Path.Combine(ModernBoxMod.ModFolder, "GameResources", "ItemTextures");
            List<string> paths = new List<string>();
            foreach (string material in new[] { "iron", "base" })
                if (File.Exists(Path.Combine(folder, "w_" + id + "_" + material + ".png"))) paths.Add("ItemTextures/w_" + id + "_" + material);
            return paths.ToArray();
        }

        private static Sprite[] LoadSprites(string[] paths)
        {
            List<Sprite> sprites = new List<Sprite>();
            foreach (string path in paths)
            {
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite != null) sprites.Add(sprite);
            }
            return sprites.ToArray();
        }

        // The old game showed ui/Icons/items/icon_<id>_<material> for an item.
        private static string ItemIcon(EquipmentSpec spec)
        {
            string path = "ui/Icons/items/icon_" + spec.Id + "_" + spec.Material;
            if (File.Exists(Path.Combine(ModernBoxMod.ModFolder, "GameResources", path + ".png"))) return path;
            if (spec.Id.IndexOf("MIRV", StringComparison.Ordinal) >= 0) return "ui/Icons/MIRV";
            if (spec.Type == EquipmentType.Ring) return spec.Tech == "Cyberware" && (spec.Id == "Meth" || spec.Id == "Crack") ? "ui/Icons/Drugs" : "ui/Icons/Cyberware";
            return spec.Tech == "LowFirearms" ? "ui/Icons/lowfirearm" : "ui/Icons/firearm";
        }

        internal static void RegisterTraits()
        {
            // TraitsGroups.cs
            AddTraitGroup("ModernBox", "ModernBox");
            AddTraitGroup("IdeologyBox", "ModernBox - Ideologies");

            // Traits.cs: markers with no stats, except Vehicle's fertility -100 (a stat
            // 0.51.2 no longer has; vehicles cannot have children anyway).
            AddVehicleTrait("Jet", "ui/Icons/Plane", "Fly through the sky...");
            AddVehicleTrait("Vehicle", "ui/Icons/tabIconModernWarfare", "A vehicle from ModernBox.");
            AddVehicleTrait("MIRVBoat", "ui/Icons/Boat", "He's a armored boy");
            AddVehicleTrait("Helicopter", "ui/Icons/Heli", "HELICOPTER HELICOPTER BRRRRRRRRRRRR");
            AddVehicleTrait("Tank", "ui/Icons/Tank", "Armoured Vehicle.");
            AddVehicleTrait("Railgun", "ui/Icons/Railgun", "Armoured Vehicle.");
            AddVehicleTrait("Humvee", "ui/Icons/Humvee", "Awesome military vehicle.");
            AddVehicleTrait("Zeppelin", "ui/Icons/Airship", "Big airship.");

            // Ideologies.cs. Each one rules out all the others.
            AddIdeology("Capitalist", "Capitalist", "He likes capitalism.", 0f, 0f, 5f, 0f, 0f, 0f, 3f);
            AddIdeology("Communist", "Communist", "He likes communism.", 0f, 7f, 1f, 0f, 0f, 0f, 8f);
            AddIdeology("Liberal", "Liberal", "Private properties and a market economy are the dream for this person.", 5f, 0f, 1f, 4f, 2f, 0f, 2f);
            AddIdeology("Conservative", "Conservative", "Traditional values are the dream for this person.", 5f, 3f, 1f, 4f, 0f, 0f, 2f);
            AddIdeology("Fascist", "Facist", "An eternal leader is the dream for this person.", 5f, 15f, -7f, 4f, -2f, 0f, 12f);
            AddIdeology("Democratic", "Democratic", "A government run by the people is the dream for this person.", 5f, 20f, 15f, 15f, 20f, 20f, 5f);
            AddIdeology("Technocrat", "Technocrat", "Believes in governance by experts and technological advancement.", 10f, 2f, 5f, 7f, 5f, 0f, 4f);
            AddIdeology("Luddite", "Luddite", "Rejects technology and advocates for a simpler, non-technological lifestyle.", -5f, -3f, -5f, -7f, -5f, 0f, 1f);
            AddIdeology("Environmental Steward", "EnvironmentalSteward", "Prioritizes the protection of the environment and sustainable practices.", 3f, -5f, 5f, 10f, 5f, 0f, 2f);
            AddIdeology("Anarchist", "Anarchist", "Hates goverments.", -3f, 5f, -100f, 10f, -100f, 0f, -1000f);
            AddIdeology("Primalism", "Primalism", "Why tf would anyone want this?", -99999999999999999f, 99999999f, -100f, 10f, -100f, 0f, -99999999f);
            foreach (string id in IdeologyIds)
            {
                ActorTrait trait = AssetManager.traits.get(id);
                foreach (string other in IdeologyIds) if (other != id) trait.addOpposite(other);
            }
        }

        private static void AddTraitGroup(string id, string name)
        {
            if (AssetManager.trait_groups.get(id) != null) return;
            AssetManager.trait_groups.add(new ActorTraitGroupAsset { id = id, name = id, color = "#FFFF00" });
            ModernLocalization.Add(id, name);
            ModernLocalization.Add("trait_group_" + id, name);
        }

        private static void AddVehicleTrait(string id, string icon, string description)
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
                needs_to_be_explored = false,
                rate_birth = 0,
                rate_inherit = 0,
                rate_acquire_grow_up = 0
            };
            AddTraitLocale(id, description);
            trait.cached_sprite = LoadWorldSafeTraitIcon(icon, id);
            AssetManager.traits.add(trait);
            trait.unlock(true);
        }

        private static void AddIdeology(string id, string iconName, string description, float intelligence, float warfare,
            float diplomacy, float stewardship, float opinion, float loyalty, float cities)
        {
            string icon = "ui/Icons/" + iconName;
            ActorTrait trait = new ActorTrait
            {
                id = id,
                base_stats = new BaseStats(),
                path_icon = icon,
                group_id = "IdeologyBox",
                type = TraitType.Positive,
                can_be_given = true,
                can_be_removed = true,
                can_be_cured = false,
                // Births and inheritance are rolled the old way by M1Ideologies.
                rate_birth = 0,
                rate_inherit = 0,
                rate_acquire_grow_up = 0,
                is_mutation_box_allowed = false
            };
            SetIfAny(trait, "intelligence", intelligence);
            SetIfAny(trait, "warfare", warfare);
            SetIfAny(trait, "diplomacy", diplomacy);
            SetIfAny(trait, "stewardship", stewardship);
            SetIfAny(trait, "opinion", opinion);
            SetIfAny(trait, "loyalty_traits", loyalty);
            SetIfAny(trait, "cities", cities);
            AddTraitLocale(id, description);
            trait.cached_sprite = LoadWorldSafeTraitIcon(icon, id);
            AssetManager.traits.add(trait);
            trait.unlock(true);
        }

        private static void SetIfAny(ActorTrait trait, string key, float value)
        {
            if (value != 0f) trait.base_stats[key] = value;
        }

        // The old game showed the trait id as its name.
        private static void AddTraitLocale(string id, string description)
        {
            string normalized = id.ToLowerInvariant().Replace(' ', '_');
            foreach (string key in new[] { id, normalized })
            {
                ModernLocalization.Add("trait_" + key, id);
                ModernLocalization.Add("trait_" + key + "_description", description);
                ModernLocalization.Add("trait_" + key + "_info", description);
            }
        }

        private static Sprite LoadWorldSafeTraitIcon(string path, string traitId)
        {
            Sprite source = Resources.Load<Sprite>(path);
            if (source == null) throw new InvalidOperationException("Missing trait icon for " + traitId + " at " + path + ".");
            // Conversation bubbles draw trait icons in the world. M1's icons are 200 to 1920 px
            // at 100 pixels per unit, which would make them several tiles wide there, so the
            // world copy is shrunk to a vanilla-like 0.32 units. The UI still uses the full icon.
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

        // Name.cs
        internal static void RegisterNames()
        {
            AddNames("Modern_Names", M1Names.HumanFirst, M1Names.HumanLast);
            AddNames("Modern_Orc_Names", M1Names.OrcFirst, M1Names.OrcLast);
            AddNames("Modern_Elf_Names", M1Names.ElfFirst, M1Names.ElfLast);
            AddNames("Modern_Dwarf_Names", M1Names.DwarfFirst, M1Names.DwarfLast);
            AddCodeNames("Jet_Names", "F-,V-,X-,J-,S-");
            AddCodeNames("Humvee_Names", "H-");
            AddCodeNames("MIRV_Names", "F-,V-,X-,J-,S-");
        }

        private static void AddNames(string id, string first, string last)
        {
            if (AssetManager.name_generator.has(id)) return;
            NameGeneratorAsset names = new NameGeneratorAsset { id = id };
            names.addPartGroup(first);
            names.addPartGroup(" ");
            names.addPartGroup(last);
            names.addTemplate("part_group");
            AddGenerator(names);
        }

        private static void AddCodeNames(string id, string prefix)
        {
            if (AssetManager.name_generator.has(id)) return;
            NameGeneratorAsset names = new NameGeneratorAsset { id = id };
            names.addPartGroup(prefix);
            names.addPartGroup("10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40");
            names.addPartGroup("A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z");
            names.addTemplate("part_group");
            AddGenerator(names);
        }

        private static void AddGenerator(NameGeneratorAsset names)
        {
            // Build 719's NameGenerator.getName always scans this array for female names.
            if (names.vowels == null || names.vowels.Length == 0)
                names.vowels = new[] { "a", "e", "i", "o", "u", "y", "A", "E", "I", "O", "U", "Y" };
            AssetManager.name_generator.add(names);
            // Some 0.51.2 code reads a name set of the same id instead of the generator.
            NameSetAsset set = AssetManager.name_sets.get(names.id);
            if (set == null)
            {
                set = new NameSetAsset { id = names.id };
                AssetManager.name_sets.add(set);
            }
            set.unit = names.id;
            set.kingdom = names.id;
            set.city = names.id;
            set.clan = names.id;
            set.culture = names.id;
            set.family = names.id;
            set.language = names.id;
            set.religion = names.id;
        }
    }
}
