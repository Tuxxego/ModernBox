using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TuxModLoader.Reflection;
using System.Reflection;
using ai;
using ai.behaviours;

public class UnitTracker : MonoBehaviour
{
    public static UnitTracker Instance { get; private set; }

    [System.Serializable]
    public class TrackedUnit
    {
        public string id;
        public Sprite sprite;

        public TrackedUnit(string id, Sprite sprite)
        {
            this.id = id;
            this.sprite = sprite;
        }
    }

    public List<TrackedUnit> units = new List<TrackedUnit>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public void RegisterUnit(string id)
    {
        Sprite sprite = LoadSpriteForUnit(id);
        if (sprite == null)
        {
            ModernBoxLogger.Warning($"Could not find a sprite for unit '{id}'. Registration skipped.");
            return;
        }

        GodPower newPower = AssetManager.powers.clone($"spawn_{id}", "$template_spawn_actor$");
        newPower.name = $"spawn_{id}";
        newPower.actor_asset_id = id;
        newPower.click_action = new PowerActionWithID(SpawnVehicle);

        AssetManager.powers.add(newPower);
        units.Add(new TrackedUnit(id, sprite));
    //    ModernBoxLogger.Log($"Registered unit: {id}");
    }

    private Sprite LoadSpriteForUnit(string id)
    {
        string[] spriteNames = { "walk_0", "idle_0", "swim_0" };
        foreach (string name in spriteNames)
        {
            Sprite found = Resources.Load<Sprite>($"Actors/{id}/main/{name}");
            if (found != null)
                return found;
        }
        return null;
    }

  public static bool SpawnVehicle(WorldTile pTile, string pPowerID) {
    if (pTile.zone.city == null) {
      WorldTip.showNow("You must spawn this vehicle within a kingdom.", true, "top", 3f);
      return false;
    }

    City pCity = pTile.zone.city;

    Actor baseVehicle = World.world.units.createNewUnit(
        "baseWarUnit",
        pTile,
        pMiracleSpawn: false,
        0f,
        null,
        null,
        pSpawnWithItems: false
    );

    if (baseVehicle == null)
        return false;

    baseVehicle.makeWait(1f);

    if (pCity != null)
    {
        object kingdom = null;
        var cityType = typeof(City);
        var kingdomField = cityType.GetField("kingdom", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var kingdomProperty = cityType.GetProperty("kingdom", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (kingdomField != null)
        {
            kingdom = kingdomField.GetValue(pCity);
        }
        else if (kingdomProperty != null)
        {
            kingdom = kingdomProperty.GetValue(pCity);
        }

        baseVehicle.setCity(pCity);
        if (kingdom is Kingdom k)
        {
            baseVehicle.setKingdom(k);
        }
        else
        {
            ModernBoxLogger.Warning("[SpawnVehicle] Could not retrieve Kingdom from City.");
        }
    }

			if (AssetManager.powers == null)
			{
				ModernBoxLogger.Error("[spawnUnit] AssetManager.powers is null!");
				return false;
			}

			GodPower godPower = AssetManager.powers.get(pPowerID);
			if (godPower == null)
			{
				ModernBoxLogger.Error($"[spawnUnit] GodPower with ID '{pPowerID}' not found!");
				return false;
			}

			ModernBoxLogger.Log($"[spawnUnit] Got GodPower: {godPower.id}");

			string text;
			if (godPower.actor_asset_ids != null && godPower.actor_asset_ids.Length > 0)
			{
				text = godPower.actor_asset_ids.GetRandom<string>();
				ModernBoxLogger.Log($"[spawnUnit] Selected random actor from actor_asset_ids: {text}");
			}
			else
			{
				text = godPower.actor_asset_id;
				ModernBoxLogger.Log($"[spawnUnit] Using fallback actor_asset_id: {text}");
			}
            
    TransformUnit(baseVehicle, text, pTile);
    return true;

    return true;
  }

private static void TransformUnit(Actor originalActor, string newActorId, WorldTile pTile)
{
    Actor newActor = World.world.units.createNewUnit(newActorId, pTile);
    if (newActor == null)
    {
        return;
    }
    ActorTool.copyUnitToOtherUnit(originalActor, newActor);
    if (originalActor.kingdom != null)
    {
        newActor.setKingdom(originalActor.kingdom);
    }
   newActor.setCity(originalActor.city);
    ActionLibrary.removeUnit(originalActor);
}


        public static bool callSpawnUnit(WorldTile pTile, string pPowerID)
        {
            ReflectionHelper.InvokeMethod(AssetManager.powers, "spawnUnit", pTile, pPowerID);
            return true;
        }

		public static Actor spawnUnit(WorldTile pTile, string pPowerID)
		{
			ModernBoxLogger.Log($"[spawnUnit] Called with pTile: {pTile}, pPowerID: {pPowerID}");

			if (AssetManager.powers == null)
			{
				ModernBoxLogger.Error("[spawnUnit] AssetManager.powers is null!");
				return null;
			}

			GodPower godPower = AssetManager.powers.get(pPowerID);
			if (godPower == null)
			{
				ModernBoxLogger.Error($"[spawnUnit] GodPower with ID '{pPowerID}' not found!");
				return null;
			}

			ModernBoxLogger.Log($"[spawnUnit] Got GodPower: {godPower.id}");

			MusicBox.playSound("event:/SFX/UNIQUE/SpawnWhoosh", (float)pTile.pos.x, (float)pTile.pos.y, false, false);
			ModernBoxLogger.Log($"[spawnUnit] Played sound at ({pTile.pos.x}, {pTile.pos.y})");

			EffectsLibrary.spawn("fx_spawn", pTile, null, null, 0f, -1f, -1f);
			ModernBoxLogger.Log("[spawnUnit] Spawned 'fx_spawn' effect.");

			string text;
			if (godPower.actor_asset_ids != null && godPower.actor_asset_ids.Length > 0)
			{
				text = godPower.actor_asset_ids.GetRandom<string>();
				ModernBoxLogger.Log($"[spawnUnit] Selected random actor from actor_asset_ids: {text}");
			}
			else
			{
				text = godPower.actor_asset_id;
				ModernBoxLogger.Log($"[spawnUnit] Using fallback actor_asset_id: {text}");
			}

			if (World.world == null || World.world.units == null)
			{
				ModernBoxLogger.Error("[spawnUnit] World or World.units is null!");
				return null;
			}

			Actor actor = World.world.units.spawnNewUnit(text, pTile, true);
			if (actor == null)
			{
				ModernBoxLogger.Error("[spawnUnit] spawnNewUnit returned null!");
				return null;
			}

			actor.addTrait("miracle_born", false);
			ModernBoxLogger.Log($"[spawnUnit] Spawned actor '{text}' at tile {pTile.pos}. Added trait 'miracle_born'.");

			return actor;
		}
}
