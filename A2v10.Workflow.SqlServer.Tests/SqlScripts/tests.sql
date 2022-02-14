/*
Copyright © 2020-2021 Alex Kukhtin

Last updated : 04 dec 2021
module version : 8072
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
	delete from a2wf.InstanceBookmarks
		where InstanceId in (select Id from a2wf.Instances where WorkflowId = @Id);

	delete from a2wf.[Instances] where WorkflowId = @Id;

	delete from a2wf.[Workflows] where Id=@Id;
	delete from a2wf.[Catalog] where Id=@Id;
	delete from a2wf.[AutoStart] where WorkflowId = @Id;
end
go
------------------------------------------------
create or alter procedure a2wf_test.[Instance.Load.Unlocked]
@UserId bigint = null,
@Id uniqueidentifier
as
begin
	set nocount on;
	set transaction isolation level read committed;

	select [Instance!TInstance!Object] = null, [Id!!Id] = i.Id, [WorkflowId], [Version], [State], 
		ExecutionStatus, Lock
	from a2wf.Instances i
	where i.Id=@Id;
end
go
------------------------------------------------
create or alter procedure a2wf_test.GetTimer
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;
	select [Model!TModel!Object] = null, [NextDate!!Utc] = dateadd(second, 1, getutcdate());
end
go
------------------------------------------------
create or alter procedure a2wf_test.AutoStartLast
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;
	declare @InstanceId uniqueidentifier;
	select top(1) @InstanceId = InstanceId from a2wf.AutoStart where InstanceId is not null
	order by DateCreated desc;
	select [Instance!TInstance!Object] = null, [Id!!Id] = i.Id, [WorkflowId], [Version], [State], 
		ExecutionStatus, Lock
	from a2wf.Instances i
	where i.Id=@InstanceId;
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2wf' and TABLE_NAME=N'Inbox')
begin
	create table a2wf.[Inbox]
	(
		Id uniqueidentifier not null,
		InstanceId uniqueidentifier not null,
		Void bit,
		Bookmark nvarchar(255),
		[For] nvarchar(255),
		ForUser bigint,
		Model nvarchar(255),
		ModelId bigint,
		constraint PK_Inbox primary key clustered(Id, InstanceId)
	);
end
go
------------------------------------------------
create or alter procedure a2wf.[Instance.Inbox.Create]
@UserId bigint = null,
@Id uniqueidentifier,
@InstanceId uniqueidentifier,
@Bookmark nvarchar(255),
@For nvarchar(255),
@ForUser bigint,
@Model nvarchar(255),
@ModelId bigint
as
begin
	set nocount on;
	set transaction isolation level read committed;
	set xact_abort on;
	insert into a2wf.[Inbox] (Id, InstanceId, Bookmark, [For], ForUser, Model, ModelId)
	values (@Id, @InstanceId, @Bookmark, @For, @ForUser, @Model, @ModelId);
end
go

------------------------------------------------
create or alter procedure a2wf.[Instance.Inbox.Remove]
@UserId bigint = null,
@Id uniqueidentifier,
@InstanceId uniqueidentifier
as
begin
	set nocount on;
	set transaction isolation level read committed;
	set xact_abort on;

	update a2wf.Inbox set Void = 1 where Id=@Id and InstanceId=@InstanceId;
end
go
------------------------------------------------
create or alter procedure a2wf_test.[Instance.External.Load]
@UserId bigint = null,
@InstanceId uniqueidentifier
as
begin
	set nocount on;
	set transaction isolation level read committed;
	set xact_abort on;

	select [Instance!TInstance!Object] = null, [Id!!Id] = Id, CorrelationId,
		[Strings!TString!Array] = null, [Integers!TInt!Array] = null,
		[Guids!TGuid!Array] = null
	from a2wf.Instances where Id=@InstanceId;
	
	select [!TString!Array] = null, [Value], [!TInstance.Strings!ParentId] = InstanceId
	from a2wf.InstanceVariablesString where InstanceId = @InstanceId;

	select [!TInt!Array] = null, [Value], [!TInstance.Integers!ParentId] = InstanceId
	from a2wf.InstanceVariablesInt where InstanceId = @InstanceId;

	select [!TGuid!Array] = null, [Value], [!TInstance.Guids!ParentId] = InstanceId
	from a2wf.InstanceVariablesGuid where InstanceId = @InstanceId;
end
go