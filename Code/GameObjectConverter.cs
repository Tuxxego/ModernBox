using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

public class GameObjectConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(GameObject);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        GameObject gameObject = (GameObject)value;

        writer.WriteStartObject();
        writer.WritePropertyName("name");
        writer.WriteValue(gameObject.name);

        writer.WritePropertyName("position");
        serializer.Serialize(writer, gameObject.transform.position);

        writer.WritePropertyName("rotation");
        var rotation = gameObject.transform.rotation;
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(rotation.x);
        writer.WritePropertyName("y");
        writer.WriteValue(rotation.y);
        writer.WritePropertyName("z");
        writer.WriteValue(rotation.z);
        writer.WritePropertyName("w");
        writer.WriteValue(rotation.w);
        writer.WriteEndObject();

        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);

        string name = obj["name"].Value<string>();
        Vector3 position = obj["position"].ToObject<Vector3>();

        Quaternion rotation = new Quaternion(
            obj["rotation"]["x"].Value<float>(),
            obj["rotation"]["y"].Value<float>(),
            obj["rotation"]["z"].Value<float>(),
            obj["rotation"]["w"].Value<float>()
        );

        GameObject gameObject = new GameObject(name);
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;

        return gameObject;
    }
}