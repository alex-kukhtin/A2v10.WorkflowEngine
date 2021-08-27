/*
Copyright © 2020-2021 Alex Kukhtin

Last updated : 27 aug 2021
module version : 8031
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
as
begin
	set nocount on;
	set transaction isolation level read committed;
	truncate table a2wf.[Workflows];
	truncate table a2wf.[Catalog];
end
go
