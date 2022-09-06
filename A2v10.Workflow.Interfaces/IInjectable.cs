// Copyright © 2020-2022 Alex Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;
public interface IInjectable
{
    public void Inject(IServiceProvider serviceProvider);
    public void SetDeferred(IDeferredTarget deferredTarget);
}

