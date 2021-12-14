// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Bpmn;
public class BpmnTask : FlowElement, IStorable, ICanComplete, IScriptable, ILoopable, ICancelable
{
	protected IToken? _token;
	protected Int32 _loopCounter;

	public Boolean IsComplete { get; protected set; }

	protected virtual Boolean CanInduceIdle => false;

	#region IStorable 
	const String TOKEN = "Token";
	const String LOOP_COUNTER = "LoopCounter";

	public virtual void Store(IActivityStorage storage)
	{
		if (!CanInduceIdle) return;
		storage.SetToken(TOKEN, _token);
		if (HasLoop)
			storage.Set<Int32>(LOOP_COUNTER, _loopCounter);
	}

	public virtual void Restore(IActivityStorage storage)
	{
		if (!CanInduceIdle) return;
		_token = storage.GetToken(TOKEN);
		if (HasLoop)
			_loopCounter = storage.Get<Int32>(LOOP_COUNTER);
	}
	#endregion


	public override async ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
	{
		_token = token;
		IsComplete = false;

		if (HasLoop && TestBefore && !CanCountinue(context))
		{
			await DoCompleteBody(context);
			return;
		}

		if (_loopCounter == 0)
		{
			// boundary events
			foreach (var ev in ParentContainer.FindAll<BoundaryEvent>(ev => ev.AttachedToRef == Id))
				await ev.ExecuteAsync(context, ParentContainer.NewToken());
		}
		_loopCounter += 1;

		await ExecuteBody(context);
	}

	public override void Cancel(IExecutionContext context)
	{
		CompleteTask(context);
		context.RemoveBookmark(Id);
		base.Cancel(context);
	}


	protected virtual ValueTask CompleteBody(IExecutionContext context)
	{
		if (HasLoop && (TestBefore || CanCountinue(context)))
		{
			IsComplete = false;
			context.Schedule(this, _token);
			return ValueTask.CompletedTask;
		}
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
		_token = null;
		IsComplete = true;
		foreach (var ev in ParentContainer.FindAll<BoundaryEvent>(ev => ev.AttachedToRef == Id))
			context.RemoveEvent(ev.Id);
	}

	#region IScriptable
	public void BuildScript(IScriptBuilder builder)
	{
		if (HasLoop)
		{
			var expr = LoopCharacteristics?.LoopCondition;
			if (String.IsNullOrEmpty(expr))
				expr = "true";
			builder.BuildEvaluate(LoopConditionEval, expr);
		}
		BuildScriptBody(builder);
	}
	#endregion

	public String LoopConditionEval => $"{Id}_Loop";
	public StandardLoopCharacteristics? LoopCharacteristics => Children?.OfType<StandardLoopCharacteristics>().FirstOrDefault();
	public Boolean TestBefore => LoopCharacteristics?.TestBefore ?? false;
	public Int32 LoopMaximum => LoopCharacteristics?.LoopMaximum ?? 0;

	#region ILoopable

	public Boolean HasLoop => Children != null && Children.OfType<StandardLoopCharacteristics>().Any();

	public Boolean CanCountinue(IExecutionContext context)
	{
		if (!HasLoop) 
			return false;
		var max = LoopMaximum;
		if (max != 0 && _loopCounter >= max)
			return false;
		return context.Evaluate<Boolean>(Id, LoopConditionEval);
	}
	#endregion
}


