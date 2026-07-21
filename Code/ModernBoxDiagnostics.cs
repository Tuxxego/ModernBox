using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    internal static class ModernBoxDiagnostics
    {
        private const int MaxLines = 160;
        private static readonly List<string> Entries = new List<string>();
        private static readonly HashSet<string> Once = new HashSet<string>(StringComparer.Ordinal);
        internal static IList<string> Lines { get { return Entries.AsReadOnly(); } }

        internal static string FindAssetConflict()
        {
            string[] probes = { "MIRVBomber", "TerranFactory", "AtomicGrenadebutton", "Parts" };
            foreach (string id in probes)
            {
                if ((AssetManager.actor_library != null && AssetManager.actor_library.has(id)) ||
                    (AssetManager.buildings != null && AssetManager.buildings.has(id)) ||
                    (AssetManager.powers != null && AssetManager.powers.has(id)) ||
                    (AssetManager.resources != null && AssetManager.resources.has(id)))
                    return "Another ModernBox edition already registered asset '" + id + "'. Disable it before loading ModernBox 2 Rewrite.";
            }
            return null;
        }

        internal static void Info(string message) { Add("INFO", message, false); }
        internal static void Warn(string message) { Add("WARN", message, true); }
        internal static void Error(string message) { Add("ERROR", message, true); }

        private static void Add(string level, string message, bool once)
        {
            string line = "[ModernBox M2 Rewrite][" + level + "] " + message;
            if (once && !Once.Add(line)) return;
            Entries.Add(DateTime.Now.ToString("HH:mm:ss") + " " + line);
            if (Entries.Count > MaxLines) Entries.RemoveAt(0);
            if (level == "ERROR") Debug.LogError(line);
            else if (level == "WARN") Debug.LogWarning(line);
            else if (ModernBoxSettings.Get("DeveloperDiagnostics")) Debug.Log(line);
        }

        internal static void ValidateAssets()
        {
            string root = Path.Combine(ModernBoxMod.ModFolder, "GameResources");
            int missing = 0;
            foreach (ModernUnitSpec spec in ContentRegistry.Units)
            {
                string folder = string.IsNullOrEmpty(spec.TextureFolder) ? spec.Id : spec.TextureFolder;
                if (folder == "t_walker")
                {
                    // The legacy t_walker path was removed. Registration now
                    // inherits the current vanilla walker asset, so validate the
                    // resolved texture instead of probing the obsolete path.
                    ActorAsset actor = AssetManager.actor_library.get(spec.Id);
                    string[] walk = actor == null ? null : actor.animation_walk;
                    string path = actor == null || actor.texture_asset == null ? null : actor.texture_asset.texture_path_main;
                    Sprite[] sprites = string.IsNullOrEmpty(path) ? null : SpriteTextureLoader.getSpriteList(path, false);
                    if (string.IsNullOrEmpty(path) || walk == null || walk.Length == 0 || sprites == null || sprites.Length == 0)
                    {
                        Warn("Missing inherited current walker art for " + spec.Id + ".");
                        missing++;
                    }
                    continue;
                }
                if (!Directory.Exists(Path.Combine(root, "actors", folder))) { Warn("Missing actor art folder: " + folder); missing++; }
            }
            foreach (BuildingSpec spec in ContentRegistry.Buildings)
            {
                string folder = string.IsNullOrEmpty(spec.SourceId) ? spec.Id : spec.SourceId;
                if (!Directory.Exists(Path.Combine(root, "buildings", folder))) { Warn("Missing building art folder: " + folder); missing++; }
            }
            string[] forbidden = { "Assembly-CSharp.dll", "UnityExplorer.dll", "NCMS.dll" };
            foreach (string name in forbidden)
            {
                if (Directory.GetFiles(ModernBoxMod.ModFolder, name, SearchOption.AllDirectories).Length > 0)
                {
                    Error("Forbidden bundled DLL: " + name);
                    missing++;
                }
            }
            foreach (string deferred in new[] { "Space", "Galaxy", "Planets", "StarMap" })
            {
                if (Directory.Exists(Path.Combine(root, deferred))) { Error("Deferred space asset folder was bundled: " + deferred); missing++; }
            }
            Info("Static asset validation completed with " + missing + " problem(s).");
        }

        internal static void ValidateRegisteredContent()
        {
            List<string> errors = new List<string>();
            DuplicateErrors(errors, ContentRegistry.Units.Select(spec => spec.Id), "unit");
            DuplicateErrors(errors, ContentRegistry.Buildings.Select(spec => spec.Id), "building");
            DuplicateErrors(errors, ContentRegistry.Equipment.Select(spec => spec.Id), "equipment");
            DuplicateErrors(errors, ContentRegistry.Bombs.Select(spec => spec.Id), "bomb");

            foreach (ModernUnitSpec spec in ContentRegistry.Units)
            {
                ActorAsset actor = AssetManager.actor_library.get(spec.Id);
                if (actor == null) { errors.Add("actor:" + spec.Id); continue; }
                if (AssetManager.items.get(actor.default_attack) == null) errors.Add("attack:" + spec.Id + "->" + actor.default_attack);
                if (string.IsNullOrEmpty(actor.name_template_unit) || !AssetManager.name_generator.has(actor.name_template_unit))
                {
                    errors.Add("actor-name-generator:" + spec.Id + "->" + actor.name_template_unit);
                }
                else
                {
                    NameGeneratorAsset generator = AssetManager.name_generator.get(actor.name_template_unit);
                    if (generator.vowels == null || generator.vowels.Length == 0)
                        errors.Add("actor-name-vowels:" + spec.Id + "->" + actor.name_template_unit);
                }
                if (!AssetManager.powers.has("modernbox_spawn_" + spec.Id)) errors.Add("spawn-power:" + spec.Id);
                if (actor.action_on_load == null) errors.Add("actor-load-state:" + spec.Id);
                if (!actor.update_z || !actor.death_animation_angle || actor.special_dead_animation) errors.Add("actor-death:" + spec.Id);
                if (!spec.Humanoid && spec.Role != M2UnitRole.Creature && actor.decision_ids != null &&
                    actor.decision_ids.Any(id => !string.IsNullOrEmpty(id) && id.IndexOf("sleep", StringComparison.OrdinalIgnoreCase) >= 0))
                    errors.Add("actor-sleep-decision:" + spec.Id);
                if (spec.Role != M2UnitRole.Creature && actor.kingdom_id_wild != "ModernKingdom") errors.Add("actor-fallback:" + spec.Id);
                if (actor.texture_asset == null || string.IsNullOrEmpty(actor.texture_asset.texture_path_main))
                {
                    errors.Add("actor-texture:" + spec.Id);
                }
                else
                {
                    AnimationContainerUnit animations = ActorAnimationLoader.getAnimationContainer(
                        actor.texture_asset.texture_path_main, actor, null, null);
                    if (animations == null || animations.walking == null || animations.walking.frames == null || animations.walking.frames.Length == 0)
                        errors.Add("actor-walk-animation:" + spec.Id + "@" + actor.texture_asset.texture_path_main);
                    if (animations == null || animations.idle == null || animations.idle.frames == null || animations.idle.frames.Length == 0)
                        errors.Add("actor-idle-animation:" + spec.Id + "@" + actor.texture_asset.texture_path_main);
                }
            }

            foreach (BuildingSpec spec in ContentRegistry.Buildings)
            {
                BuildingAsset building = AssetManager.buildings.get(spec.Id);
                if (building == null) errors.Add("building:" + spec.Id);
                else if (building.building_sprites == null) errors.Add("building-sprites:" + spec.Id);
                if (spec.UpgradeOnly)
                {
                    string source;
                    if (!ActorsAndBuildingsRegistry.UpgradeSources.TryGetValue(spec.Id, out source) || AssetManager.buildings.get(source) == null)
                        errors.Add("upgrade-source:" + spec.Id);
                }
                else
                {
                    foreach (string race in ModernBoxCatalog.SupportedRaces)
                    {
                        ActorAsset species = AssetManager.actor_library.get(race);
                        string orderId = "order_m2_" + race + "_" + spec.Id;
                        if (species == null || species.architecture_asset == null || species.architecture_asset.getBuilding(orderId) == null)
                            errors.Add("build-order:" + race + "->" + spec.Id);
                    }
                }
            }

            foreach (EquipmentSpec spec in ContentRegistry.Equipment)
            {
                EquipmentAsset item = AssetManager.items.get(spec.Id);
                if (item == null) { errors.Add("equipment:" + spec.Id); continue; }
                if (item.name_templates == null || item.name_templates.Count == 0) errors.Add("equipment-names:" + spec.Id);
                if (string.IsNullOrEmpty(item.path_icon)) errors.Add("equipment-icon:" + spec.Id);
                if (item.getGroup() == null) errors.Add("equipment-group:" + spec.Id + "->" + item.group_id);
                if (!item.show_in_meta_editor || !item.show_in_knowledge_window)
                    errors.Add("equipment-editor-visibility:" + spec.Id);
                if (spec.Type == EquipmentType.Weapon)
                {
                    string expectedGroup = string.IsNullOrEmpty(spec.Projectile) ? "sword" : "firearm";
                    if (item.group_id != expectedGroup)
                        errors.Add("equipment-editor-group:" + spec.Id + "->" + item.group_id);
                }
                if (!string.IsNullOrEmpty(spec.Projectile) && AssetManager.projectiles.get(spec.Projectile) == null)
                    errors.Add("equipment-projectile:" + spec.Id + "->" + spec.Projectile);
                if (item.projectile != spec.Projectile) errors.Add("equipment-projectile-map:" + spec.Id + "->" + item.projectile);
            }

            foreach (string projectileId in OriginalM2Projectiles.CustomProjectileIds)
                if (AssetManager.projectiles.get(projectileId) == null) errors.Add("m2-projectile:" + projectileId);
            foreach (KeyValuePair<string, string> pair in OriginalM2Projectiles.AttackProjectiles)
            {
                EquipmentAsset attack = AssetManager.items.get(pair.Key);
                if (attack == null || attack.projectile != pair.Value)
                    errors.Add("m2-attack-projectile:" + pair.Key + "->" + pair.Value);
                else if (attack.show_in_meta_editor || attack.show_in_knowledge_window || attack.can_be_given || string.IsNullOrEmpty(attack.path_icon))
                    errors.Add("m2-attack-editor-leak:" + pair.Key);
            }

            foreach (BombSpec spec in ContentRegistry.Bombs)
            {
                GodPower power = AssetManager.powers.get(spec.Id + "button");
                DropAsset drop = AssetManager.drops.get("modernbox_drop_" + spec.Id);
                if (power == null) errors.Add("bomb-power:" + spec.Id);
                if (drop == null) errors.Add("bomb-drop:" + spec.Id);
                if (power != null && power.cached_drop_asset != drop) errors.Add("bomb-cache:" + spec.Id);
                if (power != null && (!power.hold_action || power.click_power_brush_action != null)) errors.Add("bomb-input:" + spec.Id);
                if (drop != null && (Math.Abs(drop.falling_speed - 3.2f) > 0.001f || drop.falling_height.x < 60f || drop.falling_height.y > 70f))
                    errors.Add("bomb-fall:" + spec.Id);
            }

            foreach (string id in new[] { "Parts", "CyberWareParts", "Xenium" })
            {
                ResourceAsset resource = AssetManager.resources.get(id);
                if (resource == null) errors.Add("resource:" + id);
                else if (string.IsNullOrEmpty(resource.tooltip) || AssetManager.tooltips.get(resource.tooltip) == null) errors.Add("resource-tooltip:" + id);
            }
            foreach (string ideology in EquipmentAndTraitsRegistry.IdeologyIds)
            {
                ActorTrait trait = AssetManager.traits.get(ideology);
                Sprite icon = trait == null ? null : trait.getSprite();
                if (icon == null || icon.bounds.size.x > 0.34f || icon.bounds.size.y > 0.34f) errors.Add("ideology:" + ideology);
            }
            EquipmentAsset missileAttack = AssetManager.items.get("missilelauncherlong");
            ProjectileAsset missileProjectile = AssetManager.projectiles.get("MIRVartillery");
            ActorAsset missileSystem = AssetManager.actor_library.get("MissileSystem");
            ProjectileAsset strategicMissileProjectile = AssetManager.projectiles.get(MissileSystemService.StrategicProjectileId);
            DecisionAsset missileDecision = AssetManager.decisions_library.get(MissileSystemService.DecisionId);
            if (missileAttack == null || missileAttack.projectile != "MIRVartillery" ||
                Math.Abs(missileAttack.base_stats["projectiles"] - 1f) > 0.001f ||
                Math.Abs(missileAttack.base_stats["attack_speed"] - 0.1f) > 0.001f ||
                missileProjectile == null || !missileProjectile.use_min_angle_height ||
                Math.Abs(missileProjectile.speed - OriginalM2Projectiles.MirvArtillerySpeed) > 0.001f ||
                missileSystem == null || missileSystem.default_attack != "missilelauncherlong" ||
                Math.Abs(missileSystem.base_stats["attack_speed"]) > 0.001f ||
                strategicMissileProjectile == null || strategicMissileProjectile.use_min_angle_height ||
                Math.Abs(strategicMissileProjectile.speed - OriginalM2Projectiles.StrategicMirvArtillerySpeed) > 0.001f ||
                missileDecision == null || missileDecision.cooldown != MissileSystemService.LaunchCooldownSeconds)
                errors.Add("missile-system-mirv");
            BuildingAsset silo = AssetManager.buildings.get("MissileSilo");
            if (silo == null || silo.tower || silo.tower_projectile != "NUKER" || Math.Abs(silo.tower_projectile_reload - 32f) > 0.001f)
                errors.Add("missile-silo");
            if (AssetManager.world_log_library.get(SiloLaunchEvents.AssetId) == null) errors.Add("silo-world-log");
            if (AssetManager.biome_library.get(AlienJungleRegistry.BiomeId) == null ||
                AssetManager.top_tiles.get(AlienJungleRegistry.LowTileId) == null ||
                AssetManager.top_tiles.get(AlienJungleRegistry.HighTileId) == null ||
                AssetManager.buildings.get(AlienJungleRegistry.TreeId) == null ||
                AssetManager.buildings.get(AlienJungleRegistry.PlantId) == null)
                errors.Add("alien-jungle");

            if (errors.Count > 0) throw new InvalidOperationException("Registered content validation failed: " + string.Join(", ", errors.ToArray()));
            Debug.Log("[ModernBox M2 Rewrite] Registered-content validation passed.");
        }

        private static void DuplicateErrors(List<string> errors, IEnumerable<string> ids, string kind)
        {
            foreach (IGrouping<string, string> group in ids.GroupBy(id => id, StringComparer.Ordinal))
                if (group.Count() > 1) errors.Add("duplicate-" + kind + ":" + group.Key);
        }
    }
}
