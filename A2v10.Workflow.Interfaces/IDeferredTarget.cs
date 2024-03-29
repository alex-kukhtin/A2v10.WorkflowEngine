﻿// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;

public interface IDeferredTarget
{
    List<DeferredElement>? Deferred { get; }
    String Refer { get; set; }

    void AddDeffered(DeferredElement elem);
}

