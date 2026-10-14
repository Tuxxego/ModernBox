using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Globalization;
using UnityEngine;

public class StarConverter : JsonConverter<Star>
{
    // Manu-Fix 032: serialize only star data, not Unity's recursive object graph.
    // Read both previous position layouts as well as the explicit x/y/z schema.
    public override void WriteJson(JsonWriter writer, Star value, JsonSerializer serializer)
    {
        JObject obj = new JObject
        {
            { "name", value.name },
            { "starType", value.starType },
            { "planetCount", value.planetCount },
            { "selectedPlanet", value.selectedPlanet },
            { "x", value.transform.position.x },
            { "y", value.transform.position.y },
            { "z", value.transform.position.z },
            { "planetInfo", JArray.FromObject(value.planetInfo ?? new PlanetInfo[0], serializer) }
        };
        obj.WriteTo(writer);
    }

    public override Star ReadJson(JsonReader reader, Type objectType, Star existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);
        string name = RequiredText(obj, "name");
        string starType = RequiredText(obj, "starType");
        JArray planetData = obj["planetInfo"] as JArray;
        if (planetData == null || planetData.Count == 0)
            throw new JsonSerializationException("Star requires a non-empty planetInfo array.");
        foreach (JToken token in planetData)
        {
            JObject planet = token as JObject;
            if (planet == null) throw new JsonSerializationException("planetInfo contains a non-object entry.");
            string planetName = RequiredText(planet, "name");
            if (planetName == "." || planetName == ".." || planetName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new JsonSerializationException("Planet name is not a valid folder name.");
            RequiredText(planet, "planetType");
            string[] size = RequiredText(planet, "size").Split('x');
            if (size.Length != 2 || !int.TryParse(size[0].Trim(), out int width) || width <= 0 ||
                !int.TryParse(size[1].Trim(), out int height) || height <= 0)
                throw new JsonSerializationException("Planet requires a positive 'width x height' size.");
            if (planet["hasFauna"]?.Type != JTokenType.Boolean)
                throw new JsonSerializationException("Planet requires a boolean hasFauna value.");
        }
        PlanetInfo[] planets = planetData.ToObject<PlanetInfo[]>(serializer);
        JObject position = (obj["gameObject"] as JObject)?["position"] as JObject
            ?? (obj["transform"] as JObject)?["position"] as JObject
            ?? obj;
        Vector3 coordinates = new Vector3(RequiredCoordinate(position, "x"),
            RequiredCoordinate(position, "y"), RequiredCoordinate(position, "z"));
        int selectedPlanet = 0;
        JToken selection = obj["selectedPlanet"];
        if (selection != null && selection.Type != JTokenType.Null &&
            (selection.Type != JTokenType.Integer || !int.TryParse(selection.ToString(), out selectedPlanet)))
            throw new JsonSerializationException("Invalid selectedPlanet index.");
        selectedPlanet = Mathf.Clamp(selectedPlanet, 0, Math.Max(0, planets.Length - 1));

        // Allocate only after parsing succeeds, so invalid data leaves no scene object.
        GameObject starObject = new GameObject("Star");
        Star star = starObject.AddComponent<Star>();
        star.name = name;
        star.starType = starType;
        // The list is the actual data source; stale redundant counts must never
        // drive an out-of-range read. No persisted file is rewritten on load.
        star.planetCount = planets.Length;
        star.planetInfo = planets;
        star.selectedPlanet = selectedPlanet;
        star.transform.position = coordinates;
        return star;
    }

    private static string RequiredText(JObject obj, string key)
    {
        if (obj[key]?.Type != JTokenType.String || string.IsNullOrWhiteSpace((string)obj[key]))
            throw new JsonSerializationException("Missing or invalid required text: " + key);
        return (string)obj[key];
    }

    private static float RequiredCoordinate(JObject obj, string key)
    {
        JToken token = obj[key];
        if (token == null || (token.Type != JTokenType.Float && token.Type != JTokenType.Integer))
            throw new JsonSerializationException("Missing or invalid star coordinate: " + key);
        if (!float.TryParse(token.ToString(Formatting.None), NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ||
            float.IsNaN(value) || float.IsInfinity(value))
            throw new JsonSerializationException("Star coordinate is not finite: " + key);
        return value;
    }
}
