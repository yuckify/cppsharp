using System;
using System.Xml;

namespace cppsharp
{
	// TODO subclass all context members from this
	/**
	 * The base class for all objects that will be inside of a Context.
	 */
	public class ContextObject : CompileObject
	{
		public ContextObject (ContextObject obj)
		{
			_id = obj._id;
			_name = obj._name;
			_contextId = obj._contextId;
			_access = obj._access;
			_attr = obj._attr;
			_file = obj._file;
			_line = obj._line;
			_cc = obj._cc;
		}

		public ContextObject (XmlTextReader reader, CsharpGen cc)
		{
			_id = reader["id"];
			_name = reader["name"];
			_contextId = reader["context"];
			_access = reader["access"];
			if(_access == null) _access = "public";
			_attr = new Attributes(reader);
			_file = reader["file"];
			_line = reader["line"];
			_cc = cc;
		}

		public string Id { get { return _id; } }
		public string Name { get { return _name; } }
		public string ContextId { get { return _contextId; } set { _contextId = value; } }
		public Context Context { get { return _cc.ContextMap[ContextId]; } }
		public DataType ContextType { get { return _cc.Types[ContextId]; } }
		public string Access { get {return _access; } }
		public bool IsPublic { get { return Access == "public"; } }
		public Attributes Attr { get { return _attr; } set { _attr = value; } }
		public string File { get { return _file; } }
		public string Line { get { return _line; } }
		public CsharpGen CC { get { return _cc; } }

		/**
		 * This should return some human readable string to define the location of an error such as function name
		 * and file name and line number.
		 */
		public override string DebugTag { get { return _cc.FileMap[_file] + ":" + _line; } }
		public override Arg Args { get { return null; } set {} }

		string _id;
		string _name;
		string _contextId;
		string _access;
		Attributes _attr;
		string _file;
		string _line;

		CsharpGen _cc;
	}
}

