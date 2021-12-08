// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System.Collections.Generic;

using A2v10.System.Xaml;

namespace A2v10.Workflow.Bpmn;
[ContentProperty("Children")]
public class Definitions : IActivityWrapper
{
	public String? Id { get; init; }
	public String? Name { get; init; }
	public String? TargetNamespace { get; init; }
	public String? Exporter { get; set; }
	public String? ExporterVersion { get; set; }
	public String? ExpressionLanguage { get; set; }
	public String? TypeLanguage { get; set; }

	public List<Object>? Children { get; init; }

	public IActivity Root() => GetRoot();

	public Process Process => Children?.OfType<Process>()?.FirstOrDefault() ?? throw new InvalidOperationException("Process is null");

    public T? FindElement<T>(Func<T, Boolean> predicate) where T : class
    {
        return Children?.OfType<T>()?.FirstOrDefault(predicate);
    }

    public IActivity GetRoot()
	{
		var collaboration = Children?.OfType<Collaboration>().FirstOrDefault();
		if (collaboration != null)
		{
			var processes = Children?.OfType<Process>();
			if (processes != null)
				collaboration.AddProcesses(processes);
			return collaboration;
		}
		return Process;
	}
}

