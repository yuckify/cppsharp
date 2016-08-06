using System;
using System.Xml;

namespace cppsharp
{
	public class Arg
	{
		public Arg(XmlTextReader reader, CsharpGen cc)
		{
			_name = reader["name"];
			if(_name == null)
				_name = "amp";
			_typeId = reader["type"];
			_id = reader["id"];
			
			_cc = cc;
		}
		
		public Arg(DataType type, CsharpGen cc)
		{
			_name = "amp";
			if(type.ContextId != null)
				_typeId = type.ContextId;
			else
				_typeId = type.Id;
			
			_id = type.Id;
			
			_cc = cc;
		}
		
		public bool HasPRPod { get { return Type.IsPRPod || (Next!=null ? Next.HasPRPod : false); } }
		public bool HasPREnum { get { return Type.IsPREnum || (Next!=null ? Next.HasPREnum : false); } }
		public int Count { get { if(Next != null ) return 1 + Next.Count; return 1; } }

		string ESType { get { return CC.Converter.CSESType(Type); } }
		string ESArg { get { return ESType + " " + Name; } }
		public string ESArgs { get { return (Next != null) ? (ESArg + ", " + Next.ESArgs) : ESArg; } }

		string CPType { get { return CC.Converter.CPType(Type); } }
		string CPArg { get { return CPType + " " + Name; } }
		public string CPArgs { get { return (Next != null) ? (CPArg + ", " + Next.CPArgs) : CPArg; } }

		string CSPPodRef { get { return (Type.IsPRPod ? "ref " : ""); } }
		string CSPArg { get { return CSPPodRef + CC.Converter.CSPType(Type) + " " + Name; } }
		public string CSPArgs { get { return (Next != null) ? (CSPArg + ", " + Next.CSPArgs) : CSPArg; } }
		
		
		public void Add(Arg a)
		{
			if(_next != null) _next.Add(a);
			else _next = a;
		}

		public CsharpGen CC { get { return _cc; } }
		public String Name { get { return _name; } } 
		public String Id { get { return _id; } }
		public String TypeId { get { return _typeId; } }
		public DataType Type { get { return CC.Types[_typeId]; } }
		public Arg Next { get { return _next; } set { _next = value; } }
		
		bool DistinctArgNamesImpl(string str)
		{
			if(Next == null) return true;
			if(Next.Name == str) return false;
			return Next.DistinctArgNamesImpl(str);
		}
		public bool DistinctArgNames { get {
				if(Next == null) return true;
				if(DistinctArgNamesImpl(Name) == false) return false;
				return Next.DistinctArgNames;
			} }
		public bool ArgsHaveNames { get {
				if(Name == "amp") return false;
				if(Next == null) return true;
				bool ret = Next.ArgsHaveNames;
				if(ret == false) return false;
				return ret;
			} }
		public bool ContainsType(string type)
		{
			if(Type.Namespace == type) return true;
			if(Next != null) return Next.ContainsType(type);
			return false;
		}
		
		String _name;
		String _typeId;
		String _id;
		
		Arg _next;
		
		CsharpGen _cc;
	} // class Arg
}
