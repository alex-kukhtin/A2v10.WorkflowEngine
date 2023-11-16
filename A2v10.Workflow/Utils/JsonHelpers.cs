// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace A2v10.Workflow;
public class JsonNumberConverter : JsonConverter<Double>
{
    private const String NaN = "NaN";
    private const String Infinity = "Infinity";
    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (reader.GetString() == NaN)
                return Double.NaN;
            else if (reader.GetString() == Infinity)
                return Double.PositiveInfinity;
        }

        return reader.GetDouble(); // JsonException thrown if reader.TokenType != JsonTokenType.Number
    }

    public override void Write(Utf8JsonWriter writer, Double value, JsonSerializerOptions options)
    {
        if (Double.IsNaN(value))
            writer.WriteStringValue(NaN);
        else if (Double.IsInfinity(value))
            writer.WriteStringValue(Infinity);
        else
            writer.WriteNumberValue(value);
    }
}

public static class TextJsonOptions
{
    static readonly private JsonSerializerOptions _opts;
    static TextJsonOptions()
    {
        _opts = new JsonSerializerOptions();
        _opts.Converters.Add(new JsonNumberConverter());
    }

    public static JsonSerializerOptions Default => _opts;
}