using System;
using System.Xml;
using System.Collections.Generic;

namespace cppsharp
{
	public class TypeConverter
	{
		public TypeConverter(cppsharp.CsharpGen gen)
		{
			_dataTypeMap = new Dictionary<string, ConvertEngine>();
			_generator = gen;



			_dataTypeMap.Add ("bool", new ConvertEngine("bool"));
			_dataTypeMap.Add ("char", new ConvertEngine("sbyte"));
			_dataTypeMap.Add ("unsigned char", new ConvertEngine("byte"));
			_dataTypeMap.Add ("short int", new ConvertEngine("short"));
			_dataTypeMap.Add ("short unsigned int", new ConvertEngine("ushort"));
			_dataTypeMap.Add ("int", new ConvertEngine("int"));
			_dataTypeMap.Add ("unsigned int", new ConvertEngine("uint"));
			_dataTypeMap.Add ("long long int", new ConvertEngine("long"));
			_dataTypeMap.Add ("long long unsigned int", new ConvertEngine("ulong"));
			_dataTypeMap.Add ("float", new ConvertEngine("float"));
			_dataTypeMap.Add ("double", new ConvertEngine("double"));
			_dataTypeMap.Add ("::std::string", new ConvertEngine("string"));

			/*
			// architecture dependant types
			if(_generator.Platform.Arch == PlatformInfo.ArchType.x86)
			{
				_dataTypeMap.Add ("long", new ConvertEngine("int"));
				_dataTypeMap.Add ("unsigned long", new ConvertEngine("uint"));
			}
			else if(_generator.Platform.Arch == PlatformInfo.ArchType.x64)
			{
				_dataTypeMap.Add ("long", new ConvertEngine("long"));
				_dataTypeMap.Add ("unsigned long", new ConvertEngine("long"));
			}
			else {}
			*/

		}

		public bool UseHandle(DataType type)
		{
			if(_dataTypeMap.ContainsKey(type.Namespace) || type.IsEnum)
				return false;

			return true;
		}

		public bool CSBAlloc(DataType type)
		{
			if(_dataTypeMap.ContainsKey(type.Namespace) || type.IsEnum)
				return false;
			
			return true;
		}

		public string CSESType(DataType type)
		{
			if(type == null)
				return null;
			
			if(type.IsPRPod)
				return _dataTypeMap[type.Namespace].CsType + " *";

			// if the input is a fundamental type convert it with the table
			if(_dataTypeMap.ContainsKey(type.Namespace))
				return _dataTypeMap[type.Namespace].CsType;

			if(type.IsEnum)
				return "long";

			// this will trigger true for class "string"
			if(type.IsPRVClass)
				return "IntPtr";

			return type.Name;
		}

		public string CSPType(DataType type)
		{
			// if the input is a fundamental type convert it with the table
			if(_dataTypeMap.ContainsKey(type.Namespace))
				return _dataTypeMap[type.Namespace].CsType;
			
			return type.NamespaceCs;
		}

		public string CPType(DataType type)
		{
			if(type.Namespace == "::std::string") return "MonoString *";
			if(type.IsEnum) return "long long int";
			if(type.IsPRVClass) return type.Namespace + " *";
			if(type.IsPRPod) return type.Namespace + " *";

			return type.Namespace;
		}

		public string CPCType(Arg arg)
		{
			string ret = "";

			if(arg.Type.IsEnum) ret = "(" + arg.Type.Namespace + ")";
			if(arg.Type.IsRVClass || arg.Type.IsRPod) ret = "*";
			if(arg.Type.Namespace == "::std::string")
				ret = "mono_string_to_utf8(" + arg.Name + ")";
			else
				ret = ret + arg.Name;

			return ret;
		}

		public string CPCRetCast(DataType arg, Attributes attr, string str)
		{
			if(arg == null)
				return str;

			string ret = "";
			if(!arg.isVoid)
				ret = "return ";

			if(arg.IsRClass)
				ret += "&";

			if(arg.IsEnum)
				ret += "(long long int)";

			if(arg.Namespace == "::std::string")
				ret += "mono_string_new(mono_domain_get(), " + str + ".c_str())";
			else if(arg.IsPVClass || (arg.IsPClass && !attr.NoDestructor))
				ret += "new " + arg.Namespace + "(" + (arg.isPointer ? "*" : "") + str + ")";
			else
				ret += str;

			return ret;
		}

		class ConvertEngine
		{
			public ConvertEngine(string cstype, string cpptype = "")
			{
				CsType = cstype;
				CppType = cpptype;
			}

			public ConvertEngine(string cstype, string cpptype, string prefix, string postfix)
			{
				CsType = cstype;
				CppType = cpptype;
				CPPrefix = prefix;
				CPPostfix = postfix;
			}

			public string CsType;
			public string CppType;
			// the code necessary to convert a c# type to a c++ type
			public string CPPrefix;
			public string CPPostfix;
		}

		/**
		 * Map used to convert c++ fundamental data types to c# data types. The key is the c++ type, the value is the
		 * c# type.
		 */
		Dictionary<string, ConvertEngine> _dataTypeMap;

		cppsharp.CsharpGen _generator;
	}



}
