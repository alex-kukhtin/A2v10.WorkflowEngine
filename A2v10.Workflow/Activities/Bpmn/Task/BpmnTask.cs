// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;
using System.Dynamic;

namespace A2v10.Workflow.Bpmn;
public class BpmnTask : FlowElement, IStorable, ICanComplete, IScriptable, ILoopable
{
	protected IToken? _token;
	protected Int32 _loopCounter;
	protected List<IToken>? _tokens;

	public Boolean IsComplete { get; protected set; }

	protected virtual Boolean CanInduceIdle => false;

	#region IStorable 
	const String TOKEN = "Token";
	const String LOOP_COUNTER = "LoopCounter";
	const String MULTI_INSTANCE_TOKENS = "MultiInstanceTokens";

	public virtual void Store(IActivityStorage storage)
	{
		if (!CanInduceIdle) return;
		storage.SetToken(TOKEN, _token);
		if (IsLoop)
			storage.Set<Int32>(LOOP_COUNTER, _loopCounter);
		else if (IsMultiInstance && _tokens != null)
			storage.SetTokenList(MULTI_INSTANCE_TOKENS, _tokens);
	}

	public virtual void Restore(IActivityStorage storage)
	{
		if (!CanInduceIdle) return;
		_token = storage.GetToken(TOKEN);
		if (IsLoop)
			_loopCounter = storage.Get<Int32>(LOOP_COUNTER);
		else if (IsMultiInstance)
		{
			_tokens = new();
			storage.GetTokenList(MULTI_INSTANCE_TOKENS, _tokens);
		}
	}
	#endregion


	static ExpandoObject CreateVariable(Int32 index, Object value)
	{
		return new ExpandoObject()
		{
			{ "Index", index },
			{ "Value", value }
		};
	}

	public override async ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
	{
		_token = token;
		IsComplete = false;

		if (IsMultiInstance)
		{
			var coll = context.Evaluate<Object[]>(Id, MultiInstanceCollectionEval);
			if (coll == null)
			{
				await DoCompleteBody(context);
				return;
			}
			var mi = MultiInstanceCharacteristics!;
			if (_tokens == null) 
			{
				ParentContainer.KillToken(_token);
				// first run
				await AddBoundaryEvents(context);
				_tokens = new();
				for (int i = 0; i < coll.Length; i++)
				{
					var tok = ParentContainer.NewToken();
					_tokens.Add(tok);
				}
				if (mi.IsSequential)
				{
					IsComplete = false;
					context.Schedule(this, _tokens[0]);
				}
				else
				{
					for (int i = 0; i < coll.Length; i++)
					{
						context.SetVariable(Id, MultiInstanceVariableSet, CreateVariable(i, coll[i]));
						IsComplete = false;
						context.Schedule(this, _tokens[i]);
					}
				}
			} 
			else
			{
				// next run
				var ix = _tokens.FindIndex(x => x == token);
				if (ix < 0 || ix > coll.Length)
					throw new WorkflowException("BPMNTask. Invalid token index");
				context.SetVariable(Id, MultiInstanceVariableSet, CreateVariable(ix, coll[ix]));
				await ExecuteBody(context);
			}
		}
		else if (IsLoop)
		{
			if (TestBefore && !CanCountinue(context))
			{
				await DoCompleteBody(context);
				return;
			}
			if (_loopCounter == 0)
				await AddBoundaryEvents(context);
			_loopCounter += 1;
			await ExecuteBody(context);
		}
		else
		{
			await AddBoundaryEvents(context);
			await ExecuteBody(context);
		}
	}


	async ValueTask AddBoundaryEvents(IExecutionContext context)
	{
		foreach (var ev in ParentContainer.FindAll<BoundaryEvent>(ev => ev.AttachedToRef == Id))
			await ev.ExecuteAsync(context, ParentContainer.NewToken());
	}

	void RemoveBoundaryEvents(IExecutionContext context)
	{
		foreach (var ev in ParentContainer.FindAll<BoundaryEvent>(ev => ev.AttachedToRef == Id))
			context.RemoveEvent(ev.Id);
	}

	public override void Cancel(IExecutionContext context)
	{
		CompleteTask(context);
		context.RemoveBookmark(Id);
		base.Cancel(context);
	}


