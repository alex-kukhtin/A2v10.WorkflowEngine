// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public class ActivityScriptBuilder : IScriptBuilder
	{
		public const String FMAP = "__fmap__";

		private String _declaratons;
		private readonly Dictionary<String, List<String>> _methods = new();

		private readonly IActivity _activity;

		public String Declarations => _declaratons;

		public ActivityScriptBuilder(IActivity activity)
		{
			_activity = activity;
		}

		void AddMethods(String refer, List<String> methods)
		{
			if (!_methods.ContainsKey(refer))
				_methods.Add(refer, methods);
			else
				_methods[refer].AddRange(methods);
		}

		void AddMethod(String refer, String method)
		{
			if (!_methods.ContainsKey(refer))
				_methods.Add(refer, new List<String>() { method });
			else
				_methods[refer].Add(method);
		}

		#region IScriptBuilder

		public void AddVariables(IEnumerable<IVariable> variables)
		{
			if (variables == null)
				return;
			// declare
			var sb = new StringBuilder();
			foreach (var v in variables)
				sb.Append(
					v.Dir switch
					{
						VariableDirection.Const => $"const {v.Name}; ",
						_ => $"let {v.Name}{v.Assignment()}; "
					}
				);

			_declaratons = sb.ToString();

			var mtds = new List<String>();
			// arguments - In, InOut
			{
				var args = variables.Where(v => v.IsArgument).ToList();
				if (args.Count != 0)
					mtds.Add($"Arguments: (_arg_) => {{ {String.Join("; ", args.Select(x => $"{x.Name} = _arg_.{x.Name}"))}; }}");
			}
			// result - Out, InOut
			{
				var res = variables.Where(v => v.IsResult).ToList();
				if (res.Count != 0)
					mtds.Add($"Result: () => {{return {{ {String.Join(", ", res.Select(x => $"{x.Name} : {x.Name}"))} }}; }}");
			}
			// store, restore - In, Out, Local, not constant!
			{
				var strest = variables.Where(v => v.Dir != VariableDirection.Const).ToList();
				if (strest.Count != 0)
				{
					mtds.Add($"Store: () => {{return {{ {String.Join(", ", strest.Select(x => $"{x.Name} : {x.Name}"))} }}; }}");
					mtds.Add($"Restore: (_arg_) => {{ {String.Join("; ", strest.Select(x => $"{x.Name} = _arg_.{x.Name} "))}; }}");
				}
			}

			if (mtds.Count > 0)
			{
				AddMethods(_activity.Id, mtds);
			}
		}

		public void BuildExecute(String name, String expression)
		{
			AddMethod(_activity.Id, $"{name}: () => {{{expression};}}");
		}

		public void BuildExecuteResult(String name, String expression)
		{
			AddMethod(_activity.Id, $"{name}: (reply) => {{{expression};}}");
		}

		public void BuildEvaluate(String name, String expression)
		{
			AddMethod(_activity.Id, $"{name}: () => {{ return {expression};}}");
		}

		#endregion

		public String Methods
		{
			get
			{
				var sb = new StringBuilder();
				static String _methodsText(List<String> methods)
				{
					if (methods == null || methods.Count == 0)
						return "null";
					return $"{{\n{String.Join(",\n", methods)}\n}};";
				}

				foreach (var (k, v) in _methods)
					sb.Append($"{FMAP}['{k}'] = {_methodsText(v)}");
				return sb.ToString();
			}
		}
	}

	public class ActivityScript
	{
		private readonly ScriptBuilder _builder;
		private readonly IActivity _activity;

		public ActivityScript(ScriptBuilder builder, IActivity activity)
		{
			_activity = activity;
			_builder = builder;
		}

		public String Ref => _activity.Id;

		public void Build(IActivity activity)
		{
			if (activity is IScriptable scriptable)
			{
				var scriptBuilder = new ActivityScriptBuilder(activity);
				scriptable.BuildScript(scriptBuilder);

				_builder.Append(scriptBuilder.Declarations);
				_builder.Append(scriptBuilder.Methods);
			}
		}
	}

	public class ScriptBuilder
	{
		private readonly Stack<ActivityScript> _stack;
		private readonly StringBuilder _textBuilder;

		public ScriptBuilder()
		{
			_stack = new Stack<ActivityScript>();
			_textBuilder = new StringBuilder();
			_textBuilder.AppendLine("\"use strict\";");
			_textBuilder.AppendLine("() => {");
			_textBuilder.AppendLine($"let {ActivityScriptBuilder.FMAP} = {{}};");
		}

		public void EndScript()
		{
			_textBuilder.AppendLine($"return {ActivityScriptBuilder.FMAP};");
			_textBuilder.AppendLine("};");
		}

		public String Script => _textBuilder.ToString();

		public void Start(IActivity activity)
		{
			if (activity is IScoped scopedActivity)
			{
				var ascript = new ActivityScript(this, activity);
				_stack.Push(ascript);
				_textBuilder.AppendLine("(function() {");
				var globalScript = scopedActivity.GlobalScript;
				if (!String.IsNullOrEmpty(globalScript))
					_textBuilder.AppendLine(globalScript);
			}
		}

		public void Append(String text)
		{
			if (!String.IsNullOrEmpty(text))
				_textBuilder.AppendLine(text);
		}

		public void End(IActivity activity)
		{
			var ascript = _stack.Peek();
			if (ascript.Ref == activity.Id)
			{
				_textBuilder.AppendLine("})();");
				_stack.Pop();
			}
		}

		public void Build(IActivity activity)
		{
			var ascript = _stack.Peek();
			ascript.Build(activity);
		}
	}
}
