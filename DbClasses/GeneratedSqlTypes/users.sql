﻿--autogenerated by SqlUserTypeGenerator v1.0.0.0

create type [users] as table ( 
	PropLong bigint not null,
	PropLongNull bigint null,
	PropString nvarchar(10) not null,
	PropBool bit not null,
	PropBoolNull bit null,
	PropDateTime datetime not null,
	PropDateTimeNull datetime null,
	PropDecimal numeric(18) not null,
	PropDecimalNull numeric null,
	PropDouble float not null,
	PropDoubleNull float null,
	PropInt int not null,
	PropIntNull int null,
	PropGuid uniqueidentifier not null,
	PropGuidNull uniqueidentifier null,
	PropByteArray varbinary not null,
	PropByte tinyint not null,
	PropByteNull tinyint null,
	BaseProp nvarchar(23) not null
)
go