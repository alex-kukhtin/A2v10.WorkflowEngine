// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using Jint;
using Jint.Runtime.Interop;

namespace A2v10.Workflow
{
    public class JsConsole
    {
#pragma warning disable IDE1006 // Naming Styles
        public static void log(Object msg)
#pragma warning restore IDE1006 // Naming Styles
        {
            Console.WriteLine(msg);
        }
    }

    public static class JsNativeExtensions
    {
        public static void AddNativeObjects(this Engine engine, IScriptNativeObjectProvider nativeObjects)
        {
            engine.SetValue("console", new JsConsole());
            if (nativeObjects != null)
                foreach (var nativeType in nativeObjects.NativeTypes())
                    engine.SetValue(nativeType.Name, TypeReference.CreateTypeReference(engine, nativeType.Type));
        }
    }
}
