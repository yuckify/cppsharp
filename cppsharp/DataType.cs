using System;
using System.Xml;
using System.Collections.Generic;

namespace cppsharp
{

	public class TypeMap
	{
		public TypeMap() {
			_types = new Dictionary<string, DataType>();

		}

		public DataType this[string id] { get { return _types[id]; } set { _types[id] = value; } }
		public void Add(DataType type)
		{
			type.Parent = this;
			if(type.TypeName == "void") _voidTypeId = type.Id;
			_types.Add(type.Id, type);
		}
		public void Add(Context cont) { DataType t = new ContextType(cont); t.Parent = this; Add (t); }
		public void Add(Enumeration en) { EnumerationType ent = new EnumerationType(en); ent.Parent = this; Add(ent); }

		public string VoidTypeId { get { return _voidTypeId; } }
		public DataType VoidType { get { return _types[_voidTypeId]; } }

		Dictionary<string, DataType> _types;
		string _voidTypeId;
	}

	abstract public class DataType : CompileObject
	{
		public enum AccessMode
		{
			NotApplicable,
			Public,
			Protected,
			Private
		}

		public AccessMode modeFromString(string modeStr)
		{
			switch(modeStr)
			{
			case "public": return AccessMode.Public;
			case "protected": return AccessMode.Protected;
			case "private": return AccessMode.Private;
			}

			return AccessMode.Public;
		}

		public DataType (XmlTextReader reader)
		{
			_id = reader["id"];
		}

		public DataType (string id) {
			_id = id;
		}

		public DataType() {}

		public override string DebugTag { get { return null; } }
		public override Arg Args { get { return null; } set { } }

		virtual public string Id { get { return _id; } }
		virtual public string TypeId { get { return null; } }
		virtual public string Name { get {
				if(Child != null) return Child.Name;
				return null;
			} }
		virtual public string ResolvedName { get {
				if(Child != null) return Child.ResolvedName;
				return null;
			} }
		public DataType Child { get { if(TypeId != null) return Parent[TypeId]; else return null; } }
		virtual public AccessMode Access { get { return AccessMode.NotApplicable; } }
		virtual public bool Const { get { return false; } }
		virtual public bool Volatile { get { return false; } }
		virtual public int Min { get { return 0; } }
		virtual public int Max { get { return 0; } }
		virtual public string TypeName { get { return null; } }
		public TypeMap Parent { get { return _parent; } set { _parent = value; } }

		virtual public string ContextId { get {
				if(Child != null) return Child.ContextId;
				return null;
			} }

		virtual public string ClassId { get {
				if(Child != null) return Child.ClassId;
				return null;
			} }

		virtual public string TypedeftoCS { get { 
				if(Child != null) return Child.TypedeftoCS;
				return null;
			}}

		virtual public bool IsTypedef { get {
				if(Child != null) return Child.IsTypedef;
				return false;
			}}

		virtual public string Namespace { get {
				if(Child != null)
					return Child.Namespace;
				return null;
			} }

		virtual public string NamespaceCs { get {
				if(Child != null)
					return Child.NamespaceCs;
				return Name;
			} }

		virtual public string MangleNamespace { get {
				if(Child != null) return Child.MangleNamespace;
				return Name;
			} }

		public bool IsPRPod { get { // pointer or refernce to plain old datatype
				return IsPod && (isPointer || isReference);
			}}

		public bool IsPREnum { get {
				return IsEnum && (isPointer || isReference);
			} }

		public bool IsRPod { get { // reference to plain old datatype
				return IsPod && isReference;
			}}

		public bool IsPRVClass { get { // pointer or reference or value to class type
				return (isPointer || isReference || isValue) && isClass;
			}}

		public bool IsPR { get { return isPointer || isReference; } }
		public bool IsRVClass { get { return (isReference || isValue) && isClass; } }
		public bool IsRV { get { return isReference || isValue; } }
		public bool IsPClass { get { return isPointer && isClass; } }
		public bool IsRClass { get { return isReference && isClass; } }
		public bool IsVClass { get { return isValue && isClass; } }
		public bool IsPVClass { get { return (isPointer || isValue) && isClass; } }

		virtual public bool isPointer { get {
				if(Child != null) return Child.isPointer;
				return false;
			} }

		virtual public bool IsPod { get {
				if(Child != null) return Child.IsPod;
				return false;
			} }

		public bool isVoid { get {
				if(Name == "void") return true;
				return false;
			}}
		
		virtual public string FullTypeName { get {
				if(Child != null)
					return Child.FullTypeName + " " + TypeName;
				return TypeName;
			} }

		virtual public bool isReference { get {
				if(Child != null) return Child.isReference;
				return false;
			}}

		virtual public bool IsEnum { get {
				if(Child != null) return Child.IsEnum;
				return false;
			}}

		public bool isValue { get { return !isReference && !isPointer; } }
		virtual public bool isNamespace { get { 
				if(Child != null) return Child.isNamespace;
				return false;
			} }
		virtual public bool isClass { get {
				if(Child != null) return Child.isClass;
				return false;
			}}
		virtual public string ClassName { get {
				if(Child != null)
					return Child.ClassName;
				return null;
			}}


		string _id;
		TypeMap _parent;
	} // class DataType

	// this is a hack to handle whatever the f*** "_0" is
	public class UnknownType : DataType {
		public UnknownType() : base("_0") 
		{
			_name = "void";
		}

		override public string Name { get { return _name; } }
		override public string TypeName { get { return _name; } }
		override public string ResolvedName { get { return _name; } }
		override public bool IsPod { get { return true; } }
		override public string Namespace { get { return Name; } }

		string _name;
	} // class UnknownType

