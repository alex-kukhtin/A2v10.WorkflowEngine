// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Text.RegularExpressions;
using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow;

public enum ExternalActivityKind
{
    Bpmn,
    Clr
}

internal class ExternalActivity
{
    private ExternalActivity() { }

    public ExternalActivityKind Kind { get; init; }
    public Boolean IsBpmn => Kind == ExternalActivityKind.Bpmn;
    public IWorkflowIdentity? WorkflowIdentity { get; init; }
    public static ExternalActivity Parse(String name)
    {
        var regex = new Regex(@"^\s*(bpmn|clr):\s*([\w|\.|\/]+)\s*(;\s*(\w+)\s*=\s*(\w+)\s*)*$");
        var match = regex.Match(name);
        if (match.Groups.Count != 6)
            throw new WorkflowException(ErrorMessage(name));
        var type = match.Groups[1].Value.ToLowerInvariant();
        if (type == "bpmn")
        {
            var processName = match.Groups[2].Value;
            var versionTag = match.Groups[4].Value.ToLowerInvariant();
            if (!String.IsNullOrEmpty(versionTag) && versionTag != "version")
                throw new WorkflowException(ErrorMessage(name));
            var verString = match.Groups[5].Value;
            var processVersion = 0;
            if (!String.IsNullOrEmpty(verString))
                processVersion = Int32.Parse(verString);
            return new ExternalActivity()
            {
                Kind = ExternalActivityKind.Bpmn,
                WorkflowIdentity = new WorkflowIdentity(processName, processVersion)
            };
        } 
        else if (type == "clr")
        {
            throw new NotImplementedException($"External clr activity yet not implemented");

        }
        throw new NotImplementedException($"External activity: {name}");
    }

    private static String ErrorMessage(String name) =>
        $"Invalid callActivity definition: '{name}'. Possible values are 'bpmn:ProcessName[;version=version]', 'clr:TypeName;assembly=AssemblyName'";
}

