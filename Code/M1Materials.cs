using System.Collections.Generic;

namespace ModernBoxM1Rewrite
{
    // In the old game an item's cost, required tech and part of its stats came from its
    // material. M1 made every gun and MIRV iron and the cyberware and drugs steel. Those
    // rings were made through the weapon path, so they paid the steel weapon price, but
    // their stats came from the steel accessory material. A unit's built-in attack used
    // the bare item, so the material stats are kept apart.
    internal static class M1Materials
    {
        internal static void Apply(EquipmentSpec spec)
        {
            if (spec.Material == "steel")
            {
                SetCost(spec, 4, 10, "material_steel");
                spec.Value += 40;
                Add(spec, "critical_chance", 0.07f);
                return;
            }
            SetCost(spec, 3, 10, "material_iron");
            spec.Value += 30;
            Add(spec, "damage", 6f);
            Add(spec, "attack_speed", 10f);
            Add(spec, "critical_chance", 0.05f);
        }

        // The material tech each item also needed, on top of its own tech.
        internal static readonly Dictionary<string, string> MaterialTechs = new Dictionary<string, string>();

        private static void SetCost(EquipmentSpec spec, int metals, int minimumStorage, string tech)
        {
            spec.Resource1 = "common_metals";
            spec.Resource1Cost = metals;
            spec.Resource2 = "none";
            spec.Resource2Cost = 0;
            spec.MinimumStorage = minimumStorage;
            MaterialTechs[spec.Id] = tech;
        }

        private static void Add(EquipmentSpec spec, string key, float value)
        {
            spec.MaterialStats[key] = value;
        }
    }
}
