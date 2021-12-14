
// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn;
[ContentProperty("Children")]
public class SubProcess : ProcessBase, ILoopable
{
	protected Int32 _loopCounter;

	const String LOOP_COUNTER = "LoopCounter";

	public Boolean Canceled { get;  protected set; }

	public override void Store(IActivityStorage storage)
	{
		base.Store(storage);
		if (HasLoop)
			storage.Set<Int32>(LOOP_COUNTER, _loopCounter);
	}

	public override void Restore(IActivityStorage storage)
	{
		base.Restore(storage);
		if (HasLoop)
			_loopCounter = storage.Get<Int32>(LOOP_COUNTER);
	}

	public override void BuildScript(IScriptBuilder builder)
	{
		base.BuildScript(builder);
		if (HasLoop)
		{
			var expr = LoopCharacteristics?.LoopCondition;
			if (String.IsNullOrEmpty(expr))
				expr = "true";
			builder.BuildEvaluate(LoopConditionEval, expr);
		}
	}

	public override void TryComplete(IExecutionContext context, IActivity activity)
	{
		if (TokensCount > 0)
			return;
		if (HasLoop && (TestBefore || CanCountinue(context)))
			context.Schedule(this, _token);
		else
			SubProcessComplete(context);
	}

	public override async ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
	{
		_token = token;
		Canceled = false;
		if (HasLoop && TestBefore && !CanCountinue(context))
		{
			SubProcessComplete(context);
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

	public ValueTask ExecuteBody(IExecutionContext context)
	{ 
		if (Children == null)
			return ValueTask.CompletedTask;
		var start = Elems<Event>().FirstOrDefault(ev => ev.IsStart);
		if (start == null)
			throw new WorkflowException($"SubProcess (Id={Id}). Start event not found");
		context.Schedule(start, _token);
		return ValueTask.CompletedTask;
	}

	public override void Cancel(IExecutionContext context)
	{
		Canceled = true;
		base.Cancel(context);
	}

    void SubProcessComplete(IExecutionContext context)
	{
		if (ParentContainer == null)
			throw new WorkflowException("ParentContainer is null");
		foreach (var ev in ParentContainer.FindAll<BoundaryEvent>(ev => ev.AttachedToRef == Id))
			context.RemoveEvent(ev.Id);
		if (Outgoing == null)
			return;
		if (Canceled)
			return; // already complete with cancel
		var count = Outgoing.Count();
		if (count == 1)
		{
			// simple outgouning - same token
			var targetFlow = ParentContainer.FindElement<SequenceFlow>(Outgoing.First().Text);
			context.Schedule(targetFlow, _token);
			_token = null;
		}
		else if (count > 1)
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
	}

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

