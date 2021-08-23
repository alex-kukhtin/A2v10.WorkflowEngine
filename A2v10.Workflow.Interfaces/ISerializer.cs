// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;

namespace A2v10.Workflow.Interfaces
{
	public interface ISerializer
	{
		ExpandoObject Deserialize(String text);
		String Serialize(ExpandoObject obj);

		IActivity DeserializeActitity(String text, String format);
		String SerializeActitity(IActivity activity, String format);
	}
}
