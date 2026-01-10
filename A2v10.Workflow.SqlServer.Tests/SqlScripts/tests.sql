/*
Copyright © 2020-2025 Alex Kukhtin

Last updated : 22 jul 2025
module version : 8239
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
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2wf_test' and TABLE_NAME=N'Order')
create table a2wf_test.[Order]
(
	Id bigint not null,
	InstanceId uniqueidentifier not null,
	[Count] int,
	[State] nvarchar(255),
	[Log] nvarchar(max)
);
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
@InstanceId uniqueidentifier,
@ExecStatus nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	--throw 60000, @ExecStatus, 0;

	select [Model!TModel!Object] = null, [NextDate!!Utc] = dateadd(second, 1, getutcdate());
end
go
------------------------------------------------
create or alter procedure a2wf_test.AutoStartLast
@WorkflowId nvarchar(255) = null
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;
	declare @InstanceId uniqueidentifier;
	select top(1) @InstanceId = InstanceId from a2wf.AutoStart where InstanceId is not null
		and (@WorkflowId is null or WorkflowId = @WorkflowId)
	order by DateCreated desc;

	select [Instance!TInstance!Object] = null, [Id!!Id] = i.Id, [WorkflowId], [Version], [State], 
		ExecutionStatus, Lock
	from a2wf.Instances i
	where i.Id=@InstanceId;
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2wf' and TABLE_NAME=N'Inbox')
create table a2wf.[Inbox]
(
	Id uniqueidentifier not null,
	InstanceId uniqueidentifier not null,
	Void bit,
	Bookmark nvarchar(255),
	Activity nvarchar(255),
	[For] nvarchar(255),
	ForUser bigint,
	Model nvarchar(255),
	ModelId bigint,
	UserRemoved bigint,
	Answer nvarchar(255),
	DateRemoved datetime,
	constraint PK_Inbox primary key clustered(Id, InstanceId)
);
go
------------------------------------------------
if not exists (select * from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA = N'a2wf' and TABLE_NAME=N'Inbox' and COLUMN_NAME=N'Activity')
	alter table a2wf.Inbox add [Activity] nvarchar(255) null;
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2wf' and TABLE_NAME=N'UserTrack')
create table a2wf.[UserTrack]
(
	Id bigint identity(100, 1) not null,
	InstanceId uniqueidentifier not null,
	Activity nvarchar(255),
	UserId bigint,
	UtcDateCreated datetime not null
		constraint DF_UserTrack_UtcDateCreated default(getutcdate()),
	[Message] nvarchar(255)
	constraint PK_UserTrack primary key clustered(Id, InstanceId)
);
go
------------------------------------------------
create or alter procedure a2wf.[Instance.Inbox.Create]
@UserId bigint = null,
@Id uniqueidentifier,
@InstanceId uniqueidentifier,
@Bookmark nvarchar(255),
@Activity nvarchar(255),
@For nvarchar(255) = null,
@ForUser bigint = null,
@Model nvarchar(255) = null,
@ModelId bigint = null
as
begin
	set nocount on;
	set transaction isolation level read committed;
	set xact_abort on;
	insert into a2wf.[Inbox] (Id, InstanceId, Bookmark, [For], ForUser, Model, ModelId, Activity)
	values (@Id, @InstanceId, @Bookmark, @For, @ForUser, @Model, @ModelId, @Activity);

	select [Signal!TSignal!Array] = null, [User] = @ForUser, [Text] = @Bookmark;

	select [Signal!TSignal!Array] = null, [User] = @ForUser + 1, [Text] = @Bookmark;
end
go

------------------------------------------------
create or alter procedure a2wf.[Instance.Inbox.Remove]
@UserId bigint = null,
@Id uniqueidentifier,
@InstanceId uniqueidentifier,
@Answer nvarchar(255) = null
as
begin
	set nocount on;
	set transaction isolation level read committed;
	set xact_abort on;

	update a2wf.Inbox set Void = 1, UserRemoved = @UserId, Answer = @Answer, DateRemoved = getutcdate() 
		where Id=@Id and InstanceId=@InstanceId;
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
------------------------------------------------
create or alter procedure a2wf_test.[SetState]
@Id bigint
as
begin
	set nocount on;
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2wf_test' and TABLE_NAME=N'Deferred')
create table a2wf_test.[Deferred]
(
	InstanceId uniqueidentifier not null,
	Activity nvarchar(255), 
	EventTime datetime2 
		constraint DF_Deferred_EventTime default(getdate())
);
go
------------------------------------------------
create or alter procedure a2wf_test.[Tests.Prepare]
@Id nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read committed;

	update a2wf.CurrentDate set [Date] = null;

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
	delete from a2wf.Inbox
		where InstanceId in (select Id from a2wf.Instances where WorkflowId = @Id);
	delete from a2wf_test.Deferred 
		where InstanceId in (select Id from a2wf.Instances where WorkflowId = @Id);
	delete from a2wf.UserTrack
		where InstanceId in (select Id from a2wf.Instances where WorkflowId = @Id);

	update a2wf.Instances set Parent = null 
	where Parent in (select Id from a2wf.Instances where WorkflowId = @Id);

	delete from a2wf.[Instances] where WorkflowId = @Id;

	delete from a2wf.WorkflowArguments where WorkflowId = @Id;
	delete from a2wf.[Workflows] where Id = @Id;
	delete from a2wf.[Catalog] where Id = @Id;
	delete from a2wf.[AutoStart] where WorkflowId = @Id;

	delete from a2wf_test.[Order];
	insert into a2wf_test.[Order] (Id, InstanceId, [Count], [State], [Log]) values
	(224, newid(), -1, N'Undefined', 'Set');

	insert into a2wf_test.[Order] (Id, InstanceId, [Count], [State], [Log]) values
	(291, newid(), -1, N'Undefined', 'Set');
end
go
------------------------------------------------
create or alter procedure a2wf_test.[ExecDeferred]
@InstanceId uniqueidentifier,
@Activity nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read committed;

	insert into a2wf_test.Deferred (InstanceId, Activity) values (@InstanceId, @Activity);
end
go

------------------------------------------------
create or alter procedure a2wf_test.AutoStartAll
@WorkflowId nvarchar(255) = null
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;
	declare @InstanceId uniqueidentifier;
	select top(1) @InstanceId = InstanceId from a2wf.AutoStart where InstanceId is not null
		and (@WorkflowId is null or WorkflowId = @WorkflowId)
	order by DateCreated desc;

	select [Track!TTrack!Array] = null, [Id!!Id] = d.InstanceId, d.Activity
	from a2wf_test.Deferred d 
	where InstanceId in (select Id from a2wf.Instances where WorkflowId = @WorkflowId);
end
go
------------------------------------------------
create or alter procedure a2wf.[Persist.Order.LoadPersistent]
@Id bigint
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;
	select [Order!TOrder!Object] = null, [Id!!Id] = @Id, [Name] = N'Data from SQL', [Date] = cast(getdate() as date);
end
go
------------------------------------------------
create or alter procedure a2wf.[Persist.Order.SavePersistent]
@Id bigint,
@Name nvarchar(255),
@Date date = null
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

end
go

------------------------------------------------
create or alter procedure a2wf.[Persist.Order.SetInstanceId]
@Id bigint,
@InstanceId uniqueidentifier,
@WorkflowId nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;
end
go

------------------------------------------------
create or alter procedure a2wf.[AutoStartCorrelationIdObject.Order.LoadPersistent]
@Id bigint
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;
	select [Order!TOrder!Object] = null, [Id!!Id] = @Id, [Name] = N'Data from SQL', [Sum] = cast(@Id as money);
end
go
------------------------------------------------
create or alter procedure a2wf.[AutoStartCorrelationIdObject.Order.SetInstanceId]
@Id bigint,
@InstanceId uniqueidentifier,
@WorkflowId nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;
end
go
------------------------------------------------
create or alter procedure a2wf.[AutoStartCorrelationIdObject.Order.SavePersistent]
@Id bigint,
@Name nvarchar(255),
@Sum money
as
begin
	set nocount on;
	set transaction isolation level read committed;
end
go


------------------------------------------------
create or alter procedure a2wf.[TestCorrelation.Order.SetInstanceId]
@Id bigint,
@InstanceId uniqueidentifier,
@WorkflowId nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read committed;

	update a2wf_test.[Order] set InstanceId = @InstanceId where Id = @Id;
end
go

------------------------------------------------
create or alter procedure a2wf.[TestCorrelation.Order.LoadPersistent]
@Id bigint
as
begin
	set nocount on;
	set transaction isolation level read uncommitted;

	update a2wf_test.[Order] set [Log] = [Log] + N',Load' where Id = @Id;
	select [Order!TOrder!Object] = null, [Id!!Id] = Id, [State], [Count]
	from a2wf_test.[Order] where Id = @Id;
end
go
------------------------------------------------
create or alter procedure a2wf.[TestCorrelation.Order.SavePersistent]
@Id bigint,
@State nvarchar(255),
@Count int
as
begin
	set nocount on;
	set transaction isolation level read committed;

	update a2wf_test.[Order] set  [State] = @State, [Count] = @Count,
		[Log] = [Log] + N',Save:' + cast(@Count as nvarchar(255))
	where Id = @Id;

end
go
------------------------------------------------
create or alter procedure a2wf.[CorrIdCollection.Request.SetInstanceId]
@Id bigint,
@InstanceId uniqueidentifier,
@WorkflowId nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read committed;

	--throw 60000, @WorkflowId, 0;
end
go
------------------------------------------------
create or alter procedure a2wf.[CorrIdCollection.Request.LoadPersistent]
@Id bigint
as
begin
	set nocount on;
	set transaction isolation level read committed;

	select [Request!TRequest!Object] = null, [Id!!Id] = @Id,
		[Rows!TRow!Array] = null;

	declare @rows table(Id bigint);
	insert into @rows(Id) values (@Id + 1), (@Id + 2), (@Id + 3);

	select [!TRow!Array] = null, [Id!!Id] = Id,
		[!TRequest.Rows!ParentId] = @Id
	from @rows;
end
go
------------------------------------------------
create or alter procedure a2wf.[CorrIdCollection.Request.SavePersistent]
@Id bigint
as
begin
	set nocount on;
	set transaction isolation level read committed;
end
go
------------------------------------------------
create or alter procedure a2wf.[Instance.UserTrack.Add]
@UserId bigint = null,
@InstanceId uniqueidentifier,
@Message nvarchar(255),
@Activity nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read committed;
	set xact_abort on;

	insert into a2wf.[UserTrack] (InstanceId, UserId, UtcDateCreated, Activity, [Message])
	values (@InstanceId, @UserId, getutcdate(), @Activity, @Message);
end
go
------------------------------------------------
create or alter procedure a2wf_test.[GetTrack]
@InstanceId uniqueidentifier
as
begin
	select [Track!TTrack!Array] = null, Id, Activity, [Message]
	from a2wf.UserTrack
	where InstanceId = @InstanceId
	order by Id;
end
go
