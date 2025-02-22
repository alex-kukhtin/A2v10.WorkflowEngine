// Copyright © 2020-2025 Oleksandr Kukhtin. All rights reserved.

using System.Globalization;


namespace A2v10.Workflow;
public class Variable : IVariable
{
    private String? _name;
    public String Name
    {
        get => _name ?? throw new InvalidOperationException("Variable.Name is null");
        init => _name = value;
    }
    public VariableDirection Dir { get; set; }
    public VariableType Type { get; set; }

    public Boolean External { get; set; }

    public Boolean CorrelationId { get; set; }
    public String? Value { get; set; }

    public Boolean IsArgument => Dir == VariableDirection.In || Dir == VariableDirection.InOut;
    public Boolean IsResult => Dir == VariableDirection.Out || Dir == VariableDirection.InOut;

    public String ToType(String name)
    {
        return Type switch
        {
            VariableType.Number or VariableType.BigInt => $"+{name}",
            VariableType.String => $"''+{name}",
            VariableType.Boolean => $"!!{name}",
            VariableType.Date => $"new Date({name})",
            _ => name
        };
    }

    public String Assignment()
    {
        if (String.IsNullOrEmpty(Value))
            return String.Empty;
        var val = GetValue();
        return $" = {val}";
    }

    String GetValue()
    {
        switch (Type)
        {
            case VariableType.String:
                return $"'{Value?.Replace("'", "\\'")}'";
            case VariableType.BigInt:
                if (Int64.TryParse(Value, out Int64 intVal))
                    return intVal.ToString();
                throw new WorkflowException($"Unable to convert '{Value}' to BigInt");
            case VariableType.Guid:
                if (Guid.TryParse(Value, out Guid guidVal))
                    return $"'{guidVal}'";
                throw new WorkflowException($"Unable to convert '{Value}' to Guid");
            case VariableType.Number:
                if (Double.TryParse(Value, out Double dblVal))
                    return dblVal.ToString(CultureInfo.InvariantCulture);
                throw new WorkflowException($"Unable to convert '{Value}' to Number");
            case VariableType.Date:
                if (DateTime.TryParse(Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime dtVal))
                    return $"new Date({new DateTimeOffset(dtVal).ToUnixTimeMilliseconds()})";
                throw new WorkflowException($"Unable to convert '{Value}' to Date");
            case VariableType.Boolean:
                if (Value == "true" || Value == "false")
                    return Value;
                throw new WorkflowException($"Unable to convert '{Value}' to Boolean");
            case VariableType.Object:
                return Value ?? String.Empty; // TODO: Is the value an JS object?
        }
        throw new NotImplementedException($"Converting value for '{Type}'");
    }

}

public class ExternalVariable : Variable, IExternalVariable
{
    public ExternalVariable(IVariable var, String activityId)
    {
        Name = var.Name;
        Type = var.Type;
        Dir = var.Dir;
        External = var.External;
        Value = var.Value;
        ActivityId = activityId;
    }
    public String ActivityId { get; }
}


public static class IVariableExtensions
{
    public static String RestoreArgument(this IVariable var)
    {
        if (var.Type == VariableType.PersistentObject)
            return $"_loadPersistent(_arg_, '{var.Name}')";
        return $"_arg_.{var.Name}";
    }
}