﻿--autogenerated by SqlUserTypeGenerator v1.0.0.0


if type_id(N'[user]') is not null drop type [user]
go

create type [user] as table ( 
	PropLong bigint not null,
	PropLongNull bigint null,
	PropString nvarchar(42) not null,
	PropBool bit not null,
	PropBoolNull bit null,
	PropDateTime datetime not null,
	PropDateTimeNull datetime null,
	PropDecimal numeric(10) not null,
	PropDecimalNull numeric null,
	PropDouble float not null,
	PropDoubleNull float null,
	PropInt int not null,
	PropIntNull int null,
	PropGuid uniqueidentifier not null,
	PropGuidNull uniqueidentifier null,
	PropByteArray varbinary not null,
	PropByte tinyint not null,
	PropByteNull tinyint null
)
go

print 'user type re-created'		
