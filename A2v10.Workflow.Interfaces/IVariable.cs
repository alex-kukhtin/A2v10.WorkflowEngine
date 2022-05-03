// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.


namespace A2v10.Workflow.Interfaces;

public enum VariableType
{
    String,
    Number,
    Boolean,
    Object,
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

    public Boolean IsArgument { get; }
    public Boolean IsResult { get; }
    public String ToType(String name);

    String Assignment();
}

public interface IExternalVariable : IVariable
{
    public String ActivityId { get; }
}