	public class Typedef : DataType
	{
		public Typedef(XmlTextReader reader) : base(reader)
		{
			_name = reader["name"];
			_contextId = reader["context"];
			_type = reader["type"];
			_accessMode = modeFromString(reader["access"]);
		}

		override public string Name { get { return _name; } }
		override public string ContextId { get { return _contextId; } }
		override public AccessMode Access { get { return _accessMode; } }
		override public string TypeName { get { return _name; } }
		override public bool IsTypedef { get { return true; } }
		override public string TypedeftoCS { get { return Parent[_contextId].Name; } }
		override public string TypeId { get { return _type; } }

		override public string Namespace { get {
				if(ContextId != null)
					if(Parent[ContextId] != null) return Parent[ContextId].Namespace + "::" + Name;
				return null;
			} }

		string _name;
		string _contextId;
		string _type;
		DataType.AccessMode _accessMode;
	}

	public class FundamentalType : DataType
	{
		public FundamentalType(XmlTextReader reader) : base(reader)
		{
			_name = reader["name"];
		}

		override public string Name { get { return _name; } }
		override public string TypeName { get { return _name; } }
		override public string ResolvedName { get { return _name; } }
		override public bool IsPod { get { return true; } }
		override public string Namespace { get { return Name; } }

		string _name;
	}


	public class PointerType : DataType
	{
		public PointerType(XmlTextReader reader) : base(reader)
		{
			_type = reader["type"];
		}

		override public string TypeId { get { return _type; } }
		override public string TypeName { get { return "*"; } }
		override public bool isPointer { get { return true; } }

		string _type;
	}

	public class ReferenceType : DataType
	{
		public ReferenceType(XmlTextReader reader) : base(reader)
		{
			_type = reader["type"];
		}

		override public string TypeId { get { return _type; } }
		override public string TypeName { get { return "&"; } }
		override public bool isReference { get { return true; } }
		
		string _type;
	}

	public class CvQualifiedType : DataType
	{
		public CvQualifiedType(XmlTextReader reader) : base(reader)
		{
			_type = reader["type"];
			string constStr = reader["const"];
			string volatileStr = reader["volatile"];
			string restrictStr = reader["restrict"];
			if(constStr != null) if(constStr == "1") _const = true;
			if(volatileStr != null) if(volatileStr == "1") _volatile = true;
			if (restrictStr != null) if (restrictStr == "1") _restrict = true;

			if(!_const && !_volatile && !_restrict) throw new System.ArgumentException("CvQualifiedType is neither const nor" +
				"volatile");
		}

		override public bool Const { get { return _const; } }
		override public bool Volatile { get { return _volatile; } }
		override public string TypeId { get { return _type; } }
		override public string TypeName { get {
				if(_volatile) return "volatile";
				return "const";
			} }

		bool _const;
		bool _volatile;
		bool _restrict;
		string _type;
	}

	public class ArrayType : DataType
	{
		public ArrayType (XmlTextReader reader) : base(reader)
		{
			_type = reader["type"];
		}

		override public int Min { get { return 0; } }
		override public int Max { get { return 0; } }
		override public string TypeId { get { return _type; } }

		int _min;
		int _max;
		string _type;
	}

	public class ContextType : DataType
	{
		public ContextType(Context context) : base()
		{
			_context = context;
		}

		override public string Id { get { return _context.Id; } }
		override public string Name { get { return _context.Name; } }
		override public string ResolvedName { get { return Namespace; } }

		override public string ContextId { get { return _context.ContextId; } }
		override public string TypeName { get { return _context.Name; } }
		override public bool isClass { get { return _context.isClass; } }
		override public bool isNamespace { get { return _context.isNamespace; } }
		override public string ClassName { get { return TypeName; } }

		override public string ClassId { get { return _context.Id; } }

		override public string Namespace { get {
				if(ContextId != null)
					if(Parent[ContextId] != null) return Parent[ContextId].Namespace + "::" + _context.Name;
				return null;
			} }

		override public string MangleNamespace { get {
				if(ContextId != null)
					if(Parent[ContextId] != null)
						if(Parent[ContextId].NamespaceCs != null)
							return Parent[ContextId].NamespaceCs + "_" + _context.Name;
				else
					return _context.Name;
				return null;
			} }

		override public string NamespaceCs { get {
				if(ContextId != null)
					if(Parent[ContextId] != null)
						if(Parent[ContextId].NamespaceCs != null)
							return Parent[ContextId].NamespaceCs + "." + _context.Name;
						else
							return _context.Name;
				return null;
			} }

		Context _context;
	}

	public class EnumerationType : DataType
	{
		public EnumerationType(Enumeration en)
		{
			_enum = en;
		}

		override public string Id { get { return _enum.Id; } }
		override public string Name { get { return _enum.Name; } }
		override public string ResolvedName { get { return Namespace; } }
		override public string ContextId { get { return _enum.ContextId; } }
		override public string TypeName { get { return _enum.Name; } }
		override public bool IsEnum { get { return true; } }
		
		override public string Namespace { get {
				if(ContextId != null)
					if(Parent[ContextId] != null) return Parent[ContextId].Namespace + "::" + _enum.Name;
				return null;
			} }

		override public string NamespaceCs { get {
				if(ContextId != null)
					if(Parent[ContextId] != null)
						if(Parent[ContextId].NamespaceCs != null)
							return Parent[ContextId].NamespaceCs + "." + _enum.Name;
				else
					return _enum.Name;
				return null;
			} }

		Enumeration _enum;
	}

	public class FunctionType : DataType
	{
		public FunctionType (XmlTextReader reader) : base(reader)
		{

		}

		public override Arg Args { get { return _head; } set { _head = value; } }

		Arg _head;
		string _returnId;
		string _attributes;
	}

}

