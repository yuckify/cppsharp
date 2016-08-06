using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;

namespace cppsharp
{
	public class Enumeration : ContextObject
	{
		public class Value
		{
			public Value() {}
			public Value(XmlTextReader reader)
			{
				_name = reader["name"];
				_init = reader["init"];
			}

			public string Name { get { return _name; } }
			public string Init { get { return _init; } }

			string _name;
			string _init;
		}

		public Enumeration (XmlTextReader reader, CsharpGen generator)
			: base(reader, generator)
		{
			_values = new List<Value>();
		}

		public void write ()
		{
			StringWriter file = CC.Files[File].CsWriter;

			file.WriteLine ("public enum " + Name);
			file.WriteLine ("{");

			for(int i=0; i<Values.Count; i++)
			{
				file.Write ("\t" + Values[i].Name + " = " + Values[i].Init);
				if(i != Values.Count-1)
					file.WriteLine (",");
				else
					file.WriteLine ("");
			}

			file.WriteLine ("};");
		}

		public void  postProcess()
		{
			CC.ContextMap[ContextId].addEnumeration(this);
		}

		public List<Value> Values { get { return _values; } }
		public void addValue(Value val) { _values.Add (val); }

		List<Value> _values;
	}
}

