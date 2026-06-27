//========= MODERNBOX 10.0 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NCMS;
using NCMS.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ReflectionUtility;
using Assets.SimpleZip;

namespace ModernBox {
class Buttonz {

  public static void init() {

    tab.createTab("Button Tab_ModernBox", "Tab_ModernBox", "ModernBox", "Guns, Vehicles, Drugs, Casinos, MIRVs. Welcome to the Modern Age.", -150);
    loadButtons();
  }

  private static void loadButtons() {
    PowersTab tab = getPowersTab("ModernBox");

    // DropAsset spawnBoat2 = new DropAsset();
    // spawnBoat2.id = "spawnJob";
    // spawnBoat2.path_texture = "drops/drop_fireworks";
    // spawnBoat2.random_frame = true;
    // spawnBoat2.default_scale = 0.08f;
    // spawnBoat2.action_landed = new DropsAction(action_worker);
    // AssetManager.drops.add(spawnBoat2);

    GodPower MIRVPower = AssetManager.powers.clone("spawn_mirv_bomber", "_spawnActor");
    MIRVPower.name = "spawn_mirv_bomber";
    MIRVPower.actor_asset_id = "MIRVBomber";
    MIRVPower.click_action = new PowerActionWithID(action_spawn_jet);

    GodPower AirshipPower = AssetManager.powers.clone("spawn_airship", "_spawnActor");
    AirshipPower.name = "spawn_airship";
    AirshipPower.actor_asset_id = "Zeppelin";
    AirshipPower.click_action = new PowerActionWithID(action_spawn_jet);

    GodPower HeliPower = AssetManager.powers.clone("heli_spawn", "_spawnActor");
    HeliPower.name = "heli_spawn";
    HeliPower.actor_asset_id = "Heli";
    HeliPower.click_action = new PowerActionWithID(action_spawn_jet);

    GodPower GunshipPower = AssetManager.powers.clone("gunship_spawn", "_spawnActor");
    GunshipPower.name = "gunship_spawn";
    GunshipPower.actor_asset_id = "Gunship";
    GunshipPower.click_action = new PowerActionWithID(action_spawn_jet);

    GodPower TankPower = AssetManager.powers.clone("spawn_tank", "_spawnActor");
    TankPower.name = "spawn_tank";
    TankPower.actor_asset_id = "Tank";
    TankPower.click_action = new PowerActionWithID(action_spawn_jet);

    GodPower HumveePower = AssetManager.powers.clone("spawn_humvee", "_spawnActor");
    HumveePower.name = "spawn_humvee";
    HumveePower.actor_asset_id = "Humvee";
    HumveePower.click_action = new PowerActionWithID(action_spawn_jet);

    GodPower F22Power = AssetManager.powers.clone("spawn_F22", "_spawnActor");
    F22Power.name = "spawn_F22";
    F22Power.actor_asset_id = "F22";
    F22Power.click_action = new PowerActionWithID(action_spawn_jet);

    GodPower F55Power = AssetManager.powers.clone("spawn_F55", "_spawnActor");
    F55Power.name = "spawn_F55";
    F55Power.actor_asset_id = "F55";
    F55Power.click_action = new PowerActionWithID(action_spawn_jet);

    GodPower RailgunPower = AssetManager.powers.clone("spawn_Railgun", "_spawnActor");
    RailgunPower.name = "spawn_Railgun";
    RailgunPower.actor_asset_id = "Railgun";
    RailgunPower.click_action = new PowerActionWithID(action_spawn_jet);
	
    GodPower MissileSystemPower = AssetManager.powers.clone("spawn_MissileSystem", "_spawnActor");
    MissileSystemPower.name = "spawn_MissileSystem";
    MissileSystemPower.actor_asset_id = "MissileSystem";
    MissileSystemPower.click_action = new PowerActionWithID(action_spawn_jet);

    DropAsset Droppy = new DropAsset();
    Droppy.id = "spawnnuke";
    Droppy.path_texture = "drops/drop_czarbomba";
    Droppy.random_frame = true;
    Droppy.default_scale = 0.2f;
    Droppy.fallingHeight = (Vector3) new Vector2(60f, 70f);
    Droppy.action_landed = new DropsAction(action_MOABClick);
    AssetManager.drops.add(Droppy);

    DropAsset CobaltDrop = new DropAsset();
    CobaltDrop.id = "cobalts";
    CobaltDrop.path_texture = "drops/drop_czarbomba";
    CobaltDrop.random_frame = false;
    CobaltDrop.default_scale = 0.2f;
    CobaltDrop.fallingHeight = (Vector3) new Vector2(60f, 70f);
    CobaltDrop.action_landed = new DropsAction(action_CobaltClick);
    AssetManager.drops.add(CobaltDrop);

    DropAsset UltronDrop = new DropAsset();
    UltronDrop.id = "ultrons";
    UltronDrop.path_texture = "drops/drop_czarbomba";
    UltronDrop.random_frame = false;
    UltronDrop.default_scale = 0.2f;
    UltronDrop.fallingHeight = (Vector3) new Vector2(60f, 70f);
    UltronDrop.action_landed = new DropsAction(action_UltronClick);
    AssetManager.drops.add(UltronDrop);

    DropAsset XeniumDrop = new DropAsset();
    XeniumDrop.id = "xeniums";
    XeniumDrop.path_texture = "drops/drop_czarbomba";
    XeniumDrop.random_frame = false;
    XeniumDrop.default_scale = 0.2f;
    XeniumDrop.fallingHeight = (Vector3) new Vector2(60f, 70f);
    XeniumDrop.action_landed = new DropsAction(action_XeniumClick);
    AssetManager.drops.add(XeniumDrop);

    DropAsset MiniDrop = new DropAsset();
    MiniDrop.id = "minis";
    MiniDrop.path_texture = "drops/drop_czarbomba";
    MiniDrop.random_frame = false;
    MiniDrop.default_scale = 0.2f;
    MiniDrop.fallingHeight = (Vector3) new Vector2(60f, 70f);
    MiniDrop.action_landed = new DropsAction(action_MiniClick);
    AssetManager.drops.add(MiniDrop);

    DropAsset ProtonDrop = new DropAsset();
    ProtonDrop.id = "protons";
    ProtonDrop.path_texture = "drops/drop_czarbomba";
    ProtonDrop.random_frame = false;
    ProtonDrop.default_scale = 0.2f;
    ProtonDrop.fallingHeight = (Vector3) new Vector2(60f, 70f);
    ProtonDrop.action_landed = new DropsAction(action_ProtonClick);
    AssetManager.drops.add(ProtonDrop);

    DropAsset JupiterDrop = new DropAsset();
    JupiterDrop.id = "jupiters";
    JupiterDrop.path_texture = "drops/drop_czarbomba";
    JupiterDrop.random_frame = false;
    JupiterDrop.default_scale = 0.2f;
    JupiterDrop.fallingHeight = (Vector3) new Vector2(60f, 70f);
    JupiterDrop.action_landed = new DropsAction(action_JupiterClick);
    AssetManager.drops.add(JupiterDrop);

    DropAsset EraserDrop = new DropAsset();
    EraserDrop.id = "eraser";
    EraserDrop.path_texture = "drops/drop_antimatter";
    EraserDrop.random_frame = false;
    EraserDrop.default_scale = 0.2f;
    EraserDrop.fallingHeight = (Vector3)new Vector2(60f, 70f);
    EraserDrop.action_landed = new DropsAction(action_EraserClick);
    AssetManager.drops.add(EraserDrop);
	
	
    DropAsset DeathDrop = new DropAsset();
    DeathDrop.id = "deaths";
    DeathDrop.path_texture = "drops/drop_czarbomba";
    DeathDrop.random_frame = false;
    DeathDrop.default_scale = 0.2f;
    DeathDrop.fallingHeight = (Vector3) new Vector2(60f, 70f);
    DeathDrop.action_landed = new DropsAction(action_DeathClick);
    AssetManager.drops.add(DeathDrop);

    DropAsset RandomDrop = new DropAsset();
    RandomDrop.id = "randomdrop";
    RandomDrop.path_texture = "drops/drop_czarbomba";
    RandomDrop.random_frame = false;
    RandomDrop.default_scale = 0.2f;
    RandomDrop.fallingHeight = (Vector3) new Vector2(60f, 70f);
    RandomDrop.action_landed = new DropsAction(action_RandomClick);
    AssetManager.drops.add(RandomDrop);

    // GodPower WorkerPower = new GodPower();
    // WorkerPower.id = "WorkerPowerbutton";
    // WorkerPower.name = "WorkerPowerbutton";
    // WorkerPower.showToolSizes = true;
    // WorkerPower.holdAction = true;
    // WorkerPower.unselectWhenWindow = true;
    // WorkerPower.fallingChance = 0.3f;
    // WorkerPower.actor_asset_id = "boat_trading";
    // WorkerPower.dropID = "spawnJob";
    // WorkerPower.click_power_action = new PowerAction(Stuff_Drop);
    // WorkerPower.click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) => { return (bool)AssetManager.powers.CallMethod("loopWithCurrentBrushPower", pTile, pPower); });
    // AssetManager.powers.add(WorkerPower);

    // GodPower AirplanePower = new GodPower();
    // AirplanePower.id = "AirplanePowerbutton";
    // AirplanePower.name = "AirplanePowerbutton";
    // AirplanePower.showToolSizes = true;
    // AirplanePower.holdAction = true;
    // AirplanePower.unselectWhenWindow = true;
    // AirplanePower.fallingChance = 0.3f;
    // AirplanePower.actor_asset_id = "boat_trading";
    // AirplanePower.dropID = "spawnJob";
    // AirplanePower.click_power_action = new PowerAction(Stuff_Drop);
    // AirplanePower.click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) => { return (bool)AssetManager.powers.CallMethod("loopWithCurrentBrushPower", pTile, pPower); });
    // AssetManager.powers.add(AirplanePower);

    GameObject largeImageObject = new GameObject("LargeImage");
    largeImageObject.transform.SetParent(tab.transform);
    largeImageObject.transform.localPosition = new Vector3(288, 18, 0);
    largeImageObject.transform.localScale = new Vector3(1, 1, 1);

    Image largeImage = largeImageObject.AddComponent<Image>();
    largeImage.sprite = Resources.Load<Sprite>("ui/Icons/TabText");

    RectTransform largeImageRectTransform = largeImageObject.GetComponent<RectTransform>();
    largeImageRectTransform.sizeDelta = new Vector2(200, 100);
    largeImageRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
    largeImageRectTransform.anchorMax = new Vector2(0.5f, 0.5f);



    PowerButtons.CreateButton("STRONGMIRV_toggle", Resources.Load<Sprite>("ui/Icons/MIRV_nuke"), "Nuclear MIRVs", "Toggle Nuclear MIRVs from being developed (this won't remove existing Nuclear MIRVS)", new Vector2(180, -18), ButtonType.Toggle, tab.transform, MIRV.toggleMIRVSSTRONG

    );
    if (Main.savedSettings.boolOptions["STRONGMIRVOption"]) {
      PowerButtons.ToggleButton("STRONGMIRV_toggle");
      MIRV.toggleMIRVSSTRONG();
    }

    PowerButtons.CreateButton("MIRV_toggle", Resources.Load<Sprite>("ui/Icons/MIRV"), "MIRVs", "Toggle MIRVs from being developed (this won't remove existing MIRVS)", new Vector2(180, 18), ButtonType.Toggle, tab.transform, MIRV.toggleMIRVS

    );
    if (Main.savedSettings.boolOptions["MIRVOption"]) {
      PowerButtons.ToggleButton("MIRV_toggle");
      MIRV.toggleMIRVS();
    }

    GodPower MOABPower = new GodPower();
    MOABPower.id = "MOABbutton";
    MOABPower.name = "MOABbutton";
    MOABPower.holdAction = true;
    MOABPower.fallingChance = 0.01f;
    MOABPower.showToolSizes = true;
    MOABPower.unselectWhenWindow = false;
    MOABPower.ignore_cursor_icon = true;
    MOABPower.dropID = "spawnnuke";
    MOABPower.click_power_action = new PowerAction(Stuff_Drop);
    MOABPower.click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) => { return (bool) AssetManager.powers.CallMethod("loopWithCurrentBrushPower", pTile, pPower); });
    AssetManager.powers.add(MOABPower);

