// Copyright © 2020-2021 Oleksandr Kukhtin. All rights reserved.

namespace A2v10.Workflow.Interfaces;

public record DeserializeResult(IActivity Activity, IActivityWrapper? Wrapper);
public interface ISerializer
{
    ExpandoObject? Deserialize(String? text);
    String? Serialize(ExpandoObject? obj);

    DeserializeResult DeserializeActitity(String text, String format);
    String SerializeActitity(IActivity activity, String format);
}

