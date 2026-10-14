using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// M2's "send units to space" powers (arrowleft2 / arrowleft3), the colony ship they land
    /// with, and the TUDDS universe destroyer (DeleterButton).
    internal static class M2SpaceUnits
    {
        internal const string CopyPowerId = "arrowleft2";
        internal const string PastePowerId = "arrowleft3";
        internal const string DeleterPowerId = "DeleterButton";
        internal const string ColonyShipId = "colonyship";

        internal sealed class UnitData
        {
            public string statsID = "";
            public ActorData data;
            public List<string> traits = new List<string>();
            public string cultureName;
            public List<string> cultureTraits = new List<string>();
            public List<string> cultureTechs = new List<string>();
            public List<SavedItem> items = new List<SavedItem>();
            public int dictInt;
            public Vector2Int oldPos;
        }

        internal sealed class SavedItem
        {
            public string assetId;
            public string name;
            public int kills;
            public List<string> modifiers = new List<string>();
        }

        internal static readonly Dictionary<string, UnitData> unitClipboardDict = new Dictionary<string, UnitData>();
        private static readonly HashSet<long> copiedActorIds = new HashSet<long>();

        // Actor ids start again at 1 in every world.
        internal static void ResetWorldState()
        {
            copiedActorIds.Clear();
        }
        internal static int unitClipboardDictNum;
        private static bool universeDestroyed;

        internal static void Register()
        {
            RegisterEffect();
            RegisterColonyShip();

            RegisterDrop("copyer", (tile, drop) => action_copy(tile, drop));
            RegisterDrop("paster", (tile, drop) => action_paste(tile, drop));
            RegisterDrop("deleter", (tile, drop) => action_DeleterClick(tile, drop));

            RegisterPower(CopyPowerId, "copyer", 0.001f, "ui/icons/Arrowleft", "Choose Units", "Select the units you want to send to space.");
            RegisterPower(PastePowerId, "paster", 0.01f, "ui/icons/Arrowright", "Land Units", "Land Units on your planet.");
            RegisterPower(DeleterPowerId, "deleter", 0.01f, "ui/icons/UniversalDestroyer", "TUDDS", "THIS IS A LOSER BUTTON NO DESCRIPTION IS NEEDED!!!");
        }

        private static void RegisterEffect()
        {
            if (AssetManager.effects_library.get("evilspawn") != null) return;
            AssetManager.effects_library.add(new EffectAsset
            {
                id = "evilspawn",
                use_basic_prefab = true,
                sprite_path = "effects/fx_teleport_red_t",
                draw_light_area = true,
                show_on_mini_map = false,
                limit = 80,
                sorting_layer_id = "EffectsTop"
            });
        }

        private static void RegisterColonyShip()
        {
            if (AssetManager.buildings.get(ColonyShipId) != null) return;
            try
            {
                BuildingAsset ship = AssetManager.buildings.clone(ColonyShipId, "$building_civ_human$");
                ship.id = ColonyShipId;
                ship.sprite_path = "buildings/" + ColonyShipId;
                ship.main_path = ship.sprite_path;
                ship.setAtlasID("buildings", "buildings");
                ship.atlas_asset = AssetManager.dynamic_sprites_library.get("buildings");
                ship.city_building = false;
                ship.group = "modernbox_m2_world_structures";
                ship.type = "modernbox_type_" + ColonyShipId;
                ship.fundament = new BuildingFundament(1, 0, 1, 0);
                ship.cost = new ConstructionCost(0, 0, 0, 0);
                ship.base_stats["health"] = 1000f;
                ship.base_stats["size"] = 1f;
                ship.affected_by_drought = false;
                ship.burnable = false;
                ship.can_be_upgraded = false;
                ship.can_be_placed_on_liquid = true;
                ship.can_be_living_house = false;
                ship.can_units_live_here = false;
                ship.housing_slots = 0;
                ship.spawn_units = false;
                ship.spawn_units_asset = null;
                ship.can_be_demolished = true;
                ship.can_be_abandoned = false;
                ship.has_sprites_main = true;
                ship.has_sprites_ruin = true;
                ship.has_ruin_state = true;
                ship.has_ruins_graphics = true;
                ship.setShadow(0.5f, 0.03f, 0.12f);
                ship.addResource("wood", 200);
                ship.addResource("stone", 200);
                ship.addResource("common_metals", 200);
                ship.loadBuildingSprites();
                ActorsAndBuildingsRegistry.EnsureBuildingRenderSprites(ship);
                ModernLocalization.Add(ColonyShipId, "Colony Ship");
                ModernLocalization.Add(ColonyShipId + "_description", "The ship your colonists landed in.");
            }
            catch (Exception ex)
            {
                ModernBoxDiagnostics.Warn("Colony ship was not registered: " + ex.Message);
            }
        }

        private static void RegisterDrop(string id, DropsAction landed)
        {
            if (AssetManager.drops.get(id) != null) return;
            AssetManager.drops.add(new DropAsset
            {
                id = id,
                type = DropType.DropBomb,
                path_texture = "drops/drop_czarbomba",
                default_scale = 0.2f,
                random_frame = false,
                random_flip = false,
                falling_speed = 3.2f,
                falling_speed_random = 0.5f,
                falling_height = new Vector2(60f, 70f),
                falling_random_x_move = false,
                action_landed = landed
            });
        }

        private static void RegisterPower(string id, string dropId, float fallingChance, string icon, string name, string description)
        {
            if (AssetManager.powers.get(id) != null) return;
            DropAsset drop = AssetManager.drops.get(dropId);
            AssetManager.powers.add(new GodPower
            {
                id = id,
                name = id,
                rank = PowerRank.Rank0_free,
                path_icon = icon,
                hold_action = true,
                show_tool_sizes = true,
                unselect_when_window = false,
                ignore_cursor_icon = true,
                falling_chance = fallingChance,
                drop_id = dropId,
                cached_drop_asset = drop,
                click_power_action = SpawnDrop,
                click_power_brush_action = LoopBrush
            });
            ModernLocalization.Add(id, name);
            ModernLocalization.Add(id + "_description", description);
        }

        private static bool SpawnDrop(WorldTile tile, GodPower power)
        {
            if (tile == null || power == null || power.cached_drop_asset == null) return false;
            return AssetManager.powers.spawnDrops(tile, power);
        }

        private static bool LoopBrush(WorldTile tile, GodPower power)
        {
            if (tile == null || power == null) return false;
            return AssetManager.powers.loopWithCurrentBrushPowerForDropsRandom(tile, power);
        }

        internal static void action_copy(WorldTile pTile = null, string pDropID = null)
        {
            if (pTile == null) return;
            foreach (BaseSimObject obj in Finder.getAllObjectsInChunks(pTile, 3).ToList())
            {
                if (!obj.isActor()) continue;
                Actor actor = obj.a;
                if (actor == null || !actor.isAlive() || actor.asset == null || actor.asset.is_boat) continue;
                CopyUnit(actor);
            }
        }

        internal static void action_paste(WorldTile pTile = null, string pDropID = null)
        {
            if (pTile == null || unitClipboardDict.Count == 0) return;
            foreach (var entry in unitClipboardDict.ToList())
            {
                try { PasteUnit(pTile, entry.Value); }
                catch (Exception ex) { Debug.LogWarning("[ModernBox Space] Paste failed: " + ex.Message); }
                unitClipboardDict.Remove(entry.Key);
            }
            copiedActorIds.Clear();
        }

        internal static void CopyUnit(Actor targetActor)
        {
            if (targetActor == null || !copiedActorIds.Add(targetActor.id)) return;
            UnitData newSavedUnit = new UnitData
            {
                statsID = targetActor.asset.id,
                data = new ActorData(),
                dictInt = unitClipboardDictNum,
                oldPos = targetActor.current_tile.pos
            };
            ai.ActorTool.copyImportantData(targetActor.data, newSavedUnit.data, false);
            // Relations point at ids of the world being left behind.
            newSavedUnit.data.lover = -1L;
            newSavedUnit.data.best_friend_id = -1L;
            newSavedUnit.data.parent_id_1 = -1L;
            newSavedUnit.data.parent_id_2 = -1L;
            newSavedUnit.data.ancestor_family = -1L;
            foreach (ActorTrait trait in targetActor.getTraits()) newSavedUnit.traits.Add(trait.id);
            if (targetActor.culture != null)
            {
                newSavedUnit.cultureName = targetActor.culture.data.name;
                newSavedUnit.cultureTraits = targetActor.culture.getTraitsAsStrings().ToList();
                newSavedUnit.cultureTechs = M2Tech.Snapshot(targetActor.culture);
            }
            if (targetActor.equipment != null)
            {
                foreach (ActorEquipmentSlot slot in targetActor.equipment)
                {
                    Item item = slot == null || slot.isEmpty() ? null : slot.getItem();
                    if (item == null || item.data == null || string.IsNullOrEmpty(item.data.asset_id)) continue;
                    SavedItem saved = new SavedItem { assetId = item.data.asset_id, name = item.data.name, kills = item.data.kills };
                    foreach (string modifier in item.data.modifiers) saved.modifiers.Add(modifier);
                    newSavedUnit.items.Add(saved);
                }
            }
            unitClipboardDict.Add(unitClipboardDictNum.ToString(), newSavedUnit);
            unitClipboardDictNum++;
            EffectsLibrary.spawnAt("evilspawn", targetActor.current_tile.posV3, 0.1f);
        }

        internal static Actor PasteUnit(WorldTile targetTile, UnitData unitData)
        {
            if (targetTile == null || unitData == null || AssetManager.actor_library.get(unitData.statsID) == null) return null;
            Actor pastedUnit = World.world.units.createNewUnit(unitData.statsID, targetTile);
            if (pastedUnit == null) return null;

            foreach (ActorTrait trait in pastedUnit.getTraits().ToList()) pastedUnit.removeTrait(trait);
            if (unitData.data != null) ai.ActorTool.copyImportantData(unitData.data, pastedUnit.data, false);

            WorldTile shipTile = pastedUnit.current_tile ?? targetTile;
            if (shipTile.building == null && AssetManager.buildings.get(ColonyShipId) != null)
                World.world.buildings.addBuilding(ColonyShipId, shipTile, false, false, BuildPlacingType.New);

            if (pastedUnit.isSapient())
            {
                TileZone zone = targetTile.zone;
                if (zone.hasCity() && !zone.city.isNeutral() && zone.city.isPossibleToJoin(pastedUnit)) pastedUnit.joinCity(zone.city);
                else if (!zone.hasCity()) pastedUnit.buildCityAndStartCivilization();

                Culture culture = pastedUnit.culture;
                if (culture != null && !string.IsNullOrEmpty(unitData.cultureName))
                {
                    culture.setName(unitData.cultureName, false);
                    foreach (string traitId in unitData.cultureTraits)
                    {
                        if (!culture.hasTrait(traitId) && AssetManager.culture_traits.get(traitId) != null) culture.addTrait(traitId);
                    }
                }
                if (culture != null) M2Tech.AddTechs(culture, unitData.cultureTechs);
            }
            RestoreEquipment(pastedUnit, unitData.items);

            foreach (string trait in unitData.traits)
            {
                if (AssetManager.traits.get(trait) != null) pastedUnit.addTrait(trait);
            }
            pastedUnit.setStatsDirty();
            pastedUnit.restoreHealth(10 ^ 9);
            EffectsLibrary.spawnAt("evilspawn", pastedUnit.current_tile.posV3, 0.1f);
            return pastedUnit;
        }

        private static void RestoreEquipment(Actor actor, List<SavedItem> items)
        {
            if (actor == null || items == null || items.Count == 0 || World.world.items == null) return;
            if (actor.equipment == null) actor.equipment = new ActorEquipment();
            foreach (ActorEquipmentSlot slot in actor.equipment)
                if (slot != null && !slot.isEmpty()) slot.takeAwayItem();
            foreach (SavedItem saved in items)
            {
                EquipmentAsset asset = AssetManager.items.get(saved.assetId);
                if (asset == null) continue;
                Item item = World.world.items.newItem(asset);
                if (item == null) continue;
                if (!string.IsNullOrEmpty(saved.name)) item.data.name = saved.name;
                item.data.kills = saved.kills;
                foreach (string modifier in saved.modifiers) item.addMod(modifier);
                actor.equipment.setItem(item, actor);
            }
            actor.setStatsDirty();
        }

        internal static void action_DeleterClick(WorldTile pTile, string pPowerID)
        {
            if (universeDestroyed) return;
            universeDestroyed = true;
            M2SpaceManager.DeleteBomb();
            foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
            {
                if (obj != null) obj.SetActive(false);
            }
            new GameObject("EndScreenManager").AddComponent<UniverseDestructionManager>();
        }
    }

    internal sealed class UniverseDestructionManager : MonoBehaviour
    {
        private bool showEndScreen = true;
        private Texture2D blackTexture;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            blackTexture = new Texture2D(1, 1);
            blackTexture.SetPixel(0, 0, Color.black);
            blackTexture.Apply();
        }

        private void OnGUI()
        {
            if (!showEndScreen) return;
            GUI.depth = -1000;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);
            Rect windowRect = new Rect((Screen.width - 500f) / 2, (Screen.height - 200f) / 2, 500f, 200f);
            GUILayout.BeginArea(windowRect, GUI.skin.box);
            GUILayout.Label("Suddenly, in the blink of an eye, everything was destroyed in every way it is possible to be destroyed, thousands of galaxies vanished in an instant. The timeline has been destroyed.");
            GUILayout.Space(20);
            if (GUILayout.Button("Quit Game", GUILayout.Height(40))) Application.Quit();
            GUILayout.EndArea();
        }
    }
}
