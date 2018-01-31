﻿using System;

namespace SqlUserTypeGenerator
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlUserTypeColumnPropertiesAttribute : Attribute
    {
	    public int Scale { get; set; }
	    public int Presicion { get; set; }
		public int Length { get; set; }

        public const int MaxLength = -1;
    }

}