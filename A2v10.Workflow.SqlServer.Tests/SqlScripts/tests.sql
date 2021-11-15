/*
Copyright © 2020-2021 Alex Kukhtin

Last updated : 21 sep 2021
module version : 8033
*/
------------------------------------------------
set nocount on;
if not exists(select * from INFORMATION_SCHEMA.SCHEMATA where SCHEMA_NAME=N'a2wf_test')
	exec sp_executesql N'create schema a2wf_test';
go
------------------------------------------------
grant execute on schema ::a2wf_test to public;
go
------------------------------------------------
create or alter procedure a2wf_test.[Tests.Prepare]
@Id nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read committed;

	delete from a2wf.InstanceVariablesInt 
		where InstanceId in (select Id from a2wf.Instances where WorkflowId = @Id);
	delete from a2wf.InstanceVariablesString 
		where InstanceId in (select Id from a2wf.Instances where WorkflowId = @Id);
	delete from a2wf.InstanceVariablesGuid 
		where InstanceId in (select Id from a2wf.Instances where WorkflowId = @Id);
	delete from a2wf.InstanceTrack
		where InstanceId in (select Id from a2wf.Instances where WorkflowId = @Id);
	delete from a2wf.InstanceEvents
		where InstanceId in (select Id from a2wf.Instances where WorkflowId = @Id);
	delete from a2wf.[Instances] where WorkflowId = @Id;

	delete from a2wf.[Workflows] where Id=@Id;
	delete from a2wf.[Catalog] where Id=@Id;
end
go


