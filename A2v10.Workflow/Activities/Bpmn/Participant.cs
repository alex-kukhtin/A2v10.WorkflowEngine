﻿// Copyright © 2020-2023 Oleksandr Kukhtin. All rights reserved.

using System.Collections.Generic;

namespace A2v10.Workflow.Bpmn;
public class Participant : BpmnActivity, IScoped
{
    public String? ProcessRef { get; init; }

    #region IScoped
    public List<IVariable>? Variables => Elem<ExtensionElements>()?.GetVariables();
    public String? GlobalScript => Elem<ExtensionElements>()?.GetGlobalScript();

    public void BuildScript(IScriptBuilder builder)
    {
        builder.AddVariables(Variables);
    }
    #endregion

    public override IEnumerable<IActivity> EnumChildren()
    {
        if (Children != null)
            foreach (var elem in Children.OfType<Process>())
                yield return elem;
    }

    internal void EnsureChildren()
    {
        Children ??= [];
    }

    public override ValueTask ExecuteAsync(IExecutionContext context, IToken? token)
    {
        var process = (Children?.OfType<Process>().FirstOrDefault(itm => itm.Id == ProcessRef)) 
            ?? throw new WorkflowException($"Process '{ProcessRef}' not found");
        return process.ExecuteAsync(context, token);
    }
}

