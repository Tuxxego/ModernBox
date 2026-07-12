using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoModLoader.General;
using UnityEngine;

namespace ModernBoxRewrite
{
    internal static class ModernBoxDiagnostics
    {
        private const int MaxLines = 160;
        private static readonly List<string> Entries = new List<string>();
        private static readonly HashSet<string> Once = new HashSet<string>(StringComparer.Ordinal);
        internal static IList<string> Lines { get { return Entries.AsReadOnly(); } }

        internal static string FindAssetConflict()
        {
            string[] probes = { "MIRVBomber", "ModernBarracks", "MOABbutton", "Parts" };
            foreach (string id in probes)
            {
                if ((AssetManager.actor_library != null && AssetManager.actor_library.has(id)) ||
                    (AssetManager.buildings != null && AssetManager.buildings.has(id)) ||
                    (AssetManager.powers != null && AssetManager.powers.has(id)) ||
                    (AssetManager.resources != null && AssetManager.resources.has(id)))
                    return "Another ModernBox edition already registered asset '" + id + "'. Disable it before loading the clean rewrite.";
            }
            return null;
        }

        internal static void Info(string message) { Add("INFO", message, false); }
        internal static void Warn(string message) { Add("WARN", message, true); }
        internal static void Error(string message) { Add("ERROR", message, true); }

        private static void Add(string level, string message, bool once)
        {
            string line = "[ModernBox Rewrite][" + level + "] " + message;
            if (once && !Once.Add(line)) return;
            Entries.Add(DateTime.Now.ToString("HH:mm:ss") + " " + line);
            if (Entries.Count > MaxLines) Entries.RemoveAt(0);
            if (level == "ERROR") Debug.LogError(line);
            else if (level == "WARN") Debug.LogWarning(line);
            else if (ModernBoxSettings.Get("DeveloperDiagnostics")) Debug.Log(line);
        }

        internal static void ValidateAssets()
        {
            int missing = 0;
            string root = Path.Combine(ModernBoxMod.ModFolder, "GameResources");
            foreach (string id in ModernBoxCatalog.UnitIds)
            {
                string folder = Path.Combine(root, "actors", id);
                if (!Directory.Exists(folder)) { Warn("Missing actor art folder: " + id); missing++; }
            }
            foreach (string id in ModernBoxCatalog.CivilianBuildingIds)
            {
                if (!Directory.Exists(Path.Combine(root, "buildings", id))) { Warn("Missing building art folder: " + id); missing++; }
            }
            foreach (string id in ModernBoxCatalog.FactoryBuildingIds)
            {
                if (!Directory.Exists(Path.Combine(root, "buildings", id))) { Warn("Missing factory art folder: " + id); missing++; }
            }
            if (!Directory.Exists(Path.Combine(root, "buildings", "MissileSilo"))) { Warn("Missing MissileSilo art folder"); missing++; }
            Info("Asset validation completed with " + missing + " missing core path(s).");
        }

