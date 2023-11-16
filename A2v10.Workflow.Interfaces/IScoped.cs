// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Workflow.Interfaces;
public interface IScoped : IScriptable
{
    List<IVariable>? Variables { get; }
    String? GlobalScript { get; }
}

public interface IExternalScoped
{
    List<IVariable>? ExternalVariables();
}

