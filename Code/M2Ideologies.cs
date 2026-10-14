using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ModernBoxM2Rewrite
{
    /// Original M2 ideology systems (Traits.cs): birth/inherit rates, kingdom
    /// opinion, city loyalty, king-to-kingdom spreading and ideology banners.
    internal static class M2Ideologies
    {
        private static readonly string[] CommonIdeologies = { "Peoplewoven", "Martial", "Mercantile", "Dynastic" };
        private static readonly string[] SupportedRaces = { "human", "orc", "elf", "dwarf" };
        private static bool _registered;

        // Original birth chance in percent (ActorTrait.birth).
        internal static int BirthRate(string id)
        {
            return id == "Chaosvolt" ? 8 : 10;
        }

        // Original inherit chance in percent per parent (ActorTrait.inherit).
        internal static float InheritChance(string id)
        {
            switch (id)
            {
                case "Dynastic": return 1f;
                case "Mercantile": return 0.5f;
                case "Peoplewoven":
                case "Martial": return 0.8f;
            }
            return 0f;
        }

        // 0.51.2 turns rate_birth/rate_inherit into weights in a shared pool, which hands
        // ideologies out far more often, so the mod rolls the original chances itself.
        internal static void ApplyRates(bool enabled)
        {
            foreach (string id in EquipmentAndTraitsRegistry.IdeologyIds)
            {
                ActorTrait trait = AssetManager.traits.get(id);
                if (trait == null) continue;
                trait.rate_birth = 0;
                trait.rate_inherit = 0;
            }
        }

        // ActorBase.inheritTraits: each parent's ideology passes on with its inherit chance.
        internal static void InheritFrom(Actor child, Actor parent)
        {
            if (child == null || parent == null || parent.isRekt()) return;
            foreach (string id in EquipmentAndTraitsRegistry.IdeologyIds)
            {
                float chance = InheritChance(id);
                if (chance <= 0f || !parent.hasTrait(id)) continue;
                if (Randy.randomFloat(0f, 100f) <= chance) child.addTrait(id);
            }
        }

        internal static void Register()
        {
            if (_registered) return;
            _registered = true;
            AssetManager.opinion_library.add(new OpinionAsset
            {
                id = "opinion_ideology",
                translation_key = "opinion_ideology",
                translation_key_negative = "opinion_ideology_negative",
                calc = KingdomOpinion
            });
            AssetManager.loyalty_library.add(new LoyaltyAsset
            {
                id = "opinion_leader_ideology",
                translation_key = "opinion_leader_ideology",
                translation_key_negative = "opinion_leader_ideology_negative",
                calc = LeaderLoyalty
            });
            ModernLocalization.Add("opinion_ideology", "Shared ideology");
            ModernLocalization.Add("opinion_ideology_negative", "Opposing ideology");
            ModernLocalization.Add("opinion_leader_ideology", "Leader shares the king's ideology");
            ModernLocalization.Add("opinion_leader_ideology_negative", "Leader opposes the king's ideology");
            RegisterBanners();
        }

        private static int KingdomOpinion(Kingdom main, Kingdom target)
        {
            int opinion = 0;
            if (!ModernBoxSettings.Get("IdeologiesOption") || main == null || target == null || main.king == null || target.king == null)
                return opinion;
            Actor a = main.king;
            Actor b = target.king;
            bool aPeople = a.hasTrait("Peoplewoven"), aMartial = a.hasTrait("Martial"), aMerc = a.hasTrait("Mercantile"), aDyn = a.hasTrait("Dynastic"), aChaos = a.hasTrait("Chaosvolt");
            bool bPeople = b.hasTrait("Peoplewoven"), bMartial = b.hasTrait("Martial"), bMerc = b.hasTrait("Mercantile"), bDyn = b.hasTrait("Dynastic"), bChaos = b.hasTrait("Chaosvolt");
            if ((aPeople && bMartial) || (aMartial && bPeople)) opinion -= 100;
            if ((aPeople && bDyn) || (aDyn && bPeople)) opinion -= 200;
            if ((aPeople || aMartial) && bMerc) opinion -= 50;
            if ((bPeople || bMartial) && aMerc) opinion -= 50;
            if ((aPeople && bPeople) || (aMartial && bMartial) || (aMerc && bMerc) || (aDyn && bDyn)) opinion += 200;
            if (aChaos) opinion -= 500;
            if (bChaos) opinion -= 500;
            if (aChaos && bChaos) opinion -= 600;
            return opinion;
        }

        private static int LeaderLoyalty(City city)
        {
            int opinion = 0;
            if (!ModernBoxSettings.Get("IdeologiesOption") || city == null || city.kingdom == null) return opinion;
            Actor leader = city.leader;
            Actor king = city.kingdom.king;
            if (leader == null || king == null) return opinion;
            bool lPeople = leader.hasTrait("Peoplewoven"), lMartial = leader.hasTrait("Martial"), lMerc = leader.hasTrait("Mercantile"), lDyn = leader.hasTrait("Dynastic"), lChaos = leader.hasTrait("Chaosvolt");
            bool kPeople = king.hasTrait("Peoplewoven"), kMartial = king.hasTrait("Martial"), kMerc = king.hasTrait("Mercantile"), kDyn = king.hasTrait("Dynastic"), kChaos = king.hasTrait("Chaosvolt");
            if ((lPeople && kMartial) || (lMartial && kPeople)) opinion -= 250;
            if ((lMartial && kMartial) || (lPeople && kPeople) || (lMerc && kMerc) || (lDyn && kDyn)) opinion += 200;
            if ((lPeople && kDyn) || (lDyn && kPeople)) opinion -= 250;
            if ((lPeople || lMartial) && kMerc) opinion -= 50;
            if ((kPeople || kMartial) && lMerc) opinion -= 50;
            if (lChaos) opinion -= 600;
            if (kChaos) opinion -= 600;
            if (lChaos && kChaos) opinion -= 800;
            return opinion;
        }

        internal static bool IsSupportedKingdom(Kingdom kingdom)
        {
            ActorAsset species = kingdom == null ? null : kingdom.getActorAsset();
            return species != null && System.Array.IndexOf(SupportedRaces, species.id) >= 0;
        }

        // KingdomBehCheckKing_IdeologyPatch from the original.
        internal static void SpreadKingIdeology(Kingdom kingdom)
        {
            if (!ModernBoxSettings.Get("IdeologiesOption") || !IsSupportedKingdom(kingdom)) return;
            Actor king = kingdom.king;
            if (king == null || !king.isAlive()) return;

            bool kingHasChaosvolt = king.hasTrait("Chaosvolt");
            bool kingHasIdeology = kingHasChaosvolt;
            foreach (string trait in CommonIdeologies)
                if (king.hasTrait(trait)) kingHasIdeology = true;
            if (!kingHasIdeology)
                king.addTrait(CommonIdeologies[Random.Range(0, CommonIdeologies.Length)]);

            string kingIdeology = null;
            foreach (string trait in CommonIdeologies)
            {
                if (!king.hasTrait(trait)) continue;
                kingIdeology = trait;
                break;
            }
            if (kingIdeology == null && kingHasChaosvolt) kingIdeology = "Chaosvolt";
            if (string.IsNullOrEmpty(kingIdeology)) return;

            foreach (Actor actor in kingdom.units.ToArray())
            {
                if (actor == null || actor == king || !actor.isAlive()) continue;
                bool actorHasIdeology = actor.hasTrait("Chaosvolt");
                foreach (string trait in CommonIdeologies)
                    if (actor.hasTrait(trait)) actorHasIdeology = true;
                if (kingHasChaosvolt)
                {
                    if (actor.hasTrait("Chaosvolt")) continue;
                    if (!actorHasIdeology || Randy.randomChance(0.4f)) actor.addTrait("Chaosvolt");
                }
                else if (!actorHasIdeology || (!actor.hasTrait(kingIdeology) && Randy.randomChance(0.4f)))
                {
                    actor.addTrait(kingIdeology);
                }
            }
        }

        // Banner sets bundled in GameResources/banners, e.g. communisthuman.
        private static void RegisterBanners()
        {
            foreach (string prefix in new[] { "communist", "fascist", "capitalist", "monarch", "anarch" })
            {
                foreach (string race in SupportedRaces)
                {
                    string id = prefix + race;
                    if (AssetManager.kingdom_banners_library.dict.ContainsKey(id)) continue;
                    BannerAsset banner = new BannerAsset
                    {
                        id = id,
                        backgrounds = SpritePaths("banners/" + id + "/background"),
                        icons = SpritePaths("banners/" + id + "/icon"),
                        frames = new List<string>()
                    };
                    if (banner.backgrounds.Count == 0 || banner.icons.Count == 0) continue;
                    AssetManager.kingdom_banners_library.add(banner);
                }
            }
        }

        private static List<string> SpritePaths(string folder)
        {
            List<string> result = new List<string>();
            Sprite[] sprites = SpriteTextureLoader.getSpriteList(folder);
            if (sprites == null) return result;
            foreach (Sprite sprite in sprites)
                if (sprite != null) result.Add(folder + "/" + sprite.name);
            result.Sort(System.StringComparer.Ordinal);
            return result;
        }

        internal static string GetDynamicBannerId(Kingdom kingdom)
        {
            if (!ModernBoxSettings.Get("IdeologiesOption") || !IsSupportedKingdom(kingdom) || kingdom.king == null) return null;
            string race = kingdom.getActorAsset().id;
            Actor king = kingdom.king;
            string prefix = null;
            if (king.hasTrait("Peoplewoven")) prefix = "communist";
            else if (king.hasTrait("Martial")) prefix = "fascist";
            else if (king.hasTrait("Mercantile")) prefix = "capitalist";
            else if (king.hasTrait("Dynastic")) prefix = "monarch";
            else if (king.hasTrait("Chaosvolt")) prefix = "anarch";
            if (prefix == null) return null;
            string id = prefix + race;
            return AssetManager.kingdom_banners_library.dict.ContainsKey(id) ? id : null;
        }
    }

    [HarmonyPatch(typeof(ai.behaviours.KingdomBehCheckKing), nameof(ai.behaviours.KingdomBehCheckKing.execute))]
    internal static class M2KingIdeologyPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Kingdom pKingdom)
        {
            M2Ideologies.SpreadKingIdeology(pKingdom);
        }
    }

    [HarmonyPatch(typeof(Kingdom), nameof(Kingdom.getElementIcon))]
    internal static class M2IdeologyBannerIconPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Kingdom __instance, ref Sprite __result)
        {
            string id = M2Ideologies.GetDynamicBannerId(__instance);
            if (id == null) return true;
            __result = AssetManager.kingdom_banners_library.getSpriteIcon(__instance.data.banner_icon_id, id);
            return __result == null;
        }
    }

    [HarmonyPatch(typeof(Kingdom), nameof(Kingdom.getElementBackground))]
    internal static class M2IdeologyBannerBackgroundPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Kingdom __instance, ref Sprite __result)
        {
            string id = M2Ideologies.GetDynamicBannerId(__instance);
            if (id == null) return true;
            __result = AssetManager.kingdom_banners_library.getSpriteBackground(__instance.data.banner_background_id, id);
            return __result == null;
        }
    }

    [HarmonyPatch(typeof(BabyHelper), nameof(BabyHelper.traitsInherit))]
    internal static class M2IdeologyInheritPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Actor pActorTarget, Actor pParent1, Actor pParent2)
        {
            if (!ModernBoxSettings.Get("IdeologiesOption") || pActorTarget == null || pActorTarget.asset == null ||
                !EquipmentAndTraitsRegistry.SapientSpeciesIds.Contains(pActorTarget.asset.id)) return;
            M2Ideologies.InheritFrom(pActorTarget, pParent1);
            M2Ideologies.InheritFrom(pActorTarget, pParent2);
        }
    }
}
