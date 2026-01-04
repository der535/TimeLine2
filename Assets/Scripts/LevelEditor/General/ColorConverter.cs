// Assets/Scripts/Json/ColorConverter.cs
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class ColorConverter : JsonConverter<Color>
{
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("r");
        writer.WriteValue(value.r);
        writer.WritePropertyName("g");
        writer.WriteValue(value.g);
        writer.WritePropertyName("b");
        writer.WriteValue(value.b);
        writer.WritePropertyName("a");
        writer.WriteValue(value.a);
        writer.WriteEndObject();
    }

    public override Color ReadJson(JsonReader reader, System.Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);
        return new Color(
            obj["r"]?.ToObject<float>() ?? 0f,
            obj["g"]?.ToObject<float>() ?? 0f,
            obj["b"]?.ToObject<float>() ?? 0f,
            obj["a"]?.ToObject<float>() ?? 1f
        );
    }
}