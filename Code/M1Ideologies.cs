using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace ModernBoxM1Rewrite
{
    // How the old game handed out the ideology traits (birth 37, inherit 100):
    // a new unit of a civ race rolled each trait in list order with a 37% chance, skipping
    // any that clashed with one it already had. A baby then got each parent's ideology
    // (100%), unless it clashed. Since every ideology rules out the others, a unit ends up
    // with one, and the earlier ones in the list are the most common.
    // 0.51.2 turns these rates into weights in a shared pool, so the mod rolls them itself.
    // The toggle only changed the birth chance; inheritance always stayed on.
    internal static class M1Ideologies
    {
        private const float BirthChance = 0.37f;
        private static readonly ConditionalWeakTable<Actor, Actor[]> Parents = new ConditionalWeakTable<Actor, Actor[]>();

        internal static void RememberParents(Actor baby, Actor parent1, Actor parent2)
        {
            if (baby == null) return;
            Parents.Remove(baby);
            Parents.Add(baby, new[] { parent1, parent2 });
        }

        internal static void OnBirth(Actor actor)
        {
            if (actor == null || actor.asset == null || !EquipmentAndTraitsRegistry.SapientSpeciesIds.Contains(actor.asset.id)) return;
            if (M1Toggles.IdeologyBirth)
                foreach (string id in EquipmentAndTraitsRegistry.IdeologyIds)
                    if (Randy.randomChance(BirthChance) && !HasIdeology(actor)) actor.addTrait(id);
            Actor[] parents;
            if (!Parents.TryGetValue(actor, out parents)) return;
            Parents.Remove(actor);
            foreach (Actor parent in parents)
            {
                if (parent == null || parent.isRekt()) continue;
                foreach (string id in EquipmentAndTraitsRegistry.IdeologyIds)
                    if (parent.hasTrait(id) && !HasIdeology(actor)) actor.addTrait(id);
            }
        }

        private static bool HasIdeology(Actor actor)
        {
            foreach (string id in EquipmentAndTraitsRegistry.IdeologyIds)
                if (actor.hasTrait(id)) return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(BabyHelper), nameof(BabyHelper.traitsInherit))]
    internal static class M1IdeologyParentsPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Actor pActorTarget, Actor pParent1, Actor pParent2)
        {
            M1Ideologies.RememberParents(pActorTarget, pParent1, pParent2);
        }
    }

    // Runs after the parents' traits are copied and after the spawn traits are rolled.
    [HarmonyPatch(typeof(Actor), nameof(Actor.checkTraitMutationOnBirth))]
    internal static class M1IdeologyBirthPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Actor __instance)
        {
            M1Ideologies.OnBirth(__instance);
        }
    }

    // City.produceItem picked a random entry of the race's preferred weapons and made it
    // only if the culture had the item's tech and its material's tech. 0.51.2 crafts
    // differently, so M1's items are put at the end of the list (tried first), but only
    // the ones the race's toggles added and the culture has the techs for. Cyberware and
    // drugs went through the weapon list too, but they are rings, so they are offered
    // when a ring is crafted.
    [HarmonyPatch(typeof(ItemCrafting), nameof(ItemCrafting.getItemAssetToCraft))]
    internal static class M1CraftingPatch
    {
        [HarmonyPrefix]
        private static void Prefix(ref List<EquipmentAsset> pItemList, City pCity)
        {
            if (pItemList == null) return;
            List<EquipmentAsset> items = new List<EquipmentAsset>(pItemList.Count);
            EquipmentType type = EquipmentType.Weapon;
            bool typed = false;
            foreach (EquipmentAsset item in pItemList)
            {
                if (item == null) continue;
                if (!typed) { type = item.equipment_type; typed = true; }
                if (!ModernBoxCatalog.EquipmentIds.Contains(item.id)) items.Add(item);
            }
            if (typed && ModernBoxCatalog.IsSupportedCity(pCity))
            {
                string race = ModernBoxCatalog.GetRace(pCity);
                List<EquipmentAsset> modern = new List<EquipmentAsset>();
                foreach (EquipmentSpec spec in ContentRegistry.Equipment)
                {
                    if (spec.Type != type || !CanMake(spec, race, pCity)) continue;
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
            pItemList = items;
        }

        private static bool CanMake(EquipmentSpec spec, string race, City city)
        {
            string materialTech;
            M1Materials.MaterialTechs.TryGetValue(spec.Id, out materialTech);
            return M1Toggles.IsPreferred(race, spec.Id) && M1Tech.Has(city, spec.Tech) && M1Tech.Has(city, materialTech);
        }
    }
}
