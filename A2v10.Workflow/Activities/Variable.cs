// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Globalization;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public class Variable : IVariable
	{
		public String Name { get; set; }
		public VariableDirection Dir { get; set; }
		public VariableType Type { get; set; }

		public Boolean External { get; set; }
		public String Value { get; set; }

		public String Modifier => Type switch {
			VariableType.Number or VariableType.BigInt => "+",
			VariableType.String => "''+",
			VariableType.Boolean => "!!",
			_ => ""
		};

		public Boolean IsArgument => Dir == VariableDirection.In || Dir == VariableDirection.InOut;
		public Boolean IsResult => Dir == VariableDirection.Out || Dir == VariableDirection.InOut;


		public String Assignment()
		{
			if (String.IsNullOrEmpty(Value))
				return String.Empty;
			var val = GetValue();
			return $" = {val}";
		}

		String GetValue()
		{
			switch (Type)
			{
				case VariableType.String:
					return $"'{Value.Replace("'", "\\'")}'";
				case VariableType.BigInt:
					if (Int64.TryParse(Value, out Int64 intVal))
						return intVal.ToString();
					throw new WorkflowException($"Unable to convert '{Value}' to BigInt");
				case VariableType.Guid:
					if (Guid.TryParse(Value, out Guid guidVal))
						return $"'{guidVal}'";
					throw new WorkflowException($"Unable to convert '{Value}' to Guid");
				case VariableType.Number:
					if (Double.TryParse(Value, out Double dblVal))
						return dblVal.ToString(CultureInfo.InvariantCulture);
					throw new WorkflowException($"Unable to convert '{Value}' to Number");
				case VariableType.Boolean:
					if (Value == "true" || Value == "false")
						return Value;
					throw new WorkflowException($"Unable to convert '{Value}' to Boolean");
				case VariableType.Object:
					return Value; // TODO: Is the value an JS object?
			}
			throw new NotImplementedException($"Converting value for '{Type}'");
		}

	}

	public class ExternalVariable : Variable, IExternalVariable
	{
		public ExternalVariable(IVariable var, String activityId)
		{
			Name = var.Name;
			Type = var.Type;
			Dir = var.Dir;
			External = var.External;
			Value = var.Value;
			ActivityId = activityId;
		}
		public String ActivityId { get; }
	}
}