# About
**A2v10.Workflow.Engine** is a simple BPMN 2.0 workflow engine 
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
  "commands":{
    "start": {
      "type": "invokeTarget",
      "target": "Workflow.{Command}",
    }
}
```
Available commands:

|Command| Description     |Arguments                  |Returns            |
|-------|-----------------|------------------------   |-------------------|
|Save   | Save workflow   |WorkflowId, Format, Body   |                   |
|Publish| Publish workflow|WorkflowId                 |WorkflowId, Version|
|Start  | Start workflow  |WorkflowId, Version, Args  |InstanceId, Result |
|Create | Create workflow |WorkflowId, CorrelationId  |InstanceId         |
|Run    | Run workflow    |InstanceId, CorrelationId, Args  |InstanceId, Result |
|Resume | Resume workflow |InstanceId, Bookmark, Reply¹|InstanceId, Result |
|Message | Send message to workflow |InstanceId, Message|InstanceId |
|Variables  | Get instance Variables   |InstanceId    |Result             |
|CheckSyntax| Check script syntax|WorkflowId          |Errors: []         |

The *Start* command is equivalent to *Create* + *Run*.
The *Version* is optional. If not specified - the max version will be used.


¹ When the **Reply**  object includes the **UserId** property 
with the value **$(UserId)**, the system substitutes this placeholder with the current user Id."
 
# appsettings.json section

```json
"Workflow": {
  "Store": {
    "DataSource": "Connection_String_Name",
    "MultiTenant": false
  }
}
```

All values (and section) are optional.


# Global Workflow Variables

Available variables:

* ***Instance*** is the current workflow instance
```js 
{
    Id: string, 
    CorrelationId: string, 
    ExecutionStatus: string
}
```
* ***LastResult*** is the last received result of an activity *Resume* or *CallActivity* invocation.
```js 
{
  ... /*all reply properties*/
}
```

* ***CurrentUser*** is the current user identifier.
 

# Related Packages

* [A2v10.Workflow.WebAssets](https://www.nuget.org/packages/A2v10.Workflow.WebAssets)
* [A2v10.Module.Workflow](https://www.nuget.org/packages/A2v10.Module.Workflow)

# Feedback

**A2v10.Workflow.Engine** is released as open source under the MIT license. 
Bug reports and contributions are welcome at the GitHub repository.
