// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow.Bpmn
{
	using ExecutingAction = Func<IExecutionContext, IActivity, ValueTask>;

	public abstract class ProcessBase : FlowElement, IContainer, IStorable, IScoped, IScriptable
	{

		protected IToken _token;

		private readonly List<IToken> _tokens = new();

		protected IEnumerable<BpmnActivity> Activities => Elems<BpmnActivity>().ToList();
		protected Int32 TokensCount => _tokens.Count;

		public override IEnumerable<IActivity> EnumChildren()
		{
			if (Children != null)
				foreach (var elem in Activities)
					yield return elem;
		}

		public override void OnEndInit(IActivity parent)
		{
			base.OnEndInit(parent);
			if (Children == null)
				return;
			foreach (var e in Activities)
				e.OnEndInit(this);
		}

		#region IScoped
		public List<IVariable> Variables => Elem<ExtensionElements>()?.GetVariables();
		public String GlobalScript => Elem<ExtensionElements>()?.GetGlobalScript();

		public virtual void BuildScript(IScriptBuilder builder)
		{
			builder.AddVariables(Variables);
		}
		#endregion

		#region IStorable
		const String TOKEN = "Token";
		const String TOKENS = "Tokens";

		public virtual void Store(IActivityStorage storage)
		{
			storage.SetToken(TOKEN, _token);
			storage.SetTokenList(TOKENS, _tokens);
		}

		public virtual void Restore(IActivityStorage storage)
		{
			_token = storage.GetToken(TOKEN);
			storage.GetTokenList(TOKENS, _tokens);
		}
		#endregion

		public IToken NewToken()
		{
			var t = Token.Create();
			_tokens.Add(t);
			return t;
		}

		public void KillToken(IToken token)
		{
			if (token != null)
				_tokens.Remove(token);
		}


		public T FindElement<T>(String id)
		{
			var elem = Activities.FirstOrDefault(e => e.Id == id);
			if (elem == null)
				throw new WorkflowException($"BPMN. Element (Id = {id}) not found");
			if (elem is T elemT)
				return elemT;
			throw new WorkflowException($"BPMN. Invalid type for element (Id = {id}). Expected: '{typeof(T).Name}', Actual: '{elem.GetType().Name}'");
		}

		public IEnumerable<T> FindAll<T>(Predicate<T> predicate)
		{
			var list = Activities.Where(elem => elem is T t && predicate(t));
			foreach (var el in list)
			{
				if (el is T t)
					yield return t;
			}
		}
	}
}
