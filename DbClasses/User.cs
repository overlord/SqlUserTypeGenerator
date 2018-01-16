﻿using System;
using SqlUserTypeGenerator;


namespace DbClasses
{
    [SqlUserType("users")]
    public class User
    {
        public long PropLong { get; set; }     
        public long? PropLongNull { get; set; }
		[SqlUserTypeColumnProperties(42)]     
        public string PropString { get; set; }
        public bool PropBool { get; set; }
        public bool? PropBoolNull { get; set; }
        public DateTime PropDateTime { get; set; }
        public DateTime? PropDateTimeNull { get; set; }
        public decimal PropDecimal { get; set; }
        public decimal? PropDecimalNull { get; set; }
        public double PropDouble { get; set; }
        public double? PropDoubleNull { get; set; }
        public int PropInt { get; set; }     
        public int? PropIntNull { get; set; }     
        public Guid PropGuid { get; set; }
        public Guid? PropGuidNull { get; set; }
        public byte[] PropByteArray { get; set; }
        public byte PropByte { get; set; }
        public byte? PropByteNull { get; set; }
    }
}
