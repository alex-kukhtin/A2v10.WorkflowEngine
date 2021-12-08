// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;

using A2v10.System.Xaml;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Serialization;
public class WorkflowSerializer : ISerializer
{
	private readonly IXamlReaderService? _xamlCodeProvider;

	public WorkflowSerializer(IXamlReaderService? xamlCodeProvider = null)
	{
		_xamlCodeProvider = xamlCodeProvider;
	}

	private static readonly JsonConverter[] _jsonConverters = new JsonConverter[]
	{
		new DoubleConverter(),
		new StringEnumConverter(),
		new ExpandoObjectConverterArray()
	};

	private static readonly JsonSerializerSettings _actititySettings = new()
	{
		Formatting = Formatting.None,
		NullValueHandling = NullValueHandling.Ignore,
		ContractResolver = new DefaultContractResolver()
		{
			NamingStrategy = new CamelCaseNamingStrategy
			{
				OverrideSpecifiedNames = false
			}
		},
		TypeNameHandling = TypeNameHandling.Auto,
		Converters = _jsonConverters
	};

	private static readonly JsonSerializerSettings _jsonSettings = new()
	{
		Formatting = Formatting.None,
		NullValueHandling = NullValueHandling.Ignore,
		FloatFormatHandling = FloatFormatHandling.Symbol,
		FloatParseHandling = FloatParseHandling.Decimal,
		Converters = _jsonConverters
	};


	public ExpandoObject? Deserialize(String? text)
	{
		if (text == null)
			return null;
		return JsonConvert.DeserializeObject<ExpandoObject>(text, _jsonSettings);
	}

	public String? Serialize(ExpandoObject? obj)
	{
		if (obj == null)
			return null;
		return JsonConvert.SerializeObject(obj, _jsonSettings);
	}

	public DeserializeResult DeserializeActitity(String text, String format)
	{
		var res = format switch
		{
			"json" => new DeserializeResult(JsonConvert.DeserializeObject<ActivityWrapper>(text, _actititySettings)?.Root ?? 
				throw new InvalidProgramException("Invalid activity"), null),
			"xaml" or "text/xml" => DeserializeXaml(text),
			_ => throw new NotImplementedException($"Deserialize for format '{format}' is not supported"),
		};
		if (res.Activity == null)
			throw new InvalidOperationException("DeserializeActitity failed");
		res.Activity.OnEndInit(null);
		return res;
	}


	public String SerializeActitity(IActivity activity, String format)
	{
		return format switch
		{
			"json" => JsonConvert.SerializeObject(new ActivityWrapper() { Root = activity }, _actititySettings),
			_ => throw new NotImplementedException($"Deserialize for format '{format}' is not supported"),
		};
	}

	DeserializeResult DeserializeXaml(String text)
	{
		if (_xamlCodeProvider == null)
			throw new InvalidOperationException("XamlCodeProvider is null");
		var obj = _xamlCodeProvider.ParseXml(text);
		if (obj is IActivityWrapper wrapper)
			return new DeserializeResult(wrapper.Root(), wrapper);
		else if (obj is Activity activity)
			return new DeserializeResult(activity, null);
		throw new InvalidProgramException($"Invalid Activity type {obj.GetType()}");
	}
}

