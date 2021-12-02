// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


using A2v10.Workflow.Interfaces;

namespace A2v10.Workflow;
public class PendingInstance : IPendingInstance
{
	public Guid InstanceId { get; set; }
	public String? EventKey { get; set; }
}