        internal static void ValidateRegisteredContent()
        {
            List<string> errors = new List<string>();
            string language = LocalizedTextManager.instance.language;
            AddDuplicateErrors(errors, ContentRegistry.Units.Select(spec => spec.Id), "unit");
            AddDuplicateErrors(errors, ContentRegistry.Buildings.Select(spec => spec.Id), "building");
            AddDuplicateErrors(errors, ContentRegistry.Equipment.Select(spec => spec.Id), "equipment");
            AddDuplicateErrors(errors, ContentRegistry.Bombs.Select(spec => spec.Id), "bomb");

            foreach (ModernUnitSpec spec in ContentRegistry.Units)
            {
                ActorAsset actor = AssetManager.actor_library.get(spec.Id);
                if (actor == null) errors.Add("actor:" + spec.Id);
                else
                {
                    if (string.IsNullOrEmpty(actor.getDescriptionID())) errors.Add("actor-description-id:" + spec.Id);
                    else actor.getLocalizedDescription();
                    if (actor.kingdom_id_wild != "ModernKingdom") errors.Add("actor-fallback-kingdom:" + spec.Id);
                    if (actor.action_on_load == null) errors.Add("actor-load-initializer:" + spec.Id);
                    if (!actor.update_z || !actor.death_animation_angle || actor.special_dead_animation)
                        errors.Add("actor-standard-death-animation:" + spec.Id);
                }
                if (AssetManager.items.get(spec.Attack) == null) errors.Add("attack:" + spec.Id + "->" + spec.Attack);
                if (spec.Id == "MissileSystem")
                {
                    EquipmentAsset launcher = AssetManager.items.get(EquipmentAndTraitsRegistry.MissileSystemAttackId);
                    if (launcher == null || launcher.projectile != "modernbox_mirv") errors.Add("missile-system-mirv-launcher");
                }
                if (!AssetManager.powers.has("modernbox_spawn_" + spec.Id)) errors.Add("spawn-power:" + spec.Id);
                NameSetAsset nameSet = AssetManager.name_sets.get(spec.NameTemplate);
                if (nameSet == null || string.IsNullOrEmpty(nameSet.get(MetaType.Unit))) errors.Add("name-set:" + spec.Id + "->" + spec.NameTemplate);
                if (AssetManager.name_generator.get(spec.NameTemplate) == null) errors.Add("name-generator:" + spec.Id + "->" + spec.NameTemplate);
                if (!LM.Has(ModernLocalization.KeyForGame(spec.Id), language)) errors.Add("locale-actor:" + spec.Id);
                string spawnDescription = ModernLocalization.KeyForGame("modernbox_spawn_" + spec.Id + "_description");
                if (!LM.Has(spawnDescription, language)) errors.Add("locale-actor-description:" + spec.Id);
            }
            ActorAsset human = AssetManager.actor_library.get(ModernBoxCatalog.Human);
            CityBuildOrderAsset humanOrders = human == null ? null : AssetManager.city_build_orders.get(human.build_order_template_id);
            foreach (BuildingSpec spec in ContentRegistry.Buildings)
            {
                BuildingAsset registeredBuilding = AssetManager.buildings.get(spec.Id);
                if (registeredBuilding == null) errors.Add("building:" + spec.Id);
                else
                {
                    ValidateBuildingSprites(errors, registeredBuilding);
                    if (registeredBuilding.atlas_asset == null) errors.Add("building-atlas:" + spec.Id);
                    if (registeredBuilding.type != "modernbox_type_" + spec.Id) errors.Add("building-type:" + spec.Id + "->" + registeredBuilding.type);
                    if (!spec.Tower && (registeredBuilding.build_place_single || registeredBuilding.build_place_batch || !registeredBuilding.build_prefer_replace_house))
                        errors.Add("building-placement:" + spec.Id);
                }
                if (!LM.Has(ModernLocalization.KeyForGame(spec.Id), language)) errors.Add("locale-building:" + spec.Id);
                BuildOrder order = humanOrders == null ? null : humanOrders.list.FirstOrDefault(candidate => candidate.id == "order_" + spec.Id);
                if (human == null || human.architecture_asset == null || human.architecture_asset.getBuilding("order_" + spec.Id) == null)
                    errors.Add("human-order:" + spec.Id);
                if (order == null) errors.Add("build-order:" + spec.Id);
                else
                {
                    if (order.limit_type != spec.Limit) errors.Add("build-limit:" + spec.Id + "=" + order.limit_type + " expected " + spec.Limit);
                    int requiredPopulation, requiredBuildings;
                    ModernProgression.GetRequirements(spec.Tier, out requiredPopulation, out requiredBuildings);
                    if (order.required_pop != requiredPopulation || order.required_buildings != requiredBuildings)
                        errors.Add("build-threshold:" + spec.Id + "=" + order.required_pop + "/" + order.required_buildings + " expected " + requiredPopulation + "/" + requiredBuildings);
                }
            }
            foreach (string race in new[] { "orc", "elf", "dwarf" })
            {
                ActorAsset raceAsset = AssetManager.actor_library.get(race);
                if (raceAsset == null || raceAsset.architecture_asset == null) continue;
                foreach (BuildingSpec spec in ContentRegistry.Buildings)
                    if (raceAsset.architecture_asset.building_ids_for_construction.ContainsKey("order_" + spec.Id))
                        errors.Add("nonhuman-order:" + race + "->" + spec.Id);
            }
            BuildOrder houseUpgrade = humanOrders == null ? null : humanOrders.list.FirstOrDefault(order => order.id == "order_modernbox_house_upgrade");
            BuildingAsset houseSource = string.IsNullOrEmpty(ActorsAndBuildingsRegistry.HumanHouseUpgradeSourceId)
                ? null
                : AssetManager.buildings.get(ActorsAndBuildingsRegistry.HumanHouseUpgradeSourceId);
            if (houseUpgrade == null || !houseUpgrade.upgrade || houseUpgrade.required_pop != 0 || houseUpgrade.required_buildings != 0)
                errors.Add("house-modern-upgrade-order");
            if (houseSource == null || !houseSource.can_be_upgraded || houseSource.upgrade_to != "modernbuilding")
                errors.Add("house-modern-upgrade-source:" + (ActorsAndBuildingsRegistry.HumanHouseUpgradeSourceId ?? "<null>"));
            if (ActorsAndBuildingsRegistry.HumanHouseBranchUpgradeSourceIds.Count == 0)
                errors.Add("house-modern-early-branches");
            foreach (string sourceId in ActorsAndBuildingsRegistry.HumanHouseBranchUpgradeSourceIds)
            {
                BuildingAsset earlySource = AssetManager.buildings.get(sourceId);
                if (earlySource == null || string.IsNullOrEmpty(earlySource.upgrade_to) || earlySource.upgrade_to == "modernbuilding")
                    errors.Add("house-modern-normal-chain:" + sourceId);
            }
            foreach (EquipmentSpec spec in ContentRegistry.Equipment)
            {
                EquipmentAsset item = AssetManager.items.get(spec.Id);
                if (item == null)
                {
                    errors.Add("equipment:" + spec.Id);
                    continue;
                }
                List<EquipmentAsset> craftingPool;
                if (string.IsNullOrEmpty(item.equipment_subtype) ||
                    !AssetManager.items.equipment_by_subtypes.TryGetValue(item.equipment_subtype, out craftingPool) ||
                    craftingPool == null || !craftingPool.Contains(item))
                    errors.Add("equipment-crafting-pool:" + spec.Id);
                Sprite[] gameplaySprites = item.getSprites();
                if (gameplaySprites == null || gameplaySprites.Length == 0 || gameplaySprites[0] == null)
                    errors.Add("equipment-gameplay-sprite:" + spec.Id);
                else
                {
                    foreach (Sprite sprite in gameplaySprites)
                    {
                        if (sprite == null) continue;
                        long spriteId = DynamicSprites.getItemSpriteID(sprite, (ColorAsset)null);
                        if (!DynamicSpritesLibrary.items.hasSprite(spriteId))
                            errors.Add("equipment-atlas-sprite:" + spec.Id + "->" + sprite.name);
                    }
                }
                if (item.getGroup() == null) errors.Add("equipment-group:" + spec.Id + "->" + item.group_id);
                if (item.name_templates == null || item.name_templates.Count == 0)
                    errors.Add("equipment-name-templates:" + spec.Id);
                else
                {
                    foreach (string nameTemplate in item.name_templates)
                        if (string.IsNullOrEmpty(nameTemplate) || !AssetManager.name_generator.has(nameTemplate))
                            errors.Add("equipment-name-template:" + spec.Id + "->" + (nameTemplate ?? "<null>"));
                }
                if (!LM.Has(ModernLocalization.KeyForGame("item_" + spec.Id), language)) errors.Add("locale-equipment:" + spec.Id);
                if (!LM.Has(ModernLocalization.KeyForGame(spec.Id), language) || !LM.Has(ModernLocalization.KeyForGame(spec.Id + "_description"), language))
                    errors.Add("locale-equipment-runtime:" + spec.Id);
                if (!string.IsNullOrEmpty(spec.Projectile) && !AssetManager.projectiles.has(spec.Projectile))
                    errors.Add("projectile:" + spec.Id + "->" + spec.Projectile);
                if (!string.IsNullOrEmpty(spec.Projectile))
                {
                    ProjectileAsset projectile = AssetManager.projectiles.get(spec.Projectile);
                    if (projectile != null && projectile.terraform_option == null)
                        errors.Add("projectile-null-terraform:" + spec.Projectile);
                    if (projectile != null && projectile.sound_impact == null)
                        errors.Add("projectile-null-impact-sound:" + spec.Projectile);
                }
                if (ModernBoxCatalog.MirvIds.Contains(spec.Id))
                {
                    string expectedProjectile = ModernBoxSettings.Get("MIRVOption") ? spec.Projectile : "modernbox_bullet";
                    if (item.projectile != expectedProjectile) errors.Add("mirv-toggle-projectile:" + spec.Id + "->" + item.projectile);
                }
            }
            foreach (string projectileId in new[]
            {
                "modernbox_bullet", "modernbox_gunship_bullet", "modernbox_blast", "NUKER",
                "modernbox_mirv_budget", "modernbox_mirv_decent", "modernbox_mirv",
                "modernbox_mirv_bomb", "modernbox_mirv_strong"
            })
            {
                ProjectileAsset projectile = AssetManager.projectiles.get(projectileId);
                if (projectile == null) errors.Add("projectile-asset:" + projectileId);
                else if (projectile.frames == null || projectile.frames.Length == 0 || projectile.frames[0] == null)
                    errors.Add("projectile-frames:" + projectileId);
            }
            foreach (BombSpec spec in ContentRegistry.Bombs)
            {
                string powerId = spec.Id + "button";
                string dropId = "modernbox_drop_" + spec.Id;
                GodPower power = AssetManager.powers.get(powerId);
                DropAsset drop = AssetManager.drops.get(dropId);
                if (power == null) errors.Add("bomb-power:" + powerId);
                if (drop == null) errors.Add("bomb-drop:" + dropId);
                if (power != null && power.cached_drop_asset != drop) errors.Add("bomb-cache:" + powerId);
                if (!LM.Has(ModernLocalization.KeyForGame(powerId), language) || !LM.Has(ModernLocalization.KeyForGame(powerId + "_description"), language))
                    errors.Add("locale-bomb:" + powerId);
            }
            BuildingAsset missileSilo = AssetManager.buildings.get("MissileSilo");
            if (missileSilo == null || missileSilo.tower != ModernBoxSettings.Get("NukeOption") || missileSilo.tower_projectile != "NUKER")
                errors.Add("missile-silo-launch-configuration");
            if (AssetManager.world_log_library.get(SiloLaunchEvents.AssetId) == null)
                errors.Add("world-log:" + SiloLaunchEvents.AssetId);
            if (!LM.Has(ModernLocalization.KeyForGame(SiloLaunchEvents.AssetId), language))
                errors.Add("locale-world-log:" + SiloLaunchEvents.AssetId);
            foreach (string id in new[] { "Parts", "CyberWareParts", "Xenium" })
            {
                ResourceAsset resource = AssetManager.resources.get(id);
                if (resource == null)
                {
                    errors.Add("resource:" + id);
                    continue;
                }
                Sprite[] sprites = resource.getSprites();
                if (sprites == null || sprites.Length == 0 || sprites[0] == null)
                    errors.Add("resource-gameplay-sprite:" + id);
                else if (resource.getGameplaySprite() == null)
                    errors.Add("resource-gameplay-accessor:" + id);
                if (resource.getSpriteIcon() == null) errors.Add("resource-icon:" + id);
                if (string.IsNullOrEmpty(resource.tooltip) || AssetManager.tooltips.get(resource.tooltip) == null)
                    errors.Add("resource-tooltip:" + id + "->" + (resource.tooltip ?? "<null>"));
                if (!string.IsNullOrEmpty(resource.path_gameplay_sprite) && resource.path_gameplay_sprite.StartsWith("ui/Icons/", StringComparison.OrdinalIgnoreCase))
                    errors.Add("resource-oversized-gameplay-sprite:" + id);
            }
            if (!LM.Has(ModernLocalization.KeyForGame("Ideologies"), language)) errors.Add("locale-trait-group:Ideologies");
            foreach (string ideologyId in new[]
            {
                "Capitalist", "Communist", "Liberal", "Conservative", "Fascist", "Democratic",
                "Technocrat", "Luddite", "Environmental Steward", "Anarchist", "Primalism"
            })
            {
                ActorTrait ideology = AssetManager.traits.get(ideologyId);
                Sprite icon = ideology == null ? null : ideology.getSprite();
                if (icon == null || icon.bounds.size.x > 0.34f || icon.bounds.size.y > 0.34f)
                    errors.Add("ideology-world-icon-scale:" + ideologyId);
            }
            if (AssetManager.kingdoms.get("ModernKingdom") == null) errors.Add("kingdom:ModernKingdom");
            if (World.world != null && World.world.kingdoms_wild != null && World.world.kingdoms_wild.get("ModernKingdom") == null)
                errors.Add("wild-kingdom-instance:ModernKingdom");

            if (errors.Count > 0)
                throw new InvalidOperationException("Registered content validation failed: " + string.Join(", ", errors.ToArray()));
            Debug.Log("[ModernBox Rewrite] Registered-content validation passed.");
        }

        private static void AddDuplicateErrors(List<string> errors, IEnumerable<string> ids, string kind)
        {
            foreach (IGrouping<string, string> group in ids.GroupBy(id => id, StringComparer.Ordinal))
                if (group.Count() > 1) errors.Add("duplicate-" + kind + ":" + group.Key);
        }

        private static void ValidateBuildingSprites(List<string> errors, BuildingAsset building)
        {
            if (building.building_sprites == null)
            {
                errors.Add("building-sprites:" + building.id);
                return;
            }
            if (building.has_sprite_construction && building.building_sprites.construction == null)
                errors.Add("building-construction:" + building.id);
            for (int i = 0; i < building.building_sprites.animation_data.Count; i++)
            {
                BuildingAnimationData data = building.building_sprites.animation_data[i];
                if (data == null || data.main == null || data.main.Length == 0)
                    errors.Add("building-main:" + building.id + "[" + i + "]");
                if (building.has_ruins_graphics && (data == null || data.ruins == null || data.ruins.Length == 0))
                    errors.Add("building-ruin:" + building.id + "[" + i + "]");
            }
        }
    }
}
