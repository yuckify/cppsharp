using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace cppsharp
{
	public class Context : ContextObject
	{
		public Context(XmlTextReader reader, CsharpGen generator)
			: base(reader, generator)
		{
			GenStructors = false;
			if(Attr.Export)
				GenStructors = true;
			_typedef = null;
			
			switch(reader.Name)
			{
			case "Class":		type_ = "class"; break;
			case "Struct":		type_ = "struct"; break;
			case "Namespace":	type_ = "namespace"; break;
			case "Union":		type_ = "union"; break;
			default:			Console.WriteLine("cannot process context type \"" + reader.Name + "\""); break;
			}

			_bases = new List<string>();
			string baseList = reader["bases"];
			if(baseList != null && baseList.Length != 0)
			{
				string[] baseSplit = baseList.Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);
				foreach(string str in baseSplit)
					_bases.Add(str);
			}

			functions_ = new List<Function>();
			contexts_ = new List<Context>();
			_enums = new List<Enumeration>();
			_fields = new List<Field>();

			if(Name == "Mathf")
				Console.Write ("");
		}

		public bool Generate { get {
				for(int i = 0; i < functions_.Count; i++)
					if(functions_[i].Generate) return true;
				
				for(int i = 0; i < contexts_.Count; i++)
					if(contexts_[i].Generate) return true;
				
				return false;
			} }

		public void postProcess()
		{
			Attr.validate(this, true, false, false, false, false);

			for(int i=0; i<Contexts.Count; i++)
				Contexts[i].postProcess();

			for(int i=0; i<Functions.Count; i++)
				Functions[i].postProcess();

			for(int i=0; i<Fields.Count; i++)
				Fields[i].postProcess();
		}

		public string MangleNamespace { get { 
				if(ContextId != null) return CC.ContextMap[ContextId].MangleNamespace + "_" + Name;
				return null;
			} }

		string OpenNamespaceCs(DataType t)
		{
			if(t.ContextId==null || t.isNamespace) return "";
			return OpenNsCsImpl(CC.Types[t.ContextId]);
		}

		string OpenNsCsImpl(DataType t)
		{
			if(t==null || t.ContextId==null || t.isClass) return "";
			return OpenNsCsImpl(CC.Types[t.ContextId]) + "namespace " + t.Name +"\n{\n";
		}

		string CloseNamespaceCs(DataType t)
		{
			if(t.ContextId==null || t.isNamespace) return "";
			return CloseNsCsImpl(CC.Types[t.ContextId]);
		}

		string CloseNsCsImpl(DataType t)
		{
			if(t==null || t.ContextId==null || t.isClass) return "";
			return CloseNsCsImpl(CC.Types[t.ContextId]) + "}\n";
		}

		public void write ()
		{
			StringWriter csFile = null;
			if(File != null) csFile = CC.Files[File].CsWriter;

			// set to flush the buffer for this context if this context has stuff to be generated
			if(File != null && CC.Files.ContainsKey (File))
				CC.Files[File].Write = this.Generate;

			//**************************************************
			// write c file
			//**************************************************
			// write functions in the context
			for (int i = 0; i < functions_.Count; i++) {
				Function func = functions_ [i];
				if (func.Generate)
					func.writeC ();
			}

			DataType type = CC.Types[Id];
			if(csFile != null) csFile.Write(OpenNamespaceCs(type));
			if(type.isClass) csFile.Write ("public ");
			if(csFile != null) csFile.WriteLine ("class " + Name + " : cppsharp.Object {");

			// write the enumerations
			foreach(Enumeration en in _enums)
				if(en.IsPublic)
					en.write ();

			// recurse into contexts within this context
			for (int i = 0; i < contexts_.Count; i++) {
				Context cont = contexts_ [i];
				if (cont.Generate)
					cont.write ();
			}

			//**************************************************
			// write cs file
			//**************************************************
			if (type.isClass)
			{
				csFile.WriteLine ("\npublic " + Name + "(IntPtr amp, bool dtor = false)\n{\n\tCppHandle = amp;\n\tCppFree = dtor;\n}\n");
			}

			// write cs functions in the context
			for(int i = 0; i < functions_.Count; i++)
			{
				Function func = functions_[i];
				if(func.Generate)
				{
					func.writeCS();
					func.writeMain ();
				}
			}

			if(csFile != null) csFile.WriteLine ("}");
			if(csFile != null) csFile.WriteLine (CloseNamespaceCs(type));
		}

		public void addEnumeration(Enumeration en)
		{
			_enums.Add(en);
		}
		
		public void addFunction(Function f)
		{
			functions_.Add(f);
		}

		public List<Function> Functions { get { return functions_; } }
		public List<Context> Contexts { get { return contexts_; } }
		public List<Field> Fields { get { return _fields; } }
		public bool GenStructors { get { return genStructors_; }
			set { genStructors_ = value; } }
		public bool isClass { get { if(type_ != null) return type_ == "class" || type_ == "struct"; return false; } }
		public bool isNamespace { get { return type_ == "namespace"; } }
		public List<Enumeration> Enums { get { return _enums; } }
		public bool Export { get { return Attr.Export; } set { GenStructors = value; } }
		public override Arg Args { get { return null; } set { } }

		String type_; // the type of the context, class, struct, namespace, union
		DataType _typedef;
		List<string> _bases;
		
		List<Function> functions_; // the functions in this context
		List<Context> contexts_; // the contexts in this context
		List<Enumeration> _enums; // enumerations in the context
		List<Field> _fields; // the variables declared in this context

		bool genStructors_;
	} // class Context

	/*
	public class Namespace : Context
	{
		public Namespace()
		{

		}
	}

	public class Struct : Context
	{
		public Struct()
		{
			
		}
	}

	public class Class : Context
	{
		public Class()
		{
			
		}
	}

	public class Union : Context
	{
		public Union()
		{
			
		}
	}
	*/

}
