﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;

namespace SqlUserTypeGenerator
{
    public class SqlGeneratorTask : ITask
    {
	    private static string StaticSourceAssemblyPath;

		public bool Execute()
		{

			StaticSourceAssemblyPath = SourceAssemblyPath;
			
			var destFolderAbsolutePath = Path.GetFullPath(DestinationFolder);

            //BuildEngine.LogMessageEvent(new BuildMessageEventArgs("test custom task", destFolderAbsolutePath, "sender", MessageImportance.High));

            if (!Directory.Exists(destFolderAbsolutePath))
            {
                Directory.CreateDirectory(destFolderAbsolutePath);
            }						

			var generatedSql = string.Empty;
			AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;

			//load all assemblies in project output folder to reflection-only context
			var files = Directory.GetFiles(Path.GetDirectoryName(StaticSourceAssemblyPath))
				.Where(f => IsEqualStrings(Path.GetExtension(f), ".dll"));

			foreach (var file in files)
			{
				Assembly.ReflectionOnlyLoadFrom(file);
			}

			//load target assembly
			Assembly assembly = Assembly.ReflectionOnlyLoadFrom(SourceAssemblyPath); ;

			//get classes with custom attribute
			Func<Type, bool> typeWithSqltTypeAttributePredicate = t => t.GetCustomAttributesData().Any(cad => IsEqualStrings(cad.AttributeType.FullName, typeof(SqlUserTypeAttribute).FullName));

			var types = assembly.GetTypes().Where(typeWithSqltTypeAttributePredicate).ToList();

			var headerText = GetHeaderText();
			foreach (var type in types)
			{				
				var generatedType = SqlGenerator.GenerateUserType(type);
				
				generatedSql =
					$"if type_id('{generatedType.TypeName}') is not null drop type [{generatedType.TypeName}];\r\ngo\r\n\r\n"
					+ $"create type [{generatedType.TypeName}] as table ( \r\n"
					+ string.Join(",\r\n", generatedType.Columns.Select(c => "\t" + c))
					+ "\r\n)\r\ngo";

				var targetFile = Path.ChangeExtension(Path.Combine(destFolderAbsolutePath, GetSafeFilename(generatedType.TypeName)), "sql");

				File.WriteAllText(targetFile, headerText + generatedSql, Encoding.UTF8);
			}

			return true;
        }

	    private bool IsEqualStrings(string s1, string s2)
	    {
		    return string.Compare(s1, s2, StringComparison.InvariantCultureIgnoreCase) == 0;
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
            return $"--autogenerated by {an} v{av}\r\n\r\n";
        }

        private string GetSafeFilename(string filename)
        {

            return string.Join("", filename.Split(Path.GetInvalidFileNameChars()));

        }

        public string SourceAssemblyPath { get; set; }
        public string DestinationFolder { get; set; }

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }
    }
}
