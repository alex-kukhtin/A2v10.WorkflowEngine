﻿// Copyright © 2021 Alex Kukhtin. All rights reserved.

using System;

namespace A2v10.Workflow
{
	public static class StringExtensions
	{
		public static Boolean IsVariable(this String expression)
		{
			if (String.IsNullOrEmpty(expression))
				return false;
			var ex = expression.Trim();
			return ex.StartsWith("${") && ex.EndsWith("}");
		}

		public static String Variable(this String expression)
		{
			var exp = expression.Trim();
			return exp[2..^1];
		}
	}
}