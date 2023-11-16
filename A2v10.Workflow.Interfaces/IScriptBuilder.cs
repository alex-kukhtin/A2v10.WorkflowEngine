// Copyright © 2020-2022 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public interface IScriptBuilder
{
    void AddVariables(IEnumerable<IVariable>? variables);
    void BuildExecute(String name, String? expression);
    void BuildEvaluate(String name, String? expression);
    void BuildExecuteResult(String name, String? expression);
    void BuildSetVariable(String name, String? expression);
}

