using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace cppsharp
{
	/*
	public class LastFunctionObject {
		virtual public Function.Arg Args { get { return null; } }

	}
	*/


	/**
	 * 
	 * 
	 */
	public class Function : CompileObject
	{
		public Function(Function func)
		{
			_cc = func._cc;
			_id = func._id;
			_name = func._name;
			_access = func._access;
			_context = func._context;
			_returnId = func._returnId;
			_file = func._file;
			_line = func._line;
			_attr = new Attributes(func._attr);
			_static = func._static;
			_head = func._head;
			_artificial = func._artificial;
			_memberFunc = false;
		}

		public Function(String type, XmlTextReader reader, CsharpGen cc)
		{
			_cc = cc;
			_id = reader["id"];
			_name = reader["name"];
			_access = reader["access"];
			if(_access == null) _access = "public";
			_context = reader["context"];
			_returnId = reader["returns"];
			_file = reader["file"];
			_line = reader["line"];
			_memberFunc = true;

			_artificial = false;
			string artificial = reader["artificial"];
			if(artificial != null)
				if(artificial == "1")
					_artificial = true;

			//*****************************************************************
			//  setup the attributes for this function
			_attr = new Attributes(reader);
			if(_attr.Export)
			{
				if(_context != null)
					CC.ContextMap[_context].GenStructors = true;
			}

			// TODO handle __import

			String staticc = reader["static"];
			_static = false;
			if(staticc != null)
				if(staticc == "1") _static = true;
		}

		public Function(Field field)
		{
			_cc = field.CC;
			_id = field.Id;
			_name = field.Name;
			_access = "public";
			_context = field.ContextId;
			if(field.Attr.Get != null)
				_returnId = field.TypeId;
			else
				_returnId = CC.Types.VoidTypeId;
			_file = field.File;
			_line = field.Line;
			_memberFunc = true;
			_artificial = false;
			_attr = field.Attr;
			if(_attr.Get != null || _attr.Set != null)
			{
				if(_context != null)
					CC.ContextMap[_context].GenStructors = true;
			}
			_static = false;
			_head = field.Args;
		}

		string cvt(DataType t) { return CC.Converter.CSPType(t); }

		// CP = Cpp Public
		protected string CPRet { get { return CC.Converter.CPType(Return) + " "; } }
		string CPName { get { return MangleNamespace + "_" + MangleName; } }
		string CPThisPtr { get { return CC.Types[ContextId].ResolvedName + " * __arg"; } }
		bool CPThisPtrCheck { get { return !IsOperator || IsMemberFunc; } }
		protected string CPOpArg { get { return CPThisPtrCheck ? CPThisPtr : ""; } }
		protected virtual string CPArgs { get { return CPOpArg + (CPThisPtrCheck&&Args!=null ? ", " : "") + 
				(Args!=null ? Args.CPArgs : ""); } }
		string CPFunc { get { return CPRet + CPName + "(" + CPArgs + ")"; } }

		// CPC = Cpp Public Call
		string CPCFuncOp { get { return (IsOperator ? "operator " : ""); } }
		string CPCMembFunc { get { return (IsMemberFunc ? "__arg->" : ""); } }
		protected string CPCFuncName { get { return CPCMembFunc + CPCFuncOp + Name; } }
		protected string CPCAlloc(string str) { return CC.Converter.CPCRetCast(Return, Attr, str); }
		protected string CPCArgs(Arg a) { return a!=null ? (CC.Converter.CPCType(a) + 
			                                                    (a.Next!=null ? ", " : "") + CPCArgs(a.Next)) : ""; }
		protected string StaticNS { get { return (Static ? ContextType.Namespace + "::" : ""); } }
		string CPCFunc { get { return CPCAlloc(StaticNS + CPCFuncName + "(" + CPCArgs(Args) + ")") + ";"; } }

		public virtual void writeC()
		{
			if(Access != "public")
			{
				Console.WriteLine (DebugTag + " : warning cannot generate interface for non-public function.");
				return;
			}
			if(_artificial && !isConstructor)
			{
				Console.WriteLine (DebugTag + " : warning cannot generate interface for artificial function.");
				return;
			}
			if((Args != null && Args.HasPREnum) || (ReturnId != null && Return.IsPREnum))
			{
				Console.WriteLine (DebugTag + " : error can only pass enum by value not by pointer or reference.");
				return;
			}
			if(Args != null && Args.HasPRPod)
			{
				Console.WriteLine (DebugTag + " : warning DO NOT store a pointer to a fundamental type from c# space.");
			}
			if(CC.OpMap.ContainsKey(Name) && !CC.OpMap[Name].CanDefine)
			{
				Console.WriteLine (DebugTag + " : warning cannot generate interface for \"operator " + Name + "\"." +
					" Operator is not overloadable.");
				return;
			}
			if(Args != null && !Args.DistinctArgNames)
			{
				Console.WriteLine (DebugTag + " : error arguements must have names and must be distinct." +
					" Cannot generate interface.");
				return;
			}

			CHeaderWriter.WriteLine (CPFunc + ";");
			CSourceWriter.WriteLine (CPFunc);
			CSourceWriter.WriteLine ("{\n" + CPCFunc + "\n}\n");
		}

		// ES = Extern Static
		string MangleNamespace { get { return CC.ContextMap[ContextId].MangleNamespace; } }
		string ESUnsafe { get { return Args!=null ? (Args.HasPRPod ? "unsafe " : null) : ""; } }
		string ESQualifiers { get { return ESUnsafe + "extern static "; } }
		virtual protected string ESRet { get { return CC.Converter.CSESType(Return); } }
		public string ESName { get { return MangleNamespace + "_" + MangleName; } }
		string ESThisPtr { get { return (IsMemberFunc ? "IntPtr __arg" + (Args!=null ? ", " : "") : ""); } }
		protected string ESFunc { get { return ESQualifiers + ESRet + " " + ESName + "(" + 
				ESThisPtr + (Args!=null ? Args.ESArgs : "") + ");"; } }

		// CSP = C Sharp Public
		string CSPUnsafe { get { return Args!=null ? (Args.HasPRPod ? "unsafe " : "") : ""; } }
		string CSPStatic { get { return ((Static || IsOperator) && Name!="[]" ? "static " : ""); } }
		string CSPQualifiers { get { return CSPUnsafe + CSPStatic + "public "; } }
		string CSPOpName { get { return CC.OpMap.ContainsKey (Name) ? "operator" : ""; } }
		string CSPFuncName { get { return (Name=="[]" ? "this" : CSPOpName + " " + Name); } }
		virtual protected string CSPRet { get { return cvt(Return) + " "; } }
		string CSPFirstArg { get { return (IsMemberFunc && IsOperator ? ContextType.NamespaceCs + " __arg" + (Args!=null ? ", " : "") : ""); } }
		string CSPArgs { get { return "(" + CSPFirstArg + (Args!=null ? Args.CSPArgs : "") + ")"; } }
		string CSPArgsIndex { get { return "[" + (Args!=null ? Args.CSPArgs : "") + "]"; } }
		protected string CSPFunc { get { return CSPQualifiers + CSPRet + CSPFuncName + (Name=="[]" ? CSPArgsIndex : CSPArgs); } }

		// CSB = C Sharp Body
		string CSBFixedWrite(Arg a, int i) { return "fixed("+cvt (a.Type)+" * _"+i+" = &"+a.Name+")\n"; }
		string CSBFixedArg(Arg a, int i=0) { return (a.HasPRPod ? CSBFixedWrite(a, i) : "") + 
			(a.Next!=null ? CSBFixedArg(a.Next,i+1) : ""); }
		string CSBFixedArgs(string str) { return CSBFixedArg(Args) + "{\n" + str + "\n}\n"; }
		string CSBFixed(string str) { return ((Args!=null ? Args.HasPRPod : false) ? CSBFixedArgs(str) : str); }

		// CSC = C Sharp Call
		bool CSCNewCheck { get { return CC.Converter.CSBAlloc(Return); } }
		string CSCComma { get { return (Args!=null ? ", " : ""); } }
		string CSCPRValue(string str) { return (CSCNewCheck ? "return new " + cvt(Return) + "(" + str + ")" : "return " + str); }
		virtual protected string CSCRet(string str) {  return (Return.isVoid ? str : CSCPRValue(str)); }
		virtual protected string CSCEnum { get { return (Return.IsEnum ? "(" + Return.NamespaceCs + ")" : ""); } }
		string CSCMember { get { return (IsMemberFunc ? "CppHandle" + CSCComma : ""); } }
		string CSCOp { get { return (IsOperator && IsMemberFunc ? "__arg.CppHandle" + CSCComma : CSCMember); } }
		string CSCFirstArg { get { return (Name=="[]" ? "CppHandle" + CSCComma : CSCOp); } }
		string CSCArgEnum(Arg a) { return (a.Type.IsEnum ? "(long)" : ""); }
		string CSCArgFixed(Arg a, int i) { return (a.Type.IsPRPod ? "_" + i : CSCArgEnum(a) + a.Name); }
		string CSCArgClass(Arg a, int i) { return (CC.Converter.UseHandle(a.Type) ? a.Name + ".CppHandle" : CSCArgFixed(a, i)); }
		string CSCArg(Arg a, int i=0) { return (a.Next!=null ? CSCArgClass(a, i) + ", " + CSCArg(a.Next, i+1) : CSCArgClass(a, i)); }
		string CSCArgs { get { return CSCFirstArg + (Args!=null ? CSCArg(Args) : ""); } }
		string CSCIndexerGet(string str) { return (Name=="[]" ? "get\n{\n" + str + "\n}" : str); }
		string CSCIndexerSet(string str) { return (Return.IsPR && Name=="[]" ? "set\n{\n" + str + "\n}\n" : ""); }
		protected string CSBFunc { get { return CSBFixed(CSCIndexerGet(CSCRet(CSCEnum + ESName + "(" + CSCArgs + ")") + ";")); } }

		protected string CSAttr { get { return CC.CSAttr; } }

		public virtual void writeCS()
		{
			if(Access != "public") return;
			if(_artificial && !isConstructor) return;
			if(CC.OpMap.ContainsKey(Name) && !CC.OpMap[Name].CanDefine) return;
			if(Args != null && !Args.DistinctArgNames) return;
			if((Args != null && Args.HasPREnum) || (ReturnId != null && Return.IsPREnum)) return;

			// write the connection function
			CsWriter.WriteLine (CSAttr);
			CsWriter.WriteLine (ESFunc);

			if(!((CC.ContextMap[_context].Attr.Export || _attr.Export) && 
			   _attr.Get == null && _attr.Set == null)) return;

			CsWriter.WriteLine (CSPFunc + "\n{");
			CsWriter.WriteLine (CSBFunc + "\n}\n");
		}

		public virtual void writeMain()
		{
			if(Access != "public") return;
			if(_artificial && !isConstructor) return;
			if(CC.OpMap.ContainsKey(Name) && !CC.OpMap[Name].CanDefine) return;
			if(Args != null && !Args.DistinctArgNames) return;
			if((Args != null && Args.HasPREnum) || (ReturnId != null && Return.IsPREnum)) return;

			if(CC.DllImport == null)
			{
				MainWriter.Write ("mono_add_internal_call (\"");
				MainWriter.WriteLine (ContextType.NamespaceCs + "::" + ESName + "\", " + ESName + ");");
			}
		}

		public void postProcess()
		{
			if(IsOperator && !_artificial)
			{
				// check if this operator was declared globally, if so generate a new function inside
				// the class it is operating on
				string id = (Args==null ? null : Args.Type.ClassId);
				if(ContextId != id && id != null && Args.Count == 2)
				{
					if(CC.ContextMap.ContainsKey (id))
					{
						// this function is globally defined, add it to the context it is operating on
						Context cont = CC.ContextMap[id];
						Function func = new Function(this);
						func.ContextId = cont.Id;
						func.Attr.Export = cont.Export;
						cont.Functions.Add (func);

					}
				}
			}

			setupProperties();
		}

		public void setupProperties()
		{
			_attr.validate(this, true, true, true, true, true);

			if((_attr.Get != null || _attr.Set != null) && !isProperty && Access == "public")
			{
				// add the function to the parent context
				Context cont = CC.ContextMap[_context];
				Function propertyFunc = null;

				//*************************************************************
				// do some error checking by counting the number of 
				int getCount = 0;
				int setCount = 0;
				for(int i=0; i<cont.Functions.Count; i++) {
					Function func = cont.Functions[i];
					if(func.Attr.Name == _attr.Name)
					{
						if(func.Attr.Get != null)
							getCount++;
						if(func.Attr.Set != null)
							setCount++;
					}
				}

				if(getCount > 2 || setCount > 2)
				{
					Console.WriteLine (DebugTag + " error too many Properties being generated with name \"" +
					                   _attr.Name + "\" in class \"" + CC.ContextMap[_context].Name + "\"");
					return;
				}

				//*************************************************************
				// get the property function associated with this exported function
				for(int i=0; i<cont.Functions.Count; i++) {
					Function func = cont.Functions[i];
					if(func.Attr.Name == _attr.Name && func.isProperty)
						propertyFunc = func;
				}
				if(propertyFunc == null)
				{
					Property tmp = new Property(this);
					tmp.Attr.Enabled = true;
					cont.addFunction(tmp);
				}
				else
				{
					propertyFunc.Attr.merge(_attr);
				}
			}
		}
		
		public String Id { get { return _id; } }
		public String Name { get { return _name; } }
		virtual public String MangleName { get {
				if(CC.OpMap.ContainsKey(Name))
					return CC.OpMap[Name].MangleName + _id;
				return Name + Id;
			} }
		public String Access { get { return _access; } }
		public String ContextId { get { return _context; } set { _context = value; } }
		public DataType ContextType { get { return CC.Types[ContextId]; } }
		public bool Static { get { return _static; } }
		public override Arg Args { get { return _head; } set { _head = value; } }
		public DataType Return { get { return CC.Types[_returnId]; } }
		public string ReturnId { get { return _returnId; } }
		public string Line { get { return _line; } }
		public string File { get { return _file; } }
		override public string DebugTag { get {
				return CC.FileMap[_file] + ":" + _line;
			} }

		virtual public bool hasGetProperty { get {
				List<Function> funcs = CC.ContextMap[ContextId].Functions;
				for(int i=0; i<funcs.Count; i++)
				{
					Function func = funcs[i];
					if(func.Id != Id)
						if(func.Name == Name && func.Attr.Get != null)
							return true;
				}
				return false;
			} }
		virtual public bool hasSetProperty { get {
				List<Function> funcs = CC.ContextMap[ContextId].Functions;
				for(int i=0; i<funcs.Count; i++)
				{
					Function func = funcs[i];
					if(func.Id != Id)
						if(func.Name == Name && func.Attr.Set != null)
							return true;
				}
				return false;
			} }

		public bool IsOperator { get { return Name==null ? false : CC.OpMap.ContainsKey (Name); } }
		public bool IsMemberFunc { get { return _memberFunc && !Static; } }
		virtual public bool isProperty { get { return false; } }
		virtual public bool isConstructor { get { return false; } }
		virtual public bool isDestructor { get { return false; } }
		virtual public bool Generate { get { return CC.ContextMap[_context].Attr.Export || 
				_attr.Export || _attr.Get != null || _attr.Set != null; } }
		public Attributes Attr { get { return _attr; } }
		public CsharpGen CC { get { return _cc; } }
		public StringWriter CHeaderWriter { get { return CC.Files[File].CHeaderWriter; } }
		public StringWriter CSourceWriter { get { return CC.Files[File].CSourceWriter; } }
		public StringWriter CsWriter { get { return CC.Files[File].CsWriter; } }
		public StringWriter MainWriter { get { return CC.Files[File].MainWriter; } }
		
		String _id; // the unique if of this function
		String _name; // the user defined name of this function
		String _access; // access to this function, public, private, protected
		String _context; // the unique id of the context this function is defined in
		String _returnId; // the unique id for the return type of this function
		string _file; // the file this function occurs in
		string _line; // the line this function occurs on in the file
		bool _artificial;
		bool _memberFunc;
		bool _static;

		Arg _head;
		protected static int _curIndex = 0;

		// attributes set for this function
		Attributes _attr;

		protected CsharpGen _cc;
	} // class Function

	public class Constructor : Function
	{
		public Constructor(String type, XmlTextReader reader, CsharpGen generator)
			: base(type, reader, generator)
		{}

		// CPCC = C++ Public Call Constructor
		string CPCCRet { get { return CC.Types[ContextId].ResolvedName + " * "; } }
		string CPCCFunc { get { return CPCCRet + ESName + "(" + CPArgs + ")"; } }

		// CPCB = C++ Public Call Body
		string CPCBRet { get { return "return new " + CC.Types[ContextId].ResolvedName; } }
		string CPCBFunc { get { return CPCBRet + "(" + CPCArgs(Args) + ");"; } }

		public override void writeC()
		{
			if(Access != "public")
			{
				Console.WriteLine (DebugTag + " : warning cannot generate interface for non-public constructor.");
				return;
			}

			CHeaderWriter.WriteLine (CPCCFunc + ";");
			CSourceWriter.WriteLine (CPCCFunc);
			CSourceWriter.WriteLine ("{\n" + CPCBFunc + "\n}\n");
		}

		// CSPC = C# Public Constructor
		override protected string CSPRet { get { return ""; } }
		override protected string CSCEnum { get { return ""; } }
		string CSPCFunc { get { return CSPFunc; } }

		// CSCB = C# Constructor Body
		override protected string CSCRet(string str) {  return "CppHandle = " + str; }
		string CSCBFunc { get { return CSBFunc; } }

		// ESC = Extern Static Constructor
		override protected string ESRet { get { return "IntPtr"; } }
		string ESCFunc { get { return ESFunc; } }

		public override void writeCS()
		{
			if(Access != "public")
				return;

			CsWriter.WriteLine (CSAttr);
			CsWriter.WriteLine (ESCFunc);
			CsWriter.WriteLine (CSPCFunc + "\n{");
			CsWriter.WriteLine (CSCBFunc + "\nCppFree = true;\n}\n");
//			CsWriter.WriteLine ("_cppsharp_Object_setMonoObject(CppHandle, this.GetType().TypeHandle.Value);\n}\n");
		}

		override public bool isConstructor { get { return true; } }
		override public bool isDestructor { get { return false; } }
		override public bool Generate { get { return CC.ContextMap[ContextId].GenStructors ||
				CC.ContextMap[ContextId].Attr.Export; } }
		override public String MangleName { get { return "Constructor" + Id; } }
	} // class Constructor

	public class Destructor : Function
	{
		public Destructor(String type, XmlTextReader reader, CsharpGen generator)
			: base(type, reader, generator)
		{}
		
		public override void writeC()
		{
			if(Access != "public")
			{
				Console.WriteLine (DebugTag + " : error cannot generate interface for non-public destructor.");
				Environment.Exit (-1);
			}

			CHeaderWriter.WriteLine ("void " + ESName + "(" + CC.Types[ContextId].ResolvedName + " * arg);");
			CSourceWriter.WriteLine ("void " + ESName + "(" + CC.Types[ContextId].ResolvedName + " * arg)");
			CSourceWriter.WriteLine ("{\ndelete arg;");
			CSourceWriter.WriteLine ("}\n");
		}

		public override void writeCS()
		{
			// write the connection function
			CsWriter.WriteLine (CSAttr);
			CsWriter.WriteLine ("extern static void " + ESName + "(IntPtr handle);");
			
			// write the public member function
			CsWriter.WriteLine ("~" + CC.Types[ContextId].Name + "()\n{");
			CsWriter.WriteLine ("if(CppFree) " + ESName + "(CppHandle);");
			CsWriter.WriteLine ("}\n");
		}

		override public bool isConstructor { get { return false; } }
		override public bool isDestructor { get { return true; } }
		override public bool Generate { get { return CC.ContextMap[ContextId].GenStructors ||
				CC.ContextMap[ContextId].Attr.Export; } }
		override public String MangleName { get { return "Destructor" + Id; } }
	} // class Destructor

	public class FieldProperty : Function
	{
		public FieldProperty(Field field)
			: base(field)
		{
			_field = field;
		}

		string cvt(DataType t) { return CC.Converter.CSPType(t); }
		
		// CP = Cpp Public
		string CPGetName { get { return MangleNamespace + "_" + MangleName + "_Get"; } }
		string CPSetName { get { return MangleNamespace + "_" + MangleName + "_Set"; } }
		protected string CPArgs { get { return CPOpArg; } }
		string CPGetFunc { get { return CPRet + CPGetName + "(" + CPArgs + ")"; } }
		string CPSetFunc { get { return "void " + CPSetName + "(" + CPArgs + ", " + (_field.Args!=null ? _field.Args.CPArgs : "") + ")"; } }
		
		// CPC = Cpp Public Call
		string CPCSetAlloc(string str) { return CC.Converter.CPCRetCast(CC.Types.VoidType, Attr, str); }
		string CPCGetAlloc(string str) { return CPCAlloc(str); }
		string CPCSetFunc { get { return CPCSetAlloc(StaticNS + CPCFuncName) + " = " + CC.Converter.CPCType(new Arg(_field.Type, CC)) + ";"; } }
		string CPCGetFunc { get { return CPCGetAlloc(StaticNS + CPCFuncName) + ";"; } }
		
		public override void writeC()
		{
			if(Access != "public")
			{
				Console.WriteLine (DebugTag + " : warning cannot generate property for non-public member variable.");
				return;
			}
			
			if(Attr.Get != null) 
			{
				CHeaderWriter.WriteLine (CPGetFunc + ";");
				CSourceWriter.WriteLine (CPGetFunc + "\n{");
				CSourceWriter.WriteLine (CPCGetFunc + "\n}\n");
			}
			
			if(Attr.Set != null)
			{
				CHeaderWriter.WriteLine (CPSetFunc + ";");
				CSourceWriter.WriteLine (CPSetFunc + "\n{");
				CSourceWriter.WriteLine (CPCSetFunc + "\n}\n");
			}
		}
		
		// ES = Extern Static
		string MangleNamespace { get { return CC.ContextMap[ContextId].MangleNamespace; } }
		string ESUnsafe { get { return Args!=null ? (Args.HasPRPod ? "unsafe " : null) : ""; } }
		string ESQualifiers { get { return ESUnsafe + "extern static "; } }
		string ESGetRet { get { return CC.Converter.CSESType(_field.Type); } }
		string ESGetName { get { return MangleNamespace + "_" + MangleName + "_Get"; } }
		string ESSetName { get { return MangleNamespace + "_" + MangleName + "_Set"; } }
		string ESThisPtr { get { return (IsMemberFunc ? "IntPtr __arg" : ""); } }
		string ESGetFunc { get { return ESQualifiers + ESGetRet + " " + ESGetName + "(" + 
				ESThisPtr + ");"; } }
		string ESSetFunc { get { return ESQualifiers + "void " + ESSetName + "(" + 
				ESThisPtr + ", " + CC.Converter.CSESType(_field.Type) + " amp);"; } }

		public override void writeCS()
		{
			if(Access != "public") return;
			if(Attr.Get == null && Attr.Set == null) return;
			
			if(Attr.Get != null)
			{
				CsWriter.WriteLine (CSAttr);
				CsWriter.WriteLine (ESGetFunc);
			}
			if(Attr.Set != null)
			{
				CsWriter.WriteLine (CSAttr);
				CsWriter.WriteLine(ESSetFunc);
			}
			
			StringWriter file = CsWriter;
			
			file.Write ("public ");
			
			if(Attr.Get != null)
				file.Write (CC.Converter.CSPType(Return) + " ");
			else
				file.Write (CC.Converter.CSPType(_field.Type) + " ");
			
			file.WriteLine (Name + " {");
			
			if(Attr.Get != null) 
				file.WriteLine("\tget {\n\t\treturn " + ESGetName + "(CppHandle);\n\t}");
			
			if(Attr.Set != null)
			{
				file.WriteLine ("\tset {");
				file.WriteLine ("\t\t" + ESSetName + "(CppHandle, value" + (CC.Converter.UseHandle(_field.Type) ? ".CppHandle" : "") + ");");
				file.WriteLine ("\t}");
			}
			
			file.WriteLine ("}");
		}
		
		public override void writeMain()
		{
			if(Access != "public") return;

			if(CC.DllImport == null)
			{
				if(Attr.Get != null)
					MainWriter.WriteLine ("mono_add_internal_call (\"" + ContextType.NamespaceCs + 
					                      "::" + ESGetName + "\", " + ESGetName + ");");
				if(Attr.Set != null)
					MainWriter.WriteLine ("mono_add_internal_call (\"" + ContextType.NamespaceCs + 
					                      "::" + ESSetName + "\", " + ESSetName + ");");
			}
		}

		Field _field;
	}

	public class Property : Function
	{
		public Property(Function parent) : base(parent)
		{}

		public override void writeC()
		{}

		public override void writeCS()
		{
			if(!Attr.Enabled) return;

			//*****************************************************************
			// initialize the setter/getter information
			Context cont = CC.ContextMap[ContextId];
			List<Function> funcs = cont.Functions;
			Function getFunc = null;
			Function setFunc = null;
			for(int i=0; i<funcs.Count; i++)
			{
				if(funcs[i].Id == Attr.GetId)
				{
					getFunc = funcs[i];
					break;
				}
			}
			for(int i=0; i<funcs.Count; i++)
			{
				if(funcs[i].Id == Attr.SetId)
				{
					setFunc = funcs[i];
					break;
				}
			}

			//*****************************************************************
			// do some error checking and reporting, get attribute can only be attached to a function that
			// returns non void and takes no arguements, set attribute can only be attached to a function that
			// takes one arguement and returns void
			if(Attr.Get != null)
			{
				// if we are supposed to gen a get attribute for this function make sure it does not return void
				if(getFunc.Return.isVoid)
				{
					Console.WriteLine (getFunc.DebugTag + " : error cannot generate a get Property for a function " +
					                   "returning \"void\"");
					Environment.Exit(-1);
				}
				if(getFunc.Args != null && getFunc.Args.Count != 0)
				{
					Console.WriteLine (getFunc.DebugTag + " : error cannot generate a get Property for a function " +
					                   " that takes arguements");
					Environment.Exit(-1);
				}
			}
			
			if(Attr.Set != null)
			{
				if(setFunc.Args.Count != 1)
				{
					Console.WriteLine (setFunc.DebugTag + " : error cannot generate a set property for a function that takes \"" +
					                   Args.Count + "\" arguements, must be only 1");
					Environment.Exit(-1);
				}
				if(setFunc.Return.isVoid != true)
				{
					Console.WriteLine (setFunc.DebugTag + " : error cannot generate a set property for a function that" +
						" returns a value");
					Environment.Exit(-1);
				}
			}

			string propertyName = null;
			if(Attr.Get != null)
				propertyName = Attr.Get;
			if(Attr.Set != null && propertyName == null)
				propertyName = Attr.Set;

			if(Access != "public")
				Console.WriteLine (DebugTag + " : warning cannot generate interface for non-public function.");

			if(propertyName != null && Access == "public")
			{
				StringWriter file = CsWriter;

				file.Write ("public ");
				
				if(Attr.Get != null)
				{
					file.Write (CC.Converter.CSPType(getFunc.Return) + " ");
					propertyName = Attr.Get;
				}
				else
				{
					file.Write (CC.Converter.CSPType(setFunc.Args.Type) + " ");
				}
				
				file.WriteLine (propertyName + " {");

				if(Attr.Get != null) 
				{
					file.WriteLine("\tget {");
					file.WriteLine ("\t\treturn " + getFunc.ESName + "(CppHandle);");

					file.WriteLine ("\t}");
				}

				if(Attr.Set != null)
				{
					file.WriteLine ("\tset {");
					file.WriteLine ("\t\t" + setFunc.ESName + "(CppHandle, value);");
					file.WriteLine ("\t}");
				}

				file.WriteLine ("}");
			}
		}

		override public bool isProperty { get { return true; } }

	} // class Property
} // namespace cppsharp

