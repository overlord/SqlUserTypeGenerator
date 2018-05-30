﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using SqlUserTypeGenerator.Helpers;

namespace SqlUserTypeGenerator
{
	public class SqlGeneratorTask : ITask
	{
		private readonly string _newLine = Environment.NewLine;

		public string SourceAssemblyPath { get; set; }

		//absolute path to generated files
		public string DestinationFolder { get; set; }

		// generate user type settings
		public string UseSqlDateTime2 { get; set; }

		public string EncodedTypePreCreateCode { get; set; }
		public string EncodedTypePostCreateCode { get; set; }

		public IBuildEngine BuildEngine { get; set; }
		public ITaskHost HostObject { get; set; }

		public bool Execute()
		{
			//BuildEngine.LogMessageEvent(new BuildMessageEventArgs("test custom task", destFolderAbsolutePath, "sender", MessageImportance.High));

			if (!Directory.Exists(DestinationFolder))
			{
				Directory.CreateDirectory(DestinationFolder);
			}

			AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;

			LoadDependentAssemblies(SourceAssemblyPath);

			//load target assembly
			Assembly assembly = Assembly.ReflectionOnlyLoadFrom(SourceAssemblyPath); ;

			var types = GetTypesWithSqlTypeAttribute(assembly);

			var headerText = GetHeaderText();
			var generateUserTypeSettings = new GenerateUserTypeSettings
			{
				UseSqlDateTime2 = CustomParseBool(UseSqlDateTime2),
			};

			foreach (var type in types)
			{
				var generatedType = SqlGenerator.GenerateUserType(type.UserType, type.SqlUserTypeAttributeData, generateUserTypeSettings);

				var generatedSql = BuildSqlText(generatedType);

				var targetFile = Path.ChangeExtension(Path.Combine(DestinationFolder, GetSafeFilename(generatedType.TypeName)), "sql");

				File.WriteAllText(targetFile, headerText + generatedSql, Encoding.UTF8);
			}

			return true;
		}

		private List<UserTypeWithSqlUserTypeAttribute> GetTypesWithSqlTypeAttribute(Assembly assembly)
		{
			return assembly.GetTypes()
				.Select(t =>
					new UserTypeWithSqlUserTypeAttribute()
					{
						UserType = t,
						SqlUserTypeAttributeData = CustomAttributesHelper.GetSqlUserTypeAttributeData(t)
					})
				.Where(ut => ut.SqlUserTypeAttributeData != null)
				.ToList();
		}

		private string BuildSqlText(SqlUserTypeDefinition generatedType)
		{
			var typeNameReplaceString = "$typename$";
			string typePreCreateCode = StringHelper.DecodeArgument(EncodedTypePreCreateCode)?.Replace(typeNameReplaceString, generatedType.TypeName);
			string typePostCreateCode = StringHelper.DecodeArgument(EncodedTypePostCreateCode)?.Replace(typeNameReplaceString, generatedType.TypeName);

			return string.Empty
				+ (!string.IsNullOrEmpty(typePreCreateCode) ? $"{typePreCreateCode}{_newLine}" : string.Empty)

				+ $"create type [{generatedType.TypeName}] as table ( {_newLine}"
				+ string.Join($",{_newLine}", generatedType.Columns.Select(c => "\t" + c))
				+ $"{_newLine}){_newLine}go"

				+ (!string.IsNullOrEmpty(typePostCreateCode) ? $"{_newLine}{typePostCreateCode}" : string.Empty)
			;
		}

		private void LoadDependentAssemblies(string sourceAssemblyPath)
		{
			//load all assemblies in project output folder to reflection-only context
			var directoryName = Path.GetDirectoryName(sourceAssemblyPath);

			var files = Directory.GetFiles(directoryName)
				.Where(f => StringHelper.IsEqualStrings(Path.GetExtension(f), ".dll"));

			foreach (var file in files)
			{
				Assembly.ReflectionOnlyLoadFrom(file);
			}
		}

		internal class UserTypeWithSqlUserTypeAttribute
		{
			public Type UserType { get; set; }
			public CustomAttributeData SqlUserTypeAttributeData { get; set; }
		}


		private Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
		{
			string missedAssemblyFullName = args.Name;
			Assembly assembly = Assembly.ReflectionOnlyLoad(missedAssemblyFullName);
			return assembly;
		}

		private string GetHeaderText()
		{
			var assembly = typeof(SqlGeneratorTask).Assembly;
			var an = assembly.GetName().Name;
			var av = assembly.GetName().Version.ToString();
			return $"--autogenerated by {an} v{av}{_newLine + _newLine}";
		}

		private string GetSafeFilename(string filename)
		{
			return string.Join("", filename.Split(Path.GetInvalidFileNameChars()));
		}

		private bool CustomParseBool(string boolString)
		{
			int i;
			if(int.TryParse(boolString, out i))
			{
				return Convert.ToBoolean(i);
			}
			return Convert.ToBoolean(boolString);
		}
	}
}
