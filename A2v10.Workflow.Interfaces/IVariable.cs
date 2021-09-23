// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Workflow.Interfaces
{
	public enum VariableType
	{
		String,
		Number,
		Boolean,
		Object,
		BigInt,
		Guid
	}

	public enum VariableDirection
	{
		Local,
		Const,
		In, /* argument */
		Out, /* result */
		InOut /* bidirectional */
	}

	public interface IVariable
	{
		VariableType Type { get; }
		VariableDirection Dir { get; set; }
		Boolean External { get; }

		String Name { get; }
		String Value { get; }
		String Modifier { get; }

		public Boolean IsArgument { get; }
		public Boolean IsResult { get; }

		String Assignment();
	}

	public interface IExternalVariable : IVariable
	{
		public String ActivityId { get; }
	}
}
