// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	using ResumeAction = Func<IExecutionContext, String, Object, ValueTask>;
	using EventAction = Func<IExecutionContext, IWorkflowEvent, Object, ValueTask>;

	public class CallbackItem
	{
		public String Ref;
		public String CallbackName;

		public static CallbackItem FromExpando(ExpandoObject eobj)
		{
			if (eobj == null)
				return null;
			var cb = new CallbackItem()
			{
				Ref = eobj.Get<String>(nameof(Ref)),
				CallbackName = eobj.Get<String>(nameof(CallbackName))
			};
			return cb;
		}

		public static ExpandoObject CreateFrom(Delegate callback)
		{
			if (callback.Target is not IActivity activityTarget)
				throw new ArgumentException("callback.Target must be an IActivity");

			var refer = activityTarget.Id;

			var custAttr = callback.Method.GetCustomAttributes(inherit: true)
				?.FirstOrDefault(attr => attr is StoreNameAttribute);

			if (custAttr == null)
				throw new ArgumentException("callback.Method has no StoreName attribute");

			var callbackName = (custAttr as StoreNameAttribute).Name;

			var cb = new CallbackItem()
			{
				Ref = refer,
				CallbackName = callbackName
			};
			return cb.ToExpando();
		}

		private ExpandoObject ToExpando()
		{
			var eo = new ExpandoObject();
			eo.Set(nameof(Ref), Ref);
			eo.Set(nameof(CallbackName), CallbackName);
			return eo;
		}

		private MethodInfo GetMethod(IActivity activity)
		{
			foreach (var mi in activity.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
			{
				var custAttr = mi.GetCustomAttributes(inherit: true)
					?.FirstOrDefault(attr => attr is StoreNameAttribute);
				if (custAttr != null)
				{
					var callbackName = (custAttr as StoreNameAttribute).Name;
					if (callbackName == CallbackName)
					{
						return mi;
					}
				}
			}
			throw new WorkflowException($"Method '{CallbackName}' for activity '{Ref}' not found");
		}

		public ResumeAction ToBookmark(IActivity activity)
		{
			var mi = GetMethod(activity);
			return Delegate.CreateDelegate(typeof(ResumeAction), activity, mi) as ResumeAction;
		}

		public EventAction ToEvent(IActivity activity)
		{
			var mi = GetMethod(activity);
			return Delegate.CreateDelegate(typeof(EventAction), activity, mi) as EventAction;
		}
	}
}
