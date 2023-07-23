# About
A2v10.Workflow.Engine is a simple workflow engine 
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

# Feedback

A2v10.Workflow.Engine is released as open source under the MIT license. 
Bug reports and contributions are welcome at the GitHub repository.
