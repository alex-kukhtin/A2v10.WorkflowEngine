// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public interface IContainer: IActivity
{
	IToken NewToken();
	void KillToken(IToken? token);

	T FindElement<T>(String id);
	IEnumerable<T> FindAll<T>(Predicate<T> predicate);
}

