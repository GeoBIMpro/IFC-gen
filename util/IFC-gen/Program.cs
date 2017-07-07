﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;

namespace IFC_dotnet_generate
{

	internal static class TypeExtensions{
		internal static string ValidTypeName(this Type t){
			string result = t.Name;
			if(t.Name.StartsWith("Ifc") && (t.Name != "IfcSystem" || t.Name != "IfcObject")){
				result = t.Name.Remove(0,3);
			}
			return result;
		}

		internal static string ValidParameterName(this string s){
			var result = s.First().ToString().ToLower() + String.Join("", s.Skip(1));
				
			// Avoid properties named with reserved words.
			if(result == "ref")
			{
				result = "reference";
			}

			if(result == "operator")
			{
				result = "op";
			}

			return result;
		}

		internal static string ValidPropertyName(this PropertyInfo pi){
			if(pi.DeclaringType.ValidTypeName() == pi.Name){
				return pi.Name + "Property";
			}
			return pi.Name;
		}

		/// <summary>
		/// Create a string containing the parameters to the base type constructor for a Type.
		/// </summary>
		/// <returns></returns>
		internal static string BaseParameterString(this Type t){
			if(t.BaseType == null){
				return string.Empty;
			}
			return string.Join(",\n\t\t\t\t", t.ValidBaseProperties().Select(p=>$"{p.Name.ValidParameterName()}"));
		}

		/// <summary>
		/// Create a string containing the parameters for a Type.
		/// </summary>
		/// <returns></returns>
		internal static string ParameterString(this Type t){
			return string.Join(",\n\t\t\t\t", t.ValidProperties().Concat(t.ValidBaseProperties()).Select(i=>$"{i.PropertyType.ValidTypeName()} {i.Name.ValidParameterName()}"));
		}

		internal static PropertyInfo[] ValidProperties(this Type t){
			return t.GetProperties().Where(p=> p.DeclaringType == t && !p.Name.EndsWith("Specified")).ToArray();
		}

		internal static PropertyInfo[] ValidBaseProperties(this Type t){
			return t.GetProperties().Where(p=> p.DeclaringType != t && 
						p.DeclaringType.Name != "Entity" &&
						p.DeclaringType.Name != "IfcRoot" && 
						!p.Name.EndsWith("Specified")).ToArray();
		}

		/// <summary>
		/// Create a string containing the inherited parameters.
		/// </summary>
		/// <returns></returns>
		internal static string InheritedParameterString(this Type t){
			if(t.BaseType == null){
				return string.Empty;
			}
			return string.Join(",\n\t\t\t\t", t.ValidBaseProperties().Select(i=>$"{i.PropertyType.ValidTypeName()} {i.Name.ValidParameterName()}"));
		}

				/// <summary>
		/// Create a multi-line string assigning parameters to properties.
		/// </summary>
		/// <returns></returns>
		internal static string FieldAssignments(this Type t){
			var fieldAssignments = string.Join(";\n", t.ValidProperties().Select(i=>$"\t\t\tthis.{i.ValidPropertyName()} = {i.Name.ValidParameterName()}"));
			return fieldAssignments + ";";
		}

		internal static string CapitalizeFirstLetter(this string s){
			return s.First().ToString().ToUpper() + String.Join("", s.Skip(1));
		}

		internal static string PropertiesString(this Type t){
			return string.Join("\n\n", t.ValidProperties().Select(i=>i.ToPropertyDeclaration()));
		}

		internal static string ToPropertyDeclaration(this PropertyInfo p){
			var propertyDecl = $"\t\tpublic {p.PropertyType.ValidTypeName()} {p.ValidPropertyName()} {{get;set;}}";
			return propertyDecl;
		}

		internal static Type[] ValidClasses(this Assembly asm){
			return asm.GetTypes().Where(t=>t.IsPublic && t.IsClass && t.Name != "Entity" && t.Name != "IfcRoot").ToArray();
		}

		internal static bool IsRelationship(this PropertyInfo pi){
			return pi.Name.StartsWith("Rel") || 
			pi.Name.EndsWith("By");
		}

		/// <summary>
		/// Create a string representing the definition of the class.
		/// </summary>
		/// <returns></returns>
		internal static string CSharpClassDefinition(this Type t){
			
			var isInherited = t.BaseType != null;

			var classSignature = isInherited?
				$"{t.ValidTypeName()} : {t.BaseType.ValidTypeName()}":
				$"{t.ValidTypeName()}";
			
			var parameterString = isInherited?
				t.ParameterString():
				$"{t.ParameterString()}, {t.InheritedParameterString()}";
			
			var constructorSignature = t.BaseType != null?
				$"({parameterString}) : base({t.BaseParameterString()})":
				$"({parameterString})";
			
			var classModifier = t.IsAbstract? "abstract" : string.Empty;

			var classStr = 
$@"/*
This code was generated by a tool. DO NOT MODIFY this code manually, unless you really know what you are doing.
 */
using System;
				
namespace IFC4
{{
	/// <summary>
	/// http://www.buildingsmart-tech.org/ifc/IFC4/final/html/link/{t.Name.ToLower()}.htm
	/// </summary>
	internal {classModifier} partial class {classSignature} 
	{{
{t.PropertiesString()}

		public {t.ValidTypeName()}{constructorSignature}
		{{
{t.FieldAssignments()}
		}}
	}}
}}";
			return classStr;
		}

		internal static string CSharpEnumDefinition(this Type t){
			
			var enumNames = string.Join(",\n\t\t",t.GetEnumNames().Select(n=>n.ToUpper()));
			var enumStr =
$@"/*
This code was generated by a tool. DO NOT MODIFY this code manually, unless you really know what you are doing.
 */
using System;
				
namespace IFC4
{{
	/// <summary>
	/// http://www.buildingsmart-tech.org/ifc/IFC4/final/html/link/{t.Name.ToLower()}.htm
	/// </summary>
	internal enum {t.ValidTypeName()} 
	{{
		{enumNames}
	}}
}}";
		return enumStr;

		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			if(args.Length != 2)
			{
				Console.WriteLine("Usage: IFC-dotnet-generate.exe <path to IFC dll> <output directory>");
				return;
			}

			if(!File.Exists(args[0]))
			{
				Console.WriteLine("The specified file does not exist.");
				return;
			}

			if(!Directory.Exists(args[1]))
			{
				Console.WriteLine("The specified output directory does not exist.");
			}

			var asm = Assembly.LoadFrom(args[0]);
			var types = asm.GetTypes().Where(t=>t.IsPublic && t.IsClass);
			foreach (var t in asm.ValidClasses())
			{
				var csPath = Path.Combine(args[1], $"{t.ValidTypeName()}.cs");
				File.WriteAllText(csPath, t.IsEnum? t.CSharpEnumDefinition() : t.CSharpClassDefinition());
			}
		}
	}
}