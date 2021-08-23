// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using System.Text.Json;

using A2v10.Workflow.Interfaces;
using A2v10.Workflow.Tracker;

namespace A2v10.Workflow
{

	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;
	using ResumeAction = Func<IExecutionContext, String, Object, ValueTask>;
	using EventAction = Func<IExecutionContext, IWorkflowEvent, Object, ValueTask>;

	public record QueueItem
	(
		Func<IExecutionContext, IToken, ExecutingAction, ValueTask> Action,
		IActivity Activity,
		ExecutingAction OnComplete,
		IToken Token
	);

	public record EventItem
	(
		EventAction Action,
		IWorkflowEvent Event
	);

	public partial class ExecutionContext : IExecutionContext
	{
		private readonly Queue<QueueItem> _commandQueue = new();
		private readonly Dictionary<String, IActivity> _activities = new();
		private readonly Dictionary<String, ResumeAction> _bookmarks = new();
		private readonly Dictionary<String, EventItem> _events = new();

		private readonly IActivity _root;

		private readonly ScriptEngine _script;
		private readonly IServiceProvider _serviceProvider;
		private readonly ITracker _tracker;

		public ExecutionContext(IServiceProvider serviceProvider, ITracker tracker, IActivity root, Object args = null)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_tracker = tracker;
			_root = root;

			// store all activites
			var toMapArg = new TraverseArg()
			{
				Action = (activity) => _activities.Add(activity.Id, activity)
			};
			_root.Traverse(toMapArg);

			_script = BuildScript(args);
			_tracker.Start();
		}

		ScriptEngine BuildScript(Object args)
		{
			if (_root is not IScoped)
				return null;
			var sb = new ScriptBuilder();
			var sbTraverseArg = new TraverseArg()
			{
				Start = (activity) => sb.Start(activity),
				Action = (activity) => sb.Build(activity),
				End = (activity) => sb.End(activity)
			};
			_root.Traverse(sbTraverseArg);
			sb.EndScript();

			return new ScriptEngine(_serviceProvider, _tracker, _root, sb.Script, args);
		}

		public ExpandoObject GetResult()
		{
			return _script?.GetResult();
		}

		#region IExecutionContext
		public void Schedule(IActivity activity, ExecutingAction onComplete, IToken token)
		{
			if (activity == null)
				return;
			_tracker.Track(new ActivityTrackRecord(ActivityTrackAction.Schedule, activity, token));
			_commandQueue.Enqueue(new QueueItem(activity.ExecuteAsync, activity, onComplete, token));
		}

		public void SetBookmark(String bookmark, IActivity activity, ResumeAction onComplete)
		{
			_tracker.Track(new ActivityTrackRecord(ActivityTrackAction.Bookmark, activity, $"{{bookmark:'{bookmark}'}}"));
			_bookmarks.Add(bookmark, onComplete);
		}

		public void RemoveBookmark(String bookmark)
		{
			if (_bookmarks.ContainsKey(bookmark))
				_bookmarks.Remove(bookmark);
		}

		public void AddEvent(IWorkflowEvent wfEvent, IActivity activity, EventAction onTrigger)
		{
			_tracker.Track(new ActivityTrackRecord(ActivityTrackAction.Event, activity, wfEvent.ToString()));
			_events.Add(wfEvent.Key, new EventItem(onTrigger, wfEvent));
		}

		public void RemoveEvent(String eventKey)
		{
			if (_events.ContainsKey(eventKey))
				_events.Remove(eventKey);
		}

		public T Evaluate<T>(String refer, String name)
		{
			return _script.Evaluate<T>(refer, name);
		}

		public void Execute(String refer, String name)
		{
			_script.Execute(refer, name);
		}

		public void ExecuteResult(String refer, String name, Object result)
		{
			_script.ExecuteResult(refer, name, result);
		}
		#endregion

		public async ValueTask RunAsync()
		{
			while (_commandQueue.Count > 0)
			{
				var queueItem = _commandQueue.Dequeue();
				_tracker.Track(new ActivityTrackRecord(ActivityTrackAction.Execute, queueItem.Activity, queueItem.Token));
				await queueItem.Action(this, queueItem.Token, queueItem.OnComplete);
			}
			_tracker.Stop();
		}

		public ValueTask ResumeAsync(String bookmark, Object result)
		{
			if (_bookmarks.TryGetValue(bookmark, out ResumeAction action))
			{
				String strResult = result != null ? $", result:{JsonSerializer.Serialize(result)}" : String.Empty;
				_tracker.Track(new ActivityTrackRecord(ActivityTrackAction.Resume, null, $"{{bookmark:'{bookmark}'{strResult}}}"));
				return action(this, bookmark, result);
			}
			else
				throw new WorkflowException($"Bookmark '{bookmark}' not found");
		}

		public ValueTask HandleEventAsync(String eventKey, Object result)
		{
			if (_events.TryGetValue(eventKey, out EventItem eventItem))
			{
				String strResult = result != null ? $", result:{JsonSerializer.Serialize(result)}" : String.Empty;
				_tracker.Track(new ActivityTrackRecord(ActivityTrackAction.HandleEvent, null, $"{{event:'{eventKey}'{strResult}}}"));
				return eventItem.Action(this, eventItem.Event, result);
			}
			else
				throw new WorkflowException($"Event '{eventKey}' not found");
		}
	}
}
