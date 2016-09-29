using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace cppsharp
{
	public class CsharpGen
	{
		public class OpAttr
		{
			public OpAttr(string mangle, bool canDefine = true)
			{
				_mangleName = mangle;
				_canDefine = canDefine;
			}
			
			public string MangleName { get { return _mangleName; } }
			public bool CanDefine { get { return _canDefine; } }
			
			string _mangleName;
			bool _canDefine;
		}

		public CsharpGen (XmlTextReader reader)
		{
			reader_ = reader;
			contexts_ = new List<Context>();
			contextMap_ = new Dictionary<String, Context>();
			_types = new TypeMap();
			_platform = new PlatformInfo();
			_converter = new TypeConverter(this);
			_fileMap = new Dictionary<string, string>();
			_lastEnum = null;
			_enumMap = new Dictionary<string, Enumeration>();
			_files = new Dictionary<string, FileInfo>();
			setupOpMap();
		}

		static string DllImportStr(string dllImport)
		{
			return dllImport != null ? "[DllImport (" + dllImport + ")]" : 
				"[MethodImplAttribute(MethodImplOptions.InternalCall)]";
		}

		public static void writeCppsharp(string outDir, string dllImport)
		{
			// add the default boiler code
			FileInfo boiler = new FileInfo("cppsharp", outDir);
//			Files.Add("f" + Files.Count, boiler);

			/*
			StringWriter cHeaderBoilerWriter = boiler.CHeaderWriter;
			cHeaderBoilerWriter.WriteLine ("#ifndef CPPSHARP_H\n" +
			                               "#define CPPSHARP_H\n" +
			                               "\n" +
			                               "#if defined(__GCCXML__)\n" +
			                               "\t#define __nodtor		__attribute__((gccxml(\"nodtor\")))\n" +
			                               "\t#define __import		__attribute__((gccxml(\"import\")))\n" +
			                               "\t#define __export		__attribute__((gccxml(\"export\")))\n" +
			                               "\t#define __set(x)		__attribute__((gccxml(\"set,\" #x )))\n" +
			                               "\t#define __get(x)		__attribute__((gccxml(\"get,\" #x )))\n" +
			                               "#else\n" +
			                               "\t#define __nodtor\n" +
			                               "\t#define __import\n" +
			                               "\t#define __export\n" +
			                               "\t#define __set(x)\n" +
			                               "\t#define __get(x)\n" +
			                               "#endif\n" +
			                               "\n" +
			                               "#include<mono/metadata/object.h>\n" +
			                               "#include<mono/metadata/mono-config.h>\n" +
			                               "#include<mono/jit/jit.h>\n" +
			                               "#include <mono/metadata/debug-helpers.h>\n" +
			                               "\n" +
			                               "namespace cppsharp\n" +
			                               "{\n" +
			                               "\tclass Object\n" +
			                               "\t{\n" +
			                               "\tpublic:\n" +
			                               "\t\tObject() {\n" +
			                               "\t\t\t_domain = mono_domain_get();\n" +
			                               "\t\t\t//MonoMethodDesc* onCreateDesc = mono_method_desc_new(\"cppsharp.Object::OnCreate()\", true);\n" +
			                               "\t\t\t//_onCreateMethod = mono_method_desc_search_in_class(onCreateDesc, _class);\n" +
			                               "\t\t}\n" +
			                               "" +
			                               "" +
			                               "" +
			                               "\t\tMonoClass* getMonoClass() { return _class; }\n" +
			                               "\t\tvoid setMonoClass(MonoClass* clas)\n" +
			                               "\t\t{\n" +
			                               "\t\t\t_class = clas;\n" +
			                               "\t\t\tMonoMethodDesc* onCreateDesc = mono_method_desc_new(\"cppsharp.Object::OnCreate()\", true);\n" +
			                               "\t\t\t_onCreateMethod = mono_method_desc_search_in_class(onCreateDesc, _class);\n" +
			                               "\t\t}\n" +
			                               "\t\tMonoDomain* getMonoDomain() { return _domain; }\n" +
			                               "\t\tvoid setMonoDomain(MonoDomain* domain) { _domain = domain; }\n" +
			                               "\t\tvoid OnCreate() {\n" +
			                               "\t\t\tmono_runtime_invoke(_onCreateMethod, _class, NULL, NULL);\n" +
			                               "\t\t}\n" +
			                               "\t\tvoid OnUpdate() {\n" +
			                               "\t\t\t\n" +
			                               "\t\t}\n" +
			                               "\tprivate:\n" +
			                               "\t\tMonoClass* _class;\n" +
			                               "\t\tMonoDomain* _domain;\n" +
			                               "\t\tMonoMethod* _onCreateMethod;\n" +
			                               "\t\tMonoMethod* _onUpdateMethod;\n" +
			                               "\t};\n" +
			                               "}\n" +
			                               "\n" +
			                               "#ifdef __cplusplus\n" +
			                               "extern \"C\" {\n" +
			                               "#endif\n" +
			                               "\n" +
			                               "void _cppsharp_Object_setMonoObject(::cppsharp::Object* __arg, MonoObject* obj);\n" +
			                               "\n" +
			                               "#ifdef __cplusplus\n" +
			                               "}\n" +
			                               "#endif\n" +
			                               "#endif\n");
			                               */

			/*
			StringWriter cSourceBoilerWriter = boiler.CSourceWriter;
			cSourceBoilerWriter.WriteLine ("#include<cppsharp.h>\n" +
			                               "\n" +
			                               "void _cppsharp_Object_setMonoObject(::cppsharp::Object* __arg, MonoObject* obj)\n" +
			                               "{\n" +
			                               "\t__arg->setMonoClass(mono_class_from_mono_type(*(MonoType **) mono_object_unbox(obj)));\n" +
			                               "}\n");
			                               */
			
			StringWriter csBoilerWriter = boiler.CsWriter;
			csBoilerWriter.WriteLine ("using System;\n" +
			                          "using System.Runtime.CompilerServices;\n" +
			                          "\n" +
			                          "namespace cppsharp\n" +
			                          "{\n" +
			                          "\tpublic class Object\n" +
			                          "\t{\n" +
			                          "\t\t//" + DllImportStr(dllImport) + "\n" +
			                          "\t\t//protected extern static void _cppsharp_Object_setMonoObject(IntPtr __arg, IntPtr obj);\n" +
			                          "\n" +
			                          "\t\t//protected IntPtr CppHandle { get { return __handle; } set { __handle = value; } }\n" +
			                          "\t\t//protected bool CppFree { get { return __free; } set { __free = value; } }\n" +
			                          "" +
			                          "\t\tpublic virtual void OnCreate() { Console.WriteLine(\"OnCreate()\"); }\n" +
			                          "\t\tpublic virtual void OnUpdate() {}\n" +
			                          "\n" +
			                          "\t\tprotected IntPtr CppHandle;\n" +
			                          "\t\tprotected bool CppFree;\n" +
			                          "\t}\n" +
			                          "}\n");

			/*
			StringWriter mainBoilerWriter = boiler.MainWriter;
			if(dllImport == null)
			{
				mainBoilerWriter.WriteLine ("#include<cppsharp.h>");
				mainBoilerWriter.WriteLine ("mono_add_internal_call (\"cppsharp.Object::_cppsharp_Object_setMonoObject\"," +
				                            " _cppsharp_Object_setMonoObject);");
			}
			*/

			boiler.WriteFiles ();
		}

		void setupOpMap()
		{
			_opMap = new Dictionary<string, OpAttr>();
			_opMap.Add ("+", 		new OpAttr("operator_add"));
			_opMap.Add ("-", 		new OpAttr("operator_subtract"));
			_opMap.Add ("!", 		new OpAttr("operator_logical_complement"));
			_opMap.Add ("~", 		new OpAttr("operator_binary_complement"));
			_opMap.Add ("++", 		new OpAttr("operator_increment"));
			_opMap.Add ("--", 		new OpAttr("operator_decrement"));
			_opMap.Add ("true", 	new OpAttr("operator_true"));
			_opMap.Add ("false", 	new OpAttr("operator_false"));
			_opMap.Add ("*", 		new OpAttr("operator_multiply"));
			_opMap.Add ("/", 		new OpAttr("operator_divide"));
			_opMap.Add ("%", 		new OpAttr("operator_percent"));
			_opMap.Add ("&", 		new OpAttr("operator_addr_and"));
			_opMap.Add ("|", 		new OpAttr("operator_binary_or"));
			_opMap.Add ("^", 		new OpAttr("operator_binary_xor"));
			_opMap.Add ("<<", 		new OpAttr("operator_left_shift"));
			_opMap.Add (">>", 		new OpAttr("operator_right_shift"));
			_opMap.Add ("==", 		new OpAttr("operator_equals"));
			_opMap.Add ("!=", 		new OpAttr("operator_not_equals"));
			_opMap.Add ("<", 		new OpAttr("operator_less_than"));
			_opMap.Add (">", 		new OpAttr("operator_greater_than"));
			_opMap.Add ("<=", 		new OpAttr("operator_less_than_equal"));
			_opMap.Add (">=", 		new OpAttr("operator_greater_than_equal"));
			_opMap.Add ("&&", 		new OpAttr("operator_logical_and"));
			_opMap.Add ("||", 		new OpAttr("operator_logical_or"));
			_opMap.Add ("[]", 		new OpAttr("operator_index"));
			_opMap.Add ("()", 		new OpAttr("operator_cast"));
			_opMap.Add ("+=", 		new OpAttr("operator_add_set", false));
			_opMap.Add ("-=", 		new OpAttr("operator_subtract_set", false));
			_opMap.Add ("*=", 		new OpAttr("operator_multiply_set", false));
			_opMap.Add ("/=", 		new OpAttr("operator_divide_set", false));
			_opMap.Add ("%=", 		new OpAttr("operator_modulo_set", false));
			_opMap.Add ("&=", 		new OpAttr("operator_and_set", false));
			_opMap.Add ("|=", 		new OpAttr("operator_or_set", false));
			_opMap.Add ("^=", 		new OpAttr("operator_xor_set", false));
			_opMap.Add ("<<=", 		new OpAttr("operator_left_shift_set", false));
			_opMap.Add (">>=", 		new OpAttr("operator_right_shift_set", false));
			_opMap.Add ("=", 		new OpAttr("operator_set", false));
			_opMap.Add (".", 		new OpAttr("operator_member"));
		}
		
		public void process ()
		{
			_types.Add (new UnknownType ());
			while(reader_.Read())
			{
				if(reader_.NodeType == XmlNodeType.Element)
				{
					switch(reader_.Name)
					{
					case "Namespace":			addContext(reader_.Name); break;
					case "Class":				addContext(reader_.Name); break;
					case "Struct":				addContext(reader_.Name); break;
					case "Union":				addContext(reader_.Name); break;

					case "Constructor":			addMethod(reader_.Name); break;
					case "Destructor":			addMethod(reader_.Name); break;
					case "Method":				addMethod(reader_.Name); break;
					case "OperatorFunction":	addMethod(reader_.Name); break;
					case "OperatorMethod":		addMethod(reader_.Name); break;
					case "Function":			addMethod(reader_.Name); break;
					
					case "Argument":			addArguement(); break;

					case "Field":				Field f = new Field(reader_, this); ContextMap[f.ContextId].Fields.Add(f); break;

					case "Typedef":				_types.Add (new Typedef(reader_)); break;
					case "FundamentalType":		_types.Add (new FundamentalType(reader_)); break;
					case "PointerType": 		_types.Add (new PointerType(reader_)); break;
					case "ReferenceType": 		_types.Add (new ReferenceType(reader_)); break;
					case "CvQualifiedType":		_types.Add (new CvQualifiedType(reader_)); break;
					case "FunctionType":		
					{
						FunctionType ftype = new FunctionType(reader_);
						_types.Add (ftype);
						lastFunction_ = ftype;
						break;
					}
						
					case "File":				
					{
						FileMap.Add(reader_["id"], reader_["name"]);
						FileInfo info = new FileInfo(reader_, OutDir);
						Files.Add(info.Id, info);
						break;
					}
						
					case "EnumValue":			_lastEnum.addValue(new Enumeration.Value(reader_)); break;
					case "Enumeration":			addEnumeration(); break;
						
//					default:	Console.WriteLine ("could not parse \"" + reader_.Name + "\""); break;
					}
				}
			} // while
			postProcess();
		}// process()

		void postProcess()
		{
			// post process the enumerations, enumerations are processes before the contexts they exist in
			foreach(KeyValuePair<string, Enumeration> en in _enumMap)
			{
				en.Value.postProcess();
			}
			
			// process the attributes
			Context root = contextMap_ ["_1"];
			root.postProcess();
		}

		public string CSAttr { get { return DllImport != null ? "[DllImport (" + DllImport + ")]" : 
					"[MethodImplAttribute(MethodImplOptions.InternalCall)]"; } }
		
		public void write ()
		{
			// iterate over the source list and make sure the list only contains distinct strings
			List<string> sourceFiles = new List<string>(_sourceFiles);
			for(int i=sourceFiles.Count-1; i>=0; i--)
				for(int j=0; j<i; j++)
					if(sourceFiles[i] == sourceFiles[j])
						sourceFiles.RemoveAt(i);

			// write the opening statements for the c header/source files, c# file and main file
			foreach(KeyValuePair<string, FileInfo> info in Files)
			{
				StringWriter cHeaderFile = info.Value.CHeaderWriter;
				StringWriter cSourceFile = info.Value.CSourceWriter;
				StringWriter csFile = info.Value.CsWriter;
				StringWriter mainFile = info.Value.MainWriter;
				string cFileName = info.Value.CHeaderFileName;
				string file = info.Value.FileName;

				// write the opening part of the main file
				mainFile.WriteLine ("#include<" + cFileName + ">");
				cSourceFile.WriteLine ("#include<" + info.Value.CHeaderFileName + ">");
				
				// write the header includes for the c file
				cHeaderFile.WriteLine ("#include<mono/metadata/object.h>");
				cHeaderFile.WriteLine ("#include<mono/metadata/appdomain.h>");
				if(file.EndsWith (".h") || file.EndsWith (".hpp"))
					cHeaderFile.WriteLine ("#include<" + file + ">\n");
				
				cHeaderFile.WriteLine ("#if defined(__cplusplus)");
				cHeaderFile.WriteLine ("extern \"C\" {");
				cHeaderFile.WriteLine ("#endif\n");
				
				// write the using declarations for the cs file
				csFile.WriteLine ("using System;");
				csFile.WriteLine ("using System.Runtime.CompilerServices;\n");
				csFile.WriteLine ("using System.Reflection;\n");

				//*************************************************************
				// resolve dependencies
				//*************************************************************
				// get the list sources that include the current header in this loop

				List<string> scanSources = new List<string>();
				foreach(string fileName in sourceFiles)
				{
					string src = File.ReadAllText(fileName);
					if(src.Contains(info.Value.FileName))
						scanSources.Add(fileName);
				}
				List<string> extraIncludes = new List<string>();

				// iterate over the sources and get the header files
				foreach(string fileName in scanSources)
				{
					Match match = null;
					string fileData = File.ReadAllText(fileName);
					string regex = "#include\\s*([\"<])([a-zA-Z0-9_\\.]*)([\">])([\\s\\S]*)";
					while((match = Regex.Match(fileData, regex)).Success)
					{
						fileData = match.Groups[4].ToString();
						string header = match.Groups[2].ToString();
						extraIncludes.Add ("#include" + "<" + header + ">");
					}
				}

				// iterate over the header list and make sure the list only contains distinct strings
				int i=extraIncludes.Count;
				for(i=i-1; i>=0; i--)
					for(int j=0; j<i; j++)
						if(extraIncludes[i] == extraIncludes[j])
						{
							extraIncludes.RemoveAt(i);
							break;
						}

				// write the necessary headers to the generated c file
				foreach(string header in extraIncludes)
					cSourceFile.WriteLine (header);
				cSourceFile.WriteLine("");
			}

			// recurse over all the contexts, functions, etc to write them to the string buffers
			Context root = contextMap_ ["_1"];
			for (int i = 0; i < root.Contexts.Count; i++)
			{
				Context cur = root.Contexts [i];
				if (cur.Generate) cur.write();
			}

			// write the closing statement for the c files
			foreach(KeyValuePair<string, FileInfo> info in Files)
			{
				info.Value.CHeaderWriter.WriteLine ("#if defined(__cplusplus)");
				info.Value.CHeaderWriter.WriteLine ("}");
				info.Value.CHeaderWriter.WriteLine ("#endif\n");
			}

			// flush the string buffers to their respective files
			foreach(KeyValuePair<string, FileInfo> info in Files)
				info.Value.WriteFiles();
		} // write()
		
		void addEnumeration()
		{
			Enumeration tmp = new Enumeration(reader_, this);
			_enumMap.Add(tmp.Id, tmp);
			Types.Add (tmp);
			
			_lastEnum = tmp;
		}
		
		void addArguement()
		{
			if(lastFunction_ != null)
			{
				Arg arg = new Arg(reader_, this);
				if(lastFunction_.Args != null)
					lastFunction_.Args.Add(arg);
				else
					lastFunction_.Args = arg;
			}
		}

		void addContext(String type)
		{
			Context con = new Context(reader_, this);
			
			if(con.Id == null) return;
			
			if(con.ContextId != null)
			{
				if (contextMap_.ContainsKey(con.ContextId)) {
					// add the context to the ordered list
					Context oldcon = contextMap_[con.ContextId];
					oldcon.Contexts.Add(con);
				}
			}
			
			// add the context to the serial list.
			contextMap_.Add(con.Id, con);
			contexts_.Add(con);
			
			// add this as a type to the type map
			//			typeMap_.Add(con.Id, new Function.Type(con, this));
			_types.Add(con);
		}
		
		void addMethod(String type)
		{
			Function func;// = new Function(type, reader_, this);
			
			switch(type)
			{
			case "Constructor":		func = new Constructor(type, reader_, this); break;
			case "Destructor":		func = new Destructor(type, reader_, this); break;
			default:				func = new Function(type, reader_, this); break;
			}
			
			Context con = contextMap_[func.ContextId];
			con.Functions.Add(func);
			lastFunction_ = func;
		}

		// accessors
		public Dictionary<String, Context> ContextMap { get { return contextMap_; } }
		public TypeMap Types { get { return _types; } }
		public PlatformInfo Platform { get { return _platform; } }
		public TypeConverter Converter { get { return _converter; } }
		public Dictionary<string, string> FileMap { get { return _fileMap; } }
		public string DllImport { get { return _dllImport; } set { _dllImport = value; } }
		public string XmlFile { get { return _xmlFile; } set { _xmlFile = value; } }
		public Dictionary<string, FileInfo> Files { get { return _files; } }
		public List<string> SourceFiles { get { return _sourceFiles; } set { _sourceFiles = value; } }
		public Dictionary<string, OpAttr> OpMap { get { return _opMap; } }
		public string OutDir { get; set; }

		// the source files for the header we are generating an interface for
		List<string> _sourceFiles;

		Dictionary<string, OpAttr> _opMap;

		Dictionary<string, FileInfo> _files;

		CompileObject lastFunction_;
		
		PlatformInfo _platform;
		
		TypeConverter _converter;

		/**
		 * The path to the xml file.
		 */
		string _xmlFile;

		/**
		 * If this is null internal call will be used. The executable will try to resolve the c functions
		 * statically linked into the executable. Otherwise this must be set to the name of a dll or so (*nix)
		 * where the c functions can be found.
		 */
		string _dllImport;
		
		/**
		 * Serial list of contexts as found in the cpp source file. This is used to preserve the order of items for
		 * generation of the c# classes.
		 */
		List<Context> contexts_;
		
		/**
		 * Loopup map to find existing classes/namespaces easily, this is much easier than iteration over an array.
		 */
		Dictionary<String, Context> contextMap_;
		
		TypeMap _types;
		
		Dictionary<string, Enumeration> _enumMap;
		
		/**
		 * This is the xml file gccxml produced. Just need to parse this, load it into a data structure and walk it
		 * to generate the necessary interfaces for the c#/cpp bridge.
		 */
		XmlTextReader reader_;
		
		/* Map of the files that were compiled. This is useful for error reporting.
		 */
		Dictionary<string, string> _fileMap;
		
		/**
		 * This is the attribute we will search for when checking to see if a respective c# interface needs to
		 * be generated for a function.
		 */
		static public String GenString = "deprecated";
		static public String SetString = "set";
		static public String GetString = "get";
		
		Enumeration _lastEnum;
		
	}// class CsharpGen
}

