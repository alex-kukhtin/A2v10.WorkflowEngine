// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	public class Flowchart : Activity, IScoped
	{
		public List<FlowNode> Nodes { get; set; }
		public List<IVariable> Variables { get; set; }
		public String GlobalScript { get; set; }

		public override IEnumerable<IActivity> EnumChildren()
		{
			if (Nodes != null)
				foreach (var node in Nodes)
					yield return node;
		}

		public FlowNode FindNode(String refer)
		{
			return Nodes?.Find(node => node.Id == refer);
		}

		public override ValueTask ExecuteAsync(IExecutionContext context, IToken token)
		{
			if (Nodes == null)
				return ValueTask.CompletedTask;
			var start = Nodes.Find(n => n.IsStart);
			if (start == null)
				throw new WorkflowException($"Flowchart (Ref={Id}. Start node not found");
			context.Schedule(start, token);
			return ValueTask.CompletedTask;
		}

		#region IScriptable
		public virtual void BuildScript(IScriptBuilder builder)
		{
			builder.AddVariables(Variables);
		}
		#endregion

	}
}
