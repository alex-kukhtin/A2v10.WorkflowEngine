﻿// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.


namespace A2v10.Workflow.Interfaces;

public enum VariableType
{
    String,
    Number,
    Boolean,
    Object,
    PersistentObject,
    Date,
    BigInt,
    Guid
}

public enum VariableDirection
{
    Local,
    Const,
    In, /* argument */
    Out, /* result */
    InOut /* bidirectional */
}

public interface IVariable
{
    VariableType Type { get; }
    VariableDirection Dir { get; set; }
    Boolean External { get; }
    Boolean CorrelationId { get; }
    String Name { get; }
    String? Value { get; set; }

    Boolean IsArgument { get; }
    Boolean IsResult { get; }
    String ToType(String name);

    String Assignment();
}

public interface IExternalVariable : IVariable
{
    public String ActivityId { get; }
}

