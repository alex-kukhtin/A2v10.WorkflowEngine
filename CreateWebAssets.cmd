:
copy A2v10.Bpmn.Editor\dist\bpmn-viewer.js A2v10.Workflow.Assets\wwwroot\scripts\bpmn
copy A2v10.Bpmn.Editor\dist\bpmn-viewer.min.js A2v10.Workflow.Assets\wwwroot\scripts\bpmn


cd  A2v10.Workflow.Assets\
nuget pack A2v10.Workflow.Assets.nuspec -OutputDirectory C:\A2v10_Net6\NuGet.local


pause
