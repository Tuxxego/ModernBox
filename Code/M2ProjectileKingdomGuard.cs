using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    // A projectile fired by something with no kingdom keeps a null kingdom, and
    // ProjectileManager.updateProjectiles then throws on it every frame. Every projectile
    // after it in the list stops updating and hangs in the air. Give it a kingdom instead.
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.start))]
    internal static class M2ProjectileKingdomGuard
    {
        private static readonly HashSet<string> Logged = new HashSet<string>();

        [HarmonyPostfix]
        private static void Postfix(Projectile __instance)
        {
            if (__instance != null && __instance.kingdom == null) Repair(__instance);
        }

        internal static void Repair(Projectile projectile)
        {
            BaseSimObject owner = projectile.by_who;
            Kingdom kingdom = owner == null ? null : owner.kingdom;
            if (kingdom == null && owner != null && owner.isBuilding())
            {
                M2BuildingKingdomRepair.RepairOne(owner.b);
                kingdom = owner.kingdom;
            }
            if (kingdom == null && World.world != null && World.world.kingdoms_wild != null)
                kingdom = World.world.kingdoms_wild.get("nature");
            if (kingdom == null) return;
            projectile.kingdom = kingdom;
            string key = (projectile.asset == null ? "?" : projectile.asset.id) + "/" + OwnerId(owner);
            if (Logged.Add(key)) Debug.Log("[ModernBox] projectile with no kingdom: " + key + " -> " + kingdom.id);
        }

        private static string OwnerId(BaseSimObject owner)
        {
            if (owner == null) return "none";
            if (owner.isActor()) return owner.a == null || owner.a.asset == null ? "actor" : owner.a.asset.id;
            if (owner.isBuilding()) return owner.b == null || owner.b.asset == null ? "building" : owner.b.asset.id;
            return "?";
        }
    }

    [HarmonyPatch(typeof(ProjectileManager), "updateProjectiles")]
    internal static class M2ProjectileKingdomGuardUpdate
    {
        [HarmonyPrefix]
        private static void Prefix(ProjectileManager __instance)
        {
            List<Projectile> list = __instance.list;
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null && list[i].kingdom == null) M2ProjectileKingdomGuard.Repair(list[i]);
        }
    }
}
