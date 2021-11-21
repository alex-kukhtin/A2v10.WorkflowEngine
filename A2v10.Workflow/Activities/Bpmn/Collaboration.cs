// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	public class Collaboration : BpmnActivity, IScoped, IExternalScoped
	{
		#region IScoped
		public List<IVariable> Variables => Elem<ExtensionElements>()?.GetVariables();

		public String GlobalScript => Elem<ExtensionElements>()?.GetGlobalScript();

		public void BuildScript(IScriptBuilder builder)
		{
			builder.AddVariables(Variables);
		}
		#endregion

		#region IExternalScoped
		public List<IVariable> ExternalVariables()
		{
			var vars = Variables;
			var lst = new List<IVariable>();
			if (vars != null)
				lst.AddRange(vars);

			foreach (var elem in Children.OfType<BaseElement>())
			{
				if (elem is not IScoped scoped)
					continue;
				vars = scoped.Variables;
				if (vars == null)
					continue;
				foreach (var v in vars)
					lst.Add(new ExternalVariable(v, elem.Id));
			}
			if (lst.Count == 0)
				return null;
			return lst;
		}
		#endregion


		public override IEnumerable<IActivity> EnumChildren()
		{
			if (Children != null)
				foreach (var elem in Children.OfType<Participant>())
					yield return elem;
		}

		public void AddProcesses(IEnumerable<Process> processes)
		{
			if (Children == null)
				return;
			foreach (var participant in Children.OfType<Participant>())
			{
				var prc = processes.FirstOrDefault(p => p.Id == participant.ProcessRef);
				if (prc == null)
					throw new WorkflowException($"Process '{participant.ProcessRef}' not found");
				participant.EnsureChildren();
				participant.Children.Add(prc);
			}
		}

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token)
		{
			var parts = Children.OfType<Participant>();
			if (parts == null)
				throw new WorkflowException("No participants in the Collaboration");
			if (parts.Count() != 1)
				throw new WorkflowException("Collaboration has multiply participants. Yet not implemented");
			return parts.First().ExecuteAsync(context, token);
		}
	}
}
