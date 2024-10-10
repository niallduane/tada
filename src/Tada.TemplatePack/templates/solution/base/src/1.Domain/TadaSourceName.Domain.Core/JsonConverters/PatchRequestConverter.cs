using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TadaSourceName.Domain.Core.JsonConverters;

public class PatchRequestConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
        {
            return false;
        }
        return (typeToConvert.GetGenericTypeDefinition() == typeof(PatchRequest<>));
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type elementType = typeToConvert.GetGenericArguments()[0];
        JsonConverter? converter = Activator.CreateInstance(
            typeof(PatchRequestConverter<>).MakeGenericType(new Type[] { elementType }),
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            args: null,
            culture: null
        ) as JsonConverter;

        return converter!;
    }
}

public class PatchRequestConverter<T> : JsonConverter<PatchRequest<T>> where T : class
{
    public override PatchRequest<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var dictionary = new PatchRequest<T>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dictionary;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string propertyName = reader.GetString()!;

            reader.Read();

            var value = ReadValue(ref reader, options);

            dictionary.Add(propertyName, value);
        }

        throw new JsonException();
    }

    private object ReadValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString()!;
            case JsonTokenType.Number:
                if (reader.TryGetInt32(out int intValue))
                {
                    return intValue;
                }
                if (reader.TryGetInt64(out long longValue))
                {
                    return longValue;
                }
                return reader.GetDouble();
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Null:
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8603 //
            case JsonTokenType.StartArray:
                var list = new List<object>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        return list;
                    }
                    list.Add(ReadValue(ref reader, options));
                }
                throw new JsonException();
            case JsonTokenType.StartObject:
                return Read(ref reader, typeof(Dictionary<string, object>), options);
            default:
                throw new JsonException();
        }
    }

    public override void Write(Utf8JsonWriter writer, PatchRequest<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);

            JsonSerializer.Serialize(writer, kvp.Value, options);
        }

        writer.WriteEndObject();
    }
}
