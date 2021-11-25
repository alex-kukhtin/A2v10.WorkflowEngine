// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public interface ISerializer
{
	ExpandoObject? Deserialize(String? text);
	String? Serialize(ExpandoObject? obj);

	IActivity DeserializeActitity(String text, String format);
	String SerializeActitity(IActivity activity, String format);
}

