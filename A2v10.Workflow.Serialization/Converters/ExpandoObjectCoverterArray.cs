// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace A2v10.Workflow.Serialization;
public class ExpandoObjectConverterArray : ExpandoObjectConverter
{
    public override Object? ReadJson(JsonReader reader, Type objectType, Object? existingValue, JsonSerializer serializer)
    {
        return ReadValueArray(reader);
    }

    private Object? ReadValueArray(JsonReader reader)
    {
        return reader.TokenType switch
        {
            JsonToken.StartObject => ReadObjectArray(reader),
            JsonToken.StartArray => ReadListArray(reader),
            _ => reader.Value,
        };
    }

    private Object ReadListArray(JsonReader reader)
    {
        IList<Object> list = new List<Object>();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.Comment:
                    break;
                default:
                    Object? v = ReadValueArray(reader);
                    if (v != null)
                        list.Add(v);
                    break;
                case JsonToken.EndArray:
                    return list.ToArray();
            }
        }

        throw new JsonSerializationException("Unexpected end when reading ExpandoObject.");
    }

    private Object ReadObjectArray(JsonReader reader)
    {
        IDictionary<String, Object?> expandoObject = new ExpandoObject();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    String? propertyName = reader.Value!.ToString();
                    if (propertyName == null)
                        throw new JsonSerializationException("PropertyName not set");

                    if (!reader.Read())
                        throw new JsonSerializationException("Unexpected end when reading ExpandoObject.");

                    Object? v = ReadValueArray(reader);
                    expandoObject[propertyName] = v;
                    break;
                case JsonToken.Comment:
                    break;
                case JsonToken.EndObject:
                    return expandoObject;
            }
        }

        throw new JsonSerializationException("Unexpected end when reading ExpandoObject.");
    }
}

