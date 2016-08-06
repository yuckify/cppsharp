using System;
using System.Xml;
using System.IO;

namespace cppsharp
{
	public class Field : ContextObject
	{
		public Field (XmlTextReader reader, CsharpGen cc)
			: base(reader, cc)
		{
			_typeId = reader["type"];
			String staticc = reader["static"];
			_static = false;
			if(staticc != null)
				if(staticc == "1") _static = true;

			if(Attr.Set != null)
				_arg = new Arg(Type, CC);
		}

		public void postProcess()
		{
			Context.Functions.Add (new FieldProperty(this));
		}

		public string TypeId { get { return _typeId; } }
		public DataType Type { get {return CC.Types[TypeId]; } }
		bool IsMemberFunc { get { return true; } }
		public String MangleName { get { return Name + Id; } }
		public DataType Return { get { return Attr.Get!=null ? Type : null; } }
		public bool Export { get { return Attr.Get != null || Attr.Set != null; } }
		public bool Static { get { return _static; } }
		public StringWriter CHeaderWriter { get { return CC.Files[File].CHeaderWriter; } }
		public StringWriter CSourceWriter { get { return CC.Files[File].CSourceWriter; } }
		public StringWriter CsWriter { get { return CC.Files[File].CsWriter; } }
		public StringWriter MainWriter { get { return CC.Files[File].MainWriter; } }
		public override Arg Args { get { return _arg; } set {} }

		string _typeId;
		bool _static;
		Arg _arg;
	}
}

