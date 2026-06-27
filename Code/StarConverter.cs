using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

public class StarConverter : JsonConverter<Star>
{
    public override void WriteJson(JsonWriter writer, Star value, JsonSerializer serializer)
    {
        JObject obj = new JObject
        {
            { "name", value.name },
            { "starType", value.starType },
            { "planetCount", value.planetCount },
            { "x", value.transform.position.x },
            { "y", value.transform.position.y },
            { "z", value.transform.position.z },
            { "planetInfo", JArray.FromObject(value.planetInfo, serializer) },
        };
        obj.WriteTo(writer);
    }

public override Star ReadJson(JsonReader reader, Type objectType, Star existingValue, bool hasExistingValue, JsonSerializer serializer)
{
    JObject obj;
    try
    {
        obj = JObject.Load(reader);
    }
    catch (Exception ex)
    {
        // Debug.LogError("Failed to load JObject from reader. Exception: " + ex.Message);
        throw;
    }

    if (obj == null)
    {
        // Debug.LogError("JObject is null.");
        throw new JsonSerializationException("Failed to parse JSON object.");
    }

    // Debug.Log("Parsed JObject: " + obj.ToString());

    string name = obj["name"]?.ToString();
    string starType = obj["starType"]?.ToString();
    int? planetCount = obj["planetCount"]?.ToObject<int>();

    if (string.IsNullOrEmpty(name))
    {
        // Debug.LogWarning("Star name is null or empty.");
    }

    if (string.IsNullOrEmpty(starType))
    {
        // Debug.LogWarning("Star type is null or empty.");
    }

    if (planetCount == null)
    {
        // Debug.LogWarning("Planet count is null.");
    }

    GameObject starObject = new GameObject(name ?? "DefaultName");
    Star star = starObject.AddComponent<Star>();

    star.name = name ?? "DefaultName";
    star.starType = starType ?? "Unknown";
    star.planetCount = planetCount ?? 0;

    JObject gameObjectObj = (JObject)obj["gameObject"];
    if (gameObjectObj != null)
    {
        JObject positionObj = (JObject)gameObjectObj["position"];
        if (positionObj != null)
        {
            float x = positionObj["x"]?.ToObject<float>() ?? 0f;
            float y = positionObj["y"]?.ToObject<float>() ?? 0f;
            float z = positionObj["z"]?.ToObject<float>() ?? 0f;

            // Debug.Log($"Read position from JSON: ({x}, {y}, {z})");

            star.transform.position = new Vector3(x, y, z);
        }
        else
        {
            // Debug.LogWarning("Position object is null in gameObject.");
        }
    }
    else
    {
        // Debug.LogWarning("GameObject object is null in JSON.");
    }

    // Debug.Log($"Star position set to: ({star.transform.position.x}, {star.transform.position.y}, {star.transform.position.z})");

        JArray planetArray = (JArray)obj["planetInfo"];
        if (planetArray != null)
        {
            star.planetInfo = planetArray.ToObject<PlanetInfo[]>();
            // Debug.Log("Successfully deserialized planetInfo.");
        }
        else
        {
            star.planetInfo = new PlanetInfo[0];
            // Debug.Log("planetInfo is null, initialized with an empty array.");
        }
    return star;
}

}