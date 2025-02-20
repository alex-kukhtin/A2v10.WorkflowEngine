# About
A2v10.Workflow.Engine is a simple BPMN 2.0 workflow engine 
for the A2v10 platform applications.


# How to use

```csharp
services.AddWorkflowEngineScoped(opts => {
   opts.NativeTypes = ...
});

// or 
services.AddWorkflowEngineSingleton(opts => {
   opts.NativeTypes = ...
});

// optional
services.ConfigureWorkflow(Configuration);
```

# How to use for A2v10 applications

Initialize in Startup.cs:
```csharp
services.AddInvokeTargets(a =>
{
    a.RegisterEngine<WorkflowInvokeTarget>("Workflow", InvokeScope.Scoped);
});
```

Targets in model.json:
```json
{
  commands:{
    "start": {
      "type": "invokeTarget",
      "target": "Workflow.{Command}",
    }
}
```
{Command} is one of the commands: <b>Save, Publish, Start, Create, Run, Resume</b>.


# appsettings.json section

```json
"Workflow": {
  "Store": {
    "DataSource": "Connection_String_Name",
    "MultiTenant": true
  }
}
```

All values (and section) are optional.


# Related Packages

* [A2v10.Workflow.WebAssets](https://www.nuget.org/packages/A2v10.Workflow.WebAssets)

# Feedback

A2v10.Workflow.Engine is released as open source under the MIT license. 
Bug reports and contributions are welcome at the GitHub repository.
