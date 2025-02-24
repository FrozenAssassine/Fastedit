using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using TextControlBoxNS;

namespace Fastedit.Converter;

internal class CodeLanguageStringConverterJson : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        Debug.WriteLine("JSON: " + value);
        if (value is SyntaxHighlightID codeLanguage)
        {
            writer.WriteValue(codeLanguage.ToString());
        }
        else
        {
            throw new JsonSerializationException("Expected SyntaxHighlightID enum value.");
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        Debug.WriteLine("JSON: " + reader.Value);

        if (reader.TokenType == JsonToken.String)
        {
            var enumString = reader.Value.ToString();
            if (Enum.TryParse(typeof(SyntaxHighlightID), enumString, out var result))
            {
                return result;
            }
            throw new JsonSerializationException($"Invalid value for {nameof(SyntaxHighlightID)}: {enumString}");
        }
        throw new JsonSerializationException($"Unexpected token type: {reader.TokenType}");
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(SyntaxHighlightID);
    }
}