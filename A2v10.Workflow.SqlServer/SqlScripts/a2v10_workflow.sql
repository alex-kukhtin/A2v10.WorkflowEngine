/*
Copyright © 2020-2021 Alex Kukhtin

Last updated : 27 aug 2021
module version : 8031
*/
------------------------------------------------
set nocount on;
if not exists(select * from INFORMATION_SCHEMA.SCHEMATA where SCHEMA_NAME=N'a2wf')
	exec sp_executesql N'create schema a2wf';
go
------------------------------------------------
grant execute on schema ::a2wf to public;
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2wf' and TABLE_NAME=N'Catalog')
begin
	create table a2wf.[Catalog]
	(
		[Id] nvarchar(255) not null,
		[Format] nvarchar(32) not null,
		[Body] nvarchar(max) null,
		[Thumb] varbinary(max) null,
		ThumbFormat nvarchar(32) null,
		[Hash] nvarchar(255) null,
		DateCreated datetime not null constraint DF_Catalog_DateCreated default(getutcdate()),
		constraint PK_Catalog primary key clustered (Id)
	);
end
go
------------------------------------------------
if not exists(select * from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA=N'a2wf' and TABLE_NAME=N'Workflows')
begin
	create table a2wf.Workflows
	(
		[Id] nvarchar(255) not null,
		[Version] int not null,
		[Format] nvarchar(32) not null,
		[Text] nvarchar(max) null,
		[Hash] nvarchar(255) null,
		DateCreated datetime not null constraint DF_Workflows_DateCreated default(getutcdate()),
		constraint PK_Workflows primary key clustered (Id, [Version]) with (fillfactor = 70)
	);
end
go
------------------------------------------------
create or alter procedure a2wf.[Catalog.Save]
@UserId bigint = null,
@Id nvarchar(255),
@Body nvarchar(max),
@Format nvarchar(32),
@Hash nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read committed;
	declare @savedHash nvarchar(255);
	begin tran;
		select @savedHash = [Hash] from a2wf.[Catalog] where Id=@Id;
		if @savedHash is null
			insert into a2wf.[Catalog] (Id, [Format], Body, [Hash]) 
			values (@Id, @Format, @Body, @Hash)
		else if @savedHash <> @Hash
			update a2wf.[Catalog] set Body = @Body, [Hash]=@Hash where Id=@Id;
	commit tran;
end
go
------------------------------------------------
create or alter procedure a2wf.[Catalog.Publish]
@UserId bigint = null,
@Id nvarchar(255)
as
begin
	set nocount on;
	set transaction isolation level read committed;

	declare @hash nvarchar(255);
	declare @id nvarchar(255);
	declare @version int;
	begin tran;
		select top(1) @hash = [Hash], @id = [Id], @version=[Version] 
		from a2wf.Workflows 
		where Id=@Id order by [Version] desc;

		if (select [Hash] from a2wf.[Catalog] where Id=@Id) = @hash
		begin
			select Id, [Version] from a2wf.Workflows where Id=@Id and [Version]=@version;
		end
		else
		begin
			declare @retval table(Id nvarchar(255), [Version] int);
			insert into a2wf.Workflows (Id, [Format], [Text], [Hash], [Version])
			output inserted.Id, inserted.[Version] into @retval(Id, [Version])
			select Id, [Format], [Body], [Hash], [Version] = 
				(select isnull(max([Version]) + 1, 1) from a2wf.Workflows where Id=@Id)
			from a2wf.[Catalog] where Id=@Id;
			select Id, [Version] from @retval;
		end
	commit tran;
end
go