    GodPower CobaltPower = new GodPower();
    CobaltPower.id = "Cobaltbutton";
    CobaltPower.name = "Cobaltbutton";
    CobaltPower.holdAction = true;
    CobaltPower.showToolSizes = true;
    CobaltPower.fallingChance = 0.01f;
    CobaltPower.unselectWhenWindow = false;
    CobaltPower.ignore_cursor_icon = true;
    CobaltPower.dropID = "cobalts";
    CobaltPower.click_power_action = new PowerAction(Stuff_Drop);
    CobaltPower.click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) => { return (bool) AssetManager.powers.CallMethod("loopWithCurrentBrushPower", pTile, pPower); });
    AssetManager.powers.add(CobaltPower);

    GodPower UltronPower = new GodPower();
    UltronPower.id = "Ultronbutton";
    UltronPower.name = "Ultronbutton";
    UltronPower.holdAction = true;
    UltronPower.showToolSizes = true;
    UltronPower.unselectWhenWindow = false;
    UltronPower.fallingChance = 0.01f;
    UltronPower.ignore_cursor_icon = true;
    UltronPower.dropID = "ultrons";
    UltronPower.click_power_action = new PowerAction(Stuff_Drop);
    UltronPower.click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) => { return (bool) AssetManager.powers.CallMethod("loopWithCurrentBrushPower", pTile, pPower); });
    AssetManager.powers.add(UltronPower);

    GodPower XeniumPower = new GodPower();
    XeniumPower.id = "Xeniumbutton";
    XeniumPower.name = "Xeniumbutton";
    XeniumPower.holdAction = true;
    XeniumPower.fallingChance = 0.01f;
    XeniumPower.showToolSizes = true;
    XeniumPower.unselectWhenWindow = false;
    XeniumPower.ignore_cursor_icon = true;
    XeniumPower.dropID = "xeniums";
    XeniumPower.click_power_action = new PowerAction(Stuff_Drop);
    XeniumPower.click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) => { return (bool) AssetManager.powers.CallMethod("loopWithCurrentBrushPower", pTile, pPower); });
    AssetManager.powers.add(XeniumPower);

    GodPower MiniPower = new GodPower();
    MiniPower.id = "Minibutton";
    MiniPower.name = "Minibutton";
    MiniPower.holdAction = true;
    MiniPower.fallingChance = 0.01f;
    MiniPower.showToolSizes = true;
    MiniPower.unselectWhenWindow = false;
    MiniPower.ignore_cursor_icon = true;
    MiniPower.dropID = "minis";
    MiniPower.click_power_action = new PowerAction(Stuff_Drop);
    MiniPower.click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) => { return (bool) AssetManager.powers.CallMethod("loopWithCurrentBrushPower", pTile, pPower); });
    AssetManager.powers.add(MiniPower);

    GodPower ProtonPower = new GodPower();
    ProtonPower.id = "Protonbutton";
    ProtonPower.name = "Protonbutton";
    ProtonPower.fallingChance = 0.01f;
    ProtonPower.holdAction = true;
    ProtonPower.showToolSizes = true;
    ProtonPower.unselectWhenWindow = false;
    ProtonPower.ignore_cursor_icon = true;
    ProtonPower.dropID = "protons";
    ProtonPower.click_power_action = new PowerAction(Stuff_Drop);
    ProtonPower.click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) => { return (bool) AssetManager.powers.CallMethod("loopWithCurrentBrushPower", pTile, pPower); });
    AssetManager.powers.add(ProtonPower);

    GodPower JupiterPower = new GodPower();
    JupiterPower.id = "Jupiterbutton";
    JupiterPower.name = "Jupiterbutton";
    JupiterPower.fallingChance = 0.01f;
    JupiterPower.holdAction = true;
    JupiterPower.showToolSizes = true;
    JupiterPower.unselectWhenWindow = false;
    JupiterPower.ignore_cursor_icon = true;
    JupiterPower.dropID = "jupiters";
    JupiterPower.click_power_action = new PowerAction(Stuff_Drop);
    JupiterPower.click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) => { return (bool) AssetManager.powers.CallMethod("loopWithCurrentBrushPower", pTile, pPower); });
    AssetManager.powers.add(JupiterPower);

    GodPower EraserPower = new GodPower();
    EraserPower.id = "Eraserbutton";
    EraserPower.name = "Eraserbutton";
    EraserPower.fallingChance = 0.01f;
    EraserPower.holdAction = true;
    EraserPower.showToolSizes = true;
    EraserPower.ignore_cursor_icon = true;
    EraserPower.dropID = "eraser";
    EraserPower.click_power_action = new PowerAction(Stuff_Drop);
    EraserPower.click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) => { return (bool)AssetManager.powers.CallMethod("loopWithCurrentBrushPower", pTile, pPower); });
    AssetManager.powers.add(EraserPower);
	
    GodPower DeathPower = new GodPower();
    DeathPower.id = "Deathbutton";
    DeathPower.name = "Deathbutton";
    DeathPower.holdAction = true;
    DeathPower.fallingChance = 0.01f;
    DeathPower.showToolSizes = true;
    DeathPower.unselectWhenWindow = false;
    DeathPower.ignore_cursor_icon = true;
    DeathPower.dropID = "deaths";
    DeathPower.click_power_action = new PowerAction(Stuff_Drop);
    DeathPower.click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) => { return (bool) AssetManager.powers.CallMethod("loopWithCurrentBrushPower", pTile, pPower); });
    AssetManager.powers.add(DeathPower);

    GodPower RandomPower = new GodPower();
    RandomPower.id = "Randombutton";
    RandomPower.name = "Randombutton";
    RandomPower.holdAction = true;
    RandomPower.fallingChance = 0.001f;
    RandomPower.showToolSizes = true;
    RandomPower.unselectWhenWindow = false;
    RandomPower.ignore_cursor_icon = true;
    RandomPower.dropID = "randomdrop";
    RandomPower.click_power_action = new PowerAction(Stuff_Drop);
    RandomPower.click_power_brush_action = new PowerAction((WorldTile pTile, GodPower pPower) => { return (bool) AssetManager.powers.CallMethod("loopWithCurrentBrushPower", pTile, pPower); });
    AssetManager.powers.add(RandomPower);

    PowerButtons.CreateButton("MOABbutton", Resources.Load<Sprite>("ui/Icons/MOAB"), "Super-Nuke", "Also known as the 'Lag Bomb'.", new Vector2(432, 18),
                              // new Vector2(72, 18),
                              NCMS.Utils.ButtonType.GodPower, tab.transform, null);

    PowerButtons.CreateButton("Xeniumbutton", Resources.Load<Sprite>("ui/Icons/Xeno"), "Xenium Bomb", "You thought the ultron bomb was big? This thing is HUGE.", new Vector2(432, -18), NCMS.Utils.ButtonType.GodPower, tab.transform, null);
    PowerButtons.CreateButton("Cobaltbutton", Resources.Load<Sprite>("ui/Icons/Cobalt"), "Cobalt Bomb", "Small Mushroom but huge radius, watch out with this one.", new Vector2(396, -18), NCMS.Utils.ButtonType.GodPower, tab.transform, null);
    PowerButtons.CreateButton("Ultronbutton", Resources.Load<Sprite>("ui/Icons/Ultron"), "Ultron Bomb", "WOOOAH", new Vector2(468, 18), NCMS.Utils.ButtonType.GodPower, tab.transform, null);
    PowerButtons.CreateButton("Deathbutton", Resources.Load<Sprite>("ui/Icons/Death"), "Death Bomb", "Such an original name.", new Vector2(504, 18), NCMS.Utils.ButtonType.GodPower, tab.transform, null);
    PowerButtons.CreateButton("Randombutton", Resources.Load<Sprite>("ui/Icons/wat"), "Random Bomb", "You could be dropping a proton bomb, or a mini nuke, it's random!", new Vector2(540, 18), NCMS.Utils.ButtonType.GodPower, tab.transform, null);
    PowerButtons.CreateButton("Minibutton", Resources.Load<Sprite>("ui/Icons/Mini"), "Mini Nuke", "Small nukes, great for minor scuffles.", new Vector2(396, 18), NCMS.Utils.ButtonType.GodPower, tab.transform, null);
    PowerButtons.CreateButton("Protonbutton", Resources.Load<Sprite>("ui/Icons/Proton"), "Proton Bomb", "wtf is this?", new Vector2(468, -18), NCMS.Utils.ButtonType.GodPower, tab.transform, null);
    PowerButtons.CreateButton("Jupiterbutton", Resources.Load<Sprite>("ui/Icons/Jupiter"), "Jupiter Bomb", "The new monster.", new Vector2(504, -18), NCMS.Utils.ButtonType.GodPower, tab.transform, null);
    PowerButtons.CreateButton("Eraserbutton",Resources.Load<Sprite>("ui/Icons/Eraser"),"Eraser Bomb","also known as the overcompensating bomb.",new Vector2(540, -18),NCMS.Utils.ButtonType.GodPower,tab.transform,null);
    PowerButtons.CreateButton("Cyberware_toggle", Resources.Load<Sprite>("ui/Icons/Cyberware"), "Cyberware", "Toggle Cyberware from being developed (this won't remove existing Cyberware)", new Vector2(1044, 18), ButtonType.Toggle, tab.transform, cyberware.toggleCyberware);
    if (Main.savedSettings.boolOptions["CyberwareOption"]) {
      PowerButtons.ToggleButton("Cyberware_toggle");
      cyberware.toggleCyberware();
    }

    PowerButtons.CreateButton("Drugs_toggle", Resources.Load<Sprite>("ui/Icons/Drugs"), "Drugs", "Toggle Drugs from being developed (this won't remove existing Drugs already made)", new Vector2(1044, -18), ButtonType.Toggle, tab.transform, Drugs.toggleDrugs);
    if (Main.savedSettings.boolOptions["DrugsOption"]) {
      PowerButtons.ToggleButton("Drugs_toggle");
      Drugs.toggleDrugs();
    }

    PowerButtons.CreateButton("Drones_Toggle", Resources.Load<Sprite>("ui/Icons/Drone"), "Drones", "Toggle if kingdoms can create Drones.", new Vector2(972, -18), ButtonType.Toggle, tab.transform, Commerce.toggleDrones);
    if (Main.savedSettings.boolOptions["DronesOption"]) {
      PowerButtons.ToggleButton("Drones_Toggle");
      Commerce.toggleDrones();
    }

    PowerButtons.CreateButton("heli_spawn", Resources.Load<Sprite>("ui/Icons/Heli"), "Helicopter Spawn", "Spawn a Helicopter", new Vector2(612, 18), ButtonType.GodPower, tab.transform, null);

    PowerButtons.CreateButton("HelicopterFactory_toggle", Resources.Load<Sprite>("ui/Icons/Heli"), "Helicopter Factories", "Toggles if factories make Helicopters", new Vector2(612, -18), ButtonType.Toggle, tab.transform, Commerce.toggleHelicopterFactory);

    if (Main.savedSettings.boolOptions["HeliOption"]) {
      PowerButtons.ToggleButton("HelicopterFactory_toggle");
      Commerce.toggleHelicopterFactory();
    }

    PowerButtons.CreateButton("spawn_airship", Resources.Load<Sprite>("ui/Icons/Airship"), "Spawn Zeppelin", "Spawn a Zeppelin.", new Vector2(648, 18), ButtonType.GodPower, tab.transform, null);

    PowerButtons.CreateButton("AirshipFactory_toggle", Resources.Load<Sprite>("ui/Icons/Airship"), "Airship Factories", "Toggles if factories make Airships", new Vector2(648, -18), ButtonType.Toggle, tab.transform, Commerce.toggleAirshipFactory);

    if (Main.savedSettings.boolOptions["AirshipOption"]) {
      PowerButtons.ToggleButton("AirshipFactory_toggle");
      Commerce.toggleAirshipFactory();
    }

    PowerButtons.CreateButton("spawn_tank", Resources.Load<Sprite>("ui/Icons/Tank"), "Spawn Tank", "Spawn a tank.", new Vector2(684, 18), ButtonType.GodPower, tab.transform, null);

    PowerButtons.CreateButton("TankFactory_toggle", Resources.Load<Sprite>("ui/Icons/Tank"), "Tank Factories", "Toggles if factories make Tanks", new Vector2(684, -18), ButtonType.Toggle, tab.transform, Commerce.toggleTankFactory);

    if (Main.savedSettings.boolOptions["TankOption"]) {
      PowerButtons.ToggleButton("TankFactory_toggle");
      Commerce.toggleTankFactory();
    }

    PowerButtons.CreateButton("spawn_mirv_bomber", Resources.Load<Sprite>("ui/Icons/MIRVBomber"), "Spawn MIRV Bomber", "Spawn a MIRV Bomber.", new Vector2(720, 18), ButtonType.GodPower, tab.transform, null);

    PowerButtons.CreateButton("AirFactory_toggle", Resources.Load<Sprite>("ui/Icons/MIRVBomber"), "MIRV Bomber Factories", "Toggles if factories make MIRV Bombers", new Vector2(720, -18), ButtonType.Toggle, tab.transform, Commerce.toggleAirFactory);

    if (Main.savedSettings.boolOptions["MIRVBomberOption"]) {
      PowerButtons.ToggleButton("AirFactory_toggle");
      Commerce.toggleAirFactory();
    }

    PowerButtons.CreateButton("spawn_humvee", Resources.Load<Sprite>("ui/Icons/Humvee"), "Spawn Humvee", "Spawn a Humvee.", new Vector2(756, 18), ButtonType.GodPower, tab.transform, null);

    PowerButtons.CreateButton("HumveeFactory_toggle", Resources.Load<Sprite>("ui/Icons/Humvee"), "Humvee Factories", "Toggles if factories make Humvees", new Vector2(756, -18), ButtonType.Toggle, tab.transform, Commerce.toggleHumveeFactory);

    if (Main.savedSettings.boolOptions["HumveeOption"]) {
      PowerButtons.ToggleButton("HumveeFactory_toggle");
      Commerce.toggleHumveeFactory();
    }

    PowerButtons.CreateButton("spawn_F22", Resources.Load<Sprite>("ui/Icons/F22"), "Spawn F22", "Spawn a F22.", new Vector2(792, 18), ButtonType.GodPower, tab.transform, null);

    PowerButtons.CreateButton("F22Factory_toggle", Resources.Load<Sprite>("ui/Icons/F22"), "F22 Factories", "Toggles if factories make F22s", new Vector2(792, -18), ButtonType.Toggle, tab.transform, Commerce.toggleF22Factory);

    if (Main.savedSettings.boolOptions["F22Option"]) {
      PowerButtons.ToggleButton("F22Factory_toggle");
      Commerce.toggleF22Factory();
    }

    PowerButtons.CreateButton("spawn_MissileSystem", Resources.Load<Sprite>("actors/MissileSystem/swim_1"), "Spawn Artillery", "Spawn a Missile System.", new Vector2(828, 18), ButtonType.GodPower, tab.transform, null);

    PowerButtons.CreateButton("BoiFactory_toggle", Resources.Load<Sprite>("actors/MissileSystem/swim_1"), "Missile System Factories", "Toggles if factories make Missile Systems", new Vector2(828, -18), ButtonType.Toggle, tab.transform, Commerce.toggleBoiFactory);

    if (Main.savedSettings.boolOptions["BoiOption"]) {
      PowerButtons.ToggleButton("BoiFactory_toggle");
      Commerce.toggleBoiFactory();
    }

    PowerButtons.CreateButton("gunship_spawn", Resources.Load<Sprite>("ui/Icons/Gunship"), "Gunship Spawn", "Spawn a Gunship", new Vector2(864, 18), ButtonType.GodPower, tab.transform, null);

    PowerButtons.CreateButton("GunshipFactory_toggle", Resources.Load<Sprite>("ui/Icons/Gunship"), "Gunship Factories", "Toggles if factories make Gunships", new Vector2(864, -18), ButtonType.Toggle, tab.transform, Commerce.toggleGunshipFactory);

    if (Main.savedSettings.boolOptions["GunshipOption"]) {
      PowerButtons.ToggleButton("GunshipFactory_toggle");
      Commerce.toggleGunshipFactory();
    }

    PowerButtons.CreateButton("PipeGun_toggle", Resources.Load<Sprite>("ui/Icons/lowfirearm"), "Toggle Pipe Guns.", "Toggle Pipe Guns from being developed (this won't remove existing Pipe Guns already made). You're welcome LonelyFear.", new Vector2(1008, 18), // ok
                              ButtonType.Toggle, tab.transform, guns.togglePipeGuns);
    if (Main.savedSettings.boolOptions["PipeGunOption"]) {
      PowerButtons.ToggleButton("PipeGun_toggle");
      guns.togglePipeGuns();
    }

    PowerButtons.CreateButton("Gun_toggle", Resources.Load<Sprite>("ui/Icons/firearm"), "Toggle Guns.", "Toggle Guns from being developed (this won't remove existing Guns already made).", new Vector2(1008, -18), ButtonType.Toggle, tab.transform, guns.toggleGuns);
    if (Main.savedSettings.boolOptions["GunOption"]) {
      PowerButtons.ToggleButton("Gun_toggle");
      guns.toggleGuns();
    }

    // PowerButtons.CreateButton(
    // "Minimapfigure_toggle",
    // Resources.Load<Sprite>("jet/icons/minimap_figure_tank"),
    // "Draw vehicle minimap icons",
    // "Draw minimap icons for vehicles.",
    // new Vector2(720, -18),
    // ButtonType.Toggle,
    // tab.transform,
    // Vehicles.toggleMiniMapFigure
    // );
    // if (Main.savedSettings.boolOptions["MinimapFigures"])
    // {
    // PowerButtons.ToggleButton("Minimapfigure_toggle");
    // Vehicles.toggleMiniMapFigure();
    // }

    PowerButtons.CreateButton("Devmode", Resources.Load<Sprite>("ui/Icons/tabIconModernWarfare"), "Toggle Developer Mode", "Secret stuff", new Vector2(1080, 18), ButtonType.Toggle, tab.transform, DeveloperMode.toggleDevMode);
    if (Main.savedSettings.boolOptions["Developer_Mode"]) {
      PowerButtons.ToggleButton("Devmode");
      DeveloperMode.toggleDevMode();
    }

    PowerButtons.CreateButton("discord_server", Resources.Load<Sprite>("ui/Icons/DiscordServer"), "Discord Server", "Click this to join the ModernBox Discord server!", new Vector2(72, -18), ButtonType.Click, tab.transform, OpenDiscordServerLink);
    PowerButtons.CreateButton("about", Resources.Load<Sprite>("ui/icons/iconabout"), "About", "Learn about ModernBox.", new Vector2(72, 18), ButtonType.Click, tab.transform, OpenInfoWindow);
    PowerButtons.CreateButton("Soldier_toggle", Resources.Load<Sprite>("actors/Soldier/walk_0"), "Modern Militaries", "Toggles if modern soldiers can be enlisted.", new Vector2(216, -18), ButtonType.Toggle, tab.transform, Commerce.toggleBarracks);

    if (Main.savedSettings.boolOptions["SoldierOption"]) {
      PowerButtons.ToggleButton("Soldier_toggle");
      Commerce.toggleBarracks();
    }
	
    PowerButtons.CreateButton("Nuke_toggle", Resources.Load<Sprite>("effects/projectiles/NUKER/0"), "Toggle Nuke Silos", "Toggles if kingdoms can nuke each other.", new Vector2(360, -18), ButtonType.Toggle, tab.transform, Commerce.toggleNukes);
	
    if (Main.savedSettings.boolOptions["NukeOption"]) {
      PowerButtons.ToggleButton("Nuke_toggle");
      Commerce.toggleNukes();
    }
            PowerButtons.CreateButton("spawn_Railgun",Resources.Load<Sprite>("ui/Icons/Railgun"),"Spawn Railgun","Spawn a Railgun",new Vector2(900, 18),ButtonType.GodPower,tab.transform,null );
            PowerButtons.CreateButton("RailgunFactory_toggle", Resources.Load<Sprite>("ui/Icons/Railgun"), "Railgun Factories", "Toggles if factories make Railguns", new Vector2(900, -18), ButtonType.Toggle, tab.transform, Commerce.toggleRailgunFactory);

            if (Main.savedSettings.boolOptions["RailgunOption"]) { 
                PowerButtons.ToggleButton("RailgunFactory_toggle");
                Commerce.toggleRailgunFactory();
            }
    PowerButtons.CreateButton("credits", Resources.Load<Sprite>("ui/icons/iconabout"), "Credits", "All the people behind ModernBox and more!", new Vector2(108, -18), ButtonType.Click, tab.transform, OpenCreditsWindow);
    PowerButtons.CreateButton("ResetSettings", Resources.Load<Sprite>("ui/icons/Reset"), "Reset to defaults", "Resets ALL saved settings to their default values.", new Vector2(108, 18), ButtonType.Click, tab.transform, Main.resetToDefaults);
    PowerButtons.CreateButton("Ideologies_toggle", Resources.Load<Sprite>("ui/Icons/Ideologies"), "Toggle Ideologies", "bruh", new Vector2(972, 18), // 900 + 36
                              ButtonType.Toggle, tab.transform, Ideologies.toggleIdeologies);
    if (Main.savedSettings.boolOptions["IdeologiesOption"]) {
      PowerButtons.ToggleButton("Ideologies_toggle");
      Ideologies.toggleIdeologies();
    }
    PowerButtons.CreateButton("other_names_toggle", Resources.Load<Sprite>("ui/Icons/tabIconModernWarfare"), "Modern Names for Other Races.", "Enable or Disable First and Last names for other races..", new Vector2(144, 18), ButtonType.Toggle, tab.transform, Name.toggleOtherNames);
    if (Main.savedSettings.boolOptions["othernamesOption"]) {
      PowerButtons.ToggleButton("other_names_toggle");
      Name.toggleOtherNames();
    }

    PowerButtons.CreateButton("names_toggle", Resources.Load<Sprite>("ui/Icons/name_1"), "Modern Names", "Enable or Disable Modern Names.", new Vector2(144, -18), ButtonType.Toggle, tab.transform, Name.toggleNames);
    if (Main.savedSettings.boolOptions["namesOption"]) {
      PowerButtons.ToggleButton("names_toggle");
      Name.toggleNames();
    }
    // index++;
  }

  public static bool Stuff_Drop(WorldTile pTile, GodPower pPower) {
    AssetManager.powers.CallMethod("spawnDrops", pTile, pPower);
    return true;
  }

  private static void OpenDiscordServerLink() {

    string discordServerLink = "https://discord.gg/HEBNQpbCJf";

    Windows.ShowWindow("DiscordWindow");

    Application.OpenURL(discordServerLink);
  }

  private static void OpenInfoWindow() { Windows.ShowWindow("InfoWindow"); }

  private static void OpenMajorUpdateWindow() { Windows.ShowWindow("MajorUpdateWindow"); }
  private static void OpenCreditsWindow() { Windows.ShowWindow("CreditsWindow"); }

  // public static void action_worker(WorldTile pTile = null, string pPowerID = null)
  // {

  // MapBox.instance.getObjectsInChunks(pTile, 3, MapObjectType.Actor);
  // List<BaseSimObject> temp_objs = World.world.temp_map_objects;
  // for (int i = 0; i < temp_objs.Count; i++)
  // {
  // Actor pActor = (Actor)temp_objs[i];

  // if (!pActor.asset.unit || pActor.city == null || pActor.asset.baby)
  // {
  // continue;
  // }
  // pActor.CallMethod("setProfession", UnitProfession.Unit, true);
  // var pAI = (AiSystemActor)Reflection.GetField(typeof(Actor), pActor, "ai");
  // pAI.setJob("joboy");
  // }
  // }

  public static bool action_spawn_jet(WorldTile pTile, string pPowerID) {
    if (pTile.zone.city == null) {
      WorldTip.showNow("You must spawn this vehicle within a kingdom.", true, "top", 3f);
      return false;
    }

    City pCity = pTile.zone.city;

    Actor actor = spawnUnit(pTile, pPowerID);
    if (actor == null) {
      return false;
    }

    actor.becomeCitizen(pCity);

    actor.setKingdom(pCity.kingdom);
    actor.setCity(pCity);

    return true;
  }

  public static Actor spawnUnit(WorldTile pTile, string pPowerID) {
    GodPower godPower = AssetManager.powers.get(pPowerID);
    MusicBox.playSound("event:/SFX/UNIQUE/SpawnWhoosh", (float) pTile.pos.x, (float) pTile.pos.y, false, false);
    if (godPower.id == SA.sheep && pTile.Type.lava) {
      AchievementLibrary.achievementSacrifice.check(null, null, null);
    }
    EffectsLibrary.spawn("fx_spawn", pTile, null, null, 0f, -1f, -1f);
    string text;
    if (godPower.actor_asset_ids.Count > 0) {
      text = godPower.actor_asset_ids.GetRandom<string>();
    } else {
      text = godPower.actor_asset_id;
    }
    Actor actor = World.world.units.spawnNewUnit(text, pTile, true, godPower.actorSpawnHeight);
    actor.addTrait("miracle_born", false);
    actor.data.age_overgrowth = 1;
    actor.data.had_child_timeout = 8.0;
    return actor;
  }

  public static void action_MOABClick(WorldTile pTile, string pPowerID) {
    EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 0.4f, 0.6f);
    MapAction.damageWorld(pTile, 50, TerraformLibrary.czarBomba, null);
    World.world.startShake(0.3f, 0.01f, 2f, true, true);
    // return true;
  }

  public static void action_CobaltClick(WorldTile pTile, string pPowerID) {
    EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 0.2f, 0.3f);
    MapAction.damageWorld(pTile, 120, TerraformLibrary.czarBomba, null);
    World.world.startShake(0.3f, 0.01f, 2f, true, true);
    // return true;
  }

  public static void action_UltronClick(WorldTile pTile, string pPowerID) {
    EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 0.8f, 0.9f);
    MapAction.damageWorld(pTile, 100, TerraformLibrary.czarBomba, null);
    World.world.startShake(0.3f, 0.01f, 2f, true, true);
    // return true;
  }

  public static void action_DeathClick(WorldTile pTile, string pPowerID) {
    EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 1.2f, 1.6f);
    MapAction.damageWorld(pTile, 100, TerraformLibrary.czarBomba, null);
    World.world.startShake(0.3f, 0.01f, 2f, true, true);
    // return true;
  }

  public static void action_XeniumClick(WorldTile pTile, string pPowerID) {
    EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 4.3f, 7.9f);
    MapAction.damageWorld(pTile, 400, TerraformLibrary.czarBomba, null);
    World.world.startShake(0.3f, 0.01f, 2f, true, true);
    // return true;
  }

  public static void action_MiniClick(WorldTile pTile, string pPowerID) {
    EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 0.4f, 0.6f);
    MapAction.damageWorld(pTile, 5, TerraformLibrary.czarBomba, null);
    World.world.startShake(0.3f, 0.01f, 2f, true, true);
    // return true;
  }

  public static void action_ProtonClick(WorldTile pTile, string pPowerID) {
    EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 16.3f, 28.9f);
    MapAction.damageWorld(pTile, 786, TerraformLibrary.czarBomba, null);
    World.world.startShake(0.3f, 0.01f, 2f, true, true);
    // return true;
  }
  
  public static void action_JupiterClick(WorldTile pTile, string pPowerID) {
    EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, 32.3f, 56.9f);
    MapAction.damageWorld(pTile, 1486, TerraformLibrary.czarBomba, null);
    World.world.startShake(0.3f, 0.01f, 2f, true, true);
    // return true;
  }
        public static void action_EraserClick(WorldTile pTile, string pPowerID)
        {
            EffectsLibrary.spawnAtTileRandomScale("fx_antimatter_effect", pTile, 5.3f, 9.9f);
            MapAction.damageWorld(pTile, 1000, TerraformLibrary.destroy_no_flash, null);
            World.world.startShake(0.3f, 0.01f, 2f, true, true);
            // return true;
        }
  public static void action_RandomClick(WorldTile pTile, string pPowerID) {
    List<int> damageWorldNumbers = new List<int>{50, 120, 100, 100, 400, 5, 786};
    List<float> scaleNumbersMin = new List<float>{0.2f, 0.4f, 0.8f, 1.2f, 4.3f, 0.4f, 16.3f};
    List<float> scaleNumbersMax = new List<float>{0.3f, 0.6f, 0.9f, 1.6f, 7.9f, 0.6f, 28.9f};

    System.Random random = new System.Random();
    int randomIndex = random.Next(damageWorldNumbers.Count);

    int damageNumber = damageWorldNumbers[randomIndex];
    float scaleMin = scaleNumbersMin[randomIndex];
    float scaleMax = scaleNumbersMax[randomIndex];
    float randomScale = (float) random.NextDouble() * (scaleMax - scaleMin) + scaleMin;

    EffectsLibrary.spawnAtTileRandomScale("fx_explosion_huge", pTile, randomScale, randomScale);
    MapAction.damageWorld(pTile, damageNumber, TerraformLibrary.czarBomba, null);
    World.world.startShake(0.3f, 0.01f, 2f, true, true);
  }

  private static PowersTab getPowersTab(string id) {
    GameObject gameObject = GameObjects.FindEvenInactive("Tab_" + id);
    return gameObject.GetComponent<PowersTab>();
  }
}
}