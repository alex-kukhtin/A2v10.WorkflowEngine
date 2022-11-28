del /q A2v10.Workflow\bin\Release\*.nupkg
del /q A2v10.Workflow\bin\Release\*.snupkg
del /q A2v10.Workflow.Engine\bin\Release\*.nupkg
del /q A2v10.Workflow.Engine\bin\Release\*.snupkg
del /q A2v10.Workflow.Interfaces\bin\Release\*.nupkg
del /q A2v10.Workflow.Interfaces\bin\Release\*.snupkg
del /q A2v10.Workflow.Serialization\bin\Release\*.nupkg
del /q A2v10.Workflow.Serialization\bin\Release\*.snupkg
del /q A2v10.Workflow.SqlServer\bin\Release\*.nupkg
del /q A2v10.Workflow.SqlServer\bin\Release\*.snupkg

dotnet build "A2v10.Workflow\A2v10.Workflow.csproj" -c Release
dotnet build "A2v10.Workflow.Engine\A2v10.Workflow.Engine.csproj" -c Release
dotnet build "A2v10.Workflow.Interfaces\A2v10.Workflow.Interfaces.csproj" -c Release
dotnet build "A2v10.Workflow.Serialization\A2v10.Workflow.Serialization.csproj" -c Release
dotnet build "A2v10.Workflow.SqlServer\A2v10.Workflow.SqlServer.csproj" -c Release

del /q ..\NuGet.local\*.*

copy A2v10.Workflow\bin\Release\*.nupkg ..\NuGet.local
copy A2v10.Workflow\bin\Release\*.snupkg ..\NuGet.local

copy A2v10.Workflow.Engine\bin\Release\*.nupkg ..\NuGet.local
copy A2v10.Workflow.Engine\bin\Release\*.snupkg ..\NuGet.local

copy A2v10.Workflow.Interfaces\bin\Release\*.nupkg ..\NuGet.local
copy A2v10.Workflow.Interfaces\bin\Release\*.snupkg ..\NuGet.local

copy A2v10.Workflow.Serialization\bin\Release\*.nupkg ..\NuGet.local
copy A2v10.Workflow.Serialization\bin\Release\*.snupkg ..\NuGet.local

copy A2v10.Workflow.SqlServer\bin\Release\*.nupkg ..\NuGet.local
copy A2v10.Workflow.SqlServer\bin\Release\*.snupkg ..\NuGet.local

pause