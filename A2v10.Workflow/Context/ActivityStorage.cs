// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public enum StorageState
	{
		Loading,
		Storing
	}

	public class ActivityStorage : IActivityStorage
	{
		private readonly ExpandoObject _expando;
		private readonly IDictionary<String, IActivity> _activities;

		public Boolean IsLoading { get; set; }
		public Boolean IsStoring => !IsLoading;

		public ExpandoObject Value => _expando;

		public ActivityStorage(StorageState state, IDictionary<String, IActivity> activities, ExpandoObject obj = null)
		{
			IsLoading = state == StorageState.Loading;
			_activities = activities;
			_expando = obj ?? new ExpandoObject();
		}

		public T Get<T>(String name)
		{
			if (IsStoring)
				throw new InvalidOperationException("Get in storing mode");
			return _expando.Get<T>(name);
		}

		public ExecutingAction GetCallback(String name)
		{
			if (IsStoring)
				throw new InvalidOperationException("Get in storing mode");
			var itm = _expando.Get<ExpandoObject>(name);
			if (itm == null)
				return null;
			var cb = CallbackItem.FromExpando(itm);
			if (!_activities.TryGetValue(cb.Ref, out IActivity activity))
				throw new WorkflowException($"Activity {cb.Ref} not found");
			return cb.ToCallback(activity);
		}

		public void Set<T>(String name, T value)
		{
			if (IsLoading)
				throw new InvalidOperationException("Set in loading mode");
			_expando.Set(name, value);
		}

		public void SetCallback(String name, ExecutingAction callback)
		{
			if (IsLoading)
				throw new InvalidOperationException("Set in loading mode");
			if (callback == null)
				return;
			_expando.Set(name, CallbackItem.CreateFrom(callback));
		}

		public void SetToken(String name, IToken value)
		{
			if (IsLoading)
				throw new InvalidOperationException("Set in loading mode");
			if (value == null)
				return;
			_expando.Set(name, value.ToString());
		}

		public IToken GetToken(String name)
		{
			if (IsStoring)
				throw new InvalidOperationException("Get in storing mode");
			var val = _expando.Get<String>(name);
			if (val == null)
				return null;
			return Token.FromString(val);
		}

		public void SetTokenList(String name, List<IToken> list)
		{
			if (IsLoading)
				throw new InvalidOperationException("Set in loading mode");
			if (list == null || list.Count == 0)
				return;
			var vals = new List<String>();
			foreach (var l in list)
				vals.Add(l.ToString());
			_expando.Set(name, vals);
		}

		public void GetTokenList(String name, List<IToken> list)
		{
			if (IsStoring)
				throw new InvalidOperationException("Get in storing mode");
			var vals = _expando.Get<List<Object>>(name);
			if (vals == null)
				return;
			foreach (var v in vals)
				list.Add(Token.FromString(v.ToString()));
		}
	}
}
