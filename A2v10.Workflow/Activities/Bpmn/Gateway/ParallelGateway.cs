// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Workflow.Bpmn;
public class ParallelGateway : Gateway, IStorable
{
	private readonly List<IToken> _tokens = new();

	public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
	{
		// waits for all incoming tokens
		if (token != null)
			_tokens.Add(token);
		if (HasIncoming && _tokens.Count == Incoming?.Count())
			return DoOutgoing(context);
		else
			return ValueTask.CompletedTask;
	}

	#region IStorable 
	const String TOKENS = "Tokens";

	public virtual void Store(IActivityStorage storage)
	{
		storage.SetTokenList(TOKENS, _tokens);
	}

	public virtual void Restore(IActivityStorage storage)
	{
		storage.GetTokenList(TOKENS, _tokens);
	}
	#endregion

	public ValueTask DoOutgoing(IExecutionContext context)
	{
		// kill all tokens
		foreach (var t in _tokens)
			ParentContainer.KillToken(t);
		_tokens.Clear();
		if (HasOutgoing)
		{
			foreach (var og in Outgoing!)
			{
				var flow = ParentContainer.FindElement<SequenceFlow>(og.Text);
				context.Schedule(flow, ParentContainer.NewToken());
			}
		} 
		return ValueTask.CompletedTask;
	}
}