	protected virtual ValueTask CompleteBody(IExecutionContext context)
	{
		if (IsLoop)
		{
			if (TestBefore || CanCountinue(context))
			{
				IsComplete = false;
				context.Schedule(this, _token);
				return ValueTask.CompletedTask;
			}
			return DoCompleteBody(context);
		}
		else if (IsMultiInstance)
		{
			if (_tokens == null)
				throw new WorkflowException("BPMNTask. Invalid Complete body status");
			var ix = _tokens.FindIndex(x => x == _token);
			var coll = context.Evaluate<Object[]>(Id, MultiInstanceCollectionEval) 
				?? throw new WorkflowException("BPMNTask. Collection is null");
            if (ix < 0 || ix >= coll.Length)
				throw new WorkflowException("BPMNTask. Invalid Complete body index");
			_tokens[ix] = Token.Empty();
			ParentContainer.KillToken(_token);
			if (!CanCountinue(context))
			{
				_token = ParentContainer.NewToken();
				return DoCompleteBody(context);
			}
			else
			{
				var mi = MultiInstanceCharacteristics!;
				if (mi.IsSequential)
				{
					// first non empty index
					ix = _tokens.FindIndex(x => !x.IsEmpty);
					IsComplete = false;
					context.Schedule(this, _tokens[ix]);
				}
			}
			return ValueTask.CompletedTask;
		}
		else
			return DoCompleteBody(context);
	}

	ValueTask DoCompleteBody(IExecutionContext context)
	{
		if (Outgoing == null)
		{
			return ValueTask.CompletedTask;
		}

		if (Outgoing.Count() == 1)
		{
			// simple outgouning - same token
			var targetFlow = ParentContainer.FindElement<SequenceFlow>(Outgoing.First().Text);
			context.Schedule(targetFlow, _token);
			_token = null;
		}
		else
		{
			// same as task + parallelGateway
			ParentContainer.KillToken(_token);
			_token = null;
			foreach (var flowId in Outgoing)
			{
				var targetFlow = ParentContainer.FindElement<SequenceFlow>(flowId.Text);
				context.Schedule(targetFlow, ParentContainer.NewToken());
			}
		}
		CompleteTask(context);
		return ValueTask.CompletedTask;
	}

	public virtual ValueTask ExecuteBody(IExecutionContext context)
	{
		return CompleteBody(context);
	}

	public virtual void BuildScriptBody(IScriptBuilder builder)
	{
	}

	protected virtual void CompleteTask(IExecutionContext context)
	{
		ParentContainer.KillToken(_token);
		if (_tokens != null)
		{
			foreach (var t in _tokens)
			{
				if (t != null)
					ParentContainer.KillToken(t);
			}
			_tokens = null;
		}
		_token = null;
		IsComplete = true;
		RemoveBoundaryEvents(context);
	}

	#region IScriptable
	public void BuildScript(IScriptBuilder builder)
	{
		if (IsLoop)
		{
			var expr = LoopCharacteristics?.LoopCondition;
			if (String.IsNullOrEmpty(expr))
				expr = "true";
			builder.BuildEvaluate(LoopConditionEval, expr);
		}
		else if (IsMultiInstance)
		{
			var coll = MultiInstanceCharacteristics?.Collection;
			var item = MultiInstanceCharacteristics?.Variable;
			if (String.IsNullOrEmpty(coll))
				coll = "null";
			builder.BuildEvaluate(MultiInstanceCollectionEval, coll);
			builder.BuildSetVariable(MultiInstanceVariableSet, item);
		}
		BuildScriptBody(builder);
	}
	#endregion

	public String LoopConditionEval => $"{Id}_Loop";
	public String MultiInstanceCollectionEval => $"{Id}_MI_Collection";
	public String MultiInstanceVariableSet => $"{Id}_MI_Variable_Set";
	public StandardLoopCharacteristics? LoopCharacteristics => Children?.OfType<StandardLoopCharacteristics>().FirstOrDefault();
	public MultiInstanceLoopCharacteristics? MultiInstanceCharacteristics => Children?.OfType<MultiInstanceLoopCharacteristics>().FirstOrDefault();
	public Boolean TestBefore => LoopCharacteristics?.TestBefore ?? false;
	public Int32 LoopMaximum => LoopCharacteristics?.LoopMaximum ?? 0;

	#region ILoopable

	public Boolean IsLoop => Children != null && Children.OfType<StandardLoopCharacteristics>().Any();
	public Boolean IsMultiInstance => Children != null && Children.OfType<MultiInstanceLoopCharacteristics>().Any();

	public Boolean CanCountinue(IExecutionContext context)
	{
		if (IsLoop)
		{
			var max = LoopMaximum;
			if (max != 0 && _loopCounter >= max)
				return false;
			return context.Evaluate<Boolean>(Id, LoopConditionEval);
		}
		else if (IsMultiInstance)
		{
			if (_tokens == null)
				return false;
			return _tokens.Any(x => !x.IsEmpty);
		}
		return false;
	}
	#endregion
}

